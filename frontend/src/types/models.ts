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

export interface Trade {
    id: number;
    userId: number;
    instrument: string;
    direction: Direction;
    entryPrice: number;
    exitPrice: number;
    quantity: number;
    entryTime: string;
    exitTime: string;
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
    /**
     * Total commission for the trade. Omit to let the server apply its default
     * (per-unit commission × quantity).
     */
    commission?: number;
}

export type UpdateTradeInput = Partial<CreateTradeInput>;
