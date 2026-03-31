import { NextResponse } from "next/server";
import { querySingle } from "@/lib/db";

export async function GET(request: Request) {
  const { searchParams } = new URL(request.url);
  const code = searchParams.get("code");

  if (!code) {
    return NextResponse.json({ error: "Code is required" }, { status: 400 });
  }

  try {
    const product = await querySingle(
      "SELECT p.*, c.Name as CategoryName FROM products p LEFT JOIN categories c ON p.CategoryId = c.Id WHERE p.Code = ? AND p.IsActive = 1",
      [code]
    );

    if (!product) {
      return NextResponse.json({ error: "Product not found" }, { status: 404 });
    }

    return NextResponse.json(product);
  } catch (error) {
    return NextResponse.json({ error: "Database error" }, { status: 500 });
  }
}
