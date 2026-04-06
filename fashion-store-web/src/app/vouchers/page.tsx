"use client";

import { useState, useEffect, useMemo } from "react";
import { cn, formatCurrency } from "@/lib/utils";
import { 
  Ticket, Search, Plus, Trash2, Calendar, CheckCircle2,BarChart3, AlertCircle, Tag, DollarSign, Percent, Clock, ChevronRight, ArrowRight, Zap, Info, FileText, Database, ShieldCheck, History
} from "lucide-react";

export default function VouchersPage() {
  const [vouchers, setVouchers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");

  const [editingVoucher, setEditingVoucher] = useState<any>({
    Id: 0,
    Code: "",
    Value: 0,
    DiscountType: "%",
    MinInvoiceAmount: 0,
    MaxDiscountAmount: 0,
    StartDate: new Date().toISOString().split('T')[0],
    EndDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    UsageLimit: 100,
    UsedCount: 0,
    IsActive: true
  });

  useEffect(() => {
    refreshData();
  }, []);

  const refreshData = async () => {
    setLoading(true);
    try {
      const res = await fetch("/api/vouchers");
      const json = await res.json();
      if (json.success && Array.isArray(json.data)) {
        setVouchers(json.data);
      } else {
        console.error("API Error or invalid data:", json.error);
      }
    } catch (e) {
      console.error("Failed to load vouchers", e);
    } finally {
      setLoading(false);
    }
  };

  const filteredVouchers = useMemo(() => {
    return vouchers.filter(v => v.Code.toLowerCase().includes(searchTerm.toLowerCase()));
  }, [vouchers, searchTerm]);

  const handleSelect = (v: any) => {
    setEditingVoucher({
      ...v,
      StartDate: new Date(v.StartDate).toISOString().split('T')[0],
      EndDate: new Date(v.EndDate).toISOString().split('T')[0]
    });
  };

  const handleClear = () => {
    setEditingVoucher({
      Id: 0,
      Code: "",
      Value: 0,
      DiscountType: "%",
      MinInvoiceAmount: 0,
      MaxDiscountAmount: 0,
      StartDate: new Date().toISOString().split('T')[0],
      EndDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
      UsageLimit: 100,
      UsedCount: 0,
      IsActive: true
    });
  };

  const handleSave = async () => {
    if (!editingVoucher.Code) return alert("Vui lòng nhập mã voucher");
    const isNew = editingVoucher.Id === 0;
    try {
      const res = await fetch("/api/vouchers", {
        method: isNew ? "POST" : "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(editingVoucher)
      });
      const json = await res.json();
      if (json.success) {
        handleClear();
        refreshData();
      } else {
        alert("❌ Lỗi: " + (json.error || "Không thể lưu voucher"));
      }
    } catch (e) {
      alert("❌ Lỗi kết nối");
    }
  };

  const handleDelete = async () => {
    if (editingVoucher.Id === 0) return;
    if (!confirm("Xác nhận gỡ bỏ mã giảm giá này khỏi hệ thống?")) return;
    try {
      const res = await fetch(`/api/vouchers?id=${editingVoucher.Id}`, { method: "DELETE" });
      const json = await res.json();
      if (json.success) {
        handleClear();
        refreshData();
      } else {
        alert("❌ Lỗi: " + json.error);
      }
    } catch (e) {
      alert("❌ Lỗi kết nối");
    }
  };

  if (loading) return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] gap-4 no-select uppercase italic font-black text-slate-400">
      <div className="w-12 h-12 border-4 border-[#0078D4] border-t-transparent rounded-full animate-spin" />
      <p className="text-[11px] tracking-widest">Đang tải cơ sở dữ liệu Marketing...</p>
    </div>
  );

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-20 no-select">
      {/* WPF Header / Ribbon */}
      <div className="wpf-panel">
         <div className="wpf-panel-header uppercase">QUẢN TRỊ CHIẾN DỊCH KHUYẾN MÃI & MÃ GIẢM GIÁ (VOUCHER REGISTRY)</div>
         <div className="p-6 bg-white flex flex-col md:flex-row md:items-center justify-between gap-6">
            <div className="flex items-center gap-4">
               <div className="w-10 h-10 bg-[#0078D4] rounded-sm flex items-center justify-center text-white shadow-md">
                  <Ticket className="w-6 h-6" />
               </div>
               <div>
                  <h2 className="text-[20px] font-black text-slate-900 tracking-tight uppercase italic leading-none">QUẢN LÝ VOUCHER</h2>
                  <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-widest italic tracking-tighter">Fusion ERP Marketing Service v2.5</p>
               </div>
            </div>
            
            <div className="flex items-center gap-4 bg-emerald-50 border border-emerald-200 px-6 py-2 shadow-sm rounded-sm">
               <div className="flex items-center gap-3">
                  <Zap className="w-4 h-4 text-emerald-600 fill-emerald-600 animate-pulse" />
                  <span className="text-[11px] font-black uppercase text-emerald-700 tracking-widest tabular-nums">{vouchers.filter(v => v.IsActive).length} VOUCHERS ACTIVE</span>
               </div>
            </div>
         </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        {/* Left Column: Editor */}
        <div className="lg:col-span-4 space-y-6 lg:sticky lg:top-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex items-center justify-between">
                 <span className="flex items-center gap-2">
                    <Tag className="w-4 h-4 text-[#0078D4]" /> CHI TIẾT ƯU ĐÃI
                 </span>
                 <span className={cn(
                    "text-[9px] font-black px-2 py-0.5 rounded-sm border",
                    editingVoucher.Id === 0 ? "bg-amber-50 text-amber-600 border-amber-200" : "bg-blue-50 text-blue-600 border-blue-200"
                 )}>
                    {editingVoucher.Id === 0 ? "NEW VOUCHER" : `ID: VCH-${editingVoucher.Id}`}
                 </span>
              </div>
              <div className="p-6 bg-white space-y-5">
                 <div className="wpf-groupbox !mt-0">
                    <span className="wpf-groupbox-label">Thông tin định danh</span>
                    <div className="space-y-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Mã định danh (Voucher Code):</label>
                          <input 
                            type="text" 
                            required
                            value={editingVoucher.Code}
                            onChange={(e) => setEditingVoucher({...editingVoucher, Code: e.target.value.toUpperCase()})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm uppercase tracking-widest italic"
                            placeholder="NHẬP MÃ GIẢM GIÁ..." 
                          />
                       </div>
                       <div className="grid grid-cols-2 gap-4">
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Loại chiết khấu:</label>
                             <select 
                               value={editingVoucher.DiscountType}
                               onChange={(e) => setEditingVoucher({...editingVoucher, DiscountType: e.target.value})}
                               className="w-full h-10 border border-[#D1D1D1] px-3 text-[12px] font-black bg-white focus:border-[#0078D4] outline-none rounded-sm cursor-pointer"
                             >
                                <option value="%">% (Tỉ lệ)</option>
                                <option value="VND">VND (Tiền mặt)</option>
                             </select>
                          </div>
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Giá trị giảm:</label>
                             <input 
                               type="number" 
                               value={editingVoucher.Value}
                               onChange={(e) => setEditingVoucher({...editingVoucher, Value: Number(e.target.value)})}
                               className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm tabular-nums text-[#0078D4]"
                             />
                          </div>
                       </div>
                    </div>
                 </div>

                 <div className="wpf-groupbox">
                    <span className="wpf-groupbox-label">Điều kiện & Hiệu lực</span>
                    <div className="space-y-4 pt-2">
                       <div className="grid grid-cols-2 gap-4">
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Bắt đầu:</label>
                             <input 
                               type="date" 
                               value={editingVoucher.StartDate}
                               onChange={(e) => setEditingVoucher({...editingVoucher, StartDate: e.target.value})}
                               className="w-full h-10 border border-[#D1D1D1] px-4 text-[12px] font-bold bg-[#F9F9F9] rounded-sm"
                             />
                          </div>
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Kết thúc:</label>
                             <input 
                               type="date" 
                               value={editingVoucher.EndDate}
                               onChange={(e) => setEditingVoucher({...editingVoucher, EndDate: e.target.value})}
                               className="w-full h-10 border border-[#D1D1D1] px-4 text-[12px] font-bold bg-[#F9F9F9] rounded-sm"
                             />
                          </div>
                       </div>
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Đơn tối thiểu (Min Amount):</label>
                          <input 
                            type="number" 
                            value={editingVoucher.MinInvoiceAmount}
                            onChange={(e) => setEditingVoucher({...editingVoucher, MinInvoiceAmount: Number(e.target.value)})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] tabular-nums"
                          />
                       </div>
                    </div>
                 </div>

                 <div className="flex flex-col gap-3 pt-4">
                    <button 
                      onClick={handleSave}
                      className="btn-wpf btn-wpf-primary h-12 flex items-center justify-center gap-3 uppercase font-black text-[11px] border-b-4 border-[#005A9E]"
                    >
                       <ShieldCheck className="w-5 h-5" />
                       {editingVoucher.Id === 0 ? "KÍCH HOẠT VOUCHER" : "CẬP NHẬT CHIẾN DỊCH"}
                    </button>
                    
                    <div className="grid grid-cols-2 gap-3">
                       {editingVoucher.Id !== 0 && (
                         <button 
                           onClick={handleDelete}
                           className="btn-wpf h-10 text-rose-600 border-rose-200 hover:bg-rose-50 flex items-center justify-center gap-2 text-[10px] uppercase font-black"
                         >
                            <Trash2 className="w-3.5 h-3.5" /> GỠ BỎ
                         </button>
                       )}
                       <button 
                         onClick={handleClear}
                         className={cn(
                           "btn-wpf h-10 text-slate-500 border-slate-200 hover:bg-slate-50 flex items-center justify-center gap-2 text-[10px] uppercase font-black",
                           editingVoucher.Id === 0 ? "col-span-2" : "col-span-1"
                         )}
                       >
                          <RotateCcw className="w-3.5 h-3.5" /> {editingVoucher.Id === 0 ? "XÓA TRẮNG FORM" : "HỦY BỎ"}
                       </button>
                    </div>
                 </div>
              </div>
              <div className="p-4 bg-[#FFFFE1] border-t border-[#D1D1D1] flex items-start gap-3">
                 <Info className="w-4 h-4 text-amber-600 shrink-0 mt-0.5" />
                 <p className="text-[10px] font-bold text-amber-800 italic uppercase leading-relaxed">Hệ thống Loyalty sẽ kiểm tra hạn dùng & điều kiện đơn hàng tại POS.</p>
              </div>
           </div>
        </div>

        {/* Right Column: Registry List */}
        <div className="lg:col-span-8 space-y-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex flex-col md:flex-row md:items-center justify-between gap-4 py-3 h-auto">
                 <span className="flex items-center gap-2">
                    <FileText className="w-4 h-4" /> VOUCHER REGISTRY DATABASE
                 </span>
                 
                 <div className="relative group">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
                    <input 
                      type="text" 
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="h-8 border border-[#D1D1D1] pl-8 pr-4 text-[10px] font-bold text-slate-600 focus:border-[#0078D4] outline-none rounded-sm bg-[#F9F9F9] focus:bg-white w-[220px] uppercase"
                      placeholder="TRA CỨU MÃ VOUCHER..." 
                    />
                 </div>
              </div>
              
              <div className="overflow-x-auto">
                <table className="wpf-datagrid">
                  <thead>
                    <tr>
                      <th className="w-[140px]">MÃ VOUCHER</th>
                      <th className="text-right w-[120px]">GIÁ TRỊ GIẢM</th>
                      <th className="text-right w-[140px]">ĐƠN TỐI THIỂU</th>
                      <th className="text-center w-[120px]">Tỉ LỆ DÙNG</th>
                      <th className="text-center w-[120px]">HẾT HẠN</th>
                      <th className="text-center w-[100px]">STATUS</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredVouchers.map((v: any) => {
                      const isExpired = new Date(v.EndDate) < new Date();
                      return (
                        <tr 
                          key={v.Id} 
                          onClick={() => handleSelect(v)}
                          className={cn("cursor-pointer", editingVoucher.Id === v.Id ? "bg-[#E5F1FB]" : "")}
                        >
                          <td>
                             <span className={cn("text-[13px] font-black uppercase italic tracking-widest leading-none", editingVoucher.Id === v.Id ? "text-[#0078D4]" : "text-slate-800")}>
                                {v.Code}
                             </span>
                          </td>
                          <td className="text-right">
                             <span className="text-[13px] font-black text-rose-500 tabular-nums">
                                {v.DiscountType === '%' ? `-${v.Value}%` : `-${formatCurrency(v.Value)}`}
                             </span>
                          </td>
                          <td className="text-right text-[12px] font-bold text-slate-500 tabular-nums">
                             {formatCurrency(v.MinInvoiceAmount)}
                          </td>
                          <td className="text-center">
                             <div className="flex flex-col items-center gap-1">
                                <span className="text-[9px] font-black text-slate-400">{v.UsedCount} / {v.UsageLimit}</span>
                                <div className="w-16 h-1 bg-slate-100 rounded-full overflow-hidden border border-slate-200">
                                   <div className="h-full bg-[#0078D4]" style={{ width: `${Math.min(100, (v.UsedCount/v.UsageLimit)*100)}%` }} />
                                </div>
                             </div>
                          </td>
                          <td className="text-center text-[10px] font-bold text-slate-400 uppercase italic">
                             {new Date(v.EndDate).toLocaleDateString('vi-VN')}
                          </td>
                          <td className="text-center">
                             <div className={cn(
                               "inline-flex items-center gap-1.5 px-2 py-0.5 rounded-sm text-[8px] font-black uppercase tracking-widest border",
                               !isExpired && v.IsActive ? "bg-emerald-50 text-emerald-600 border-emerald-200" : "bg-slate-50 text-slate-300 border-slate-200"
                             )}>
                                {!isExpired && v.IsActive ? "LIVE" : "ENDED"}
                             </div>
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
              
              <div className="p-4 bg-[#F0F0F0] border-t border-[#D1D1D1] flex items-center justify-between">
                 <div className="flex items-center gap-6">
                    <div className="text-right">
                       <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Dịch vụ Marketing</p>
                       <span className="text-[11px] font-black text-[#0078D4] uppercase italic">LOYALTY ENGINE v1.8</span>
                    </div>
                 </div>
                 <div className="flex items-center gap-2 text-emerald-600 bg-white px-4 py-2 border border-slate-200 rounded-sm italic text-[11px] font-black uppercase">
                    <Database className="w-3.5 h-3.5" /> {vouchers.length} VCH INDEXED
                 </div>
              </div>
           </div>

           <div className="wpf-panel p-6 bg-slate-800 text-white border-slate-700 shadow-md">
              <div className="flex items-center gap-4">
                 <div className="w-10 h-10 bg-white/10 rounded-sm flex items-center justify-center">
                    <BarChart3 className="w-6 h-6 text-amber-500" />
                 </div>
                 <div>
                    <h4 className="text-[14px] font-black uppercase italic leading-none mb-1">HIỆU QUẢ VOUCHER</h4>
                    <p className="text-[9px] font-bold text-white/40 uppercase tracking-[0.2em]">Campaign Conversion Analysis</p>
                 </div>
                 <div className="ml-auto text-right">
                    <span className="text-[20px] font-black text-emerald-400 italic">88.5% STABLE</span>
                 </div>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
}
function RotateCcw(props: any) {
  return (
    <svg
      {...props}
      xmlns="http://www.w3.org/2000/svg"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8" />
      <path d="M3 3v5h5" />
    </svg>
  );
}
