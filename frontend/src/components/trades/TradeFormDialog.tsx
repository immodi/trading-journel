import { useState, type FormEvent } from "react";
import { Loader2 } from "lucide-react";
import { Direction, DIRECTION_LABELS, type CreateTradeInput, type Trade } from "@/types/models";
import { useCreateTrade, useUpdateTrade } from "@/hooks/useTrades";
import { getErrorMessage } from "@/api/client";
import { fromDateTimeLocal, nowAsDateTimeLocal, toDateTimeLocal } from "@/lib/datetime";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";

interface TradeFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    /** When provided the dialog edits the trade, otherwise it creates a new one. */
    trade?: Trade | null;
}

interface FormState {
    instrument: string;
    direction: number;
    quantity: string;
    entryPrice: string;
    exitPrice: string;
    entryTime: string;
    exitTime: string;
    commission: string;
}

const DIRECTION_ITEMS = {
    [Direction.Long]: DIRECTION_LABELS[Direction.Long],
    [Direction.Short]: DIRECTION_LABELS[Direction.Short],
};

/**
 * Per-unit commission applied by the API when a trade is submitted without an
 * explicit commission (mirrors `TradeMapper.CommissionPerUnit`). Shown here so
 * the default is visible and editable.
 */
const DEFAULT_COMMISSION_PER_UNIT = 1.82;

function defaultCommission(quantity: string): string {
    const parsed = Number(quantity);

    if (!Number.isInteger(parsed) || parsed <= 0) return "";

    return (DEFAULT_COMMISSION_PER_UNIT * parsed).toFixed(2);
}

function initialForm(trade?: Trade | null): FormState {
    if (!trade) {
        const quantity = "1";

        return {
            instrument: "",
            direction: Direction.Long,
            quantity,
            entryPrice: "",
            exitPrice: "",
            entryTime: nowAsDateTimeLocal(),
            exitTime: nowAsDateTimeLocal(),
            commission: defaultCommission(quantity),
        };
    }

    return {
        instrument: trade.instrument,
        direction: trade.direction,
        quantity: String(trade.quantity),
        entryPrice: String(trade.entryPrice),
        exitPrice: String(trade.exitPrice),
        entryTime: toDateTimeLocal(trade.entryTime),
        exitTime: toDateTimeLocal(trade.exitTime),
        commission: trade.commission.toFixed(2),
    };
}

