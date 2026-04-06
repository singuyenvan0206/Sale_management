import { NextResponse } from "next/server";
import pool from "@/lib/db";
import { Success, ErrorResponse, ServerError, Unauthorized } from "@/lib/api-response";
import { getServerSession } from "next-auth";

export async function POST(request: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  const body = await request.json();
  const { SupplierId, Items, Notes } = body;

  if (!Items || Items.length === 0) {
    return ErrorResponse("Vui lòng chọn ít nhất một sản phẩm để nhập kho.");
  }

  const client = await pool.connect();

  try {
    await client.query("BEGIN");

    for (const item of Items) {
      const { ProductId, Quantity, PurchasePrice } = item;
      
      if (!ProductId || !Quantity) throw new Error("Dữ liệu sản phẩm không hợp lệ.");

      // 1. Log Movement (Audit Trail)
      await client.query(
        'INSERT INTO "StockMovements" ("ProductId", "Type", "Quantity", "Note", "CreatedDate") VALUES ($1, $2, $3, $4, NOW())',
        [ProductId, 'IN', Quantity, Notes || `Nhập hàng từ NCC [ID:${SupplierId}]`]
      );

      // 2. Update Product Inventory & COGS
      await client.query(
        `UPDATE "products" 
         SET "StockQuantity" = "StockQuantity" + $1, 
             "PurchasePrice" = $2, 
             "UpdatedDate" = NOW() 
         WHERE "Id" = $3`,
        [Quantity, PurchasePrice, ProductId]
      );
    }

    await client.query("COMMIT");
    return Success(null, "Nhập kho thành công. Số dư tồn kho đã được cập nhật.");

  } catch (error: any) {
    await client.query("ROLLBACK");
    return ErrorResponse(error.message || "Lỗi hệ thống khi xử lý nhập kho.");
  } finally {
    client.release();
  }
}
