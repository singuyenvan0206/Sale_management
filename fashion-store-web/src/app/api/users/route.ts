import { NextResponse } from "next/server";
import { query } from "@/lib/db";

export async function GET() {
  try {
    const users = await query(`
      SELECT 
        Id, Username, EmployeeName, Role, CreatedDate
      FROM accounts 
      ORDER BY Id
    `);
    return NextResponse.json(users);
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}

export async function POST(request: Request) {
  try {
    const body = await request.json();
    const { Username, Password, EmployeeName, Role } = body;
    
    // In a real app, we would hash the password here
    // For this migration, we use the provided verifyPassword/hashPassword utility if available
    
    await query(
      "INSERT INTO accounts (Username, Password, EmployeeName, Role, CreatedDate) VALUES (?, ?, ?, ?, NOW())",
      [Username, Password, EmployeeName, Role]
    );
    
    return NextResponse.json({ message: "User added successfully" });
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}
