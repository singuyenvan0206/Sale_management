import { query, querySingle } from "@/lib/db";
import { ProductForm } from "../../ProductForm";
import { notFound } from "next/navigation";

export const dynamic = 'force-dynamic';

async function getProduct(id: string) {
  return await querySingle('SELECT * FROM "products" WHERE "Id" = $1', [id]);
}

async function getCategories() {
  return await query('SELECT "Id", "Name" FROM "categories" WHERE "IsActive" = true ORDER BY "Name" ASC');
}

type Params = Promise<{ id: string }>;

export default async function EditProductPage({ params }: { params: Params }) {
  const { id } = await params;
  const [product, categories] = await Promise.all([
    getProduct(id),
    getCategories()
  ]);

  if (!product) notFound();

  return (
    <div className="pt-10">
      <ProductForm initialData={product} categories={categories} isEdit={true} />
    </div>
  );
}
