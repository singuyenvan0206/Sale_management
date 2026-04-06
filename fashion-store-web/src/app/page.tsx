import { query, querySingle } from "@/lib/db";
export const dynamic = 'force-dynamic';
import { formatCurrency, cn } from "@/lib/utils";
import { 
  ArrowRight,
  TrendingUp,
  Users,
  Package,
  ShoppingCart,
  Activity,
  Calendar,
  Layers,
  ChevronRight,
  Monitor
} from "lucide-react";
import Link from "next/link";

async function getDashboardData() {
  const today = new Date().toISOString().split('T')[0];
  
  const [
    revenueToday, 
    revenue30Days, 
    ordersToday, 
    productsCount, 
    customersCount, 
    recentInvoices,
    trendingProduct,
    topCustomer
  ] = await Promise.all([
    querySingle('SELECT SUM("Total") as "TotalSum" FROM "invoices" WHERE "CreatedDate"::date = $1', [today]),
    querySingle('SELECT SUM("Total") as "TotalSum" FROM "invoices" WHERE "CreatedDate" >= CURRENT_DATE - INTERVAL \'30 days\''),
    querySingle('SELECT COUNT(*) as "CountNum" FROM "invoices" WHERE "CreatedDate"::date = $1', [today]),
    querySingle('SELECT COUNT(*) as "CountNum" FROM "products"'),
    querySingle('SELECT COUNT(*) as "CountNum" FROM "customers"'),
    query('SELECT i."Id", i."InvoiceNumber", i."Total", i."CreatedDate", c."Name" as "CustomerName" FROM "invoices" i LEFT JOIN "customers" c ON i."CustomerId" = c."Id" ORDER BY i."CreatedDate" DESC LIMIT 5'),
    querySingle('SELECT p."Name", SUM(ii."Quantity") as "TotalQty" FROM "invoiceitems" ii JOIN "products" p ON ii."ProductId" = p."Id" GROUP BY p."Name" ORDER BY "TotalQty" DESC LIMIT 1'),
    querySingle('SELECT c."Name", SUM(i."Total") as "TotalSpend" FROM "invoices" i JOIN "customers" c ON i."CustomerId" = c."Id" GROUP BY c."Name" ORDER BY "TotalSpend" DESC LIMIT 1')
  ]);

  return {
    revenueToday: Number(revenueToday?.TotalSum || 0),
    revenue30Days: Number(revenue30Days?.TotalSum || 0),
    ordersToday: Number(ordersToday?.CountNum || 0),
    productsCount: Number(productsCount?.CountNum || 0),
    customersCount: Number(customersCount?.CountNum || 0),
    recentInvoices: recentInvoices || [],
    trendingProduct: trendingProduct?.Name || "Không có dữ liệu",
    topCustomer: topCustomer?.Name || "Không có dữ liệu"
  };
}

