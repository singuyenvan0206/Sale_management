import { query } from "@/lib/db";
import { PosClient } from "@/app/pos/PosClient";

async function getProducts() {
  return await query(`
    SELECT p.*, c.Name as CategoryName 
    FROM products p 
    LEFT JOIN categories c ON p.CategoryId = c.Id 
    WHERE p.IsActive = 1 AND p.StockQuantity > 0
    ORDER BY p.Name ASC
  `);
}

async function getCategories() {
  return await query("SELECT * FROM categories WHERE IsActive = 1 ORDER BY Name");
}

export default async function PosPage() {
  const [products, categories] = await Promise.all([
    getProducts(),
    getCategories()
  ]);

  return (
    <div className="h-[calc(100vh-8rem)]">
      <PosClient products={products} categories={categories} />
    </div>
  );
}
