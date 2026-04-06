import { NextRequest, NextResponse } from "next/server";
import { query, querySingle } from "@/lib/db";
import { Success, ErrorResponse, ServerError, Unauthorized } from "@/lib/api-response";
import { getServerSession } from "next-auth";

export async function GET(req: NextRequest) {
  try {
    const { searchParams } = new URL(req.url);
    const q = searchParams.get("q") || "";

    let sql = `
      SELECT 
        c.*, 
        COALESCE(SUM(i."Total"), 0) as "TotalSpent"
      FROM "customers" c
      LEFT JOIN "invoices" i ON c."Id" = i."CustomerId"
      WHERE c."IsActive" = true
    `;
    const params: any[] = [];

    if (q) {
      sql += ` AND (c."Name" ILIKE $${params.length + 1} OR c."Phone" ILIKE $${params.length + 2})`;
      params.push(`%${q}%`, `%${q}%`);
    }

    sql += ` GROUP BY c."Id" ORDER BY c."CreatedDate" DESC`;

    const customers = await query(sql, params);
    return Success(customers);
  } catch (error) {
    return ServerError(error);
  }
}

export async function POST(request: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const body = await request.json();
    const { Name, Phone, Email, Address, CustomerType } = body;

    if (!Name || !Phone) return ErrorResponse("Vui lòng nhập Tên và Số điện thoại khách hàng.");

    await query(
      'INSERT INTO "customers" ("Name", "Phone", "Email", "Address", "CustomerType", "IsActive", "CreatedDate") VALUES ($1, $2, $3, $4, $5, true, NOW())',
      [Name, Phone, Email, Address, CustomerType || 'Regular']
    );

    return Success(null, "Khách hàng đã được thêm thành công.", 201);
  } catch (error) {
    return ServerError(error);
  }
}

export async function PUT(request: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const body = await request.json();
    const { Id, Name, Phone, Email, Address, CustomerType, IsActive, Points } = body;

    if (!Id) return ErrorResponse("Thiếu ID khách hàng.");

    await query(
      `UPDATE "customers" 
       SET "Name" = $1, "Phone" = $2, "Email" = $3, "Address" = $4, 
           "CustomerType" = $5, "IsActive" = $6, "Points" = $7, "UpdatedDate" = NOW() 
       WHERE "Id" = $8`,
      [Name, Phone, Email, Address, CustomerType, IsActive, Points, Id]
    );

    return Success(null, "Cập nhật thông tin khách hàng thành công.");
  } catch (error) {
    return ServerError(error);
  }
}

export async function DELETE(request: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const { searchParams } = new URL(request.url);
    const id = searchParams.get("id");
    if (!id) return ErrorResponse("Thiếu ID khách hàng.");

    // Check purchase history
    const hasHistory = await querySingle('SELECT 1 FROM "invoices" WHERE "CustomerId" = $1 LIMIT 1', [id]);
    if (hasHistory) {
      await query('UPDATE "customers" SET "IsActive" = false, "UpdatedDate" = NOW() WHERE "Id" = $1', [id]);
      return Success(null, "Khách hàng có lịch sử giao dịch nên đã được chuyển sang trạng thái ngưng hoạt động.");
    }

    await query('DELETE FROM "customers" WHERE "Id" = $1', [id]);
    return Success(null, "Đã xóa khách hàng thành công.");
  } catch (error) {
    return ServerError(error);
  }
}
