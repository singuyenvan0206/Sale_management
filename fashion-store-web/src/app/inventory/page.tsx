import { query } from "@/lib/db";
export const dynamic = 'force-dynamic';
import { formatCurrency, cn } from "@/lib/utils";
import { 
  Package, 
  ArrowUpRight, 
  ArrowDownRight, 
  AlertTriangle, 
  History,
  TrendingUp,
  Box,
  LayoutGrid,
  Zap,
  Clock,
  ArrowRight,
  ShieldCheck,
  ChevronRight,
  BarChart3,
  Truck,
  Layers,
  FileText
} from "lucide-react";
import Link from "next/link";

async function getInventoryData() {
  const [stockSummary, lowStock, recentMovements] = await Promise.all([
    query('SELECT SUM("StockQuantity") as "TotalQty", COUNT(*) as "TotalItems" FROM "products"'),
    query('SELECT * FROM "products" WHERE "StockQuantity" <= 10 ORDER BY "StockQuantity" ASC LIMIT 5'),
    query(`
      SELECT sm.*, p."Name" as "ProductName" 
      FROM "StockMovements" sm 
      JOIN "products" p ON sm."ProductId" = p."Id" 
      ORDER BY sm."CreatedDate" DESC 
      LIMIT 12
    `)
  ]);

  return {
    summary: stockSummary[0] || { TotalQty: 0, TotalItems: 0 },
    lowStock: lowStock || [],
    recentMovements: recentMovements || []
  };
}