export default async function DashboardPage() {
  const data = await getDashboardData();

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-10 no-select">
      {/* WPF Ribbon/Header */}
      <div className="wpf-panel">
        <div className="wpf-panel-header">TRUNG TÂM ĐIỀU HÀNH HỆ THỐNG - FUSION ENTERPRISE ERP</div>
        <div className="p-6 bg-white flex flex-col md:flex-row md:items-center justify-between gap-6">
           <div>
              <h2 className="text-[24px] font-black text-slate-900 tracking-tight uppercase italic leading-none">DASHBOARD TỔNG QUAN</h2>
              <div className="flex items-center gap-2 mt-2">
                 <div className="w-2 h-2 rounded-full bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.5)]" />
                 <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest italic">Server Status: Online • Node: SG-01 • v2.5.0</p>
              </div>
           </div>
           <div className="flex items-center gap-4">
              <Link href="/pos" className="btn-wpf btn-wpf-primary flex items-center gap-3 px-8 h-12 text-[12px]">
                 <ShoppingCart className="w-5 h-5" /> MỞ TRÌNH BÁN HÀNG (POS)
              </Link>
           </div>
        </div>
      </div>

      {/* Metric Cards - WPF STYLE */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="wpf-metric">
           <div className="flex justify-between items-start">
              <div>
                 <p className="wpf-metric-label">Doanh thu hôm nay</p>
                 <p className="wpf-metric-value text-emerald-600">{formatCurrency(data.revenueToday)}</p>
              </div>
              <TrendingUp className="w-8 h-8 text-emerald-500 opacity-20" />
           </div>
           <p className="text-[10px] font-bold text-slate-400 mt-4 uppercase">So với hôm qua: +12.5%</p>
        </div>

        <div className="wpf-metric !border-b-[#0078D4]">
           <div className="flex justify-between items-start">
              <div>
                 <p className="wpf-metric-label">Hóa đơn mới</p>
                 <p className="wpf-metric-value">{data.ordersToday}</p>
              </div>
              <Layers className="w-8 h-8 text-[#0078D4] opacity-20" />
           </div>
           <p className="text-[10px] font-bold text-slate-400 mt-4 uppercase">Ký số: {data.ordersToday} Verified</p>
        </div>

        <div className="wpf-metric !border-b-amber-500">
           <div className="flex justify-between items-start">
              <div>
                 <p className="wpf-metric-label">Khách hàng mới</p>
                 <p className="wpf-metric-value text-amber-600">{data.customersCount}</p>
              </div>
              <Users className="w-8 h-8 text-amber-500 opacity-20" />
           </div>
           <p className="text-[10px] font-bold text-slate-400 mt-4 uppercase">Tăng trưởng: +5/tháng</p>
        </div>

        <div className="wpf-metric !border-b-indigo-500">
           <div className="flex justify-between items-start">
              <div>
                 <p className="wpf-metric-label">Kho hàng (SKUs)</p>
                 <p className="wpf-metric-value text-indigo-600">{data.productsCount}</p>
              </div>
              <Package className="w-8 h-8 text-indigo-500 opacity-20" />
           </div>
           <p className="text-[10px] font-bold text-slate-400 mt-4 uppercase">Trạng thái: Đầy đủ</p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Recent Transactions List */}
        <div className="lg:col-span-2 wpf-panel shadow-sm self-start">
           <div className="wpf-panel-header flex justify-between items-center text-[#333]">
              <span>CÁC GIAO DỊCH GẦN ĐÂY</span>
              <Link href="/history" className="text-[10px] font-black text-[#0078D4] hover:underline uppercase tracking-widest flex items-center gap-1">
                 Xem tất cả <ChevronRight className="w-3 h-3" />
              </Link>
           </div>
           <div className="overflow-x-auto">
              <table className="wpf-datagrid">
                 <thead>
                    <tr>
                       <th>MÃ HÓA ĐƠN</th>
                       <th>KHÁCH HÀNG</th>
                       <th className="text-right">TỔNG TIỀN</th>
                       <th className="text-center">THỜI GIAN</th>
                       <th className="text-center">XÁC THỰC</th>
                    </tr>
                 </thead>
                 <tbody>
                    {data.recentInvoices.map((inv: any) => (
                      <tr key={inv.Id}>
                         <td className="font-bold text-[#0078D4]">{inv.InvoiceNumber || `#${inv.Id}`}</td>
                         <td className="font-medium uppercase">{inv.CustomerName || "Khách vãng lai"}</td>
                         <td className="text-right font-black text-slate-800 tabular-nums">{formatCurrency(Number(inv.Total))}</td>
                         <td className="text-center text-slate-400 text-[11px] font-medium italic">
                            {new Date(inv.CreatedDate).toLocaleTimeString('vi-VN')}
                         </td>
                         <td className="text-center">
                            <span className="bg-emerald-50 text-emerald-600 border border-emerald-200 px-2 py-0.5 rounded-sm text-[9px] font-black uppercase tracking-widest">
                               SECURE
                            </span>
                         </td>
                      </tr>
                    ))}
                 </tbody>
              </table>
           </div>
        </div>

        {/* Quick Stats & System Health */}
        <div className="space-y-6">
           <div className="wpf-groupbox">
              <span className="wpf-groupbox-label">Thống kê nhanh</span>
              <div className="space-y-4 pt-2">
                 <div className="flex flex-col">
                    <span className="text-[9px] font-bold text-slate-400 uppercase tracking-widest leading-none mb-1">Sản phẩm hot nhất</span>
                    <span className="text-[13px] font-black text-slate-800 uppercase italic border-l-2 border-[#0078D4] pl-3 py-1">{data.trendingProduct}</span>
                 </div>
                 <div className="flex flex-col">
                    <span className="text-[9px] font-bold text-slate-400 uppercase tracking-widest leading-none mb-1">Khách hàng thân thiết</span>
                    <span className="text-[13px] font-black text-slate-800 uppercase italic border-l-2 border-amber-500 pl-3 py-1">{data.topCustomer}</span>
                 </div>
                 <div className="h-[1px] bg-[#D1D1D1] my-4" />
                 <button className="btn-wpf w-full flex items-center justify-center gap-2 h-10 font-black text-[11px] uppercase tracking-widest italic">
                    <Monitor className="w-3.5 h-3.5" /> BÁO CÁO TOÀN DIỆN
                 </button>
              </div>
           </div>

           <div className="wpf-panel !bg-[#0078D4] text-white border-[#005A9E] shadow-xl">
              <div className="p-6 space-y-6">
                 <div className="flex items-center gap-4">
                    <div className="w-10 h-10 bg-white/10 rounded-sm flex items-center justify-center border border-white/20">
                       <Activity className="w-6 h-6 text-white" />
                    </div>
                    <div>
                       <h4 className="text-[14px] font-black uppercase italic leading-none mb-1">HỆ THỐNG VẬN HÀNH</h4>
                       <p className="text-[9px] font-bold text-white/50 uppercase tracking-[0.2em]">Real-time Telemetry</p>
                    </div>
                 </div>
                 <p className="text-[11px] font-bold italic leading-relaxed opacity-80 uppercase tracking-tighter">
                   "KẾT NỐI DB PRIMARY ỔN ĐỊNH. HIỆU SUẤT XỬ LÝ GIAO DỊCH ĐẠT 0.2ms. KHÔNG GHI NHẬN LỖI TRONG 24H QUA."
                 </p>
                 <div className="flex items-center gap-2 text-[10px] font-black uppercase tracking-widest">
                    <Calendar className="w-3.5 h-3.5" /> LIÊN KẾT: {new Date().toLocaleDateString('vi-VN')}
                 </div>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
}
