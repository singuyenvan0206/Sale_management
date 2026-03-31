import { NextResponse } from "next/server";
import { query } from "@/lib/db";

export async function GET() {
  try {
    const customers = await query("SELECT * FROM customers WHERE IsActive = 1 ORDER BY Name");
    return NextResponse.json(customers);
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}

export async function POST(request: Request) {
  try {
    const body = await request.json();
    const { Name, Phone, Email, Address, CustomerType } = body;
    
    await query(
      "INSERT INTO customers (Name, Phone, Email, Address, CustomerType, CreatedDate) VALUES (?, ?, ?, ?, ?, NOW())",
      [Name, Phone, Email, Address, CustomerType || 'Regular']
    );
    
    return NextResponse.json({ message: "Customer added successfully" });
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}
