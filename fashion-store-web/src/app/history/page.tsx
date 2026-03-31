import { query } from "@/lib/db";
import { formatCurrency, cn } from "@/lib/utils";
import { 
  History, 
  Search, 
  Filter, 
  MoreVertical, 
  Download, 
  Calendar, 
  CreditCard, 
  User, 
  ArrowRight,
  TrendingDown,
  Clock
} from "lucide-react";
import Link from "next/link";

interface PageProps {
  searchParams: {
    q?: string;
    status?: string;
  }
}

async function getInvoices(search?: string, status?: string) {
  let sql = `
    SELECT i.*, c.Name as CustomerName 
    FROM invoices i 
    LEFT JOIN customers c ON i.CustomerId = c.Id 
    WHERE 1=1
  `;
  const params: any[] = [];

  if (search) {
    sql += " AND (i.InvoiceNumber LIKE ? OR c.Name LIKE ?)";
    params.push(`%${search}%`, `%${search}%`);
  }

  if (status && status !== "all") {
    sql += " AND i.Status = ?";
    params.push(status);
  }

  sql += " ORDER BY i.CreatedDate DESC";

  return await query(sql, params);
}

export default async function HistoryPage({ searchParams }: PageProps) {
  const q = searchParams.q || "";
  const status = searchParams.status || "all";
  const invoices = await getInvoices(q, status);

  const statuses = [
    { label: "Tất cả", value: "all", color: "bg-slate-100 text-slate-500" },
    { label: "Hoàn tất", value: "Completed", color: "bg-emerald-50 text-emerald-600 border-emerald-100" },
    { label: "Chờ xử lý", value: "Pending", color: "bg-amber-50 text-amber-600 border-amber-100" },
    { label: "Đã hủy", value: "Canceled", color: "bg-rose-50 text-rose-600 border-rose-100" },
  ];

  return (
    <div className="space-y-8 animate-in fade-in duration-1000">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold tracking-tight text-slate-900 mb-1">
            Lịch Sử <span className="amber-gradient-text">Giao Dịch</span>
          </h2>
          <p className="text-slate-500">Xem và quản lý các hóa đơn bán hàng.</p>
        </div>
        <div className="flex gap-3">
          <button className="bg-white hover:bg-slate-50 text-slate-600 font-bold px-6 py-3 rounded-2xl transition-all border border-slate-200 shadow-sm flex items-center gap-2 active:scale-95">
            <Download className="w-5 h-5" />
            Xuất Báo Cáo
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        <div className="lg:col-span-1 space-y-6">
          <div className="glass-card p-6 rounded-3xl border-slate-200 sticky top-8">
            <h3 className="text-sm font-bold text-slate-900 mb-6 flex items-center gap-2 uppercase tracking-widest">
              <Filter className="w-4 h-4 text-amber-600" />
              Bộ Lọc
            </h3>

            <form action="/history" method="GET" className="space-y-6">
              <div className="space-y-3">
                <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tìm kiếm</label>
                <div className="relative group">
                  <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 group-focus-within:text-amber-600 transition-colors" />
                  <input 
                    name="q"
                    defaultValue={q}
                    type="text" 
                    placeholder="Mã HD, tên khách..."
                    className="w-full bg-slate-50 border border-slate-200 rounded-2xl py-3 pl-10 pr-4 text-sm text-slate-900 focus:outline-none focus:ring-4 focus:ring-amber-500/10 focus:border-amber-500/50 transition-all font-medium"
                  />
                </div>
              </div>

              <div className="space-y-3">
                <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Trạng thái</label>
                <select 
                  name="status"
                  defaultValue={status}
                  className="w-full bg-slate-50 border border-slate-200 rounded-2xl py-3 px-4 text-sm text-slate-900 focus:outline-none focus:ring-4 focus:ring-amber-500/10 focus:border-amber-500/50 transition-all appearance-none cursor-pointer font-medium"
                >
                  {statuses.map((s) => (
                    <option key={s.value} value={s.value}>{s.label}</option>
                  ))}
                </select>
              </div>

              <button type="submit" className="w-full bg-slate-900 hover:bg-slate-800 text-white font-bold py-3 rounded-2xl transition-all shadow-lg shadow-slate-900/10 active:scale-95 text-sm uppercase tracking-widest">
                Áp Dụng Lọc
              </button>
            </form>

            <div className="mt-8 pt-8 border-t border-slate-100">
              <div className="flex items-center justify-between mb-4">
                <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tóm Tắt</span>
                <Clock className="w-4 h-4 text-amber-500" />
              </div>
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <span className="text-xs text-slate-500">Tổng hóa đơn:</span>
                  <span className="text-sm font-black text-slate-900">{invoices.length}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-xs text-slate-500">Doanh thu đạt:</span>
                  <span className="text-sm font-black text-amber-600">
                    {formatCurrency(invoices.reduce((a: any, b: any) => a + Number(b.Total), 0))}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="lg:col-span-3">
          <div className="glass-card rounded-[2rem] overflow-hidden border-slate-200">
            <div className="overflow-x-auto">
              <table className="w-full text-left">
                <thead>
                  <tr className="bg-slate-50/50">
                    <th className="px-8 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Hóa Đơn / Ngày</th>
                    <th className="px-8 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Khách Hàng</th>
                    <th className="px-8 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] text-right">Tổng Tiền</th>
                    <th className="px-8 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] text-center">Trạng Thái</th>
                    <th className="px-8 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] text-center">Hành Động</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {invoices.map((inv: any) => (
                    <tr key={inv.Id} className="group hover:bg-slate-50/50 transition-colors">
                      <td className="px-8 py-6">
                        <div className="flex flex-col">
                          <span className="text-sm font-black text-amber-600 uppercase tracking-tighter">
                            {inv.InvoiceNumber || `#${inv.Id}`}
                          </span>
                          <span className="text-[10px] font-bold text-slate-400 mt-1 flex items-center gap-1 uppercase">
                            <Calendar className="w-2.5 h-2.5" />
                            {new Date(inv.CreatedDate).toLocaleString("vi-VN", { dateStyle: "medium" })}
                          </span>
                        </div>
                      </td>
                      <td className="px-8 py-6">
                        <div className="flex items-center gap-3">
                          <div className="p-2 bg-slate-50 rounded-xl border border-slate-100">
                            <User className="w-4 h-4 text-slate-400" />
                          </div>
                          <span className="text-sm font-black text-slate-900 group-hover:text-amber-600 transition-colors uppercase tracking-tight">
                            {inv.CustomerName || "Khách lẻ"}
                          </span>
                        </div>
                      </td>
                      <td className="px-8 py-6 text-right">
                        <div className="flex flex-col items-end">
                          <span className="text-sm font-black text-slate-900">{formatCurrency(inv.Total)}</span>
                          <span className="text-[10px] font-bold text-slate-400 mt-1 flex items-center gap-1 uppercase">
                            <CreditCard className="w-2.5 h-2.5" />
                            {inv.PaymentMethod || "Tiền mặt"}
                          </span>
                        </div>
                      </td>
                      <td className="px-8 py-6 text-center">
                        <span className={cn(
                          "px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border",
                          inv.Status === "Completed" ? "bg-emerald-50 text-emerald-600 border-emerald-100" :
                          inv.Status === "Pending" ? "bg-amber-50 text-amber-600 border-amber-100" :
                          "bg-rose-50 text-rose-600 border-rose-100"
                        )}>
                          {inv.Status === "Completed" ? "Thành Công" : inv.Status === "Pending" ? "Đang Xử Lý" : "Đã Hủy"}
                        </span>
                      </td>
                      <td className="px-8 py-6 text-center">
                        <Link href={`/history/${inv.Id}`}>
                          <button className="p-2.5 hover:bg-white text-slate-300 hover:text-amber-500 rounded-xl transition-all shadow-sm hover:shadow-md border border-transparent hover:border-slate-100 group/btn">
                            <ArrowRight className="w-4 h-4 transition-transform group-hover/btn:translate-x-1" />
                          </button>
                        </Link>
                      </td>
                    </tr>
                  ))}
                  {invoices.length === 0 && (
                    <tr>
                      <td colSpan={5} className="px-8 py-24 text-center">
                        <History className="w-20 h-20 text-slate-100 mx-auto mb-4" />
                        <p className="text-slate-400 font-bold tracking-widest uppercase text-sm">Chưa có giao dịch nào được thực hiện</p>
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
