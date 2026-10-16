const currencyFormatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
});

const priceFormatter = new Intl.NumberFormat("en-US", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 4,
});

const integerFormatter = new Intl.NumberFormat("en-US");

export function formatCurrency(value: number): string {
    return currencyFormatter.format(value);
}

export function formatSignedCurrency(value: number): string {
    const formatted = currencyFormatter.format(Math.abs(value));
    if (value > 0) return `+${formatted}`;
    if (value < 0) return `-${formatted}`;
    return formatted;
}

export function formatPrice(value: number): string {
    return priceFormatter.format(value);
}

export function formatInteger(value: number): string {
    return integerFormatter.format(value);
}

export function formatPercent(value: number, fractionDigits = 1): string {
    return `${(value * 100).toFixed(fractionDigits)}%`;
}

export function formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString(undefined, {
        year: "numeric",
        month: "short",
        day: "numeric",
    });
}

export function formatDateTime(iso: string): string {
    return new Date(iso).toLocaleString(undefined, {
        year: "numeric",
        month: "short",
        day: "numeric",
        hour: "2-digit",
        minute: "2-digit",
    });
}

/**
 * Human readable duration between two ISO timestamps, e.g. "1h 25m".
 */
export function formatDuration(startIso: string, endIso: string): string {
    const milliseconds = new Date(endIso).getTime() - new Date(startIso).getTime();

    if (!Number.isFinite(milliseconds) || milliseconds < 0) return "—";

    const totalMinutes = Math.floor(milliseconds / 60000);
    const days = Math.floor(totalMinutes / 1440);
    const hours = Math.floor((totalMinutes % 1440) / 60);
    const minutes = totalMinutes % 60;

    if (days > 0) return `${days}d ${hours}h`;
    if (hours > 0) return `${hours}h ${minutes}m`;
    return `${minutes}m`;
}
