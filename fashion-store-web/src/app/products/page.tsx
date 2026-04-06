import { query } from "@/lib/db";
export const dynamic = 'force-dynamic';
import { formatCurrency, cn } from "@/lib/utils";
import { 
  Plus, 
  Package,
  CheckCircle2,
  XCircle,
  Layers,
  FileText,
  TrendingUp,
  History
} from "lucide-react";
import Link from "next/link";
import { ProductActions } from "./ProductActions";
import { ProductFilterSection } from "./ProductFilterSection";

interface Product {
  Id: number;
  Name: string;
  Code: string;
  CategoryName: string;
  SalePrice: number;
  StockQuantity: number;
  IsActive: boolean;
}

async function getProducts(search?: string, category?: string, status?: string, sort?: string, order?: string) {
  let sql = `
    SELECT p.*, c."Name" as "CategoryName" 
    FROM "products" p 
    LEFT JOIN "categories" c ON p."CategoryId" = c."Id"
    WHERE 1=1
  `;
  const params: any[] = [];

  if (search) {
    sql += ` AND (p."Name" ILIKE $${params.length + 1} OR p."Code" ILIKE $${params.length + 1})`;
    params.push(`%${search}%`);
  }

  if (category && category !== "all") {
    sql += ` AND p."CategoryId" = $${params.length + 1}`;
    params.push(category);
  }

  if (status && status !== "all") {
    sql += ` AND p."IsActive" = $${params.length + 1}`;
    params.push(status === "active");
  }

  const allowedSortCols: Record<string, string> = {
    name: 'p."Name"',
    price: 'p."SalePrice"',
    stock: 'p."StockQuantity"',
    date: 'p."CreatedDate"'
  };
  const sortCol = allowedSortCols[sort || ''] || 'p."CreatedDate"';
  const sortOrder = order?.toUpperCase() === 'ASC' ? 'ASC' : 'DESC';

  sql += ` ORDER BY ${sortCol} ${sortOrder}`;
  
  return await query<Product>(sql, params);
}

async function getCategories() {
  return await query('SELECT "Id", "Name" FROM "categories" WHERE "IsActive" = true ORDER BY "Name" ASC');
}

