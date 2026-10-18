import type {
    CreateTradeInput,
    ImportTradesInput,
    Trade,
    UpdateTradeInput,
} from "@/types/models";
import { api } from "./client";

export async function listTrades(): Promise<Trade[]> {
    const response = await api.get<Trade[]>("/trade");
    return response.data;
}

export async function getTrade(id: number): Promise<Trade> {
    const response = await api.get<Trade>(`/trade/${id}`);
    return response.data;
}

export async function createTrade(input: CreateTradeInput): Promise<Trade> {
    const response = await api.post<Trade>("/trade", input);
    return response.data;
}

export async function updateTrade(id: number, input: UpdateTradeInput): Promise<void> {
    await api.put(`/trade/${id}`, input);
}

export async function deleteTrade(id: number): Promise<void> {
    await api.delete(`/trade/${id}`);
}

/** Bulk-adds trades under one market and returns how many were imported. */
export async function importTrades(input: ImportTradesInput): Promise<number> {
    const response = await api.post<{ imported: number }>("/trade/import", input);
    return response.data.imported;
}
