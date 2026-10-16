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
 * Computes the performance metrics shown on the dashboard from the full trade
 * history. Trades are ordered chronologically by exit time for the equity curve.
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
