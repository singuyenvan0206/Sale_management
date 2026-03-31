import { NextResponse } from "next/server";
import { query } from "@/lib/db";

export async function GET() {
  try {
    const promotions = await query("SELECT * FROM promotions WHERE IsActive = 1 ORDER BY EndDate DESC");
    return NextResponse.json(promotions);
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}

export async function POST(request: Request) {
  try {
    const body = await request.json();
    const { Name, Type, DiscountPercent, DiscountAmount, StartDate, EndDate, TargetCategoryId, RequiredProductId } = body;
    
    await query(
      "INSERT INTO promotions (Name, Type, DiscountPercent, DiscountAmount, StartDate, EndDate, TargetCategoryId, RequiredProductId, IsActive, CreatedDate) VALUES (?, ?, ?, ?, ?, ?, ?, ?, 1, NOW())",
      [Name, Type, DiscountPercent || 0, DiscountAmount || 0, StartDate, EndDate, TargetCategoryId, RequiredProductId]
    );
    
    return NextResponse.json({ message: "Promotion added successfully" });
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}
