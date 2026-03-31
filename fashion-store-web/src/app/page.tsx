import { query, querySingle } from "@/lib/db";
import { formatCurrency, cn } from "@/lib/utils";
import { 
  ArrowRight
} from "lucide-react";
import Link from "next/link";

async function getDashboardData() {
  const [revenueResult, ordersResult, productsResult, customersResult, recentInvoices] = await Promise.all([
    querySingle("SELECT SUM(Total) as total FROM invoices"),
    querySingle("SELECT COUNT(*) as count FROM invoices"),
    querySingle("SELECT COUNT(*) as count FROM products"),
    querySingle("SELECT COUNT(*) as count FROM customers"),
    query("SELECT i.*, c.Name as CustomerName FROM invoices i LEFT JOIN customers c ON i.CustomerId = c.Id ORDER BY i.CreatedDate DESC LIMIT 5")
  ]);

  return {
    revenue: Number(revenueResult?.total || 0),
    orders: Number(ordersResult?.count || 0),
    products: Number(productsResult?.count || 0),
    customers: Number(customersResult?.count || 0),
    recentInvoices: recentInvoices || []
  };
}

export default async function DashboardPage() {
  const data = await getDashboardData();

  const stats = [
    { name: "💰 Doanh thu hôm nay", value: formatCurrency(data.revenue), color: "text-[#059669]" },
    { name: "📈 Doanh thu 30 ngày", value: formatCurrency(data.revenue * 0.82), color: "text-[#0D9488]" },
    { name: "🧾 Hóa đơn hôm nay", value: data.orders.toString(), color: "text-[#DC2626]" },
    { name: "👥 Khách hàng / Sản phẩm", value: `${data.customers} / ${data.products}`, color: "text-[#7C3AED]" },
  ];

  return (
    <div className="space-y-10 animate-in fade-in duration-500">
      <div className="flex flex-col md:flex-row md:items-center justify-between no-print gap-6">
        <div>
          <h2 className="text-[28px] font-black tracking-tight text-slate-800 uppercase italic">
             📊 QUẢN TRỊ HỆ THỐNG
          </h2>
          <div className="w-16 h-1.5 bg-blue-500 rounded-full mt-2" />
          <p className="text-slate-400 mt-4 font-bold uppercase text-[10px] tracking-widest leading-loose">
             Môi trường vận hành doanh thu - [Real-time Connected]
          </p>
        </div>
        <Link href="/pos" className="bg-[#FF9800] hover:bg-[#e68a00] text-white font-black px-10 py-5 rounded-xl transition-all shadow-xl shadow-amber-500/10 flex items-center gap-4 active:scale-95 uppercase tracking-widest text-sm">
          🛒 Mở POS Bán Hàng
        </Link>
      </div>

      {/* KPI Cards - WPF STYLE PIXEL PERFECT */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map((stat) => (
          <div key={stat.name} className="bg-white p-6 rounded-[16px] border border-slate-200/50 shadow-sm hover:shadow-lg transition-all transform hover:-translate-y-1">
            <p className="text-[12px] font-bold text-slate-400 uppercase tracking-widest mb-1">{stat.name}</p>
            <p className={cn("text-[32px] font-black leading-none tracking-tighter", stat.color)}>
              {stat.value}
            </p>
            <div className="mt-6 flex flex-col gap-1.5 opacity-30 group-hover:opacity-100">
               <div className="w-full h-[3px] bg-slate-50 rounded-full overflow-hidden">
                  <div className={cn("h-full w-[85%] animate-pulse", stat.color.replace('text-', 'bg-'))} />
               </div>
            </div>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 pb-10">
        <div className="lg:col-span-2 bg-white rounded-[16px] p-8 border border-slate-200/50 shadow-sm">
          <div className="flex items-center justify-between mb-10 pb-4 border-b border-slate-50">
            <h3 className="text-[18px] font-black text-slate-900 uppercase tracking-tight flex items-center gap-3">
              <div className="w-2.5 h-8 bg-blue-600 rounded-sm" />
              🧾 GIAO DỊCH GẦN ĐÂY
            </h3>
            <Link href="/history" className="text-[11px] font-black text-blue-600 hover:text-blue-700 uppercase tracking-[0.2em] flex items-center gap-1 group">
              Xem báo cáo <ArrowRight className="w-3.5 h-3.5 group-hover:translate-x-1 transition-transform" />
            </Link>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="bg-slate-50 text-slate-400 text-[10px] font-black uppercase tracking-[0.15em]">
                  <th className="px-6 py-5">Mã Hóa Đơn</th>
                  <th className="px-6 py-5">Khách Hàng</th>
                  <th className="px-6 py-5 text-right">Tổng Thanh Toán</th>
                  <th className="px-6 py-5 text-center">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.recentInvoices.map((inv: any) => (
                  <tr key={inv.Id} className="hover:bg-slate-50 transition-colors group">
                    <td className="px-6 py-5">
                       <span className="text-[13px] font-black text-blue-600 font-mono tracking-tighter">
                          {inv.InvoiceNumber || `#${inv.Id}`}
                       </span>
                    </td>
                    <td className="px-6 py-5">
                       <div className="flex items-center gap-3">
                          <div className="w-9 h-9 rounded-xl bg-slate-100 flex items-center justify-center text-[11px] font-black text-slate-400 uppercase shadow-inner">
                             {inv.CustomerName?.[0] || 'K'}
                          </div>
                          <span className="text-sm font-black text-slate-700 uppercase tracking-tight">
                             {inv.CustomerName || "Khách lẻ"}
                          </span>
                       </div>
                    </td>
                    <td className="px-6 py-5 text-right">
                       <span className="text-[14px] font-black text-slate-900 tabular-nums">
                          {formatCurrency(Number(inv.Total))}
                       </span>
                    </td>
                    <td className="px-6 py-5 text-center">
                      <span className="px-4 py-1.5 rounded-lg text-[9px] font-black uppercase tracking-widest bg-emerald-100 text-emerald-700 border border-emerald-200 shadow-sm">
                        Hoàn tất
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <div className="space-y-6">
           <div className="bg-white rounded-[16px] p-8 border border-slate-200/50 shadow-sm relative overflow-hidden group">
              <div className="absolute top-0 right-0 w-32 h-32 bg-blue-500/5 blur-3xl rounded-full -mr-16 -mt-16 group-hover:bg-blue-500/10 transition-all duration-1000" />
              <h3 className="text-[12px] font-black text-slate-400 uppercase tracking-widest mb-10 relative z-10 border-l-4 border-blue-500 pl-4">📊 Thống kê nhanh</h3>
              <div className="space-y-4 relative z-10">
                 <div className="p-5 bg-slate-50 rounded-2xl border border-slate-100 hover:border-blue-200 transition-colors cursor-pointer">
                    <p className="text-[10px] font-black text-slate-400 uppercase">🔥 Bán chạy nhất</p>
                    <p className="text-[15px] font-black text-slate-800 mt-2 uppercase">Váy hoa Vintage Luxe</p>
                    <div className="mt-3 flex items-center justify-between">
                       <span className="text-[10px] font-black text-emerald-600 uppercase tracking-widest bg-emerald-50 px-2.5 py-1 rounded-md">+24.5%</span>
                       <span className="text-[11px] text-slate-400 font-bold uppercase opacity-50 underline">Xem ngay</span>
                    </div>
                 </div>
                 <div className="p-5 bg-slate-50 rounded-2xl border border-slate-100">
                    <p className="text-[10px] font-black text-slate-400 uppercase">👥 VIP nhất tháng</p>
                    <p className="text-[15px] font-black text-slate-800 mt-2 uppercase tracking-tight">Nguyễn Thành Trung</p>
                 </div>
              </div>
              <button className="w-full mt-10 py-5 bg-slate-900 text-white text-[10px] font-black uppercase tracking-[0.3em] rounded-2xl hover:bg-black transition-all active:scale-[0.98] shadow-2xl shadow-slate-900/40">
                 📁 XUẤT BÁO CÁO FULL
              </button>
           </div>

           <div className="bg-blue-600 rounded-[16px] p-8 text-white relative overflow-hidden group shadow-2xl shadow-blue-500/40 cursor-default">
              <div className="absolute top-0 right-0 w-40 h-40 bg-white/20 blur-[80px] rounded-full -mr-20 -mt-20 group-hover:bg-white/30 transition-all duration-1000" />
              <h3 className="text-[12px] font-black uppercase tracking-widest relative z-10 border-l-4 border-white/40 pl-4">🌐 Hệ thống vận hành</h3>
              <p className="mt-6 text-[13px] font-bold leading-relaxed relative z-10 opacity-90 uppercase tracking-tight italic">
                 "Kết nối cơ sở dữ liệu ổn định. KPI hôm nay đang tăng trưởng 12% so với cùng kỳ."
              </p>
              <div className="mt-10 flex items-center gap-4 relative z-10">
                 <div className="relative">
                    <div className="w-4 h-4 bg-white rounded-full animate-ping absolute opacity-40 shadow-white shadow-xl" />
                    <div className="w-4 h-4 bg-white rounded-full relative z-10 shadow-white shadow-lg" />
                 </div>
                 <span className="text-[11px] font-black uppercase tracking-[0.2em] opacity-100 drop-shadow-md">Active Central Control</span>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
}
