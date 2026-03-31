"use client";

import { useState, useEffect } from "react";
import { formatCurrency, cn } from "@/lib/utils";
import { 
  TrendingUp, 
  ShoppingBag, 
  Package, 
  ArrowUpRight, 
  ArrowDownRight,
  Calendar,
  Layers,
  ChevronRight,
  Filter,
  BarChart3,
  PieChart,
  Activity
} from "lucide-react";

export default function StatsPage() {
  const [stats, setStats] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const res = await fetch("/api/stats"); // I'll need to create this API
        const data = await res.json();
        setStats(data);
      } catch (e) {
        console.error("Failed to load stats", e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  if (loading) return <div className="p-20 text-center font-black animate-pulse">ĐANG PHÂN TÍCH DỮ LIỆU KINH DOANH...</div>;
  if (!stats) return <div className="p-20 text-center">Lỗi tải dữ liệu.</div>;

  const trend = stats.revenueYesterday === 0 ? 100 : ((stats.revenueToday - stats.revenueYesterday) / stats.revenueYesterday) * 100;

  return (
    <div className="space-y-6 animate-in fade-in duration-500 pb-20">
      {/* Header Bar - WPF style */}
      <div className="bg-[#1CB5E0] p-6 -mx-8 -mt-8 mb-8 shadow-lg">
        <h2 className="text-[24px] font-bold text-white text-center tracking-tight">
           📊 Báo Cáo & Thống Kê Chi Tiết
        </h2>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {/* KPI Cards - WPF Style */}
        <div className="bg-white p-8 rounded-[12px] shadow-sm border-t-4 border-[#1CB5E0] relative overflow-hidden">
           <div className="flex justify-between items-start relative z-10">
              <div className="space-y-1">
                 <p className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Doanh thu hôm nay</p>
                 <h3 className="text-[32px] font-black text-slate-900 tracking-tighter">{formatCurrency(stats.revenueToday)}</h3>
              </div>
              <div className={cn(
                 "p-3 rounded-xl",
                 trend >= 0 ? "bg-emerald-50 text-emerald-600" : "bg-rose-50 text-rose-600"
              )}>
                 {trend >= 0 ? <TrendingUp className="w-6 h-6" /> : <Activity className="w-6 h-6" />}
              </div>
           </div>
           <div className="mt-4 flex items-center gap-2">
              <span className={cn(
                "text-[10px] font-black px-2 py-0.5 rounded flex items-center gap-1",
                trend >= 0 ? "bg-emerald-100 text-emerald-700" : "bg-rose-100 text-rose-700"
              )}>
                {trend >= 0 ? <ArrowUpRight className="w-3 h-3" /> : <ArrowDownRight className="w-3 h-3" />}
                {Math.abs(trend).toFixed(1)}% So với hôm qua
              </span>
           </div>
           <div className="absolute -right-4 -bottom-4 w-24 h-24 bg-blue-50/50 rounded-full blur-2xl" />
        </div>

        <div className="bg-white p-8 rounded-[12px] shadow-sm border-t-4 border-amber-500">
           <div className="flex justify-between items-start">
              <div className="space-y-1">
                 <p className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Đơn hàng mới</p>
                 <h3 className="text-[32px] font-black text-slate-900 tracking-tighter">
                    {(stats.last7Days && stats.last7Days.length > 0) 
                      ? (stats.last7Days[stats.last7Days.length - 1]?.count || 0) 
                      : 0} <span className="text-[14px] text-slate-400">Đơn</span>
                 </h3>
              </div>
              <div className="p-3 bg-amber-50 text-amber-600 rounded-xl">
                 <ShoppingBag className="w-6 h-6" />
              </div>
           </div>
           <div className="mt-4">
              <div className="h-1.5 bg-slate-100 rounded-full overflow-hidden">
                 <div className="h-full bg-amber-500 w-[70%]" />
              </div>
           </div>
        </div>

        <div className="bg-white p-8 rounded-[12px] shadow-sm border-t-4 border-rose-500">
           <div className="flex justify-between items-start">
              <div className="space-y-1">
                 <p className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Hiệu suất bán hàng</p>
                 <h3 className="text-[32px] font-black text-slate-900 tracking-tighter">94.2<span className="text-[14px] text-slate-400">%</span></h3>
              </div>
              <div className="p-3 bg-rose-50 text-rose-600 rounded-xl">
                 <BarChart3 className="w-6 h-6" />
              </div>
           </div>
           <div className="mt-4 flex items-center gap-1.5 text-[11px] font-bold text-emerald-600 uppercase tracking-tighter">
              <ArrowUpRight className="w-3.5 h-3.5" /> Chỉ số đạt mức cao
           </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
         {/* Top Products Table */}
         <div className="bg-white rounded-[12px] shadow-sm overflow-hidden flex flex-col">
            <div className="p-6 bg-[#f8f9fa] border-b flex items-center justify-between">
               <h3 className="text-[14px] font-black text-slate-800 uppercase tracking-widest flex items-center gap-2">
                  <PieChart className="w-4 h-4 text-[#1CB5E0]" /> Top 5 Sản Phẩm Bán Chạy
               </h3>
               <button className="text-[10px] font-black text-[#1CB5E0] uppercase tracking-widest hover:underline transition-all">Chi tiết</button>
            </div>
            <div className="overflow-x-auto h-[400px]">
               <table className="w-full text-left">
                  <thead className="bg-[#f8f9fa] sticky top-0 font-serif italic text-slate-400 text-[10px] uppercase">
                     <tr>
                        <th className="px-6 py-4">#</th>
                        <th className="px-6 py-4">Sản Phẩm</th>
                        <th className="px-6 py-4 text-right">Số Lượng</th>
                        <th className="px-6 py-4 text-right">Doanh Thu</th>
                     </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-50 italic">
                     {(stats.topProducts || []).map((p: any, idx: number) => (
                        <tr key={idx} className="hover:bg-blue-50 transition-colors">
                           <td className="px-6 py-5 font-black text-slate-300">0{idx + 1}</td>
                           <td className="px-6 py-5">
                              <div className="flex flex-col">
                                 <span className="text-[14px] font-black uppercase tracking-tighter">{p.Name}</span>
                                 <span className="text-[10px] opacity-50 uppercase">{p.Code}</span>
                              </div>
                           </td>
                           <td className="px-6 py-5 text-right font-black text-[#1CB5E0]">{p.total_qty}</td>
                           <td className="px-6 py-5 text-right font-black text-emerald-600">{formatCurrency(p.total_revenue)}</td>
                        </tr>
                     ))}
                     {(!stats.topProducts || stats.topProducts.length === 0) && (
                        <tr>
                           <td colSpan={4} className="px-6 py-10 text-center text-slate-400 italic">Chưa có dữ liệu bán hàng</td>
                        </tr>
                     )}
                  </tbody>
               </table>
            </div>
         </div>

         {/* Revenue Chart - CSS Bars */}
         <div className="bg-white rounded-[12px] shadow-sm p-8 flex flex-col justify-between">
            <div>
               <h3 className="text-[14px] font-black text-slate-800 uppercase tracking-widest mb-1">Doanh thu 7 ngày qua</h3>
               <p className="text-[11px] font-bold text-slate-400 uppercase italic">Biểu đồ tăng trưởng trực quan</p>
            </div>
            
            <div className="mt-12 h-64 flex items-end justify-between gap-4">
                {(stats.last7Days || []).length > 0 ? (stats.last7Days.map((d: any, idx: number) => {
                  const maxRevenue = Math.max(...(stats.last7Days || []).map((x: any) => Number(x.revenue || 0))) || 1;
                  const height = (Number(d.revenue || 0) / maxRevenue) * 100;
                  return (
                    <div key={idx} className="flex-1 flex flex-col items-center gap-3 group h-full justify-end">
                      <div className="w-full relative h-[80%] flex flex-col justify-end">
                        <div 
                           className="w-full bg-[#E0E7FF] group-hover:bg-[#1CB5E0] transition-all rounded-t-lg shadow-sm group-hover:shadow-[#1CB5E0]/20"
                           style={{ height: `${height}%` }}
                        >
                           {/* Hover Price Tag */}
                           <div className="absolute bottom-full mb-3 left-1/2 -translate-x-1/2 opacity-0 group-hover:opacity-100 transition-all bg-slate-900 text-white text-[9px] font-black px-2 py-1 rounded shadow-lg pointer-events-none whitespace-nowrap z-20">
                              {formatCurrency(d.revenue || 0)}
                           </div>
                        </div>
                      </div>
                      <span className="text-[10px] font-black text-slate-400 uppercase tracking-tighter text-center">
                         {new Date(d.date).toLocaleDateString('vi-VN', { weekday: 'short' })}
                         <br/>
                         {new Date(d.date).getDate()}/{new Date(d.date).getMonth()+1}
                      </span>
                    </div>
                  );
                })) : (
                  <div className="w-full text-center text-slate-300 italic text-sm">Chưa có dữ liệu tăng trưởng</div>
                )}
            </div>
         </div>
      </div>

      {/* Row 3 - Low Stock Alerts */}
      <div className="bg-slate-900 rounded-[12px] p-10 text-white relative overflow-hidden">
         <div className="absolute top-0 right-0 w-64 h-64 bg-amber-500/10 rounded-full blur-[100px] -mr-32 -mt-32" />
         <div className="relative z-10 flex flex-col md:flex-row md:items-center justify-between gap-8">
            <div className="max-w-md">
               <h3 className="text-[20px] font-black uppercase tracking-tight flex items-center gap-3">
                  <Package className="w-6 h-6 text-amber-500" /> Cảnh Báo Tồn Kho Thấp
               </h3>
               <p className="mt-4 text-slate-400 text-[13px] font-medium italic leading-relaxed">
                  Các sản phẩm dưới đây đang ở dưới ngưỡng tồn tối thiểu. Bạn cần thực hiện nhập hàng ngay để đảm bảo dòng vận hành cửa hàng không bị gián đoạn.
               </p>
            </div>
            <div className="flex-1 grid grid-cols-2 md:grid-cols-4 gap-4">
               {(stats.lowStock || []).slice(0, 4).map((p: any, idx: number) => (
                  <div key={idx} className="bg-white/5 border border-white/10 p-5 rounded-xl hover:bg-white/10 transition-colors">
                     <p className="text-[10px] font-black text-amber-500 uppercase tracking-widest truncate">{p.Name}</p>
                     <p className="text-xl font-black mt-2">{p.StockQuantity} <span className="text-xs opacity-40">/ {p.MinStockLevel}</span></p>
                     <div className="mt-3 h-1 bg-white/10 rounded-full overflow-hidden">
                        <div className="h-full bg-rose-500" style={{ width: `${(p.StockQuantity / p.MinStockLevel) * 100}%` }} />
                     </div>
                  </div>
               ))}
               {(!stats.lowStock || stats.lowStock.length === 0) && (
                 <div className="col-span-full py-4 text-emerald-400 font-bold italic text-sm">Mọi sản phẩm đều đạt ngưỡng an toàn</div>
               )}
            </div>
         </div>
      </div>
    </div>
  );
}
