import { NextResponse } from "next/server";
import pool from "@/lib/db";
import { getServerSession } from "next-auth";
import { Success, ErrorResponse, ServerError, Unauthorized } from "@/lib/api-response";

export async function POST(req: Request) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const { customerId, items, subtotal, tax, total, voucherId, discountAmount, paymentMethod } = await req.json();
    const employeeId = (session as any).user?.id || 1;

    if (!items || items.length === 0) return ErrorResponse("Giỏ hàng trống.");

    const client = await pool.connect();

    try {
      await client.query("BEGIN");

      // 1. Stock Guard: Check availability before any mutations
      for (const item of items) {
        const stockRes = await client.query('SELECT "Name", "StockQuantity" FROM "products" WHERE "Id" = $1 FOR UPDATE', [item.Id]);
        if (stockRes.rows.length === 0) throw new Error(`Sản phẩm [ID:${item.Id}] không tồn tại.`);
        
        const currentStock = stockRes.rows[0].StockQuantity;
        if (currentStock < item.quantity) {
          throw new Error(`Sản phẩm "${stockRes.rows[0].Name}" chỉ còn ${currentStock} cái. Vui lòng cập nhật giỏ hàng.`);
        }
      }

      // 2. Create Invoice
      const invoiceNumber = `INV-${Date.now()}`;
      const invoiceRes = await client.query(
        `INSERT INTO "invoices" ("InvoiceNumber", "CustomerId", "EmployeeId", "VoucherId", "Subtotal", "TaxAmount", "DiscountAmount", "Total", "Paid", "Status", "PaymentMethod", "CreatedDate") 
         VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, NOW()) RETURNING "Id"`,
        [invoiceNumber, customerId, employeeId, voucherId || null, subtotal, tax, discountAmount || 0, total, total, "Paid", paymentMethod || 'CASH']
      );
      const invoiceId = invoiceRes.rows[0].Id;

      // 3. Voucher Usage
      if (voucherId) {
        // Double check usage limit
        const vCheck = await client.query('SELECT "Code", "UsageLimit", "UsedCount" FROM "vouchers" WHERE "Id" = $1', [voucherId]);
        if (vCheck.rows.length > 0) {
          const { Code, UsageLimit, UsedCount } = vCheck.rows[0];
          if (UsedCount >= UsageLimit) throw new Error(`Voucher "${Code}" đã hết lượt sử dụng.`);
          
          await client.query('UPDATE "vouchers" SET "UsedCount" = "UsedCount" + 1 WHERE "Id" = $1', [voucherId]);
          await client.query('INSERT INTO "customervoucherusage" ("CustomerId", "VoucherId", "InvoiceId") VALUES ($1, $2, $3)', [customerId, voucherId, invoiceId]);
        }
      }

      // 4. Record Items, Reduce Stock, Track Movements
      for (const item of items) {
        // Record details
        await client.query(
          `INSERT INTO "invoiceitems" ("InvoiceId", "ProductId", "EmployeeId", "UnitPrice", "Quantity", "LineTotal") 
           VALUES ($1, $2, $3, $4, $5, $6)`,
          [invoiceId, item.Id, employeeId, item.SalePrice, item.quantity, Number(item.SalePrice) * item.quantity]
        );

        // Deduct Stock
        await client.query('UPDATE "products" SET "StockQuantity" = "StockQuantity" - $1, "UpdatedDate" = NOW() WHERE "Id" = $2', [item.quantity, item.Id]);

        // Audit Trail
        await client.query(
          'INSERT INTO "StockMovements" ("ProductId", "Type", "Quantity", "ReferenceId", "Note", "CreatedDate") VALUES ($1, $2, $3, $4, $5, NOW())',
          [item.Id, 'OUT', item.quantity, invoiceId, `Xuất kho POS - HD ${invoiceNumber}`]
        );
      }

      await client.query("COMMIT");
      return Success({ invoiceNumber, invoiceId }, "Giao dịch hoàn tất thành công.");

    } catch (transactionError: any) {
      await client.query("ROLLBACK");
      return ErrorResponse(transactionError.message || "Lỗi xử lý giao dịch đơn hàng.");
    } finally {
      client.release();
    }

  } catch (error) {
    return ServerError(error);
  }
}
