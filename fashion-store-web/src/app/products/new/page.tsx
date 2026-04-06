import { query } from "@/lib/db";
import { ProductForm } from "../ProductForm";

export const dynamic = 'force-dynamic';

async function getCategories() {
  return await query('SELECT "Id", "Name" FROM "categories" WHERE "IsActive" = true ORDER BY "Name" ASC');
}

export default async function NewProductPage() {
  const categories = await getCategories();

  return (
    <div className="pt-10">
      <ProductForm categories={categories} />
    </div>
  );
}
