import { useMemo } from "react";
import { Link } from "react-router-dom";
import { Activity, Hash, Percent, Scale, TrendingDown, TrendingUp } from "lucide-react";
import { useTrades } from "@/hooks/useTrades";
import { calculateStats } from "@/lib/analytics";
import {
    formatCurrency,
    formatDuration,
    formatPercent,
    formatPrice,
    formatSignedCurrency,
} from "@/lib/format";
import { getErrorMessage } from "@/api/client";
import { cn } from "@/lib/utils";
import { PageHeader } from "@/components/layout/PageHeader";
import { StatCard } from "@/components/dashboard/StatCard";
import { EquityCurve } from "@/components/dashboard/EquityCurve";
import { DirectionBadge, ProfitLossValue } from "@/components/trades/DirectionBadge";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { buttonVariants } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";

export default function DashboardPage() {
    const { data: trades = [], isLoading, isError, error } = useTrades();

    const stats = useMemo(() => calculateStats(trades), [trades]);

    const recentTrades = useMemo(
        () =>
            [...trades]
                .sort(
                    (a, b) =>
                        new Date(b.exitTime).getTime() - new Date(a.exitTime).getTime()
                )
                .slice(0, 5),
        [trades]
    );

    if (isError) {
        return (
            <div className="grid gap-6">
                <PageHeader title="Dashboard" />
                <Alert variant="destructive">
                    <AlertDescription>{getErrorMessage(error)}</AlertDescription>
                </Alert>
            </div>
        );
    }

    const netTone =
        stats.netPnl > 0 ? "positive" : stats.netPnl < 0 ? "negative" : "default";

    return (
        <div className="grid gap-6">
            <PageHeader
                title="Dashboard"
                description="A snapshot of your trading performance."
            />

            {isLoading ? (
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    {Array.from({ length: 6 }).map((_, index) => (
                        <Skeleton key={index} className="h-28 rounded-xl" />
                    ))}
                </div>
            ) : (
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    <StatCard
                        label="Net P/L"
                        value={formatSignedCurrency(stats.netPnl)}
                        hint={`${stats.wins}W / ${stats.losses}L / ${stats.breakEven}BE`}
                        icon={<Activity className="size-4" />}
                        tone={netTone}
                    />
                    <StatCard
                        label="Win rate"
                        value={formatPercent(stats.winRate)}
                        hint={`${stats.wins + stats.losses} decisive trades`}
                        icon={<Percent className="size-4" />}
                    />
                    <StatCard
                        label="Profit factor"
                        value={stats.profitFactor === null ? "—" : stats.profitFactor.toFixed(2)}
                        hint={
                            stats.profitFactor === null
                                ? "No losing trades yet"
                                : "Gross profit ÷ gross loss"
                        }
                        icon={<Scale className="size-4" />}
                    />
                    <StatCard
                        label="Average win"
                        value={formatCurrency(stats.averageWin)}
                        hint={
                            stats.bestTrade
                                ? `Best ${formatSignedCurrency(stats.bestTrade.profitLoss)}`
                                : undefined
                        }
                        icon={<TrendingUp className="size-4" />}
                        tone="positive"
                    />
                    <StatCard
                        label="Average loss"
                        value={formatCurrency(stats.averageLoss)}
                        hint={
                            stats.worstTrade
                                ? `Worst ${formatSignedCurrency(stats.worstTrade.profitLoss)}`
                                : undefined
                        }
                        icon={<TrendingDown className="size-4" />}
                        tone="negative"
                    />
                    <StatCard
                        label="Total trades"
                        value={String(stats.totalTrades)}
                        hint={`${formatCurrency(stats.totalCommission)} in commission`}
                        icon={<Hash className="size-4" />}
                    />
                </div>
            )}

            <Card>
                <CardHeader>
                    <CardTitle>Equity curve</CardTitle>
                    <CardDescription>
                        Cumulative profit/loss across your closed trades.
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    {isLoading ? (
                        <Skeleton className="h-56 w-full rounded-xl" />
                    ) : (
                        <EquityCurve points={stats.equityCurve} />
                    )}
                </CardContent>
            </Card>

            <Card>
                <CardHeader className="grid-cols-[1fr_auto]">
                    <CardTitle>Recent trades</CardTitle>
                    <CardDescription>Your five most recent trades.</CardDescription>
                    <Link
                        to="/trades"
                        className={cn(
                            buttonVariants({ variant: "outline", size: "sm" }),
                            "col-start-2 row-span-2 row-start-1 self-start justify-self-end"
                        )}
                    >
                        View all
                    </Link>
                </CardHeader>
                <CardContent>
                    {isLoading ? (
                        <div className="grid gap-2">
                            {Array.from({ length: 3 }).map((_, index) => (
                                <Skeleton key={index} className="h-10 w-full" />
                            ))}
                        </div>
                    ) : recentTrades.length === 0 ? (
                        <div className="flex flex-col items-center gap-3 py-8 text-center">
                            <p className="text-sm text-muted-foreground">
                                You have not logged any trades yet.
                            </p>
                            <Link to="/trades" className={cn(buttonVariants())}>
                                Log your first trade
                            </Link>
                        </div>
                    ) : (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Instrument</TableHead>
                                    <TableHead>Direction</TableHead>
                                    <TableHead className="text-right">Entry</TableHead>
                                    <TableHead className="text-right">Exit</TableHead>
                                    <TableHead className="text-right">Duration</TableHead>
                                    <TableHead className="text-right">P/L</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {recentTrades.map((trade) => (
                                    <TableRow key={trade.id}>
                                        <TableCell className="font-medium">
                                            {trade.instrument}
                                        </TableCell>
                                        <TableCell>
                                            <DirectionBadge direction={trade.direction} />
                                        </TableCell>
                                        <TableCell className="text-right tabular-nums">
                                            {formatPrice(trade.entryPrice)}
                                        </TableCell>
                                        <TableCell className="text-right tabular-nums">
                                            {formatPrice(trade.exitPrice)}
                                        </TableCell>
                                        <TableCell className="text-right text-muted-foreground">
                                            {formatDuration(trade.entryTime, trade.exitTime)}
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <ProfitLossValue value={trade.profitLoss} />
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
