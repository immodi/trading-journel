import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    createTrade,
    deleteTrade,
    listTrades,
    updateTrade,
} from "@/api/trades";
import type { CreateTradeInput, UpdateTradeInput } from "@/types/models";

export const tradeKeys = {
    all: ["trades"] as const,
    detail: (id: number) => ["trades", id] as const,
};

export function useTrades() {
    return useQuery({
        queryKey: tradeKeys.all,
        queryFn: listTrades,
    });
}

export function useCreateTrade() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (input: CreateTradeInput) => createTrade(input),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: tradeKeys.all });
        },
    });
}

export function useUpdateTrade() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ id, input }: { id: number; input: UpdateTradeInput }) =>
            updateTrade(id, input),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: tradeKeys.all });
        },
    });
}

export function useDeleteTrade() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: number) => deleteTrade(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: tradeKeys.all });
        },
    });
}
