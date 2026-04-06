import { NextRequest, NextResponse } from "next/server";
import { query, querySingle } from "@/lib/db";
import { Success, ErrorResponse, ServerError, Unauthorized } from "@/lib/api-response";
import { getServerSession } from "next-auth";

export async function GET(req: NextRequest) {
  try {
    const { searchParams } = new URL(req.url);
    const q = searchParams.get("q") || "";
    const activeOnly = searchParams.get("active") === "true";

    let sql = 'SELECT * FROM "vouchers" WHERE 1=1';
    const params: any[] = [];

    if (q) {
      sql += ` AND "Code" ILIKE $${params.length + 1}`;
      params.push(`%${q}%`);
    }

    if (activeOnly) {
      sql += ' AND "IsActive" = true AND "EndDate" >= NOW() AND "UsedCount" < "UsageLimit"';
    }

    sql += ' ORDER BY "CreatedDate" DESC';

    const vouchers = await query(sql, params);
    return Success(vouchers);
  } catch (error) {
    return ServerError(error);
  }
}

export async function POST(request: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const body = await request.json();
    const { Code, DiscountType, DiscountValue, MinInvoiceAmount, StartDate, EndDate, UsageLimit } = body;

    if (!Code || !DiscountValue) return ErrorResponse("Vui lòng nhập Mã và Giá trị giảm.");

    // Check duplicate
    const existing = await querySingle('SELECT "Id" FROM "vouchers" WHERE "Code" = $1', [Code]);
    if (existing) return ErrorResponse(`Mã voucher "${Code}" đã tồn tại.`);

    await query(
      `INSERT INTO "vouchers" ("Code", "DiscountType", "DiscountValue", "MinInvoiceAmount", "StartDate", "EndDate", "UsageLimit", "UsedCount", "IsActive", "CreatedDate") 
       VALUES ($1, $2, $3, $4, $5, $6, $7, 0, true, NOW())`,
      [Code, DiscountType, DiscountValue, MinInvoiceAmount, StartDate, EndDate, UsageLimit]
    );

    return Success(null, "Tạo voucher mới thành công.", 201);
  } catch (error) {
    return ServerError(error);
  }
}

export async function PUT(request: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const body = await request.json();
    const { Id, Code, DiscountType, DiscountValue, MinInvoiceAmount, StartDate, EndDate, UsageLimit, IsActive } = body;

    if (!Id) return ErrorResponse("Thiếu ID voucher.");

    await query(
      `UPDATE "vouchers" 
       SET "Code" = $1, "DiscountType" = $2, "DiscountValue" = $3, 
           "MinInvoiceAmount" = $4, "StartDate" = $5, "EndDate" = $6, 
           "UsageLimit" = $7, "IsActive" = $8, "UpdatedDate" = NOW() 
       WHERE "Id" = $9`,
      [Code, DiscountType, DiscountValue, MinInvoiceAmount, StartDate, EndDate, UsageLimit, IsActive, Id]
    );

    return Success(null, "Cập nhật voucher thành công.");
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
    if (!id) return ErrorResponse("Thiếu ID voucher.");

    // Check if voucher used
    const hasUsage = await querySingle('SELECT 1 FROM "customervoucherusage" WHERE "VoucherId" = $1 LIMIT 1', [id]);
    if (hasUsage) {
      // Soft delete
      await query('UPDATE "vouchers" SET "IsActive" = false, "UpdatedDate" = NOW() WHERE "Id" = $1', [id]);
      return Success(null, "Voucher đã từng được sử dụng nên đã được ngưng hoạt động.");
    }
    
    await query('DELETE FROM "vouchers" WHERE "Id" = $1', [id]);
    return Success(null, "Đã xóa voucher vĩnh viễn.");
  } catch (error) {
    return ServerError(error);
  }
}
