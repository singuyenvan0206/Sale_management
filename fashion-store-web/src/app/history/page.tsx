import { query } from "@/lib/db";
export const dynamic = 'force-dynamic';
import { formatCurrency, cn } from "@/lib/utils";
import { 
  History, 
  Search, 
  Calendar, 
  ChevronRight,
  Download,
  Filter,
  FileText,
  DollarSign,
  ShoppingCart,
  TrendingUp,
  RotateCcw,
  Monitor,
  Printer
} from "lucide-react";
import Link from "next/link";
import ExportButton from "@/components/history/ExportButton";

interface PageProps {
  searchParams: {
    q?: string;
    status?: string;
    from?: string;
    to?: string;
  }
}

async function getInvoices(search?: string, status?: string, from?: string, to?: string) {
  let sql = `
    SELECT i.*, c."Name" as "CustomerName" 
    FROM "invoices" i 
    LEFT JOIN "customers" c ON i."CustomerId" = c."Id" 
    WHERE 1=1
  `;
  const params: any[] = [];

  if (search) {
    sql += ` AND (i."InvoiceNumber" ILIKE $${params.length + 1} OR c."Name" ILIKE $${params.length + 2})`;
    params.push(`%${search}%`, `%${search}%`);
  }

  if (status && status !== "all") {
    sql += ` AND i."Status" = $${params.length + 1}`;
    params.push(status);
  }

  if (from) {
    sql += ` AND i."CreatedDate" >= $${params.length + 1}`;
    params.push(from);
  }

  if (to) {
    sql += ` AND i."CreatedDate" <= $${params.length + 1}`;
    params.push(to);
  }

  sql += ' ORDER BY i."CreatedDate" DESC';

  return await query(sql, params);
}

