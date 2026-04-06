import { NextRequest, NextResponse } from "next/server";
import { query, querySingle } from "@/lib/db";
import { Success, ErrorResponse, ServerError, Unauthorized } from "@/lib/api-response";
import { getServerSession } from "next-auth";

export async function GET(req: NextRequest) {
  try {
    const { searchParams } = new URL(req.url);
    const q = searchParams.get("q") || "";

    let sql = 'SELECT * FROM "categories" WHERE "IsActive" = true';
    const params: any[] = [];

    if (q) {
      sql += ` AND "Name" ILIKE $1`;
      params.push(`%${q}%`);
    }

    sql += ' ORDER BY "Name" ASC';

    const categories = await query(sql, params);
    return Success(categories);
  } catch (error) {
    return ServerError(error);
  }
}

export async function POST(request: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const body = await request.json();
    const { Name, TaxPercent, Description } = body;

    if (!Name) return ErrorResponse("Vui lòng nhập tên danh mục.");

    const res = await query(
      'INSERT INTO "categories" ("Name", "TaxPercent", "Description", "IsActive", "CreatedDate") VALUES ($1, $2, $3, true, NOW()) RETURNING "Id"',
      [Name, TaxPercent || 0, Description]
    );

    return Success({ id: res[0].Id }, "Danh mục đã được tạo thành công.", 201);
  } catch (error) {
    return ServerError(error);
  }
}

export async function PUT(request: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const body = await request.json();
    const { Id, Name, TaxPercent, Description, IsActive } = body;

    if (!Id) return ErrorResponse("Thiếu ID danh mục.");

    await query(
      `UPDATE "categories" SET "Name" = $1, "TaxPercent" = $2, "Description" = $3, "IsActive" = $4, "UpdatedDate" = NOW() WHERE "Id" = $5`,
      [Name, TaxPercent, Description, IsActive, Id]
    );

    return Success(null, "Cập nhật danh mục thành công.");
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
    if (!id) return ErrorResponse("Thiếu ID danh mục.");

    // Check if category has products
    const hasProducts = await querySingle('SELECT 1 FROM "products" WHERE "CategoryId" = $1 LIMIT 1', [id]);
    if (hasProducts) {
      // Soft delete
      await query('UPDATE "categories" SET "IsActive" = false, "UpdatedDate" = NOW() WHERE "Id" = $1', [id]);
      return Success(null, "Danh mục đang chứa sản phẩm nên đã được chuyển sang trạng thái ngưng hoạt động.");
    }

    await query('DELETE FROM "categories" WHERE "Id" = $1', [id]);
    return Success(null, "Đã xóa danh mục thành công.");
  } catch (error) {
    return ServerError(error);
  }
}
