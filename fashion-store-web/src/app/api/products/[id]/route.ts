import { query, querySingle } from "@/lib/db";
import { Success, ErrorResponse, ServerError } from "@/lib/api-response";

type Params = Promise<{ id: string }>;

export async function GET(
  request: Request,
  { params }: { params: Params }
) {
  try {
    const { id } = await params;
    const product: any = await querySingle(
      'SELECT * FROM "products" WHERE "Id" = $1',
      [id]
    );

    if (!product) return ErrorResponse("Product not found", 404);
    return Success(product, "Product details retrieved successfully");
  } catch (error) {
    return ServerError(error);
  }
}

export async function PUT(
  request: Request,
  { params }: { params: Params }
) {
  try {
    const { id } = await params;
    const body = await request.json();
    const { Name, Code, CategoryId, SalePrice, PurchasePrice, StockQuantity, Description, IsActive } = body;

    await query(
      `UPDATE "products" 
       SET "Name" = $1, "Code" = $2, "CategoryId" = $3, "SalePrice" = $4, 
           "PurchasePrice" = $5, "StockQuantity" = $6, "Description" = $7, 
           "IsActive" = $8, "UpdatedDate" = NOW() 
       WHERE "Id" = $9`,
      [Name, Code, CategoryId, SalePrice, PurchasePrice, StockQuantity, Description, IsActive, id]
    );

    return Success(null, "Product updated successfully");
  } catch (error) {
    return ServerError(error);
  }
}

export async function DELETE(
  request: Request,
  { params }: { params: Params }
) {
  try {
    const { id } = await params;
    // Check if product has invoices
    const hasInvoices = await querySingle(
      'SELECT 1 FROM "invoiceitems" WHERE "ProductId" = $1 LIMIT 1',
      [id]
    );

    if (hasInvoices) {
      // Soft delete if has history
      await query('UPDATE "products" SET "IsActive" = false WHERE "Id" = $1', [id]);
      return Success(null, "Product deactivated (has sales history)");
    }

    await query('DELETE FROM "products" WHERE "Id" = $1', [id]);
    return Success(null, "Product deleted successfully");
  } catch (error) {
    return ServerError(error);
  }
}
