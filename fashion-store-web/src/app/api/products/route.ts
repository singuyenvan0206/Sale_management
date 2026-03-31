import { NextResponse } from "next/server";
import { query } from "@/lib/db";

export async function GET() {
  try {
    const products = await query(`
      SELECT 
        p.Id, p.Name, p.Code, p.CategoryId, p.SalePrice, 
        p.PurchasePrice, p.StockQuantity, p.PurchaseUnit,
        p.PromoDiscountPercent, p.IsActive,
        c.Name as CategoryName,
        s.Name as SupplierName
      FROM products p 
      LEFT JOIN categories c ON p.CategoryId = c.Id 
      LEFT JOIN suppliers s ON p.SupplierId = s.Id
      WHERE p.IsActive = 1
      ORDER BY p.UpdatedDate DESC
    `);
    
    return NextResponse.json(products);
  } catch (error) {
    return NextResponse.json({ error: "Failed to fetch products" }, { status: 500 });
  }
}