export function TradeFormDialog({ open, onOpenChange, trade }: TradeFormDialogProps) {
    const isEditing = Boolean(trade);
    const createTrade = useCreateTrade();
    const updateTrade = useUpdateTrade();
    const isSaving = createTrade.isPending || updateTrade.isPending;

    const [form, setForm] = useState<FormState>(() => initialForm(trade));
    const [error, setError] = useState("");
    // Once the commission is edited by hand it stops tracking the quantity.
    const [commissionEdited, setCommissionEdited] = useState(isEditing);

    function update<K extends keyof FormState>(key: K, value: FormState[K]) {
        setForm((previous) => ({ ...previous, [key]: value }));
    }

    function updateQuantity(value: string) {
        setForm((previous) => ({
            ...previous,
            quantity: value,
            commission: commissionEdited ? previous.commission : defaultCommission(value),
        }));
    }

    function updateCommission(value: string) {
        setCommissionEdited(true);
        update("commission", value);
    }

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();

        const quantity = Number(form.quantity);
        const entryPrice = Number(form.entryPrice);
        const exitPrice = Number(form.exitPrice);
        const commission =
            form.commission.trim() === "" ? undefined : Number(form.commission);

        if (!form.instrument.trim()) {
            setError("Instrument is required.");
            return;
        }
        if (!Number.isInteger(quantity) || quantity <= 0) {
            setError("Quantity must be a whole number greater than zero.");
            return;
        }
        if (!Number.isFinite(entryPrice) || entryPrice < 0) {
            setError("Entry price must be a non-negative number.");
            return;
        }
        if (!Number.isFinite(exitPrice) || exitPrice < 0) {
            setError("Exit price must be a non-negative number.");
            return;
        }
        if (commission !== undefined && (!Number.isFinite(commission) || commission < 0)) {
            setError("Commission must be a non-negative number.");
            return;
        }
        if (!form.entryTime || !form.exitTime) {
            setError("Entry and exit times are required.");
            return;
        }
        if (new Date(form.exitTime) < new Date(form.entryTime)) {
            setError("Exit time cannot be before the entry time.");
            return;
        }

        const input: CreateTradeInput = {
            instrument: form.instrument.trim().toUpperCase(),
            direction: form.direction as Direction,
            quantity,
            entryPrice,
            exitPrice,
            entryTime: fromDateTimeLocal(form.entryTime),
            exitTime: fromDateTimeLocal(form.exitTime),
            ...(commission !== undefined ? { commission } : {}),
        };

        try {
            if (trade) {
                await updateTrade.mutateAsync({ id: trade.id, input });
            } else {
                await createTrade.mutateAsync(input);
            }

            onOpenChange(false);
        } catch (submitError) {
            setError(getErrorMessage(submitError));
        }
    }

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-lg">
                <DialogHeader>
                    <DialogTitle>{isEditing ? "Edit trade" : "New trade"}</DialogTitle>
                    <DialogDescription>
                        Profit/loss is calculated by the server from these values.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="grid gap-4">
                    <div className="grid gap-4 sm:grid-cols-2">
                        <div className="grid gap-2">
                            <Label htmlFor="instrument">Instrument</Label>
                            <Input
                                id="instrument"
                                placeholder="MNQ"
                                value={form.instrument}
                                onChange={(event) => update("instrument", event.target.value)}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="direction">Direction</Label>
                            <Select
                                items={DIRECTION_ITEMS}
                                value={form.direction}
                                onValueChange={(value) =>
                                    update("direction", value as number)
                                }
                            >
                                <SelectTrigger id="direction" className="w-full">
                                    <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value={Direction.Long}>
                                        {DIRECTION_LABELS[Direction.Long]}
                                    </SelectItem>
                                    <SelectItem value={Direction.Short}>
                                        {DIRECTION_LABELS[Direction.Short]}
                                    </SelectItem>
                                </SelectContent>
                            </Select>
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="quantity">Quantity</Label>
                            <Input
                                id="quantity"
                                type="number"
                                min="1"
                                step="1"
                                value={form.quantity}
                                onChange={(event) => updateQuantity(event.target.value)}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="entryPrice">Entry price</Label>
                            <Input
                                id="entryPrice"
                                type="number"
                                min="0"
                                step="0.01"
                                placeholder="0.00"
                                value={form.entryPrice}
                                onChange={(event) => update("entryPrice", event.target.value)}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="exitPrice">Exit price</Label>
                            <Input
                                id="exitPrice"
                                type="number"
                                min="0"
                                step="0.01"
                                placeholder="0.00"
                                value={form.exitPrice}
                                onChange={(event) => update("exitPrice", event.target.value)}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="commission">Commission</Label>
                            <Input
                                id="commission"
                                type="number"
                                min="0"
                                step="0.01"
                                placeholder="0.00"
                                value={form.commission}
                                onChange={(event) => updateCommission(event.target.value)}
                            />
                            <p className="text-xs text-muted-foreground">
                                Total for the trade. Defaults to{" "}
                                {DEFAULT_COMMISSION_PER_UNIT} × quantity.
                            </p>
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="entryTime">Entry time</Label>
                            <Input
                                id="entryTime"
                                type="datetime-local"
                                value={form.entryTime}
                                onChange={(event) => update("entryTime", event.target.value)}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="exitTime">Exit time</Label>
                            <Input
                                id="exitTime"
                                type="datetime-local"
                                value={form.exitTime}
                                onChange={(event) => update("exitTime", event.target.value)}
                            />
                        </div>
                    </div>

                    {error && (
                        <Alert variant="destructive">
                            <AlertDescription>{error}</AlertDescription>
                        </Alert>
                    )}

                    <DialogFooter>
                        <Button
                            type="button"
                            variant="outline"
                            onClick={() => onOpenChange(false)}
                        >
                            Cancel
                        </Button>
                        <Button type="submit" disabled={isSaving}>
                            {isSaving && <Loader2 className="animate-spin" />}
                            {isEditing ? "Save changes" : "Add trade"}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
