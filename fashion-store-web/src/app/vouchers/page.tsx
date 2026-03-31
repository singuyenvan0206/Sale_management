"use client";

import { useState, useEffect, useMemo } from "react";
import { cn } from "@/lib/utils";
import { 
  Ticket, 
  Search, 
  RotateCcw, 
  Plus,
  Trash2,
  Edit2,
  Calendar,
  Zap,
  CheckCircle2,
  AlertCircle
} from "lucide-react";

export default function VouchersPage() {
  const [vouchers, setVouchers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  // Form State
  const [editingVoucher, setEditingVoucher] = useState<any>({
    Id: 0,
    Code: "",
    DiscountType: "VND",
    DiscountValue: 0,
    MinInvoiceAmount: 0,
    StartDate: new Date().toISOString().split('T')[0],
    EndDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    UsageLimit: 100,
    IsActive: true
  });

  // Filter State
  const [searchTerm, setSearchTerm] = useState("");
  const [filterType, setFilterType] = useState("All");

  const [statusText, setStatusText] = useState("Sẵn sàng");

  useEffect(() => {
    const load = async () => {
      try {
        const res = await fetch("/api/vouchers");
        const data = await res.json();
        if (Array.isArray(data)) setVouchers(data);
      } catch (e) {
        console.error("Failed to load vouchers", e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const filteredVouchers = useMemo(() => {
    if (!Array.isArray(vouchers)) return [];
    return vouchers.filter(v => {
      const matchesSearch = !searchTerm || v.Code.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesType = filterType === "All" || v.DiscountType === filterType;
      return matchesSearch && matchesType;
    });
  }, [vouchers, searchTerm, filterType]);

  const handleSelectVoucher = (voucher: any) => {
    setEditingVoucher({
      ...voucher,
      StartDate: new Date(voucher.StartDate).toISOString().split('T')[0],
      EndDate: new Date(voucher.EndDate).toISOString().split('T')[0]
    });
    setStatusText(`Đang chọn: ${voucher.Code}`);
  };

  const handleClear = () => {
    setEditingVoucher({
      Id: 0,
      Code: "",
      DiscountType: "VND",
      DiscountValue: 0,
      MinInvoiceAmount: 0,
      StartDate: new Date().toISOString().split('T')[0],
      EndDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
      UsageLimit: 100,
      IsActive: true
    });
    setStatusText("Mới");
  };

  if (loading) return <div className="p-20 text-center font-black animate-pulse uppercase tracking-[0.3em]">ĐANG TẢI MÃ GIẢM GIÁ...</div>;

  return (
    <div className="space-y-6 animate-in fade-in duration-500 pb-20">
      <div className="bg-[#1CB5E0] p-6 -mx-8 -mt-8 mb-8 shadow-lg">
        <h2 className="text-[24px] font-bold text-white text-center tracking-tight">
           🎟️ Quản Lý Mã Giảm Giá
        </h2>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
        {/* Left Panel */}
        <div className="lg:col-span-1">
           <div className="bg-white p-6 rounded-[10px] shadow-sm space-y-6 sticky top-4 border-t-4 border-rose-500">
              <div className="flex items-center justify-between border-b pb-3">
                 <h3 className="text-[14px] font-black text-slate-800 uppercase italic">Chi tiết Voucher</h3>
                 <span className="text-[10px] font-bold text-rose-500 px-2 py-0.5 bg-rose-50 rounded uppercase">
                    {editingVoucher.Id === 0 ? "Tạo mới" : `ID: ${editingVoucher.Id}`}
                 </span>
              </div>
              
              <div className="space-y-4">
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Mã Code *</label>
                    <input 
                      type="text" 
                      value={editingVoucher.Code}
                      onChange={(e) => setEditingVoucher({...editingVoucher, Code: e.target.value.toUpperCase()})}
                      className="w-full bg-[#f8f9fa] rounded-md py-3 px-3 text-sm font-black text-blue-600 focus:bg-white focus:ring-2 focus:ring-blue-100 outline-none transition-all uppercase" 
                      placeholder="NEWYEAR2024..." 
                    />
                 </div>
                 <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                       <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Loại</label>
                       <select 
                        value={editingVoucher.DiscountType}
                        onChange={(e) => setEditingVoucher({...editingVoucher, DiscountType: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2.5 px-3 text-sm font-bold"
                       >
                          <option value="VND">Giảm Tiền (VND)</option>
                          <option value="%">Giảm %</option>
                       </select>
                    </div>
                    <div className="space-y-1.5">
                       <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Giá trị *</label>
                       <input 
                         type="number" 
                         value={editingVoucher.DiscountValue}
                         onChange={(e) => setEditingVoucher({...editingVoucher, DiscountValue: Number(e.target.value)})}
                         className="w-full bg-[#f8f9fa] rounded-md py-2.5 px-3 text-sm font-black text-emerald-600" 
                        />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Hóa đơn tối thiểu (VND)</label>
                    <input 
                      type="number" 
                      value={editingVoucher.MinInvoiceAmount}
                      onChange={(e) => setEditingVoucher({...editingVoucher, MinInvoiceAmount: Number(e.target.value)})}
                      className="w-full bg-[#f8f9fa] rounded-md py-2.5 px-3 text-sm font-bold" 
                    />
                 </div>
                 <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                       <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Bắt đầu</label>
                       <input 
                        type="date" 
                        value={editingVoucher.StartDate}
                        onChange={(e) => setEditingVoucher({...editingVoucher, StartDate: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold" 
                       />
                    </div>
                    <div className="space-y-1.5">
                       <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Kết thúc</label>
                       <input 
                        type="date" 
                        value={editingVoucher.EndDate}
                        onChange={(e) => setEditingVoucher({...editingVoucher, EndDate: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold" 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Giới hạn sử dụng (Lượt)</label>
                    <input 
                      type="number" 
                      value={editingVoucher.UsageLimit}
                      onChange={(e) => setEditingVoucher({...editingVoucher, UsageLimit: Number(e.target.value)})}
                      className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold" 
                    />
                 </div>
              </div>

              <div className="grid grid-cols-3 gap-2 pt-4">
                 <button className="bg-[#4CAF50] hover:bg-[#43a047] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-tighter shadow-md active:scale-95">➕ Thêm</button>
                 <button className="bg-[#2196F3] hover:bg-[#1e88e5] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-tighter shadow-md active:scale-95">📝 Sửa</button>
                 <button className="bg-[#F44336] hover:bg-[#e53935] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-tighter shadow-md active:scale-95">🗑️ Xóa</button>
              </div>
              <button onClick={handleClear} className="w-full bg-[#9E9E9E] text-white font-black py-3 rounded-md text-[10px] uppercase tracking-widest active:scale-95 transition-all"> Làm mới </button>
           </div>
        </div>

        {/* Right Panel */}
        <div className="lg:col-span-3">
          <div className="bg-white rounded-[10px] shadow-sm overflow-hidden min-h-[700px] flex flex-col">
            <div className="p-6 bg-[#f8f9fa]/50 border-b border-slate-50">
               <h3 className="text-[18px] font-black text-[#1CB5E0] uppercase tracking-tight mb-6">🎟️ Danh Sách Mã Giảm Giá Đang Chạy</h3>
               
               <div className="bg-[#F8F9FA] p-5 rounded-[8px]">
                  <div className="flex items-center gap-2">
                    <div className="flex-1 relative">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                        <input 
                        type="text" 
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        className="w-full h-12 bg-white rounded-md pl-10 pr-3 text-sm font-bold outline-none shadow-sm focus:ring-2 focus:ring-blue-100" 
                        placeholder="Tra cứu mã nhanh..." 
                        />
                    </div>
                    <button className="bg-[#FF9800] hover:bg-[#f57c00] text-white font-black h-12 px-10 rounded-md text-[12px] uppercase tracking-widest active:scale-95 shadow-md">🔍 Tìm</button>
                  </div>
               </div>
            </div>

            <div className="flex-1 overflow-x-auto">
              <table className="w-full text-left">
                <thead className="bg-[#f8f9fa] sticky top-0 z-10 border-b border-slate-100">
                  <tr className="text-slate-800 text-[11px] font-black uppercase tracking-tight font-serif italic">
                    <th className="px-6 py-5">#</th>
                    <th className="px-6 py-5">🎟️ Mã Code</th>
                    <th className="px-6 py-5 text-right">💰 Giảm Giá</th>
                    <th className="px-6 py-5 text-right">🏦 HĐ Tối Thiểu</th>
                    <th className="px-6 py-5 text-center">📅 Thời Hạn</th>
                    <th className="px-6 py-5 text-center">📊 Đã dùng</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 italic">
                  {filteredVouchers.map((v: any, idx: number) => (
                    <tr 
                      key={v.Id} 
                      onClick={() => handleSelectVoucher(v)}
                      className={cn(
                        "hover:bg-[#FC5C7D] hover:text-white transition-all cursor-pointer group font-medium",
                        idx % 2 === 1 ? "bg-[#F8F9FA]" : "bg-white",
                        editingVoucher.Id === v.Id ? "bg-[#FC5C7D] text-white font-black shadow-lg" : "text-slate-700"
                      )}
                    >
                      <td className="px-6 py-5 font-bold text-[12px] opacity-40">{v.Id}</td>
                      <td className="px-6 py-5">
                         <div className="flex items-center gap-3">
                            <Ticket className="w-4 h-4 opacity-50" />
                            <span className="text-[16px] font-black uppercase tracking-tighter">{v.Code}</span>
                         </div>
                      </td>
                      <td className="px-6 py-5 text-right">
                         <span className={cn(
                            "font-black text-[15px]",
                            editingVoucher.Id === v.Id ? "text-white" : "text-rose-600"
                         )}>
                            {v.DiscountType === '%' ? `-${v.DiscountValue}%` : `-${new Intl.NumberFormat('vi-VN').format(v.DiscountValue)}đ`}
                         </span>
                      </td>
                      <td className="px-6 py-5 text-right font-mono text-[14px]">
                         {new Intl.NumberFormat('vi-VN').format(v.MinInvoiceAmount)}đ
                      </td>
                      <td className="px-6 py-5 text-center text-[11px]">
                         <div className="flex flex-col">
                            <span>Từ: {new Date(v.StartDate).toLocaleDateString('vi-VN')}</span>
                            <span>Đến: {new Date(v.EndDate).toLocaleDateString('vi-VN')}</span>
                         </div>
                      </td>
                      <td className="px-6 py-5 text-center">
                         <div className="flex items-center justify-center gap-2">
                            <span className="font-black">{v.UsedCount}</span>
                            <span className="opacity-40">/</span>
                            <span className="opacity-40">{v.UsageLimit}</span>
                         </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
