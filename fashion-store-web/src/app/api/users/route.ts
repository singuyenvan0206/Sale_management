import { NextResponse } from "next/server";
import { query, querySingle } from "@/lib/db";

export async function GET() {
  try {
    const users = await query(`
      SELECT 
        "Id", "Username", "EmployeeName", "Role", "CreatedDate"
      FROM "employees" 
      ORDER BY "Id"
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
    
    await query(
      'INSERT INTO "employees" ("Username", "Password", "EmployeeName", "Role", "CreatedDate") VALUES ($1, $2, $3, $4, NOW())',
      [Username, Password, EmployeeName || "", Role]
    );
    
    return NextResponse.json({ message: "User added successfully" });
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}

export async function PUT(request: Request) {
  try {
    const body = await request.json();
    const { Id, Username, Password, EmployeeName, Role } = body;
    
    if (Password) {
      await query(
        'UPDATE "employees" SET "Username" = $1, "Password" = $2, "EmployeeName" = $3, "Role" = $4 WHERE "Id" = $5',
        [Username, Password, EmployeeName, Role, Id]
      );
    } else {
      await query(
        'UPDATE "employees" SET "Username" = $1, "EmployeeName" = $2, "Role" = $3 WHERE "Id" = $4',
        [Username, EmployeeName, Role, Id]
      );
    }
    
    return NextResponse.json({ message: "User updated successfully" });
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}

export async function DELETE(request: Request) {
  try {
    const { searchParams } = new URL(request.url);
    const id = searchParams.get("id");
    
    // Check if employee has processed invoices
    const hasInvoices = await querySingle('SELECT 1 FROM "invoices" WHERE "EmployeeId" = $1 LIMIT 1', [id]);
    if (hasInvoices) {
      return NextResponse.json({ error: "Không thể xóa nhân viên đã có lịch sử giao dịch" }, { status: 400 });
    }
    
    await query('DELETE FROM "employees" WHERE "Id" = $1', [id]);
    return NextResponse.json({ message: "User deleted successfully" });
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}
