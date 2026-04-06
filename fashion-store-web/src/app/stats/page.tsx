"use client";

import { useState, useEffect } from "react";
import { cn, formatCurrency } from "@/lib/utils";
import { 
  TrendingUp, 
  TrendingDown, 
  ShoppingBag, 
  BarChart3, 
  ArrowUpRight, 
  ArrowDownRight,
  Activity,
  Calendar,
  Layers,
  History,
  FileText,
  PieChart
} from "lucide-react";

export default function StatsPage() {
  const [stats, setStats] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const res = await fetch("/api/stats");
        const json = await res.json();
        if (json.success) {
          setStats(json.data);
        } else {
          console.error("API error:", json.error);
        }
      } catch (e) {
        console.error("Failed to load stats", e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  if (loading || !stats) return (
     <div className="flex flex-col items-center justify-center min-h-[60vh] gap-6 animate-in fade-in duration-700">
       <div className="w-16 h-16 border-t-4 border-[#0078D4] rounded-full animate-spin border-opacity-30 border-l-4 border-[#0078D4]" />
       <p className="text-[11px] font-black text-slate-400 uppercase tracking-[0.4em] animate-pulse">Fetching Analytical Data Base...</p>
     </div>
  );

  const trend = stats.revenueYesterday === 0 ? 100 : ((stats.revenueToday - stats.revenueYesterday) / stats.revenueYesterday) * 100;

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-20">
      {/* WPF Header Banner */}
      <div className="wpf-panel mb-8">
        <div className="wpf-panel-header">BÁO CÁO PHÂN TÍCH KINH DOANH CHUYÊN NGHIỆP</div>
        <div className="p-8 bg-white flex flex-col md:flex-row md:items-center justify-between gap-8">
           <div>
              <h2 className="text-[28px] font-black text-slate-900 tracking-tighter uppercase italic leading-none">THỐNG KÊ CHI TIẾT</h2>
              <p className="text-[11px] font-bold text-[#0078D4] mt-2 uppercase tracking-widest flex items-center gap-2">
                 <Calendar className="w-3.5 h-3.5" /> Dữ liệu được cập nhật mới nhất từ hệ thống Fusion ERP
              </p>
           </div>
           <div className="flex items-center gap-4">
              <button className="btn-wpf flex items-center gap-2 px-10 h-10 border-[#0078D4] text-[#0078D4] font-black">
                 <FileText className="w-4 h-4" /> XUẤT PHIẾU BÁO CÁO
              </button>
           </div>
        </div>
      </div>

      {/* KPI Cards Strip */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-white border border-[#D1D1D1] p-8 shadow-sm flex flex-col justify-between h-[180px]">
           <div className="flex justify-between items-start">
              <div>
                 <p className="text-[11px] font-black text-slate-400 uppercase tracking-widest mb-1">DOANH THU HÔM NAY</p>
                 <h3 className="text-[32px] font-black text-slate-900 tracking-tighter">{formatCurrency(stats.revenueToday)}</h3>
              </div>
              <div className={cn(
                 "p-3 rounded-sm border",
                 trend >= 0 ? "bg-emerald-50 text-emerald-600 border-emerald-200" : "bg-rose-50 text-rose-600 border-rose-200"
              )}>
                 {trend >= 0 ? <TrendingUp className="w-8 h-8" /> : <TrendingDown className="w-8 h-8" />}
              </div>
           </div>
           <div className="flex items-center gap-2 mt-auto">
              <span className={cn(
                "text-[11px] font-bold px-2 py-0.5 rounded-sm flex items-center gap-1",
                trend >= 0 ? "bg-emerald-100 text-emerald-700" : "bg-rose-100 text-rose-700"
              )}>
                {trend >= 0 ? <ArrowUpRight className="w-3.5 h-3.5" /> : <ArrowDownRight className="w-3.5 h-3.5" />}
                {Math.abs(trend).toFixed(1)}% So với hôm qua
              </span>
           </div>
        </div>

        <div className="bg-white border border-[#D1D1D1] p-8 shadow-sm flex flex-col justify-between h-[180px]">
           <div className="flex justify-between items-start">
              <div>
                 <p className="text-[11px] font-black text-slate-400 uppercase tracking-widest mb-1">ĐƠN HÀNG MỚI</p>
                 <h3 className="text-[32px] font-black text-slate-900 tracking-tighter">
                    {stats.ordersToday} <span className="text-[14px] text-slate-400 font-bold uppercase underline">Hóa đơn</span>
                 </h3>
              </div>
              <div className="p-3 bg-blue-50 text-[#0078D4] border border-[#CCE4F7] rounded-sm">
                 <ShoppingBag className="w-8 h-8" />
              </div>
           </div>
           <div className="mt-auto">
              <div className="h-2 bg-[#F3F3F3] border border-[#D1D1D1] rounded-sm overflow-hidden">
                 <div 
                   className="h-full bg-[#0078D4] transition-all duration-1000" 
                   style={{ width: `${Math.min(100, (stats.ordersToday / (stats.ordersYesterday || 1)) * 100)}%` }} 
                 />
              </div>
              <p className="text-[9px] font-black text-slate-300 uppercase tracking-widest mt-2">Dự kiến đạt {( (stats.ordersToday / (stats.ordersYesterday || 1)) * 100 ).toFixed(0)}% chỉ tiêu ngày</p>
           </div>
        </div>

        <div className="bg-white border border-[#D1D1D1] p-8 shadow-sm flex flex-col justify-between h-[180px]">
           <div className="flex justify-between items-start">
              <div>
                 <p className="text-[11px] font-black text-slate-400 uppercase tracking-widest mb-1">HIỆU SUẤT VẬN HÀNH</p>
                 <h3 className="text-[32px] font-black text-slate-900 tracking-tighter">{stats.performance}<span className="text-[14px] text-slate-400 font-black">%</span></h3>
              </div>
              <div className="p-3 bg-[#F0F0F0] text-slate-500 border border-[#D1D1D1] rounded-sm">
                 <BarChart3 className="w-8 h-8" />
              </div>
           </div>
           <div className="mt-auto flex items-center gap-2 text-[11px] font-black text-emerald-600 uppercase tracking-tight italic">
              <Activity className="w-4 h-4" /> Hệ thống đang hoạt động tối ưu
           </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Top Products - WPF Grid */}
        <div className="wpf-panel shadow-sm">
          <div className="wpf-panel-header flex justify-between items-center bg-[#F0F0F0] px-4 py-2 border-b border-[#D1D1D1]">
             <span>SẢN PHẨM BÁN CHẠY NHẤT</span>
             <PieChart className="w-3.5 h-3.5 text-slate-400" />
          </div>
          <div className="overflow-x-auto">
            <table className="wpf-datagrid">
              <thead>
                <tr>
                  <th>Tên sản phẩm</th>
                  <th className="text-right">Số lượng</th>
                  <th className="text-right">Doanh thu</th>
                </tr>
              </thead>
              <tbody>
                {stats.topProducts.map((p: any) => (
                  <tr key={p.Code}>
                    <td className="font-bold">{p.Name}</td>
                    <td className="text-right">{p.TotalQty}</td>
                    <td className="text-right font-bold text-emerald-600">{formatCurrency(p.TotalRevenue)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Low Stock - WPF Grid */}
        <div className="wpf-panel shadow-sm">
          <div className="wpf-panel-header flex justify-between items-center bg-[#F0F0F0] px-4 py-2 border-b border-[#D1D1D1]">
             <span>CẢNH BÁO TỒN KHO THẤP</span>
             <Layers className="w-3.5 h-3.5 text-rose-500" />
          </div>
          <div className="overflow-x-auto">
            <table className="wpf-datagrid">
              <thead>
                <tr>
                  <th>Sản phẩm</th>
                  <th className="text-center">Số lượng</th>
                  <th className="text-center">Ngưỡng tối thiểu</th>
                  <th className="text-center">Hệ số</th>
                </tr>
              </thead>
              <tbody>
                {stats.lowStock.map((p: any) => (
                  <tr key={p.Code}>
                    <td className="font-bold">{p.Name}</td>
                    <td className="text-center text-rose-600 font-bold">{p.StockQuantity}</td>
                    <td className="text-center text-slate-400">{p.MinStockLevel}</td>
                    <td className="text-center">
                       <span className="w-2.5 h-2.5 bg-rose-500 rounded-full inline-block animate-pulse" />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
      
      {/* Footer Info Strip */}
      <div className="bg-[#EAEAEA] border border-[#D1D1D1] px-6 py-3 rounded-sm flex items-center justify-between no-print">
         <div className="flex items-center gap-6">
            <div className="flex items-center gap-2 text-[10px] font-black text-slate-500 uppercase tracking-widest">
               <History className="w-3.5 h-3.5" /> Ghi log: {new Date().toLocaleTimeString()}
            </div>
            <div className="flex items-center gap-2 text-[10px] font-black text-slate-500 uppercase tracking-widest">
               <Layers className="w-3.5 h-3.5" /> Node: Primary DB Server
            </div>
         </div>
         <span className="text-[11px] font-bold text-[#0078D4] italic">Fusion Enterprise Dashboard Service v2.5.0</span>
      </div>
    </div>
  );
}
