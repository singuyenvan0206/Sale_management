import { NextRequest, NextResponse } from "next/server";
import { query, querySingle } from "@/lib/db";
import { Success, ErrorResponse, ServerError, Unauthorized } from "@/lib/api-response";
import { getServerSession } from "next-auth";

export async function GET(req: NextRequest) {
  try {
    const { searchParams } = new URL(req.url);
    const q = searchParams.get("q") || "";

    let sql = 'SELECT * FROM "suppliers" WHERE "IsActive" = true';
    const params: any[] = [];

    if (q) {
      sql += ` AND ("Name" ILIKE $1 OR "Phone" ILIKE $2)`;
      params.push(`%${q}%`, `%${q}%`);
    }

    sql += ' ORDER BY "Name" ASC';

    const suppliers = await query(sql, params);
    return Success(suppliers);
  } catch (error) {
    return ServerError(error);
  }
}

export async function POST(request: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const body = await request.json();
    const { Name, Phone, Email, Address, Description } = body;

    if (!Name) return ErrorResponse("Vui lòng nhập tên nhà cung cấp.");

    const res = await query(
      'INSERT INTO "suppliers" ("Name", "Phone", "Email", "Address", "Description", "IsActive", "CreatedDate") VALUES ($1, $2, $3, $4, $5, true, NOW()) RETURNING "Id"',
      [Name, Phone, Email, Address, Description || ""]
    );

    return Success({ id: res[0].Id }, "Nhà cung cấp đã được thêm thành công.", 201);
  } catch (error) {
    return ServerError(error);
  }
}

export async function PUT(request: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const body = await request.json();
    const { Id, Name, Phone, Email, Address, Description, IsActive } = body;

    if (!Id) return ErrorResponse("Thiếu ID nhà cung cấp.");

    await query(
      `UPDATE "suppliers" 
       SET "Name" = $1, "Phone" = $2, "Email" = $3, "Address" = $4, 
           "Description" = $5, "IsActive" = $6, "UpdatedDate" = NOW() 
       WHERE "Id" = $7`,
      [Name, Phone, Email, Address, Description, IsActive, Id]
    );

    return Success(null, "Cập nhật thông tin nhà cung cấp thành công.");
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
    if (!id) return ErrorResponse("Thiếu ID nhà cung cấp.");

    // Check if supplier has any product imports
    const hasHistory = await querySingle('SELECT 1 FROM "StockMovements" WHERE "Note" ILIKE $1 LIMIT 1', [`%[ID:${id}]%`]);
    if (hasHistory) {
      await query('UPDATE "suppliers" SET "IsActive" = false, "UpdatedDate" = NOW() WHERE "Id" = $1', [id]);
      return Success(null, "Nhà cung cấp đã có lịch sử nhập hàng nên đã được chuyển sang trạng thái ngưng hoạt động.");
    }

    await query('DELETE FROM "suppliers" WHERE "Id" = $1', [id]);
    return Success(null, "Đã xóa nhà cung cấp thành công.");
  } catch (error) {
    return ServerError(error);
  }
}
