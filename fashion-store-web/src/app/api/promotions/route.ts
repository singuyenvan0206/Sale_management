import { NextRequest, NextResponse } from "next/server";
import { query, querySingle } from "@/lib/db";
import { Success, ErrorResponse, ServerError, Unauthorized } from "@/lib/api-response";
import { getServerSession } from "next-auth";

export async function GET(req: NextRequest) {
  try {
    const { searchParams } = new URL(req.url);
    const q = searchParams.get("q") || "";
    const activeOnly = searchParams.get("active") === "true";

    let sql = 'SELECT * FROM "promotions" WHERE 1=1';
    const params: any[] = [];

    if (q) {
      sql += ` AND "Name" ILIKE $${params.length + 1}`;
      params.push(`%${q}%`);
    }

    if (activeOnly) {
      sql += ' AND "IsActive" = true AND "StartDate" <= NOW() AND "EndDate" >= NOW()';
    }

    sql += ' ORDER BY "CreatedDate" DESC';

    const promos = await query(sql, params);
    return Success(promos);
  } catch (error) {
    return ServerError(error);
  }
}

export async function POST(request: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const body = await request.json();
    const { Name, Type, DiscountPercent, DiscountValue, StartDate, EndDate, TargetCategoryId, RequiredProductId } = body;

    if (!Name) return ErrorResponse("Vui lòng điền tên chiến dịch.");

    const res = await query(
      `INSERT INTO "promotions" ("Name", "Type", "DiscountPercent", "DiscountValue", "StartDate", "EndDate", "TargetCategoryId", "RequiredProductId", "IsActive", "CreatedDate") 
       VALUES ($1, $2, $3, $4, $5, $6, $7, $8, true, NOW()) RETURNING "Id"`,
      [Name, Type || 'FlashSale', DiscountPercent || 0, DiscountValue || 0, StartDate, EndDate, TargetCategoryId || null, RequiredProductId || null]
    );

    return Success({ id: res[0].Id }, "Tạo chiến dịch khuyến mãi thành công.", 201);
  } catch (error) {
    return ServerError(error);
  }
}

export async function PUT(request: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const body = await request.json();
    const { Id, Name, Type, DiscountPercent, DiscountValue, StartDate, EndDate, TargetCategoryId, RequiredProductId, IsActive } = body;

    if (!Id) return ErrorResponse("Thiếu ID chiến dịch.");

    await query(
      `UPDATE "promotions" 
       SET "Name" = $1, "Type" = $2, "DiscountPercent" = $3, 
           "DiscountValue" = $4, "StartDate" = $5, "EndDate" = $6, 
           "TargetCategoryId" = $7, "RequiredProductId" = $8, 
           "IsActive" = $9, "UpdatedDate" = NOW() 
       WHERE "Id" = $10`,
      [Name, Type, DiscountPercent, DiscountValue, StartDate, EndDate, TargetCategoryId, RequiredProductId, IsActive, Id]
    );

    return Success(null, "Cập nhật chiến dịch khuyến mãi thành công.");
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
    if (!id) return ErrorResponse("Thiếu ID chiến dịch.");

    // Simple deletion for promotions since they are temporary
    await query('DELETE FROM "promotions" WHERE "Id" = $1', [id]);
    return Success(null, "Đã xóa chiến dịch thành công.");
  } catch (error) {
    return ServerError(error);
  }
}
