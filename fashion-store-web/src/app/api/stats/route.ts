import { NextResponse } from "next/server";
import { query } from "@/lib/db";

export async function GET() {
  try {
    // 1. Revenue Today vs Yesterday
    const today = new Date().toISOString().split('T')[0];
    const yesterday = new Date(Date.now() - 86400000).toISOString().split('T')[0];
    
    const revenueTodayQuery = await query("SELECT SUM(Total) as total FROM invoices WHERE DATE(CreatedDate) = ?", [today]);
    const revenueYesterdayQuery = await query("SELECT SUM(Total) as total FROM invoices WHERE DATE(CreatedDate) = ?", [yesterday]);

    // 2. Top 5 Selling Products
    const topProducts = await query(`
      SELECT p.Name, p.Code, p.SalePrice, SUM(ii.Quantity) as total_qty, SUM(ii.TotalPrice) as total_revenue
      FROM invoiceitems ii
      JOIN products p ON ii.ProductId = p.Id
      GROUP BY p.Id
      ORDER BY total_revenue DESC
      LIMIT 5
    `);

    // 3. Low Stock Items
    const lowStock = await query("SELECT Name, Code, StockQuantity, 10 as MinStockLevel FROM products WHERE StockQuantity <= 10 ORDER BY StockQuantity ASC LIMIT 4");

    // 4. Sales over last 7 days (Aggregate)
    const last7Days = await query(`
      SELECT DATE(CreatedDate) as date, SUM(Total) as revenue, COUNT(*) as count 
      FROM invoices 
      WHERE CreatedDate >= DATE_SUB(CURDATE(), INTERVAL 7 DAY) 
      GROUP BY DATE(CreatedDate) 
      ORDER BY date ASC
    `);

    return NextResponse.json({
      revenueToday: Number((revenueTodayQuery as any)[0]?.total || 0),
      revenueYesterday: Number((revenueYesterdayQuery as any)[0]?.total || 0),
      topProducts,
      lowStock,
      last7Days
    });
  } catch (error) {
    return NextResponse.json({ error: "Failed to fetch stats" }, { status: 500 });
  }
}
