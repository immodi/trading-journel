import type { CreateMarketInput, Market, UpdateMarketInput } from "@/types/models";
import { api } from "./client";

export async function listMarkets(): Promise<Market[]> {
    const response = await api.get<Market[]>("/market");
    return response.data;
}

export async function getMarket(id: number): Promise<Market> {
    const response = await api.get<Market>(`/market/${id}`);
    return response.data;
}

export async function createMarket(input: CreateMarketInput): Promise<Market> {
    const response = await api.post<Market>("/market", input);
    return response.data;
}

export async function updateMarket(id: number, input: UpdateMarketInput): Promise<void> {
    await api.put(`/market/${id}`, input);
}

export async function deleteMarket(id: number): Promise<void> {
    await api.delete(`/market/${id}`);
}
