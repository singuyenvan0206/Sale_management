import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatCurrency(amount: number | string | Decimal) {
  const value = typeof amount === "string" ? parseFloat(amount) : Number(amount);
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
  }).format(value);
}

// Replicate the Decimal type from Prisma if needed for helper functions
type Decimal = {
  toNumber(): number;
};
