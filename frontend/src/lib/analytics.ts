import type { Trade } from "@/types/models";

export interface EquityPoint {
    index: number;
    equity: number;
    date: string;
}

export interface TradeStats {
    totalTrades: number;
    wins: number;
    losses: number;
    breakEven: number;
    netPnl: number;
    grossProfit: number;
    grossLoss: number;
    totalCommission: number;
    winRate: number;
    averageWin: number;
    averageLoss: number;
    /** null when there are no losing trades (division by zero). */
    profitFactor: number | null;
    bestTrade: Trade | null;
    worstTrade: Trade | null;
    equityCurve: EquityPoint[];
}

/**
 * Trade statistics for a single market currency. Monetary figures are only
 * ever aggregated within one currency, so they can be compared and displayed
 * together safely.
 */
export interface CurrencyStats {
    /** Normalized ISO 4217 currency code all trades in this group share. */
    currency: string;
    stats: TradeStats;
}

/**
 * Currency-independent metrics. These count trades rather than summing money,
 * so they remain meaningful when the journal spans several currencies.
 */
export interface OverallStats {
    totalTrades: number;
    wins: number;
    losses: number;
    breakEven: number;
    winRate: number;
}

/**
 * A market present in the loaded trade history. Derived from the trades, which
 * already carry their market's name and currency, so the dashboard needs no
 * extra markets request to build its selector.
 */
export interface MarketSummary {
    id: number;
    name: string;
    /** Normalized ISO 4217 currency inherited by every trade in the market. */
    currency: string;
    tradeCount: number;
}

const EMPTY_STATS: TradeStats = {
    totalTrades: 0,
    wins: 0,
    losses: 0,
    breakEven: 0,
    netPnl: 0,
    grossProfit: 0,
    grossLoss: 0,
    totalCommission: 0,
    winRate: 0,
    averageWin: 0,
    averageLoss: 0,
    profitFactor: null,
    bestTrade: null,
    worstTrade: null,
    equityCurve: [],
};

/**
 * Normalizes a market currency code so groups are consistent regardless of
 * casing or surrounding whitespace. Missing codes fall back to USD, matching
 * the default used when formatting money.
 */
export function normalizeCurrency(currency: string | null | undefined): string {
    return currency?.trim().toUpperCase() || "USD";
}

/**
 * Computes the performance metrics shown on the dashboard from the full trade
 * history. Trades are ordered chronologically by exit time for the equity curve.
 *
 * Monetary metrics are summed directly, so every trade passed in must share the
 * same currency. Use {@link calculateStatsByCurrency} when the history may span
 * several currencies.
 */
export function calculateStats(trades: Trade[]): TradeStats {
    if (trades.length === 0) return EMPTY_STATS;

    const chronological = [...trades].sort(
        (a, b) => new Date(a.exitTime).getTime() - new Date(b.exitTime).getTime()
    );

    let wins = 0;
    let losses = 0;
    let breakEven = 0;
    let grossProfit = 0;
    let grossLoss = 0;
    let totalCommission = 0;

    let bestTrade: Trade | null = null;
    let worstTrade: Trade | null = null;

    const equityCurve: EquityPoint[] = [];
    let equity = 0;

    chronological.forEach((trade, index) => {
        const pnl = trade.profitLoss;

        if (pnl > 0) {
            wins += 1;
            grossProfit += pnl;
        } else if (pnl < 0) {
            losses += 1;
            grossLoss += Math.abs(pnl);
        } else {
            breakEven += 1;
        }

        totalCommission += trade.commission;

        if (bestTrade === null || pnl > bestTrade.profitLoss) bestTrade = trade;
        if (worstTrade === null || pnl < worstTrade.profitLoss) worstTrade = trade;

        equity += pnl;
        equityCurve.push({ index, equity, date: trade.exitTime });
    });

    const decisiveTrades = wins + losses;

    return {
        totalTrades: trades.length,
        wins,
        losses,
        breakEven,
        netPnl: equity,
        grossProfit,
        grossLoss,
        totalCommission,
        winRate: decisiveTrades === 0 ? 0 : wins / decisiveTrades,
        averageWin: wins === 0 ? 0 : grossProfit / wins,
        averageLoss: losses === 0 ? 0 : grossLoss / losses,
        profitFactor: grossLoss === 0 ? null : grossProfit / grossLoss,
        bestTrade,
        worstTrade,
        equityCurve,
    };
}

/**
 * Computes currency-independent metrics across the whole journal. Only counts
 * and ratios are produced, so trades in different currencies can be combined
 * without producing a misleading monetary total.
 */
export function calculateOverallStats(trades: Trade[]): OverallStats {
    let wins = 0;
    let losses = 0;
    let breakEven = 0;

    for (const trade of trades) {
        const pnl = trade.profitLoss;

        if (pnl > 0) wins += 1;
        else if (pnl < 0) losses += 1;
        else breakEven += 1;
    }

    const decisiveTrades = wins + losses;

    return {
        totalTrades: trades.length,
        wins,
        losses,
        breakEven,
        winRate: decisiveTrades === 0 ? 0 : wins / decisiveTrades,
    };
}

/**
 * Groups the journal by market currency and computes {@link calculateStats}
 * independently for each group. Each returned entry contains its own monetary
 * totals and equity curve, so trades in different currencies are never summed
 * together.
 */
export function calculateStatsByCurrency(trades: Trade[]): CurrencyStats[] {
    const groups = new Map<string, Trade[]>();

    for (const trade of trades) {
        const currency = normalizeCurrency(trade.currency);
        const group = groups.get(currency);

        if (group) group.push(trade);
        else groups.set(currency, [trade]);
    }

    return Array.from(groups, ([currency, group]) => ({
        currency,
        stats: calculateStats(group),
    })).sort((a, b) => a.currency.localeCompare(b.currency));
}

/**
 * Lists the markets that appear in the trade history, each with its currency
 * and trade count. Sorted by name for a stable, scannable selector.
 */
export function getMarketSummaries(trades: Trade[]): MarketSummary[] {
    const markets = new Map<number, MarketSummary>();

    for (const trade of trades) {
        const existing = markets.get(trade.marketId);

        if (existing) {
            existing.tradeCount += 1;
        } else {
            markets.set(trade.marketId, {
                id: trade.marketId,
                name: trade.marketName,
                currency: normalizeCurrency(trade.currency),
                tradeCount: 1,
            });
        }
    }

    return Array.from(markets.values()).sort(
        (a, b) => a.name.localeCompare(b.name) || a.id - b.id
    );
}

/**
 * Computes statistics for a single market. Every trade in a market shares that
 * market's currency, so the resulting monetary totals and equity curve stay
 * within one currency.
 */
export function calculateStatsForMarket(trades: Trade[], marketId: number): TradeStats {
    return calculateStats(trades.filter((trade) => trade.marketId === marketId));
}