export default async function ProductsPage({
  searchParams,
}: {
  searchParams: { q?: string; category?: string; status?: string; sort?: string; order?: string };
}) {
  const q = searchParams.q || "";
  const category = searchParams.category || "all";
  const status = searchParams.status || "all";
  const sort = searchParams.sort || "date";
  const order = searchParams.order || "desc";

  const [products, categories] = await Promise.all([
    getProducts(q, category, status, sort, order),
    getCategories()
  ]);

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-20 no-select">
      {/* WPF Top Header / Ribbon */}
      <div className="wpf-panel">
         <div className="wpf-panel-header uppercase">HỆ THỐNG QUẢN LÝ DANH MỤC SẢN PHẨM & KHO HÀNG (INVENTORY)</div>
         <div className="p-6 bg-white flex flex-col md:flex-row md:items-center justify-between gap-6">
            <div className="flex items-center gap-4">
               <div className="w-10 h-10 bg-[#0078D4] rounded-sm flex items-center justify-center text-white">
                  <Package className="w-6 h-6" />
               </div>
               <div>
                  <h2 className="text-[20px] font-black text-slate-900 tracking-tight uppercase italic leading-none">SẢN PHẨM & TỒN KHO</h2>
                  <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-widest italic">Fusion ERP Inventory Subsystem v2.5</p>
               </div>
            </div>
            <div className="flex items-center gap-3">
               <Link href="/products/new" className="btn-wpf btn-wpf-primary h-12 px-8 flex items-center gap-3 uppercase font-black text-[12px] shadow-lg border-b-4 border-[#005A9E] active:translate-y-1 active:border-b-0 transition-all">
                  <Plus className="w-5 h-5" /> THÊM SẢN PHẨM MỚI (F2)
               </Link>
            </div>
         </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 no-print">
         <div className="bg-white border border-[#D1D1D1] p-4 flex items-center gap-4 shadow-sm">
            <div className="w-10 h-10 bg-[#F0F0F0] text-[#0078D4] rounded-sm flex items-center justify-center">
               <Layers className="w-6 h-6" />
            </div>
            <div>
               <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Tổng sản phẩm</p>
               <h4 className="text-[18px] font-black text-slate-800 tabular-nums leading-none">{products.length} SKU(s)</h4>
            </div>
         </div>
         <div className="bg-white border border-[#D1D1D1] p-4 flex items-center gap-4 shadow-sm">
            <div className="w-10 h-10 bg-[#F0F0F0] text-emerald-600 rounded-sm flex items-center justify-center">
               <TrendingUp className="w-6 h-6" />
            </div>
            <div>
               <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Đang kinh doanh</p>
               <h4 className="text-[18px] font-black text-slate-800 tabular-nums leading-none">{products.filter(p => p.IsActive).length} SKUs Active</h4>
            </div>
         </div>
         <div className="bg-white border border-[#D1D1D1] p-4 flex items-center gap-4 shadow-sm italic text-[11px] text-slate-400 font-medium">
            <Info className="w-4 h-4 text-amber-500 shrink-0" />
            Dữ liệu tồn kho được đồng bộ hóa thời gian thực (Real-time) từ hệ thống POS và Kho tổng.
         </div>
      </div>

      {/* Main Content: Filter + DataGrid */}
      <div className="wpf-panel shadow-md overflow-hidden">
        <div className="p-4 bg-[#F0F0F0] border-b border-[#D1D1D1]">
           <ProductFilterSection categories={categories} />
        </div>

        <div className="overflow-x-auto relative z-10">
          <table className="wpf-datagrid">
            <thead>
              <tr>
                <th className="w-[120px]">MÃ SKU</th>
                <th>TÊN SẢN PHẨM (PRODUCT NAME)</th>
                <th className="w-[200px]">PHÂN LOẠI (CATEGORY)</th>
                <th className="text-right w-[150px]">GIÁ NIÊM YẾT</th>
                <th className="text-center w-[120px]">TỒN KHO</th>
                <th className="text-center w-[120px]">TRẠNG THÁI</th>
                <th className="text-right w-[120px]">THAO TÁC</th>
              </tr>
            </thead>
            <tbody>
              {products.map((product) => (
                <tr key={product.Id}>
                  <td className="font-bold text-[#0078D4] tabular-nums tracking-tighter uppercase">{product.Code}</td>
                  <td className="font-black text-slate-800 uppercase italic tracking-tight">{product.Name}</td>
                  <td className="uppercase font-bold text-slate-400 text-[11px]">
                     <span className="bg-slate-100 px-2 py-0.5 rounded-sm border border-slate-200">{product.CategoryName || "N/A"}</span>
                  </td>
                  <td className="text-right font-black text-slate-900 tabular-nums">
                    {formatCurrency(Number(product.SalePrice))}
                  </td>
                  <td className="text-center">
                    <div className="flex flex-col items-center gap-1">
                       <span className={cn(
                         "text-[13px] font-black tabular-nums italic",
                         Number(product.StockQuantity) > 10 ? "text-emerald-600" : 
                         Number(product.StockQuantity) > 0 ? "text-amber-500" : 
                         "text-rose-500"
                       )}>
                         {product.StockQuantity} SP
                       </span>
                       <div className="w-16 h-1 bg-slate-100 border border-slate-200 rounded-full overflow-hidden">
                          <div 
                            className={cn(
                               "h-full transition-all duration-1000",
                               Number(product.StockQuantity) > 10 ? "bg-emerald-500" : 
                               Number(product.StockQuantity) > 0 ? "bg-amber-500" : 
                               "bg-rose-500"
                            )} 
                            style={{ width: `${Math.min(100, (Number(product.StockQuantity) / 100) * 100)}%` }} 
                          />
                       </div>
                    </div>
                  </td>
                  <td className="text-center">
                    <div className="inline-flex items-center gap-2 px-3 py-1 bg-[#F9F9F9] border border-[#D1D1D1] rounded-sm">
                       {product.IsActive ? (
                         <>
                            <div className="w-2 h-2 rounded-full bg-emerald-500" />
                            <span className="text-[9px] font-black uppercase tracking-widest text-emerald-700">ACTIVE</span>
                         </>
                       ) : (
                         <>
                            <div className="w-2 h-2 rounded-full bg-slate-300" />
                            <span className="text-[9px] font-black uppercase tracking-widest text-slate-400">IDLE</span>
                         </>
                       )}
                    </div>
                  </td>
                  <td className="text-right">
                    <ProductActions id={product.Id} name={product.Name} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        
        {products.length === 0 && (
          <div className="py-40 text-center bg-white">
            <Package className="w-16 h-16 text-slate-100 mx-auto mb-6" />
            <h4 className="text-[18px] font-black text-slate-300 uppercase tracking-widest italic">Hệ thống chưa ghi nhận sản phẩm phù hợp</h4>
            <p className="text-slate-300 font-bold uppercase tracking-widest text-[9px] mt-2 italic underline underline-offset-4">Vui lòng kiểm tra lại bộ lọc tìm kiếm hoặc thêm mã sản phẩm mới.</p>
          </div>
        )}

        {/* Footer Info Strip */}
        <div className="bg-[#F0F0F0] px-6 py-2.5 border-t border-[#D1D1D1] flex justify-between items-center text-[11px] font-bold text-slate-400">
           <div className="flex items-center gap-6">
              <span>TỔNG SKUs: {products.length}</span>
              <span className="border-l border-slate-300 pl-6 uppercase italic">Product Registry Shell v2.5.0</span>
           </div>
           <div className="flex items-center gap-2 italic uppercase">
              <History className="w-3.5 h-3.5" /> SYNC VERIFIED: {new Date().toLocaleTimeString()}
           </div>
        </div>
      </div>
    </div>
  );
}

// Internal icon for help
import { Info } from "lucide-react";
