import { NextResponse } from "next/server";
import { query } from "@/lib/db";

export async function GET() {
  try {
    const suppliers = await query("SELECT * FROM suppliers WHERE IsActive = 1 ORDER BY Name");
    return NextResponse.json(suppliers);
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}

export async function POST(request: Request) {
  try {
    const body = await request.json();
    const { Name, Phone, Email, Address, Description } = body;
    
    await query(
      "INSERT INTO suppliers (Name, Phone, Email, Address, Description, CreatedDate) VALUES (?, ?, ?, ?, ?, NOW())",
      [Name, Phone, Email, Address, Description || ""]
    );
    
    return NextResponse.json({ message: "Supplier added successfully" });
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}
