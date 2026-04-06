import { query } from "@/lib/db";
import { PosClient } from "./PosClient";

export const dynamic = 'force-dynamic';

async function getPosData() {
  const [products, categories, customers, promotions] = await Promise.all([
    query(`
      SELECT p.*, c."Name" as "CategoryName" 
      FROM "products" p 
      LEFT JOIN "categories" c ON p."CategoryId" = c."Id" 
      WHERE p."IsActive" = true
    `),
    query('SELECT * FROM "categories" WHERE "IsActive" = true'),
    query('SELECT * FROM "customers" WHERE "IsActive" = true'),
    query(`
      SELECT * FROM "promotions" 
      WHERE "IsActive" = true 
      AND "StartDate" <= NOW() 
      AND "EndDate" >= NOW()
    `)
  ]);

  return {
    products: products || [],
    categories: categories || [],
    customers: customers || [],
    promotions: promotions || []
  };
}

export default async function PosPage() {
  const { products, categories, customers, promotions } = await getPosData();

  return (
    <div className="h-[calc(100vh-140px)]">
      <PosClient 
        products={products} 
        categories={categories} 
        customers={customers} 
        promotions={promotions}
      />
    </div>
  );
}