export default async function HistoryPage({ searchParams }: PageProps) {
  const q = searchParams.q || "";
  const status = searchParams.status || "all";
  const from = searchParams.from || "";
  const to = searchParams.to || "";
  
  const invoices = await getInvoices(q, status, from, to);

  const totalRevenue = invoices.reduce((a: any, b: any) => a + Number(b.FinalAmount || 0), 0);

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-20 no-select">
      {/* WPF Header / Ribbon */}
      <div className="wpf-panel">
         <div className="wpf-panel-header uppercase">HỆ THỐNG TRUY VẤN LỊCH SỬ GIAO DỊCH & KIỂM TOÁN (AUDIT LOG)</div>
         <div className="p-6 bg-white flex flex-col md:flex-row md:items-center justify-between gap-6">
            <div className="flex items-center gap-4">
               <div className="w-10 h-10 bg-[#0078D4] rounded-sm flex items-center justify-center text-white shadow-md">
                  <History className="w-6 h-6" />
               </div>
               <div>
                  <h2 className="text-[20px] font-black text-slate-900 tracking-tight uppercase italic leading-none">NHẬT KÝ HÓA ĐƠN</h2>
                  <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-widest italic tracking-tighter">Fusion ERP Transaction Registry Shell v2.5</p>
               </div>
            </div>
            
            <div className="flex items-center gap-4">
                <ExportButton data={JSON.parse(JSON.stringify(invoices))} />
                <div className="h-8 w-[1px] bg-slate-200 hidden md:block" />
                <div className="text-right">
                    <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Phiên bản dữ liệu</p>
                    <span className="text-[11px] font-black text-[#0078D4] uppercase italic underline decoration-[#0078D4]/30">LIVE-PRODUCTION</span>
                </div>
            </div>
         </div>
      </div>

      {/* Query Toolbar */}
      <div className="wpf-panel shadow-sm">
         <div className="wpf-panel-header !py-2 text-[11px] font-bold text-[#333]">BỘ LỌC TRUY VẤN (QUERY FILTERS)</div>
         <div className="p-4 bg-white">
            <form action="/history" method="GET" className="flex flex-wrap items-center gap-6">
               <div className="flex items-center gap-3">
                  <label className="text-[10px] font-black text-slate-500 uppercase tracking-widest">Từ khóa:</label>
                  <div className="relative group">
                     <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400 group-focus-within:text-[#0078D4]" />
                     <input 
                       name="q"
                       type="text" 
                       defaultValue={q}
                       placeholder="SỐ HĐ, KHÁCH HÀNG..."
                       className="h-9 border border-[#D1D1D1] pl-9 pr-4 text-[13px] font-bold bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm min-w-[260px] uppercase italic transition-all shadow-inner"
                     />
                  </div>
               </div>
               
               <div className="flex items-center gap-3">
                  <label className="text-[10px] font-black text-slate-500 uppercase tracking-widest">Trạng thái:</label>
                  <select 
                    name="status"
                    defaultValue={status}
                    className="h-9 border border-[#D1D1D1] px-3 text-[12px] font-black bg-white focus:border-[#0078D4] outline-none rounded-sm min-w-[160px] cursor-pointer uppercase"
                  >
                     <option value="all">TẤT CẢ TRẠNG THÁI</option>
                     <option value="Paid">ĐÃ THANH TOÁN</option>
                     <option value="Pending">ĐANG XỬ LÝ</option>
                     <option value="Canceled">HỦY BỎ</option>
                  </select>
               </div>

               <div className="flex items-center gap-3">
                  <label className="text-[10px] font-black text-slate-500 uppercase tracking-widest">Khoảng thời gian:</label>
                  <div className="flex items-center bg-[#F9F9F9] border border-[#D1D1D1] rounded-sm p-1 gap-2">
                     <input name="from" defaultValue={from} type="date" className="h-7 border-none px-2 text-[11px] font-black bg-transparent outline-none uppercase" />
                     <span className="text-slate-300 font-bold">→</span>
                     <input name="to" defaultValue={to} type="date" className="h-7 border-none px-2 text-[11px] font-black bg-transparent outline-none uppercase" />
                  </div>
               </div>

               <button type="submit" className="btn-wpf btn-wpf-primary flex items-center justify-center gap-2 h-9 px-8 ml-auto uppercase font-black text-[11px] border-b-4 border-[#005A9E]">
                  <Filter className="w-4 h-4" /> THỰC THI TRUY VẤN
               </button>
            </form>
         </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-12 gap-6 items-start">
         {/* Left: Summary Cards */}
         <div className="md:col-span-3 space-y-6 lg:sticky lg:top-6">
            <div className="wpf-panel shadow-md border-emerald-200">
               <div className="p-6 bg-white flex items-center gap-4">
                  <div className="w-12 h-12 bg-emerald-600 rounded-sm flex items-center justify-center text-white shadow-lg">
                     <DollarSign className="w-8 h-8" />
                  </div>
                  <div>
                     <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">TỔNG DOANH THU LOG</p>
                     <h4 className="text-[20px] font-black text-slate-900 tracking-tight tabular-nums italic">{formatCurrency(totalRevenue)}</h4>
                  </div>
               </div>
            </div>

            <div className="wpf-panel shadow-md border-blue-200">
               <div className="p-6 bg-white flex items-center gap-4">
                  <div className="w-12 h-12 bg-[#0078D4] rounded-sm flex items-center justify-center text-white shadow-lg">
                     <ShoppingCart className="w-8 h-8" />
                  </div>
                  <div>
                     <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">TỔNG SỐ GIAO DỊCH</p>
                     <h4 className="text-[20px] font-black text-slate-900 tracking-tight tabular-nums italic">{invoices.length} VOUCHERS</h4>
                  </div>
               </div>
            </div>

            <div className="wpf-panel !bg-slate-800 text-white border-slate-700 p-6 flex items-center gap-4">
               <Monitor className="w-6 h-6 text-[#0078D4]" />
               <div className="flex flex-col">
                  <span className="text-[10px] font-black uppercase tracking-widest leading-none mb-1">TRẠNG THÁI KIỂM KÊ</span>
                  <span className="text-[11px] font-bold text-emerald-400 uppercase">HỆ THỐNG ĐANG KHỚP LỆNH</span>
               </div>
            </div>
         </div>

         {/* Right: DataGrid */}
         <div className="md:col-span-9">
            <div className="wpf-panel shadow-md overflow-hidden">
               <div className="wpf-panel-header flex items-center gap-2">
                  <FileText className="w-4 h-4" /> TRANSACTIONS DATABASE REGISTRY
               </div>
               <div className="overflow-x-auto bg-white">
                  <table className="wpf-datagrid">
                     <thead>
                        <tr>
                           <th className="w-[120px]">SỐ HÓA ĐƠN</th>
                           <th className="w-[180px]">THỜI GIAN GIAO DỊCH</th>
                           <th>KHÁCH HÀNG / ĐƯỢC ỦY QUYỀN</th>
                           <th className="text-right w-[160px]">TỔNG THANH TOÁN (VND)</th>
                           <th className="text-center w-[100px]">P.THỨC</th>
                           <th className="text-center w-[120px]">TRẠNG THÁI</th>
                           <th className="w-[80px] text-right">ACTION</th>
                        </tr>
                     </thead>
                     <tbody>
                        {invoices.map((inv: any) => (
                        <tr key={inv.Id}>
                           <td className="font-black text-[#0078D4] tabular-nums italic">{inv.InvoiceNumber || `INV-${inv.Id}`}</td>
                           <td className="text-slate-400 font-bold italic text-[11px] uppercase">{new Date(inv.CreatedDate).toLocaleString('vi-VN')}</td>
                           <td>
                              <div className="flex flex-col">
                                 <span className="text-[13px] font-black uppercase italic leading-none mb-1 text-slate-800">{inv.CustomerName || "KHÁCH VÃNG LAI"}</span>
                                 <span className="text-[9px] font-bold text-slate-400">AUTHORIZED TRANSACTION</span>
                              </div>
                           </td>
                           <td className="text-right font-black text-slate-900 tabular-nums text-[15px]">
                              {formatCurrency(inv.FinalAmount)}
                           </td>
                           <td className="text-center">
                              <span className="px-2 py-0.5 bg-slate-50 text-slate-500 rounded-sm text-[9px] font-black uppercase border border-slate-200">{inv.PaymentMethod}</span>
                           </td>
                           <td className="text-center">
                              <span className={cn(
                              "px-2 py-0.5 rounded-sm text-[9px] font-black uppercase border",
                              inv.Status === 'Paid' ? "bg-emerald-50 text-emerald-700 border-emerald-200" : 
                              inv.Status === 'Pending' ? "bg-amber-50 text-amber-700 border-amber-200" :
                              "bg-rose-50 text-rose-700 border-rose-200"
                              )}>
                              {inv.Status === 'Paid' ? "HOÀN TẤT" : inv.Status === 'Pending' ? "CHỜ XỬ LÝ" : "ĐÃ HỦY"}
                              </span>
                           </td>
                           <td className="text-right">
                              <Link 
                                 href={`/history/${inv.Id}`}
                                 className="text-[#0078D4] hover:text-[#005A9E] transition-colors"
                              >
                                 <ChevronRight className="w-5 h-5 ml-auto" />
                              </Link>
                           </td>
                        </tr>
                        ))}
                        {invoices.length === 0 && (
                        <tr>
                           <td colSpan={7} className="py-40 text-center text-slate-200 italic uppercase font-black tracking-[0.3em] bg-slate-50/50">
                              <History className="w-12 h-12 mx-auto mb-4 opacity-10" />
                              Hệ thống không tìm thấy truy vấn phù hợp.
                           </td>
                        </tr>
                        )}
                     </tbody>
                  </table>
               </div>
               
               {/* Footer System Status */}
               <div className="p-4 bg-[#F0F0F0] border-t border-[#D1D1D1] flex items-center justify-between">
                  <div className="flex items-center gap-4">
                     <TrendingUp className="w-4 h-4 text-emerald-500" />
                     <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest italic leading-none">Security Audit Matrix Active • Records Synced: {invoices.length}</p>
                  </div>
                  <div className="flex items-center gap-2 text-slate-400 text-[10px] font-black uppercase">
                     <Printer className="w-3.5 h-3.5" /> READY FOR REPORT EXPORT
                  </div>
               </div>
            </div>
         </div>
      </div>
    </div>
  );
}
