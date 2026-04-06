import { NextRequest, NextResponse } from "next/server";
import { query, querySingle } from "@/lib/db";
import { Success, ErrorResponse, ServerError, Unauthorized } from "@/lib/api-response";
import { getServerSession } from "next-auth";

export async function GET() {
  try {
    const settings = await query('SELECT "Key", "Value" FROM "settings"');
    
    // Transform array to key-value map for easier client-side consumption
    const settingsObj = settings.reduce((acc: any, curr: any) => {
      if (curr.Key) acc[curr.Key] = curr.Value;
      return acc;
    }, {});
    
    return Success(settingsObj, "Cấu hình hệ thống đã được đồng bộ.");
  } catch (error) {
    return ServerError(error);
  }
}

export async function POST(request: Request) {
  const session = await getServerSession();
  if (!session || (session as any).user?.role !== 'Admin') return Unauthorized();

  try {
    const body = await request.json();
    
    // Batch upsert each setting key-value pair
    for (const [key, value] of Object.entries(body)) {
      await query(
        `INSERT INTO "settings" ("Key", "Value", "UpdatedAt") 
         VALUES ($1, $2, NOW()) 
         ON CONFLICT ("Key") 
         DO UPDATE SET "Value" = EXCLUDED."Value", "UpdatedAt" = NOW()`,
        [key, value]
      );
    }
    
    return Success(null, "Các thay đổi cấu hình đã được ghi nhận thành công.");
  } catch (error) {
    return ServerError(error);
  }
}
