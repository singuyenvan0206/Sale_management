import { NextResponse } from "next/server";
import { query } from "@/lib/db";
import { Success, ServerError, Unauthorized } from "@/lib/api-response";
import { getServerSession } from "next-auth";

export async function GET() {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const today = new Date().toISOString().split('T')[0];
    const yesterday = new Date(Date.now() - 86400000).toISOString().split('T')[0];

    // 1. Parallel KPI Aggragation
    const [revenueToday, revenueYesterday, ordersToday, ordersYesterday, topProducts, lowStock, last7Days] = await Promise.all([
      query('SELECT COALESCE(SUM("Total"), 0) as "TotalSum" FROM "invoices" WHERE "CreatedDate"::date = $1', [today]),
      query('SELECT COALESCE(SUM("Total"), 0) as "TotalSum" FROM "invoices" WHERE "CreatedDate"::date = $1', [yesterday]),
      query('SELECT COUNT(*) as "Count" FROM "invoices" WHERE "CreatedDate"::date = $1', [today]),
      query('SELECT COUNT(*) as "Count" FROM "invoices" WHERE "CreatedDate"::date = $1', [yesterday]),
      query(`
        SELECT p."Name", p."Code", p."SalePrice", SUM(ii."Quantity") as "TotalQty", SUM(ii."LineTotal") as "TotalRevenue"
        FROM "invoiceitems" ii
        JOIN "products" p ON ii."ProductId" = p."Id"
        GROUP BY p."Id", p."Name", p."Code", p."SalePrice"
        ORDER BY "TotalRevenue" DESC
        LIMIT 5
      `),
      query('SELECT "Name", "Code", "StockQuantity" FROM "products" WHERE "StockQuantity" <= 10 ORDER BY "StockQuantity" ASC LIMIT 4'),
      query(`
        SELECT "CreatedDate"::date as "Date", SUM("Total") as "Revenue", COUNT(*) as "Count" 
        FROM "invoices" 
        WHERE "CreatedDate" >= CURRENT_DATE - INTERVAL '7 days' 
        GROUP BY "CreatedDate"::date 
        ORDER BY "Date" ASC
      `)
    ]);

    // 2. Performance Index Calculation
    const revenueSumToday = Number(revenueToday[0]?.TotalSum || 0);
    const revenueSumYesterday = Number(revenueYesterday[0]?.TotalSum || 0);
    const avgRevenue = last7Days.length > 0 
      ? last7Days.reduce((a: any, b: any) => a + Number(b.Revenue || 0), 0) / last7Days.length 
      : 1;
    
    // Performance vs 7-day average
    const performance = Math.min(100, Math.round((revenueSumToday / (avgRevenue || 1)) * 100));

    const statsData = {
      revenueToday: revenueSumToday,
      revenueYesterday: revenueSumYesterday,
      ordersToday: Number(ordersToday[0]?.Count || 0),
      ordersYesterday: Number(ordersYesterday[0]?.Count || 0),
      performance: performance || 0,
      topProducts,
      lowStock,
      last7Days
    };

    return Success(statsData, "Dữ liệu phân tích Dashboard đã được cập nhật.");
  } catch (error) {
    return ServerError(error);
  }
}
