import { query } from "@/lib/db";
export const dynamic = 'force-dynamic';
import { formatCurrency, cn } from "@/lib/utils";
import { 
  ArrowLeft, 
  Printer, 
  MapPin, 
  Calendar, 
  User, 
  Receipt,
  ChevronRight,
  Package,
  CreditCard,
  Briefcase,
  Layers,
  FileText
} from "lucide-react";
import Link from "next/link";
import PrintButton from "@/components/PrintButton";

async function getInvoiceDetails(id: string) {
  // Fetch Invoice with Customer and Employee details
  const invoices = await query(`
    SELECT 
      i.*, 
      c."Name" as "CustomerName", 
      c."Phone" as "CustomerPhone", 
      c."Address" as "CustomerAddress", 
      e."EmployeeName" as "StaffName" 
    FROM "invoices" i 
    LEFT JOIN "customers" c ON i."CustomerId" = c."Id" 
    LEFT JOIN "accounts" e ON i."EmployeeId" = e."Id" 
    WHERE i."Id" = $1
  `, [id]);

  if (!invoices || (invoices as any[]).length === 0) return null;

  const items = await query(`
    SELECT 
      ii.*,
      p."Name" as "ProductName",
      p."Code" as "ProductCode"
    FROM "invoiceitems" ii
    LEFT JOIN "products" p ON ii."ProductId" = p."Id"
    WHERE ii."InvoiceId" = $1
  `, [id]);

  const settingsArray = await query('SELECT "Key", "Value" FROM "settings"');
  const settings = settingsArray.reduce((acc: any, curr: any) => {
    acc[curr.Key] = curr.Value;
    return acc;
  }, {});

  return {
    invoice: (invoices as any[])[0],
    items: items as any[],
    settings
  };
}

