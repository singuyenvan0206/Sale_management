"use client";

import { useState, useEffect, useMemo } from "react";
import { cn } from "@/lib/utils";
import { 
  Factory, 
  Search, 
  RotateCcw, 
  Plus,
  Trash2,
  Edit2,
  Phone,
  Mail,
  MapPin,
  Building
} from "lucide-react";

export default function SuppliersPage() {
  const [suppliers, setSuppliers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  // Form State
  const [editingSupplier, setEditingSupplier] = useState<any>({
    Id: 0,
    Name: "",
    Phone: "",
    Email: "",
    Address: "",
    Description: ""
  });

  // Filter State
  const [searchTerm, setSearchTerm] = useState("");

  const [statusText, setStatusText] = useState("Sẵn sàng");

  useEffect(() => {
    const load = async () => {
      try {
        const res = await fetch("/api/suppliers");
        const data = await res.json();
        if (Array.isArray(data)) setSuppliers(data);
      } catch (e) {
        console.error("Failed to load suppliers", e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const filteredSuppliers = useMemo(() => {
    if (!Array.isArray(suppliers)) return [];
    return suppliers.filter(s => {
      return !searchTerm || 
        s.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        s.Phone.includes(searchTerm) ||
        (s.Email || "").toLowerCase().includes(searchTerm.toLowerCase());
    });
  }, [suppliers, searchTerm]);

  const handleSelectSupplier = (supplier: any) => {
    setEditingSupplier({...supplier});
    setStatusText(`Đang chọn: ${supplier.Name}`);
  };

  const handleClear = () => {
    setEditingSupplier({
      Id: 0,
      Name: "",
      Phone: "",
      Email: "",
      Address: "",
      Description: ""
    });
    setStatusText("Mới");
  };

  if (loading) return <div className="p-20 text-center font-black animate-pulse">ĐANG TẢI DỮ LIỆU NHÀ CUNG CẤP...</div>;

  return (
    <div className="space-y-6 animate-in fade-in duration-500 pb-20">
      {/* Header Bar - WPF style */}
      <div className="bg-[#1CB5E0] p-6 -mx-8 -mt-8 mb-8 shadow-lg">
        <h2 className="text-[24px] font-bold text-white text-center tracking-tight">
           🏭 Quản Lý Nhà Cung Cấp
        </h2>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
        {/* Left Panel - Input Form */}
        <div className="lg:col-span-1">
           <div className="bg-white p-6 rounded-[10px] shadow-sm space-y-6 sticky top-4 border-t-4 border-blue-400">
              <div className="flex items-center justify-between border-b pb-3">
                 <h3 className="text-[14px] font-black text-slate-800 uppercase italic tracking-tight">Đối tác cung ứng</h3>
                 <span className="text-[10px] font-bold text-blue-500 px-2 py-0.5 bg-blue-50 rounded uppercase">
                    {editingSupplier.Id === 0 ? "Mới" : `ID: ${editingSupplier.Id}`}
                 </span>
              </div>
              
              <div className="space-y-4">
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Tên nhà cung cấp *</label>
                    <div className="relative">
                       <Building className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                       <input 
                        type="text" 
                        value={editingSupplier.Name}
                        onChange={(e) => setEditingSupplier({...editingSupplier, Name: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 pl-10 pr-3 text-sm font-bold focus:bg-white focus:ring-2 focus:ring-blue-100 outline-none transition-all" 
                        placeholder="Công ty May Mặc..." 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Số điện thoại</label>
                    <div className="relative">
                       <Phone className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                       <input 
                        type="text" 
                        value={editingSupplier.Phone}
                        onChange={(e) => setEditingSupplier({...editingSupplier, Phone: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 pl-10 pr-3 text-sm font-bold focus:bg-white outline-none" 
                        placeholder="09..." 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Email liên hệ</label>
                    <div className="relative">
                       <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                       <input 
                        type="email" 
                        value={editingSupplier.Email}
                        onChange={(e) => setEditingSupplier({...editingSupplier, Email: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 pl-10 pr-3 text-sm font-bold focus:bg-white outline-none" 
                        placeholder="contact@supplier.com" 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Địa chỉ văn phòng</label>
                    <textarea 
                      rows={2}
                      value={editingSupplier.Address || ""}
                      onChange={(e) => setEditingSupplier({...editingSupplier, Address: e.target.value})}
                      className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold focus:bg-white outline-none transition-all resize-none" 
                      placeholder="Địa chỉ giao dịch..." 
                    />
                 </div>
              </div>

              <div className="grid grid-cols-2 gap-2 pt-4">
                 <button className="bg-[#4CAF50] hover:bg-[#43a047] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-widest shadow-md active:scale-95 flex items-center justify-center gap-2">
                    <Plus className="w-3.5 h-3.5" /> Thêm Đối Tác
                 </button>
                 <button className="bg-[#2196F3] hover:bg-[#1e88e5] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-widest shadow-md active:scale-95 flex items-center justify-center gap-2">
                    <Edit2 className="w-3.5 h-3.5" /> Cập nhật
                 </button>
                 <button className="col-span-2 bg-[#F44336] hover:bg-[#e53935] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-[0.2em] shadow-md active:scale-95 flex items-center justify-center gap-2">
                    <Trash2 className="w-3.5 h-3.5" /> Xóa đối tác
                 </button>
              </div>
              <button 
                onClick={handleClear}
                className="w-full bg-[#9E9E9E] hover:bg-[#757575] text-white font-black py-3 rounded-md text-[10px] uppercase tracking-widest flex items-center justify-center gap-2 active:scale-95 transition-all"
              >
                 <RotateCcw className="w-3.5 h-3.5" /> Làm mới
              </button>
              <div className="text-[10px] font-bold text-slate-400 italic text-center pt-2">
                 Trạng thái: {statusText}
              </div>
           </div>
        </div>

        {/* Right Panel - Supplier List */}
        <div className="lg:col-span-3">
          <div className="bg-white rounded-[10px] shadow-sm overflow-hidden min-h-[700px] flex flex-col">
            <div className="p-6 bg-[#f8f9fa]/50 border-b border-slate-50">
               <h3 className="text-[18px] font-black text-[#1CB5E0] uppercase tracking-tight mb-6">🏭 Hồ Sơ Nhà Cung Cấp</h3>
               
               {/* Search Panel */}
               <div className="bg-[#F8F9FA] p-5 rounded-[8px]">
                  <div className="flex items-center gap-2">
                    <div className="flex-1 relative">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                        <input 
                        type="text" 
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        className="w-full h-11 bg-white rounded-md pl-10 pr-3 text-sm font-bold outline-none shadow-sm focus:ring-2 focus:ring-blue-100" 
                        placeholder="Tìm kiếm đối tác nhanh..." 
                        />
                    </div>
                    <button className="bg-[#FF9800] hover:bg-[#f57c00] text-white font-black h-11 px-10 rounded-md text-[12px] uppercase tracking-widest active:scale-95 shadow-md">🔍 Lọc</button>
                  </div>
               </div>
            </div>

            <div className="flex-1 overflow-x-auto">
              <table className="w-full text-left">
                <thead className="bg-[#f8f9fa] sticky top-0 z-10 border-b border-slate-100">
                  <tr className="text-slate-800 text-[11px] font-black uppercase tracking-tight">
                    <th className="px-6 py-5"># ID</th>
                    <th className="px-6 py-5">🏭 Tên Nhà Cung Cấp</th>
                    <th className="px-6 py-5">📞 Điện thoại / Email</th>
                    <th className="px-6 py-5">📍 Địa chỉ</th>
                    <th className="px-6 py-5 text-center">📅 Ngày hợp tác</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {filteredSuppliers.map((s: any, idx: number) => (
                    <tr 
                      key={s.Id} 
                      onClick={() => handleSelectSupplier(s)}
                      className={cn(
                        "hover:bg-emerald-600 hover:text-white transition-all cursor-pointer group font-medium",
                        idx % 2 === 1 ? "bg-[#F8F9FA]" : "bg-white",
                        editingSupplier.Id === s.Id ? "bg-emerald-600 text-white font-black shadow-lg" : "text-slate-700"
                      )}
                    >
                      <td className="px-6 py-5 font-bold text-[12px] opacity-40">{s.Id}</td>
                      <td className="px-6 py-5">
                         <div className="flex flex-col">
                            <span className="text-[15px] font-black uppercase tracking-tighter">{s.Name}</span>
                            <span className="text-[10px] opacity-70 italic tracking-widest">Partner ID: SUP-{s.Id}</span>
                         </div>
                      </td>
                      <td className="px-6 py-5">
                         <div className="flex flex-col gap-0.5 text-[12px]">
                            <span className="font-bold">{s.Phone || "---"}</span>
                            <span className="opacity-60">{s.Email || "không có email"}</span>
                         </div>
                      </td>
                      <td className="px-6 py-5 text-[13px] opacity-80 limit-lines-1">{s.Address || "---"}</td>
                      <td className="px-6 py-5 text-center text-[12px] opacity-60">
                         {new Date(s.CreatedDate).toLocaleDateString('vi-VN')}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            
            <div className="p-8 bg-[#F8F9FA] text-right">
               <span className="text-[12px] font-black text-[#1CB5E0] uppercase tracking-tighter shadow-sm p-4 bg-white rounded-lg">
                  Tổng cộng: {filteredSuppliers.length} đối tác
               </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
