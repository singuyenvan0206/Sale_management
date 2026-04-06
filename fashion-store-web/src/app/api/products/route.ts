import { NextRequest, NextResponse } from "next/server";
import { query, querySingle } from "@/lib/db";
import { Success, ErrorResponse, ServerError } from "@/lib/api-response";

export async function GET(req: NextRequest) {
  try {
    const { searchParams } = new URL(req.url);
    const q = searchParams.get("q") || "";
    const categoryId = searchParams.get("categoryId");
    const sortBy = searchParams.get("sortBy") || "CreatedDate";
    const order = searchParams.get("order")?.toUpperCase() === "ASC" ? "ASC" : "DESC";

    let sql = `
      SELECT p.*, c."Name" as "CategoryName" 
      FROM "products" p 
      LEFT JOIN "categories" c ON p."CategoryId" = c."Id" 
      WHERE p."IsActive" = true
    `;
    const params: any[] = [];

    if (q) {
      sql += ` AND (p."Name" ILIKE $${params.length + 1} OR p."Code" ILIKE $${params.length + 2})`;
      params.push(`%${q}%`, `%${q}%`);
    }

    if (categoryId && categoryId !== "0") {
      sql += ` AND p."CategoryId" = $${params.length + 1}`;
      params.push(categoryId);
    }

    // Sanitize sortBy columns (Allow-list)
    const allowedSortColumns = ["Name", "Code", "SalePrice", "StockQuantity", "CreatedDate"];
    const verifiedSort = allowedSortColumns.includes(sortBy) ? sortBy : "CreatedDate";
    
    sql += ` ORDER BY p."${verifiedSort}" ${order}`;

    const products = await query(sql, params);
    return Success(products);
  } catch (error) {
    return ServerError(error);
  }
}

export async function POST(request: Request) {
  try {
    const body = await request.json();
    const { Name, Code, CategoryId, SalePrice, PurchasePrice, StockQuantity, Description } = body;

    if (!Name || !Code || !CategoryId) return ErrorResponse("Vui lòng điền đủ Tên, Mã và Danh mục.");

    // Check duplicate code
    const existing = await querySingle('SELECT "Id" FROM "products" WHERE "Code" = $1', [Code]);
    if (existing) return ErrorResponse(`Mã sản phẩm "${Code}" đã tồn tại trên hệ thống.`);

    const res = await query(
      `INSERT INTO "products" ("Name", "Code", "CategoryId", "SalePrice", "PurchasePrice", "StockQuantity", "Description", "IsActive", "CreatedDate") 
       VALUES ($1, $2, $3, $4, $5, $6, $7, true, NOW()) RETURNING "Id"`,
      [Name, Code, CategoryId, SalePrice, PurchasePrice, StockQuantity, Description]
    );

    return Success({ id: res[0].Id }, "Sản phẩm đã được tạo thành công.", 201);
  } catch (error) {
    return ServerError(error);
  }
}

export async function PUT(request: Request) {
  try {
    const body = await request.json();
    const { Id, Name, Code, CategoryId, SalePrice, PurchasePrice, StockQuantity, Description, IsActive } = body;

    if (!Id) return ErrorResponse("Thiếu định danh sản phẩm (ID).");

    await query(
      `UPDATE "products" 
       SET "Name" = $1, "Code" = $2, "CategoryId" = $3, 
           "SalePrice" = $4, "PurchasePrice" = $5, "StockQuantity" = $6, 
           "Description" = $7, "IsActive" = $8, "UpdatedDate" = NOW() 
       WHERE "Id" = $9`,
      [Name, Code, CategoryId, SalePrice, PurchasePrice, StockQuantity, Description, IsActive, Id]
    );

    return Success(null, "Cập nhật sản phẩm thành công.");
  } catch (error) {
    return ServerError(error);
  }
}

export async function DELETE(request: Request) {
  try {
    const { searchParams } = new URL(request.url);
    const id = searchParams.get("id");
    if (!id) return ErrorResponse("Thiếu ID sản phẩm.");

    // Check if product has transaction history
    const hasHistory = await querySingle('SELECT 1 FROM "invoiceitems" WHERE "ProductId" = $1 LIMIT 1', [id]);
    if (hasHistory) {
      // Soft delete if history exists
      await query('UPDATE "products" SET "IsActive" = false, "UpdatedDate" = NOW() WHERE "Id" = $1', [id]);
      return Success(null, "Sản phẩm có lịch sử giao dịch nên đã được chuyển sang trạng thái ngưng hoạt động.");
    }

    await query('DELETE FROM "products" WHERE "Id" = $1', [id]);
    return Success(null, "Đã xóa sản phẩm vĩnh viễn.");
  } catch (error) {
    return ServerError(error);
  }
}