export default async function InvoiceDetailsPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const data = await getInvoiceDetails(id);

  if (!data) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] gap-6 animate-in fade-in duration-700 no-select">
        <div className="w-20 h-20 bg-rose-50 border border-rose-100 rounded-sm flex items-center justify-center text-rose-500 shadow-inner">
           <Receipt className="w-10 h-10" />
        </div>
        <p className="text-[12px] font-black text-rose-400 uppercase tracking-[0.3em] font-bold">Lỗi: Hóa đơn không tồn tại (ID: {id})</p>
        <Link href="/history" className="btn-wpf px-10 h-10 flex items-center">
           <ArrowLeft className="w-4 h-4 mr-2" /> QUAY LẠI TRUY VẤN
        </Link>
      </div>
    );
  }

  const { invoice, items, settings } = data;

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-20 no-select">
      {/* Visual Header / Breadcrumb - No Print */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-8 no-print">
        <div className="flex items-center gap-6">
          <Link href="/history" className="btn-wpf h-10 w-10 flex items-center justify-center group">
            <ArrowLeft className="w-5 h-5 text-slate-400 group-hover:text-[#0078D4]" />
          </Link>
          <div>
            <h2 className="text-[20px] font-black tracking-tight text-slate-900 uppercase italic leading-none">
              CHI TIẾT GIAO DỊCH <span className="text-[#0078D4]">{invoice.InvoiceNumber || `#${invoice.Id}`}</span>
            </h2>
            <div className="flex items-center gap-2 mt-2">
               <div className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
               <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest italic">Fusion ERP Management Shell • Transaction Verified</p>
            </div>
          </div>
        </div>
        <div className="no-print">
           <PrintButton />
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 no-print">
        {/* Main Content Area: DataGrid of Items */}
        <div className="lg:col-span-8 flex flex-col gap-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex justify-between items-center bg-[#F0F0F0] px-4 py-2 border-b border-[#D1D1D1]">
                 <span>BẢNG KÊ CHI TIẾT SẢN PHẨM</span>
                 <Layers className="w-3.5 h-3.5 text-slate-400" />
              </div>
              <div className="overflow-x-auto">
                 <table className="wpf-datagrid">
                    <thead>
                       <tr>
                          <th>TÊN SẢN PHẨM / DỊCH VỤ</th>
                          <th className="text-center w-[80px]">SL</th>
                          <th className="text-right w-[150px]">ĐƠN GIÁ</th>
                          <th className="text-right w-[180px]">THÀNH TIỀN</th>
                       </tr>
                    </thead>
                    <tbody>
                       {items.map((item: any) => (
                         <tr key={item.Id}>
                            <td className="font-bold uppercase tracking-tight">
                               <div className="flex flex-col">
                                  <span>{item.ProductName}</span>
                                  <span className="text-[10px] font-medium text-slate-400 italic">MÃ: {item.ProductCode}</span>
                               </div>
                            </td>
                            <td className="text-center font-bold text-slate-900 tabular-nums">{item.Quantity}</td>
                            <td className="text-right text-slate-500 tabular-nums">{formatCurrency(item.UnitPrice)}</td>
                            <td className="text-right font-black text-[#0078D4] tabular-nums">{formatCurrency(item.LineTotal)}</td>
                         </tr>
                       ))}
                    </tbody>
                 </table>
              </div>
              
              {/* Summary Calculation Panel */}
              <div className="p-8 bg-[#F9F9F9] border-t border-[#D1D1D1] grid grid-cols-1 md:grid-cols-12 gap-8">
                 <div className="md:col-span-6">
                    <div className="flex items-center gap-2 mb-3">
                       <FileText className="w-4 h-4 text-slate-400" />
                       <span className="text-[11px] font-black uppercase text-slate-400">Ghi chú & Pháp lý:</span>
                    </div>
                    <div className="bg-white border border-[#D1D1D1] p-4 text-[11px] text-slate-500 leading-relaxed italic border-l-4 border-l-[#0078D4]">
                       Hóa đơn điện tử được xác thực bởi Fusion Store. Quý khách vui lòng kiểm tra kỹ sản phẩm trước khi rời quầy. Chính sách đổi trả áp dụng trong 7 ngày kể từ ngày lập hóa đơn.
                    </div>
                 </div>
                 <div className="md:col-span-6 space-y-2">
                    <div className="flex justify-between items-center text-[11px] font-bold text-slate-400">
                       <span>TỔNG TIỀN HÀNG (ORIGINAL):</span>
                       <span className="text-slate-800">{formatCurrency(invoice.TotalAmount)}</span>
                    </div>
                    <div className="flex justify-between items-center text-[11px] font-bold text-emerald-600">
                       <span>THUẾ GIÁ TRỊ GIA TĂNG (8%):</span>
                       <span className="tabular-nums">+{formatCurrency(invoice.TaxAmount)}</span>
                    </div>
                    {Number(invoice.DiscountAmount) > 0 && (
                       <div className="flex justify-between items-center text-[11px] font-bold text-rose-500">
                          <span>CHIẾT KHẤU / GIẢM GIÁ:</span>
                          <span className="tabular-nums">-{formatCurrency(invoice.DiscountAmount)}</span>
                       </div>
                    )}
                    <div className="h-[1px] bg-[#EAEAEA] my-4" />
                    <div className="flex justify-between items-end">
                       <span className="text-[13px] font-black text-slate-900 uppercase italic">TỔNG THANH TOÁN:</span>
                       <span className="text-[32px] font-black text-[#0078D4] leading-none tracking-tighter italic tabular-nums">{formatCurrency(invoice.FinalAmount)}</span>
                    </div>
                 </div>
              </div>
           </div>
        </div>

        {/* Sidebar Column: Customer & Staff Info Panels */}
        <div className="lg:col-span-4 space-y-6">
           <div className="wpf-panel shadow-sm">
              <div className="wpf-panel-header bg-[#F0F0F0] px-4 py-2 border-b border-[#D1D1D1]">THÔNG TIN ĐỐI TÁC KHÁCH HÀNG</div>
              <div className="p-6 space-y-4 bg-white">
                 <div className="flex items-center gap-4">
                    <div className="w-12 h-12 bg-white border border-[#D1D1D1] rounded-sm flex items-center justify-center text-[#0078D4] shadow-sm">
                       <User className="w-6 h-6" />
                    </div>
                    <div>
                       <p className="text-[15px] font-black text-slate-900 uppercase italic tracking-tighter leading-none mb-1">{invoice.CustomerName || "KHÁCH VÃNG LAI"}</p>
                       <p className="text-[10px] font-bold text-slate-500 flex items-center gap-2 uppercase">
                          <CreditCard className="w-3.5 h-3.5" /> ID: {invoice.CustomerId || "0000"} • VERIFIED
                       </p>
                    </div>
                 </div>
                 <div className="space-y-3 pt-4 border-t border-[#F0F0F0]">
                    <div className="flex items-start gap-4">
                       <MapPin className="w-3.5 h-3.5 text-slate-300 mt-0.5" />
                       <p className="text-[11px] font-medium text-slate-500">{invoice.CustomerAddress || "Địa chỉ không được ghi nhận trong phiên giao dịch."}</p>
                    </div>
                    <div className="flex items-center gap-4">
                       <Briefcase className="w-3.5 h-3.5 text-slate-300" />
                       <p className="text-[11px] font-bold text-[#0078D4] uppercase italic">Phương thức: {invoice.PaymentMethod}</p>
                    </div>
                 </div>
              </div>
           </div>

           <div className="wpf-panel shadow-sm bg-slate-800 text-white border-slate-700">
              <div className="wpf-panel-header bg-slate-900 px-4 py-2 border-b border-slate-700 text-white/50">NHÂN VIÊN XỬ LÝ (STAFF-ID)</div>
              <div className="p-6 flex items-center gap-4">
                 <div className="w-12 h-12 bg-white/5 border border-white/10 rounded-sm flex items-center justify-center text-white/40">
                    <User className="w-6 h-6" />
                 </div>
                 <div>
                    <h5 className="text-[14px] font-black uppercase italic tracking-tight mb-1">{invoice.StaffName || "AUTOMATED SYSTEM"}</h5>
                    <p className="text-[9px] font-bold text-white/40 uppercase tracking-[0.2em] italic">Electronic Signature Secured</p>
                 </div>
              </div>
           </div>
        </div>
      </div>

      {/* Actual Print Receipt (Original layout preserved for thermal printing but with WPF-compatible typography) */}
      <div className="p-8 hidden print:block bg-white text-black font-serif" style={{ width: '80mm', margin: '0 auto' }}>
        <div className="text-center space-y-2 border-b-2 border-dashed border-black pb-4 mb-4">
           <h1 className="text-2xl font-black">{settings.StoreName || "FASHION STORE"}</h1>
           <p className="text-[11px] font-bold">{settings.StoreAddress || "123 Đường Fashion, Q.1, TP. HCM"}</p>
           <p className="text-[11px] font-bold">Hotline: {settings.StorePhone || "0123.456.789"}</p>
           <div className="h-4" />
           <p className="text-xl font-bold uppercase underline decoration-2 underline-offset-4">HÓA ĐƠN BÁN HÀNG</p>
           <p className="text-[10px] font-bold mt-1 uppercase italic tracking-widest">HĐ: {invoice.InvoiceNumber || invoice.Id}</p>
           <p className="text-[9px] font-medium opacity-50 lowercase">{new Date(invoice.CreatedDate).toLocaleString('vi-VN')}</p>
        </div>

        <div className="space-y-1 mb-4 text-[11px] font-bold">
           <p className="flex justify-between"><span>KHÁCH HÀNG:</span> <span>{(invoice.CustomerName || "KHÁCH LẺ").toUpperCase()}</span></p>
           <p className="flex justify-between"><span>SĐT:</span> <span>{invoice.CustomerPhone || "N/A"}</span></p>
           <p className="flex justify-between"><span>THU NGÂN:</span> <span>{(invoice.StaffName || "SYSTEM").toUpperCase()}</span></p>
        </div>

        <div className="border-b-2 border-dashed border-black mb-4 pb-2">
           <div className="flex font-black text-[12px] mb-2 uppercase border-b border-black/10 pb-1">
              <span className="w-[50%] text-left">SẢN PHẨM</span>
              <span className="w-[15%] text-center">SL</span>
              <span className="w-[35%] text-right">TỔNG</span>
           </div>
           {items.map((item: any) => (
              <div key={item.Id} className="flex text-[11px] font-bold mb-2 items-start">
                 <span className="w-[50%] text-left leading-tight break-words uppercase">{item.ProductName}</span>
                 <span className="w-[15%] text-center">{item.Quantity}</span>
                 <span className="w-[35%] text-right">{formatCurrency(item.LineTotal)}</span>
              </div>
           ))}
        </div>

        <div className="space-y-2 pb-4 mb-4 text-[12px]">
           <div className="flex justify-between font-bold">
              <span>TỔNG TIỀN:</span>
              <span>{formatCurrency(invoice.TotalAmount)}</span>
           </div>
           <div className="flex justify-between font-bold">
              <span>THUẾ VAT (8%):</span>
              <span>{formatCurrency(invoice.TaxAmount)}</span>
           </div>
           {Number(invoice.DiscountAmount) > 0 && (
              <div className="flex justify-between font-bold">
                 <span>GIẢM GIÁ:</span>
                 <span>-{formatCurrency(invoice.DiscountAmount)}</span>
              </div>
           )}
           <div className="flex justify-between font-black text-xl pt-2 mt-2 border-t border-black">
              <span>THANH TOÁN:</span>
              <span>{formatCurrency(invoice.FinalAmount)}</span>
           </div>
        </div>

        <div className="text-center space-y-6 pt-4">
           <p className="text-[11px] font-bold italic">CẢM ƠN QUÝ KHÁCH & HẸN GẶP LẠI!</p>
           <div className="flex flex-col items-center gap-2">
              <img 
                src={`https://img.vietqr.io/image/${settings.BankId}-${settings.AccountNumber}-compact.png?amount=${Math.round(invoice.FinalAmount)}&addInfo=${invoice.InvoiceNumber}&accountName=${encodeURIComponent(settings.AccountName)}`}
                alt="Payment QR"
                className="w-32 h-32 object-contain shadow-sm border border-slate-100"
              />
              <p className="text-[8px] font-medium italic">VietQR Consolidated Standard</p>
           </div>
           <p className="text-[8px] font-medium opacity-50">Powered by Fusion System v2.5 (WPF Core)</p>
        </div>
      </div>
    </div>
  );
}
