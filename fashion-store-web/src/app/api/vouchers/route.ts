import { NextResponse } from "next/server";
import { query } from "@/lib/db";

export async function GET() {
  try {
    const vouchers = await query("SELECT * FROM vouchers WHERE IsActive = 1 ORDER BY CreatedDate DESC");
    return NextResponse.json(vouchers);
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}

export async function POST(request: Request) {
  try {
    const body = await request.json();
    const { Code, DiscountType, DiscountValue, MinInvoiceAmount, StartDate, EndDate, UsageLimit } = body;
    
    await query(
      "INSERT INTO vouchers (Code, DiscountType, DiscountValue, MinInvoiceAmount, StartDate, EndDate, UsageLimit, UsedCount, IsActive, CreatedDate) VALUES (?, ?, ?, ?, ?, ?, ?, 0, 1, NOW())",
      [Code, DiscountType, DiscountValue, MinInvoiceAmount, StartDate, EndDate, UsageLimit]
    );
    
    return NextResponse.json({ message: "Voucher added successfully" });
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}
