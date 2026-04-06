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
  Star,
  Save,
  CheckCircle2,
  AlertCircle,
  Archive,
  ArrowUpDown,
  Filter,
  Plus,
  History,
  FileText,
  User,
  ChevronRight,
} from "lucide-react";

interface Customer {
  Id: number;
  Name: string;
  Phone: string;
  Email: string;
  Address: string;
  CustomerType: string;
  Points: number;
  TotalSpent: number;
  CreatedDate: string;
}

export default function CustomersPage() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [message, setMessage] = useState<{ text: string, type: 'success' | 'error' | null }>({ text: "", type: null });

  const [editingCustomer, setEditingCustomer] = useState<Customer>({
    Id: 0,
    Name: "",
    Phone: "",
    Email: "",
    Address: "",
    CustomerType: "Regular",
    Points: 0,
    TotalSpent: 0,
    CreatedDate: new Date().toISOString()
  });

  const [searchTerm, setSearchTerm] = useState("");
  const [typeFilter, setTypeFilter] = useState("All");
  const [sortCol, setSortCol] = useState<'Name' | 'TotalSpent' | 'Points'>('TotalSpent');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');

  const loadCustomers = async () => {
    setLoading(true);
    try {
      const res = await fetch("/api/customers");
      const json = await res.json();
      if (json.success) setCustomers(json.data);
    } catch (e) {
      showMsg("Lỗi: Không thể truy xuất cơ sở dữ liệu khách hàng", 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCustomers();
  }, []);

  const showMsg = (text: string, type: 'success' | 'error') => {
    setMessage({ text, type });
    setTimeout(() => setMessage({ text: "", type: null }), 3000);
  };

  const filteredCustomers = useMemo(() => {
    if (!Array.isArray(customers)) return [];
    let result = customers.filter(c => {
      const matchesSearch = !searchTerm || 
        c.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        c.Phone.includes(searchTerm);
      
      const matchesType = typeFilter === "All" || c.CustomerType === typeFilter;
      return matchesSearch && matchesType;
    });

    return result.sort((a, b) => {
      const valA = a[sortCol];
      const valB = b[sortCol];
      if (valA < valB) return sortOrder === 'asc' ? -1 : 1;
      if (valA > valB) return sortOrder === 'asc' ? 1 : -1;
      return 0;
    });
  }, [customers, searchTerm, typeFilter, sortCol, sortOrder]);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingCustomer.Name.trim() || !editingCustomer.Phone.trim()) {
      showMsg("Tên và Số điện thoại là trường dữ liệu bắt buộc", 'error');
      return;
    }

    setActionLoading(true);
    try {
      const isNew = editingCustomer.Id === 0;
      const res = await fetch("/api/customers", {
        method: isNew ? "POST" : "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(editingCustomer)
      });
      
      const json = await res.json();
      if (json.success) {
        showMsg(isNew ? "Đăng ký khách hàng thành công" : "Cập nhật hồ sơ thành công", 'success');
        if (isNew) handleClear();
        loadCustomers();
      } else {
        showMsg(json.error || "Lỗi hệ thống", 'error');
      }
    } catch (e) {
      showMsg("Lỗi kết nối máy chủ", 'error');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDelete = async () => {
    if (editingCustomer.Id === 0) return;
    if (!confirm(`Xác nhận lưu trữ/xóa hồ sơ khách hàng: "${editingCustomer.Name}"?`)) return;

    setActionLoading(true);
    try {
      const res = await fetch(`/api/customers?id=${editingCustomer.Id}`, {
        method: "DELETE"
      });
      const json = await res.json();
      if (json.success) {
        showMsg("Đã xóa hồ sơ khách hàng", 'success');
        handleClear();
        loadCustomers();
      } else {
        showMsg(json.error || "Thao tác thất bại", 'error');
      }
    } catch (e) {
      showMsg("Lỗi mạng", 'error');
    } finally {
      setActionLoading(false);
    }
  };

  const handleClear = () => {
    setEditingCustomer({
      Id: 0,
      Name: "",
      Phone: "",
      Email: "",
      Address: "",
      CustomerType: "Regular",
      Points: 0,
      TotalSpent: 0,
      CreatedDate: new Date().toISOString()
    });
  };

  const toggleSort = (col: typeof sortCol) => {
    if (sortCol === col) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortCol(col);
      setSortOrder('desc');
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  if (loading && customers.length === 0) return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] gap-4 no-select uppercase italic font-black text-slate-400">
      <div className="w-12 h-12 border-4 border-[#0078D4] border-t-transparent rounded-full animate-spin" />
      <p className="text-[11px] tracking-widest">Đang kết nối cơ sở dữ liệu CRM...</p>
    </div>
  );

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-20 no-select">
      {/* WPF Header / Ribbon */}
      <div className="wpf-panel">
         <div className="wpf-panel-header uppercase">HỆ THỐNG QUẢN LÝ QUAN HỆ KHÁCH HÀNG (CRM SUBSYSTEM)</div>
         <div className="p-6 bg-white flex flex-col md:flex-row md:items-center justify-between gap-6">
            <div className="flex items-center gap-4">
               <div className="w-10 h-10 bg-[#0078D4] rounded-sm flex items-center justify-center text-white shadow-md">
                  <Users className="w-6 h-6" />
               </div>
               <div>
                  <h2 className="text-[20px] font-black text-slate-900 tracking-tight uppercase italic leading-none">HỒ SƠ KHÁCH HÀNG</h2>
                  <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-widest italic tracking-tighter">Fusion ERP Client Registry Shell v2.5</p>
               </div>
            </div>
            
            {message.type && (
               <div className={cn(
                 "px-6 py-3 border rounded-sm flex items-center gap-3 animate-in fade-in zoom-in duration-300 shadow-sm",
                 message.type === 'success' ? "bg-emerald-50 text-emerald-600 border-emerald-200" : "bg-rose-50 text-rose-600 border-rose-200"
               )}>
                  {message.type === 'success' ? <CheckCircle2 className="w-4 h-4" /> : <AlertCircle className="w-4 h-4" />}
                  <span className="text-[10px] font-black uppercase tracking-widest leading-none">{message.text}</span>
               </div>
            )}
         </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        {/* Left Column: Editor Panel */}
        <div className="lg:col-span-4 space-y-6 lg:sticky lg:top-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex justify-between items-center text-[#333]">
                 <span className="flex items-center gap-2">
                    <UserPlus className="w-4 h-4" /> BIÊN TẬP HỒ SƠ
                 </span>
                 <span className={cn(
                    "text-[9px] font-black px-2 py-0.5 rounded-sm border",
                    editingCustomer.Id === 0 ? "bg-blue-50 text-blue-600 border-blue-200" : "bg-emerald-50 text-emerald-600 border-emerald-200"
                 )}>
                    {editingCustomer.Id === 0 ? "NEW ENTRY" : `ID: CUS-${editingCustomer.Id}`}
                 </span>
              </div>
              <form onSubmit={handleSave} className="p-6 bg-white space-y-5">
                 <div className="wpf-groupbox !mt-0">
                    <span className="wpf-groupbox-label">Thông tin cơ bản</span>
                    <div className="space-y-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Họ và tên khách hàng:</label>
                          <input 
                            type="text" 
                            required
                            value={editingCustomer.Name}
                            onChange={(e) => setEditingCustomer({...editingCustomer, Name: e.target.value})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-bold bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm uppercase italic"
                            placeholder="NHẬP TÊN..." 
                          />
                       </div>
                       <div className="grid grid-cols-2 gap-4">
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Số điện thoại:</label>
                             <input 
                               type="text" 
                               required
                               value={editingCustomer.Phone}
                               onChange={(e) => setEditingCustomer({...editingCustomer, Phone: e.target.value})}
                               className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm tabular-nums"
                               placeholder="09..." 
                             />
                          </div>
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Phân loại:</label>
                             <select 
                               value={editingCustomer.CustomerType}
                               onChange={(e) => setEditingCustomer({...editingCustomer, CustomerType: e.target.value})}
                               className="w-full h-10 border border-[#D1D1D1] px-3 text-[11px] font-black bg-white focus:border-[#0078D4] outline-none rounded-sm cursor-pointer uppercase"
                             >
                                <option value="Regular">KHÁCH THƯỜNG</option>
                                <option value="VIP">KHÁCH VIP</option>
                                <option value="Loyal">LOYALTY</option>
                             </select>
                          </div>
                       </div>
                    </div>
                 </div>

                 <div className="wpf-groupbox">
                    <span className="wpf-groupbox-label">Liên hệ & Địa chỉ</span>
                    <div className="space-y-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Địa chỉ Email:</label>
                          <input 
                            type="email" 
                            value={editingCustomer.Email || ""}
                            onChange={(e) => setEditingCustomer({...editingCustomer, Email: e.target.value})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[12px] font-medium bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm"
                            placeholder="example@mail.com" 
                          />
                       </div>
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Địa chỉ thường trú:</label>
                          <textarea 
                            rows={3}
                            value={editingCustomer.Address || ""}
                            onChange={(e) => setEditingCustomer({...editingCustomer, Address: e.target.value})}
                            className="w-full border border-[#D1D1D1] p-3 text-[12px] font-medium bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm resize-none italic"
                            placeholder="Dữ liệu địa chỉ..." 
                          />
                       </div>
                    </div>
                 </div>

                 <div className="flex flex-col gap-3 pt-4">
                    <button 
                      type="submit"
                      disabled={actionLoading}
                      className={cn(
                        "btn-wpf h-12 flex items-center justify-center gap-3 uppercase font-black text-[11px] border-b-4",
                        editingCustomer.Id === 0 ? "btn-wpf-primary border-[#005A9E]" : "bg-emerald-600 text-white border-emerald-800 hover:bg-emerald-700"
                      )}
                    >
                       {actionLoading ? <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" /> : (editingCustomer.Id === 0 ? <Plus className="w-5 h-5" /> : <Save className="w-5 h-5" />)}
                       {editingCustomer.Id === 0 ? "ĐĂNG KÝ MỚI" : "LƯU THAY ĐỔI"}
                    </button>
                    
                    <div className="grid grid-cols-2 gap-3">
                       {editingCustomer.Id !== 0 && (
                         <button 
                           type="button"
                           onClick={handleDelete}
                           disabled={actionLoading}
                           className="btn-wpf h-10 text-rose-600 border-rose-200 hover:bg-rose-50 flex items-center justify-center gap-2 text-[10px] uppercase font-black"
                         >
                            <Trash2 className="w-3.5 h-3.5" /> XÓA HỒ SƠ
                         </button>
                       )}
                       <button 
                         type="button"
                         onClick={handleClear}
                         className={cn(
                           "btn-wpf h-10 text-slate-500 border-slate-200 hover:bg-slate-50 flex items-center justify-center gap-2 text-[10px] uppercase font-black",
                           editingCustomer.Id === 0 ? "col-span-2" : "col-span-1"
                         )}
                       >
                          <RotateCcw className="w-3.5 h-3.5" /> {editingCustomer.Id === 0 ? "RESET FORM" : "HỦY BỎ"}
                       </button>
                    </div>
                 </div>
              </form>
           </div>

           <div className="wpf-panel !bg-slate-800 text-white border-slate-700 p-6 shadow-md">
              <div className="flex items-center gap-4 mb-4">
                 <div className="w-10 h-10 bg-white/10 rounded-sm flex items-center justify-center border border-white/20">
                    <Star className="w-6 h-6 text-amber-500 fill-amber-500" />
                 </div>
                 <div>
                    <h4 className="text-[14px] font-black uppercase italic leading-none mb-1 text-white">CHỈ SỐ CRM</h4>
                    <p className="text-[9px] font-bold text-white/40 uppercase tracking-[0.2em]">Loyalty Analytics</p>
                 </div>
              </div>
              <div className="space-y-4">
                 <div className="flex justify-between items-center text-[11px] font-bold text-white/60">
                    <span>Loyalty Rate:</span>
                    <span className="text-emerald-400">85.4%</span>
                 </div>
                 <div className="w-full bg-white/5 h-1.5 rounded-sm">
                    <div className="h-full bg-emerald-500 w-[85%]" />
                 </div>
              </div>
           </div>
        </div>

        {/* Right Column: Registry Table */}
        <div className="lg:col-span-8 space-y-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex flex-col md:flex-row md:items-center justify-between gap-4 py-3 h-auto">
                 <span className="flex items-center gap-2">
                    <FileText className="w-4 h-4" /> DANH BẠ KHÁCH HÀNG (DATABASE)
                 </span>
                 
                 <div className="flex flex-wrap items-center gap-3">
                    <div className="relative group">
                       <Filter className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
                       <select 
                         value={typeFilter}
                         onChange={(e) => setTypeFilter(e.target.value)}
                         className="h-8 border border-[#D1D1D1] pl-8 pr-4 text-[9px] font-black uppercase tracking-widest text-[#0078D4] focus:border-[#0078D4] outline-none rounded-sm bg-white cursor-pointer"
                       >
                          <option value="All">TẤT CẢ PHÂN LOẠI</option>
                          <option value="Regular">KHÁCH THƯỜNG</option>
                          <option value="VIP">KHÁCH VIP</option>
                          <option value="Loyal">LOYALTY</option>
                       </select>
                    </div>

                    <div className="relative group">
                       <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
                       <input 
                         type="text" 
                         value={searchTerm}
                         onChange={(e) => setSearchTerm(e.target.value)}
                         className="h-8 border border-[#D1D1D1] pl-8 pr-4 text-[10px] font-bold text-slate-600 focus:border-[#0078D4] outline-none rounded-sm bg-[#F9F9F9] focus:bg-white w-[180px] uppercase"
                         placeholder="TÌM TÊN / SĐT..." 
                       />
                    </div>
                 </div>
              </div>
              
              <div className="overflow-x-auto">
                <table className="wpf-datagrid">
                  <thead>
                    <tr>
                      <th className="w-[80px]">ID</th>
                      <th className="cursor-pointer hover:bg-slate-100" onClick={() => toggleSort('Name')}>
                         HỌ TÊN {sortCol === 'Name' && <ArrowUpDown className="w-3 h-3 inline ml-1" />}
                      </th>
                      <th className="w-[100px] text-center">LOẠI</th>
                      <th className="text-right cursor-pointer hover:bg-slate-100" onClick={() => toggleSort('TotalSpent')}>
                         TỔNG MUA {sortCol === 'TotalSpent' && <ArrowUpDown className="w-3 h-3 inline ml-1" />}
                      </th>
                      <th className="text-center cursor-pointer hover:bg-slate-100" onClick={() => toggleSort('Points')}>
                         ĐIỂM {sortCol === 'Points' && <ArrowUpDown className="w-3 h-3 inline ml-1" />}
                      </th>
                      <th className="text-right w-[60px]">EL</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredCustomers.map((c: Customer) => (
                      <tr 
                        key={c.Id} 
                        onClick={() => setEditingCustomer({...c})}
                        className={cn("cursor-pointer", editingCustomer.Id === c.Id ? "bg-[#E5F1FB]" : "")}
                      >
                        <td className="text-center font-bold text-slate-300 tabular-nums text-[10px]">{c.Id}</td>
                        <td>
                           <div className="flex flex-col">
                              <span className={cn("text-[13px] font-black uppercase italic leading-none mb-1", editingCustomer.Id === c.Id ? "text-[#0078D4]" : "text-slate-800")}>
                                 {c.Name}
                              </span>
                              <span className="text-[10px] font-bold text-slate-400 tabular-nums">{c.Phone}</span>
                           </div>
                        </td>
                        <td className="text-center">
                           <span className={cn(
                               "px-2 py-0.5 rounded-sm text-[9px] font-black uppercase border",
                               c.CustomerType === 'VIP' ? "bg-rose-50 text-rose-500 border-rose-200" : 
                               c.CustomerType === 'Loyal' ? "bg-amber-50 text-amber-500 border-amber-200" : "bg-slate-50 text-slate-400 border-slate-200"
                           )}>
                              {c.CustomerType}
                           </span>
                        </td>
                        <td className="text-right font-black text-slate-800 tabular-nums">
                           {formatCurrency(c.TotalSpent || 0)}
                        </td>
                        <td className="text-center">
                          <div className="flex items-center justify-center gap-1">
                             <Star className={cn("w-3 h-3", c.Points > 0 ? "text-amber-500 fill-amber-500" : "text-slate-200")} />
                             <span className="text-[11px] font-black text-slate-700 tabular-nums">{c.Points}</span>
                          </div>
                        </td>
                        <td className="text-right">
                           {editingCustomer.Id === c.Id ? <div className="w-1.5 h-6 bg-[#0078D4] rounded-sm ml-auto" /> : <ChevronRight className="w-4 h-4 text-slate-100 ml-auto" />}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                
                {filteredCustomers.length === 0 && (
                  <div className="py-32 text-center bg-white">
                     <Users className="w-12 h-12 text-slate-100 mx-auto mb-4" />
                     <p className="text-[11px] font-black text-slate-300 uppercase tracking-widest italic">DATABASE: Không tìm thấy dữ liệu phù hợp.</p>
                  </div>
                )}
              </div>
              
              <div className="p-4 bg-[#F0F0F0] border-t border-[#D1D1D1] flex items-center justify-between no-print">
                 <div className="flex items-center gap-6">
                    <div className="text-right">
                       <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Tổng doanh số nhóm</p>
                       <p className="text-[15px] font-black text-slate-700 italic tabular-nums leading-none">
                          {formatCurrency(customers.reduce((a, b) => a + (b.TotalSpent || 0), 0))}
                       </p>
                    </div>
                    <div className="h-8 w-[1px] bg-slate-300" />
                    <div className="flex flex-col">
                       <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Phiên làm việc</p>
                       <span className="text-[11px] font-black text-[#0078D4] uppercase italic">VERIFIED REGISTRY</span>
                    </div>
                 </div>
                 <div className="flex items-center gap-2 text-emerald-600 bg-white px-4 py-2 border border-slate-200 rounded-sm italic text-[11px] font-black">
                    <History className="w-3.5 h-3.5" /> {customers.length} RECORDS SYNCED
                 </div>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
}