export default async function InventoryPage() {
  const data = await getInventoryData();

  const totalItems = Number(data.summary.TotalItems || 0);
  const lowStockCount = data.lowStock.length;
  const healthPercent = totalItems > 0 ? Math.round(((totalItems - lowStockCount) / totalItems) * 100) : 100;

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-20 no-select">
      {/* WPF Header / Ribbon */}
      <div className="wpf-panel">
         <div className="wpf-panel-header uppercase">HỆ THỐNG ĐIỀU PHỐI KHO VẬN & CHUỖI CUNG ỨNG (SUPPLY CHAIN)</div>
         <div className="p-6 bg-white flex flex-col md:flex-row md:items-center justify-between gap-6">
            <div className="flex items-center gap-4">
               <div className="w-10 h-10 bg-[#0078D4] rounded-sm flex items-center justify-center text-white">
                  <Truck className="w-6 h-6" />
               </div>
               <div>
                  <h2 className="text-[20px] font-black text-slate-900 tracking-tight uppercase italic leading-none">QUẢN TRỊ KHO VẬN</h2>
                  <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-widest italic">Fusion ERP Inventory Control Subsystem</p>
               </div>
            </div>
            <div className="flex items-center gap-3">
               <Link href="/inventory/import" className="btn-wpf btn-wpf-primary h-12 px-8 flex items-center gap-3 uppercase font-black text-[12px] shadow-lg border-b-4 border-[#005A9E] active:translate-y-1 active:border-b-0 transition-all">
                  <Package className="w-5 h-5" /> NHẬP KHO THÀNH PHẨM (F5)
               </Link>
            </div>
         </div>
      </div>

      {/* KPI Section */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
         <div className="wpf-metric">
            <p className="wpf-metric-label">Tổng tồn thực tế</p>
            <p className="wpf-metric-value">{Number(data.summary.TotalQty || 0).toLocaleString('vi-VN')} SP</p>
            <div className="flex items-center gap-2 text-emerald-500 font-black text-[9px] uppercase tracking-widest mt-4">
               <Zap className="w-3 h-3 animate-pulse" /> Live Sync Verified
            </div>
         </div>

         <div className="wpf-metric !border-b-[#0078D4]">
            <p className="wpf-metric-label">Số lượng Mã SKU</p>
            <p className="wpf-metric-value">{data.summary.TotalItems || 0}</p>
            <div className="flex items-center gap-2 text-[#0078D4] font-black text-[9px] uppercase tracking-widest mt-4">
               <Layers className="w-3 h-3" /> Active Catalog SKUs
            </div>
         </div>

         <div className="md:col-span-2 wpf-panel !bg-slate-800 text-white border-slate-700 shadow-md">
            <div className="p-6">
               <div className="flex justify-between items-start mb-6">
                  <div>
                     <p className="text-[10px] font-black text-white/40 uppercase tracking-widest mb-1">Chỉ số sức khỏe kho hàng (Health Index)</p>
                     <h4 className="text-[32px] font-black text-white tracking-tighter italic leading-none uppercase">{healthPercent}% LOADED</h4>
                  </div>
                  <BarChart3 className={cn("w-10 h-10", healthPercent > 80 ? "text-emerald-500" : "text-amber-500")} />
               </div>
               <div className="w-full bg-white/5 h-2 rounded-sm overflow-hidden border border-white/5">
                  <div 
                    className={cn("h-full transition-all duration-1000", healthPercent > 80 ? "bg-emerald-500 shadow-[0_0_15px_rgba(16,185,129,0.3)]" : "bg-amber-500")} 
                    style={{ width: `${healthPercent}%` }} 
                  />
               </div>
               <p className="text-[9px] font-black text-white/30 uppercase tracking-widest mt-3 italic underline decoration-white/10">Detecting {lowStockCount} items below safety threshold alert level.</p>
            </div>
         </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
         {/* Low Stock DataGrid */}
         <div className="lg:col-span-5 wpf-panel shadow-sm">
            <div className="wpf-panel-header flex justify-between items-center text-rose-600 bg-rose-50 border-b-rose-200">
               <span className="flex items-center gap-2">
                  <AlertTriangle className="w-4 h-4 animate-pulse" /> CẢNH BÁO TỒN KHO THẤP
               </span>
               <Link href="/products" className="text-[10px] font-black text-slate-400 hover:text-slate-900 transition-colors uppercase tracking-widest">XEM TẤT CẢ SKU</Link>
            </div>
            <div className="overflow-x-auto">
               <table className="wpf-datagrid">
                  <thead>
                     <tr>
                        <th>SẢN PHẨM</th>
                        <th className="text-center">MÃ SKU</th>
                        <th className="text-right">TỒN</th>
                     </tr>
                  </thead>
                  <tbody>
                     {data.lowStock.map((p: any) => (
                        <tr key={p.Id}>
                           <td className="font-bold text-slate-800 uppercase italic truncate max-w-[150px]">{p.Name}</td>
                           <td className="text-center font-bold text-slate-400 tracking-widest">{p.Code}</td>
                           <td className="text-right font-black text-rose-600 tabular-nums uppercase">{p.StockQuantity} SP</td>
                        </tr>
                     ))}
                     {data.lowStock.length === 0 && (
                        <tr>
                           <td colSpan={3} className="py-20 text-center opacity-20">
                              <ShieldCheck className="w-12 h-12 mx-auto mb-4" />
                              <p className="text-[10px] font-black uppercase tracking-widest">Inventory Health is Optimal</p>
                           </td>
                        </tr>
                     )}
                  </tbody>
               </table>
            </div>
         </div>

         {/* Stock Movements Log DataGrid */}
         <div className="lg:col-span-7 wpf-panel shadow-sm">
            <div className="wpf-panel-header flex justify-between items-center text-[#333]">
               <span className="flex items-center gap-2">
                  <History className="w-4 h-4" /> NHẬT KÝ BIẾN ĐỘNG KHO (MOVE LOG)
               </span>
               <button className="text-[10px] font-black text-[#0078D4] hover:underline uppercase tracking-widest">XUẤT NHẬT KÝ CSV</button>
            </div>
            <div className="overflow-x-auto">
               <table className="wpf-datagrid">
                  <thead>
                     <tr>
                        <th className="w-[100px]">LOẠI</th>
                        <th>SẢN PHẨM / DIỄN GIẢI</th>
                        <th className="text-right w-[100px]">SL</th>
                        <th className="text-center w-[120px]">THỜI GIAN</th>
                        <th className="text-center w-[100px]">T.THÁI</th>
                     </tr>
                  </thead>
                  <tbody>
                     {data.recentMovements.map((move: any) => (
                        <tr key={move.Id}>
                           <td className="text-center">
                              <span className={cn(
                                 "px-2 py-0.5 rounded-sm text-[9px] font-black uppercase border",
                                 move.Type === 'IN' || move.MovementType === 'Import' ? "bg-emerald-50 text-emerald-600 border-emerald-200" : "bg-blue-50 text-blue-600 border-blue-200"
                              )}>
                                 {move.Type === 'IN' || move.MovementType === 'Import' ? "NHẬP KHO" : "XUẤT KHO"}
                              </span>
                           </td>
                           <td>
                              <div className="flex flex-col">
                                 <span className="text-[12px] font-bold text-slate-800 uppercase leading-none mb-1">{move.ProductName}</span>
                                 <span className="text-[9px] font-medium text-slate-400 italic truncate max-w-[200px]">
                                    {move.Note || (move.Type === 'IN' || move.MovementType === 'Import' ? 'NHẬP HÀNG TỪ NCC' : 'XUẤT KHO BÁN LẺ POS')}
                                 </span>
                              </div>
                           </td>
                           <td className={cn(
                              "text-right font-black tabular-nums italic",
                              move.Type === 'IN' || move.MovementType === 'Import' ? "text-emerald-500" : "text-blue-500"
                           )}>
                              {move.Type === 'IN' || move.MovementType === 'Import' ? '+' : '-'}{move.Quantity}
                           </td>
                           <td className="text-center text-slate-400 text-[10px] uppercase font-bold">
                              {new Date(move.CreatedDate).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                           </td>
                           <td className="text-center">
                              <div className="w-5 h-5 bg-slate-50 border border-slate-200 rounded-sm flex items-center justify-center mx-auto">
                                 <div className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
                              </div>
                           </td>
                        </tr>
                     ))}
                  </tbody>
               </table>
            </div>
            <div className="p-3 bg-[#F0F0F0] border-t border-[#D1D1D1] flex justify-between text-[11px] font-bold text-slate-400 uppercase">
               <span>Nhật ký thời gian thực (Real-time telemetry enabled)</span>
               <span className="italic">Subsystem ID: KHO_01_SECURE</span>
            </div>
         </div>
      </div>
    </div>
  );
}
