"use client";

import { useEffect, useState } from "react";
import { query } from "@/lib/db";
import { formatCurrency, cn } from "@/lib/utils";
import { 
  ArrowLeft, 
  Printer, 
  Mail, 
  Phone, 
  MapPin, 
  Calendar, 
  User, 
  ShieldCheck, 
  Tag, 
  CreditCard,
  Package,
  Receipt
} from "lucide-react";
import Link from "next/link";

async function getInvoiceDetails(id: string) {
  const invoices = await query(`
    SELECT i.*, c.Name as CustomerName, c.Phone as CustomerPhone, c.Address as CustomerAddress, e.employeeName as StaffName 
    FROM invoices i 
    LEFT JOIN customers c ON i.CustomerId = c.Id 
    LEFT JOIN accounts e ON i.EmployeeId = e.Id 
    WHERE i.Id = ?
  `, [id]);

  if (!invoices || (invoices as any[]).length === 0) return null;

  const items = await query(`
    SELECT ii.* 
    FROM invoiceitems ii 
    WHERE ii.InvoiceId = ?
  `, [id]);

  return {
    invoice: (invoices as any[])[0],
    items: items as any[]
  };
}

export default function InvoiceDetailsPage({ params }: { params: { id: string } }) {
  const [data, setData] = useState<{invoice: any, items: any[]} | null>(null);

  useEffect(() => {
    getInvoiceDetails(params.id).then(setData);
  }, [params.id]);

  if (!data) {
    return <div className="p-20 text-center animate-pulse uppercase font-black text-slate-300 tracking-widest">Đang tải dữ liệu...</div>;
  }

  const { invoice, items } = data;

  return (
    <div className="space-y-8 animate-in fade-in duration-1000 pb-20">
      {/* Web Header (No Print) */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 no-print">
        <div className="flex items-center gap-4">
          <Link href="/history" className="p-3 bg-white hover:bg-slate-50 border border-slate-200 rounded-2xl transition-all shadow-sm active:scale-95">
            <ArrowLeft className="w-5 h-5 text-slate-600" />
          </Link>
          <div>
            <h2 className="text-3xl font-bold tracking-tight text-slate-900 mb-1">
              Chi Tiết <span className="amber-gradient-text uppercase">{invoice.InvoiceNumber || `#${invoice.Id}`}</span>
            </h2>
            <p className="text-slate-500">Xem lại chi tiết giao dịch và in hóa đơn.</p>
          </div>
        </div>
        <button 
          onClick={() => window.print()}
          className="bg-slate-900 hover:bg-slate-800 text-white font-bold px-8 py-3 rounded-2xl transition-all shadow-lg active:scale-95 flex items-center gap-2"
        >
          <Printer className="w-5 h-5" />
          In Hóa Đơn
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Main Content (Web View) */}
        <div className="lg:col-span-2 space-y-6 no-print">
           <div className="glass-card p-10 rounded-[2.5rem] border-slate-200 bg-white/50">
              <div className="flex justify-between items-start mb-10 pb-6 border-b border-slate-100">
                 <div className="flex items-center gap-4">
                    <div className="p-4 bg-amber-500 rounded-2xl text-white shadow-lg shadow-amber-500/20">
                       <Receipt className="w-8 h-8" />
                    </div>
                    <div>
                       <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Mã hóa đơn</p>
                       <h3 className="text-2xl font-black text-slate-900 uppercase">{invoice.InvoiceNumber || `#${invoice.Id}`}</h3>
                    </div>
                 </div>
                 <div className="text-right">
                    <span className={cn(
                       "px-4 py-1.5 rounded-full text-[10px] font-black uppercase tracking-[0.2em] border",
                       invoice.Status === "Completed" ? "bg-emerald-50 text-emerald-600 border-emerald-100" : "bg-rose-50 text-rose-600 border-rose-100"
                    )}>
                       {invoice.Status === "Completed" ? "Thành Công" : "Đã Hủy"}
                    </span>
                    <p className="mt-3 text-[10px] font-bold text-slate-400 uppercase flex items-center justify-end gap-1.5">
                       <Calendar className="w-3 h-3" />
                       {new Date(invoice.CreatedDate).toLocaleString('vi-VN', { dateStyle: 'long', timeStyle: 'short' })}
                    </p>
                 </div>
              </div>

              <div className="space-y-6">
                 <table className="w-full text-left">
                    <thead>
                       <tr className="bg-slate-50/50">
                          <th className="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">Sản Phẩm</th>
                          <th className="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest text-center">SL</th>
                          <th className="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest text-right">Đơn Giá</th>
                          <th className="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest text-right">Thành Tiền</th>
                       </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                       {items.map((item: any) => (
                         <tr key={item.Id} className="group">
                            <td className="px-6 py-5">
                               <p className="text-sm font-black text-slate-900 uppercase tracking-tight">{item.ProductName}</p>
                               <p className="text-[10px] font-bold text-slate-400 mt-0.5">{item.ProductCode}</p>
                            </td>
                            <td className="px-6 py-5 text-center">
                               <span className="text-sm font-black text-slate-900">{item.Quantity}</span>
                            </td>
                            <td className="px-6 py-5 text-right font-medium text-slate-500">
                               {formatCurrency(item.UnitPrice)}
                            </td>
                            <td className="px-6 py-5 text-right font-black text-slate-900">
                               {formatCurrency(item.LineTotal)}
                            </td>
                         </tr>
                       ))}
                    </tbody>
                 </table>
              </div>

              <div className="mt-12 pt-10 border-t border-slate-100 grid grid-cols-1 md:grid-cols-2 gap-10">
                 <div className="space-y-4">
                    <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Ghi chú giao dịch</p>
                    <p className="text-sm text-slate-500 leading-relaxed italic">
                       {invoice.Notes || "Không có ghi chú nào đi kèm với hóa đơn này."}
                    </p>
                 </div>
                 <div className="space-y-3">
                    <div className="flex justify-between items-center text-sm">
                       <span className="font-bold text-slate-400 uppercase tracking-widest text-[10px]">Tạm tính</span>
                       <span className="font-black text-slate-900">{formatCurrency(invoice.Subtotal)}</span>
                    </div>
                    <div className="flex justify-between items-center text-sm">
                       <span className="font-bold text-slate-400 uppercase tracking-widest text-[10px]">Thuế (0%)</span>
                       <span className="font-black text-slate-900">{formatCurrency(invoice.TaxAmount)}</span>
                    </div>
                    {Number(invoice.DiscountAmount) > 0 && (
                       <div className="flex justify-between items-center text-sm text-rose-500">
                          <span className="font-bold uppercase tracking-widest text-[10px]">Giảm giá</span>
                          <span className="font-black">-{formatCurrency(invoice.DiscountAmount)}</span>
                       </div>
                    )}
                    <div className="pt-4 border-t border-slate-100 flex justify-between items-center">
                       <span className="font-black text-slate-900 uppercase tracking-widest text-sm">Tổng thanh toán</span>
                       <span className="text-3xl font-black text-amber-600 tracking-tighter">{formatCurrency(invoice.Total)}</span>
                    </div>
                 </div>
              </div>
           </div>
        </div>

        {/* Sidebar Info (Web View) */}
        <div className="space-y-8 no-print">
           <div className="glass-card p-8 rounded-[2.5rem] border-slate-200">
              <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest flex items-center gap-2 mb-8">
                 <User className="w-4 h-4 text-amber-500" /> Khách hàng
              </h3>
              <div className="space-y-6">
                 <div className="flex items-center gap-4">
                    <div className="w-12 h-12 bg-slate-50 rounded-2xl flex items-center justify-center border border-slate-100">
                       <ShieldCheck className="w-6 h-6 text-slate-400" />
                    </div>
                    <div>
                       <p className="text-xs font-black text-slate-900 uppercase tracking-tight">{invoice.CustomerName || "Khách lẻ"}</p>
                       <p className="text-[10px] font-bold text-slate-400">{invoice.CustomerPhone || "Chưa có SĐT"}</p>
                    </div>
                 </div>
                 <div className="space-y-3 pt-4 border-t border-slate-50">
                    <div className="flex items-center gap-3 text-[10px] font-bold text-slate-500 uppercase tracking-widest">
                       <MapPin className="w-3.5 h-3.5 text-slate-300" />
                       <span className="truncate">{invoice.CustomerAddress || "N/A"}</span>
                    </div>
                    <div className="flex items-center gap-3 text-[10px] font-bold text-slate-500 uppercase tracking-widest">
                       <CreditCard className="w-3.5 h-3.5 text-slate-300" />
                       Hàng đã thanh toán via {invoice.PaymentMethod}
                    </div>
                 </div>
              </div>
           </div>

           <div className="glass-card p-8 rounded-[2.5rem] bg-slate-900 text-white relative overflow-hidden">
              <div className="absolute top-0 right-0 w-32 h-32 bg-amber-500/10 blur-3xl rounded-full -mr-16 -mt-16" />
              <h3 className="text-sm font-black uppercase tracking-widest flex items-center gap-2 mb-8 relative z-10">
                 <ShieldCheck className="w-4 h-4 text-amber-500" /> Nhân viên phụ trách
              </h3>
              <div className="flex items-center gap-4 relative z-10">
                 <div className="w-12 h-12 bg-white/5 rounded-2xl border border-white/5 flex items-center justify-center">
                    <User className="w-6 h-6 text-slate-400" />
                 </div>
                 <div>
                    <p className="text-sm font-black uppercase tracking-tight">{invoice.StaffName || "Hệ thống"}</p>
                    <p className="text-[10px] font-bold text-slate-500 uppercase">Nhân viên bán hàng</p>
                 </div>
              </div>
           </div>
        </div>
      </div>

      {/* Actual Print Receipt (Print Only) */}
      <div className="hidden print:block print-only">
         <div className="receipt-container p-4 border-2 border-slate-900 mx-auto bg-white">
            <div className="text-center space-y-2 border-b-2 border-dashed border-slate-900 pb-4 mb-4">
               <h1 className="text-2xl font-black uppercase tracking-tighter">FASHION STORE</h1>
               <p className="text-[12px] font-bold">123 Đường Fashion, Q.1, TP. HCM</p>
               <p className="text-[12px] font-bold">Hotline: 0123.456.789</p>
               <p className="text-xl font-black mt-4 uppercase underline decoration-2 underline-offset-4">HÓA ĐƠN BÁN HÀNG</p>
               <p className="text-[10px] font-bold mt-1">SỐ: {invoice.InvoiceNumber || invoice.Id}</p>
            </div>

            <div className="space-y-1 mb-6 text-[11px] font-bold">
               <p className="flex justify-between"><span>KHÁCH HÀNG:</span> <span>{invoice.CustomerName?.toUpperCase() || "KHÁCH LẺ"}</span></p>
               <p className="flex justify-between"><span>SĐT:</span> <span>{invoice.CustomerPhone || "N/A"}</span></p>
               <p className="flex justify-between"><span>NGÀY:</span> <span>{new Date(invoice.CreatedDate).toLocaleDateString('vi-VN')}</span></p>
               <p className="flex justify-between"><span>GIỜ:</span> <span>{new Date(invoice.CreatedDate).toLocaleTimeString('vi-VN')}</span></p>
               <p className="flex justify-between"><span>THU NGÂN:</span> <span>{invoice.StaffName?.toUpperCase() || "SYSTEM"}</span></p>
            </div>

            <div className="border-b-2 border-dashed border-slate-900 mb-4 pb-2">
               <div className="flex font-black text-[12px] mb-2 uppercase">
                  <span className="w-[45%] text-left">SẢN PHẨM</span>
                  <span className="w-[15%] text-center">SL</span>
                  <span className="w-[40%] text-right">TỔNG</span>
               </div>
               {items.map((item: any) => (
                  <div key={item.Id} className="flex text-[11px] font-bold mb-2">
                     <span className="w-[45%] text-left leading-tight truncate">{item.ProductName.toUpperCase()}</span>
                     <span className="w-[15%] text-center">{item.Quantity}</span>
                     <span className="w-[40%] text-right">{formatCurrency(item.LineTotal)}</span>
                  </div>
               ))}
            </div>

            <div className="space-y-2 border-b-2 border-dashed border-slate-900 pb-4 mb-4 text-[12px]">
               <div className="flex justify-between font-bold">
                  <span>TỔNG TIỀN:</span>
                  <span>{formatCurrency(invoice.Subtotal)}</span>
               </div>
               {Number(invoice.DiscountAmount) > 0 && (
                  <div className="flex justify-between font-bold">
                     <span>GIẢM GIÁ:</span>
                     <span>-{formatCurrency(invoice.DiscountAmount)}</span>
                  </div>
               )}
               <div className="flex justify-between font-black text-xl pt-2 mt-2 border-t border-slate-900">
                  <span>THANH TOÁN:</span>
                  <span>{formatCurrency(invoice.Total)}</span>
               </div>
            </div>

            <div className="text-center space-y-4">
               <p className="text-[11px] font-bold italic">CẢM ƠN QUÝ KHÁCH & HẸN GẶP LẠI!</p>
               <div className="w-32 h-32 border-2 border-slate-900 mx-auto flex items-center justify-center font-black opacity-20">
                  QR CODE
               </div>
               <p className="text-[9px] font-medium opacity-50">Powered by Fashion Store Management System</p>
            </div>
         </div>
      </div>
    </div>
  );
}
