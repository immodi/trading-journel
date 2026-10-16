/**
 * Trade direction. Mirrors the backend `Domain.Enum.Direction`:
 * 0 = Long, 1 = Short, 2 = Sideways.
 */
export const Direction = {
    Long: 0,
    Short: 1,
} as const;

export type Direction = (typeof Direction)[keyof typeof Direction];

export const DIRECTION_LABELS: Record<number, string> = {
    [Direction.Long]: "Long",
    [Direction.Short]: "Short",
};

export interface Market {
    id: number;
    userId: number;
    name: string;
    /** ISO 4217 currency code applied to every trade in the market. */
    currency: string;
    /** Number of trades currently associated with the market. */
    tradeCount: number;
    createdAt: string;
    updatedAt: string;
}

export interface CreateMarketInput {
    name: string;
    currency: string;
}

export type UpdateMarketInput = Partial<CreateMarketInput>;

export interface Trade {
    id: number;
    userId: number;
    marketId: number;
    marketName: string;
    /** Currency inherited from the trade's market. */
    currency: string;
    instrument: string;
    direction: Direction;
    entryPrice: number;
    exitPrice: number;
    quantity: number;
    entryTime: string;
    exitTime: string;
    /** Flat commission charged per unit. */
    commissionPerUnit: number;
    /** Total commission, derived by the server as `commissionPerUnit × quantity`. */
    commission: number;
    profitLoss: number;
    createdAt: string;
    updatedAt: string;
}

export interface User {
    id: number;
    username: string;
    email: string;
    createdAt: string;
    trades: Trade[];
}

export interface CreateTradeInput {
    instrument: string;
    direction: Direction;
    entryPrice: number;
    exitPrice: number;
    quantity: number;
    entryTime: string;
    exitTime: string;
    /** The market the trade belongs to; its currency is inherited by the trade. */
    marketId: number;
    /** Flat commission charged per unit; the server derives the total. */
    commissionPerUnit: number;
}

export type UpdateTradeInput = Partial<CreateTradeInput>;
