import { NextResponse } from "next/server";
import { query } from "@/lib/db";

export async function GET() {
  try {
    const categories = await query("SELECT * FROM categories WHERE IsActive = 1 ORDER BY Name");
    return NextResponse.json(categories);
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}

export async function POST(request: Request) {
  try {
    const body = await request.json();
    const { Name, Description, TaxPercent } = body;
    
    await query(
      "INSERT INTO categories (Name, Description, TaxPercent, CreatedDate) VALUES (?, ?, ?, NOW())",
      [Name, Description || "", TaxPercent || 0]
    );
    
    return NextResponse.json({ message: "Category added successfully" });
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}
