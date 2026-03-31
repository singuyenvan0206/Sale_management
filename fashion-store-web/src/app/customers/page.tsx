"use client";

import { useState, useEffect, useMemo } from "react";
import { cn } from "@/lib/utils";
import { 
  Users, 
  Search, 
  RotateCcw, 
  UserPlus,
  Trash2,
  Edit2,
  Phone,
  Mail,
  MapPin,
  Star
} from "lucide-react";

export default function CustomersPage() {
  const [customers, setCustomers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  // Form State
  const [editingCustomer, setEditingCustomer] = useState<any>({
    Id: 0,
    Name: "",
    Phone: "",
    Email: "",
    Address: "",
    CustomerType: "Regular",
    TotalSpended: 0
  });

  // Filter State
  const [searchTerm, setSearchTerm] = useState("");
  const [filterType, setFilterType] = useState("All");

  const [statusText, setStatusText] = useState("Sẵn sàng");

  useEffect(() => {
    const load = async () => {
      try {
        const res = await fetch("/api/customers");
        const data = await res.json();
        if (Array.isArray(data)) setCustomers(data);
      } catch (e) {
        console.error("Failed to load customers", e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const filteredCustomers = useMemo(() => {
    if (!Array.isArray(customers)) return [];
    return customers.filter(c => {
      const matchesSearch = !searchTerm || 
        c.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        c.Phone.includes(searchTerm);
      
      const matchesType = filterType === "All" || c.CustomerType === filterType;

      return matchesSearch && matchesType;
    });
  }, [customers, searchTerm, filterType]);

  const handleSelectCustomer = (customer: any) => {
    setEditingCustomer({...customer});
    setStatusText(`Đang chọn: ${customer.Name}`);
  };

  const handleClear = () => {
    setEditingCustomer({
      Id: 0,
      Name: "",
      Phone: "",
      Email: "",
      Address: "",
      CustomerType: "Regular",
      TotalSpended: 0
    });
    setStatusText("Mới");
  };

  if (loading) return <div className="p-20 text-center font-black animate-pulse">ĐANG TẢI DỮ LIỆU KHÁCH HÀNG...</div>;

  return (
    <div className="space-y-6 animate-in fade-in duration-500 pb-20">
      {/* Header Bar - WPF style */}
      <div className="bg-[#1CB5E0] p-6 -mx-8 -mt-8 mb-8 shadow-lg">
        <h2 className="text-[24px] font-bold text-white text-center tracking-tight">
           👥 Quản Lý Khách Hàng
        </h2>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
        {/* Left Panel - Input Form */}
        <div className="lg:col-span-1">
           <div className="bg-white p-6 rounded-[10px] shadow-sm space-y-6 sticky top-4 border-t-4 border-emerald-500">
              <div className="flex items-center justify-between border-b pb-3">
                 <h3 className="text-[14px] font-black text-slate-800 uppercase italic tracking-tight">Thông tin chi tiết</h3>
                 <span className="text-[10px] font-bold text-emerald-500 px-2 py-0.5 bg-emerald-50 rounded">
                    {editingCustomer.Id === 0 ? "Khách mới" : `ID: ${editingCustomer.Id}`}
                 </span>
              </div>
              
              <div className="space-y-4">
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Họ và tên *</label>
                    <input 
                      type="text" 
                      value={editingCustomer.Name}
                      onChange={(e) => setEditingCustomer({...editingCustomer, Name: e.target.value})}
                      className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold focus:bg-white focus:ring-2 focus:ring-emerald-100 outline-none transition-all" 
                      placeholder="Nguyễn Văn A..." 
                    />
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Số điện thoại *</label>
                    <div className="relative">
                       <Phone className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
                       <input 
                        type="text" 
                        value={editingCustomer.Phone}
                        onChange={(e) => setEditingCustomer({...editingCustomer, Phone: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 pl-10 pr-3 text-sm font-bold focus:bg-white outline-none" 
                        placeholder="0901..." 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Email</label>
                    <div className="relative">
                       <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
                       <input 
                        type="email" 
                        value={editingCustomer.Email}
                        onChange={(e) => setEditingCustomer({...editingCustomer, Email: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 pl-10 pr-3 text-sm font-bold focus:bg-white outline-none" 
                        placeholder="example@mail.com" 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Địa chỉ</label>
                    <div className="relative">
                       <MapPin className="absolute left-3 top-3 w-3.5 h-3.5 text-slate-400" />
                       <textarea 
                        rows={2}
                        value={editingCustomer.Address}
                        onChange={(e) => setEditingCustomer({...editingCustomer, Address: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 pl-10 pr-3 text-sm font-bold focus:bg-white outline-none transition-all resize-none" 
                        placeholder="Số nhà, đường, quận..." 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Loại khách hàng</label>
                    <select 
                      value={editingCustomer.CustomerType}
                      onChange={(e) => setEditingCustomer({...editingCustomer, CustomerType: e.target.value})}
                      className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold cursor-pointer"
                    >
                       <option value="Regular">Khách thường</option>
                       <option value="VIP">Khách VIP</option>
                       <option value="Loyal">Khách thân thiết</option>
                    </select>
                 </div>
              </div>

              <div className="grid grid-cols-3 gap-2 pt-4">
                 <button className="bg-[#4CAF50] hover:bg-[#43a047] text-white font-black py-3 rounded-md text-[10px] uppercase tracking-tighter shadow-md active:scale-95">➕ Thêm</button>
                 <button className="bg-[#2196F3] hover:bg-[#1e88e5] text-white font-black py-3 rounded-md text-[10px] uppercase tracking-tighter shadow-md active:scale-95">📝 Sửa</button>
                 <button className="bg-[#F44336] hover:bg-[#e53935] text-white font-black py-3 rounded-md text-[10px] uppercase tracking-tighter shadow-md active:scale-95">🗑️ Xóa</button>
              </div>
              <button 
                onClick={handleClear}
                className="w-full bg-[#9E9E9E] hover:bg-[#757575] text-white font-black py-3 rounded-md text-[10px] uppercase tracking-widest flex items-center justify-center gap-2 active:scale-95"
              >
                 <RotateCcw className="w-3.5 h-3.5" /> Làm mới
              </button>
              <div className="text-[10px] font-bold text-slate-400 italic text-center pt-2">
                 Trạng thái: {statusText}
              </div>
           </div>
        </div>

        {/* Right Panel - Customer List */}
        <div className="lg:col-span-3">
          <div className="bg-white rounded-[10px] shadow-sm overflow-hidden min-h-[700px] flex flex-col">
            <div className="p-6 bg-[#f8f9fa]/50">
               <h3 className="text-[18px] font-black text-[#1CB5E0] uppercase tracking-tight mb-6">👥 Danh Sách Khách Hàng</h3>
               
               {/* Filter Panel */}
               <div className="bg-[#F8F9FA] p-5 rounded-[8px] space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                     <div className="space-y-1">
                        <span className="text-[10px] font-black text-slate-400 uppercase">Lọc Loại Khách</span>
                        <select 
                          value={filterType}
                          onChange={(e) => setFilterType(e.target.value)}
                          className="w-full h-10 bg-white rounded-md px-3 text-sm font-bold cursor-pointer outline-none shadow-sm focus:ring-2 focus:ring-blue-100"
                        >
                           <option value="All">Tất cả các loại</option>
                           <option value="Regular">Khách thường</option>
                           <option value="VIP">Khách VIP</option>
                           <option value="Loyal">Khách thân thiết</option>
                        </select>
                     </div>
                     <div className="md:col-span-3 flex items-end gap-2">
                        <div className="flex-1 relative">
                           <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                           <input 
                            type="text" 
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full h-10 bg-white rounded-md pl-10 pr-3 text-sm font-bold outline-none shadow-sm focus:ring-2 focus:ring-blue-100" 
                            placeholder="Tìm kiếm theo Tên hoặc Số điện thoại..." 
                           />
                        </div>
                        <button className="bg-[#FF9800] hover:bg-[#f57c00] text-white font-black h-10 px-8 rounded-md text-[11px] uppercase tracking-widest whitespace-nowrap active:scale-95 shadow-md">🔍 Tìm</button>
                     </div>
                  </div>
               </div>
            </div>

            <div className="flex-1 overflow-x-auto">
              <table className="w-full text-left">
                <thead className="bg-[#f8f9fa] sticky top-0 z-10">
                  <tr className="text-slate-800 text-[11px] font-black uppercase tracking-tight">
                    <th className="px-6 py-5">ID</th>
                    <th className="px-6 py-5">👤 Khách Hàng</th>
                    <th className="px-6 py-5">📞 Điện thoại</th>
                    <th className="px-6 py-5">🏷️ Loại Khách</th>
                    <th className="px-6 py-5 text-right font-mono">💰 Tổng mua</th>
                    <th className="px-6 py-5 text-center">📅 Ngày tạo</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {filteredCustomers.map((c: any, idx: number) => (
                    <tr 
                      key={c.Id} 
                      onClick={() => handleSelectCustomer(c)}
                      className={cn(
                        "hover:bg-blue-600 hover:text-white transition-all cursor-pointer group",
                        idx % 2 === 1 ? "bg-[#F8F9FA]" : "bg-white",
                        editingCustomer.Id === c.Id ? "bg-blue-600 text-white font-black" : "text-slate-700"
                      )}
                    >
                      <td className="px-6 py-5 font-bold text-[12px] opacity-40">{c.Id}</td>
                      <td className="px-6 py-5">
                         <div className="flex flex-col">
                            <span className="text-[14px] font-black uppercase tracking-tight">{c.Name}</span>
                            <span className="text-[10px] opacity-50 lowercase tracking-widest">{c.Email || "không có email"}</span>
                         </div>
                      </td>
                      <td className="px-6 py-5 font-mono text-[13px]">{c.Phone}</td>
                      <td className="px-6 py-5 text-center">
                         <span className={cn(
                            "px-3 py-1 rounded text-[10px] font-black uppercase",
                            c.CustomerType === 'VIP' ? "bg-rose-100 text-rose-600" : 
                            c.CustomerType === 'Loyal' ? "bg-amber-100 text-amber-600" : "bg-slate-100 text-slate-600"
                         )}>
                            {c.CustomerType}
                         </span>
                      </td>
                      <td className="px-6 py-5 text-right">
                        <span className={cn(
                          "font-mono font-black text-[14px]",
                          editingCustomer.Id === c.Id ? "text-white" : "text-emerald-600"
                        )}>
                          {new Intl.NumberFormat('vi-VN').format(c.TotalSpended || 0)}
                        </span>
                      </td>
                      <td className="px-6 py-5 text-center text-[12px]">
                         {new Date(c.CreatedDate).toLocaleDateString('vi-VN')}
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
