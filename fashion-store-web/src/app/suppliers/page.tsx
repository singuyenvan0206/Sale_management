"use client";

import { useState, useEffect, useMemo } from "react";
import { cn } from "@/lib/utils";
import { 
  Building, 
  Search, 
  RotateCcw, 
  Plus,
  Trash2,
  Edit2,
  Phone,
  Mail,
  MapPin,
  Factory,
  Save,
  CheckCircle2,
  AlertCircle,
  Truck,
  History,
  TrendingUp,
  ChevronRight,
  FileText,
  X,
  ArrowUpDown
} from "lucide-react";

interface Supplier {
  Id: number;
  Name: string;
  Phone: string;
  Email: string;
  Address: string;
  Description: string;
  CreatedDate: string;
}

export default function SuppliersPage() {
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [message, setMessage] = useState<{ text: string, type: 'success' | 'error' | null }>({ text: "", type: null });

  const [editingSupplier, setEditingSupplier] = useState<Supplier>({
    Id: 0,
    Name: "",
    Phone: "",
    Email: "",
    Address: "",
    Description: "",
    CreatedDate: new Date().toISOString()
  });

  const [searchTerm, setSearchTerm] = useState("");
  const [sortCol, setSortCol] = useState<'Name' | 'CreatedDate'>('Name');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');

  const loadSuppliers = async () => {
    setLoading(true);
    try {
      const res = await fetch("/api/suppliers");
      const json = await res.json();
      if (json.success && Array.isArray(json.data)) {
        setSuppliers(json.data);
      } else {
        showMsg(json.error || "Lỗi tải danh sách đối tác", 'error');
      }
    } catch (e) {
      showMsg("Lỗi: Không thể truy xuất cơ sở dữ liệu đối tác", 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSuppliers();
  }, []);

  const showMsg = (text: string, type: 'success' | 'error') => {
    setMessage({ text, type });
    setTimeout(() => setMessage({ text: "", type: null }), 3000);
  };

  const filteredSuppliers = useMemo(() => {
    if (!Array.isArray(suppliers)) return [];
    let result = suppliers.filter(s => {
      const search = searchTerm.toLowerCase();
      return !searchTerm || 
        s.Name.toLowerCase().includes(search) ||
        s.Phone.includes(search) ||
        (s.Email || "").toLowerCase().includes(search);
    });

    return result.sort((a, b) => {
      const valA = a[sortCol];
      const valB = b[sortCol];
      if (valA < valB) return sortOrder === 'asc' ? -1 : 1;
      if (valA > valB) return sortOrder === 'asc' ? 1 : -1;
      return 0;
    });
  }, [suppliers, searchTerm, sortCol, sortOrder]);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingSupplier.Name.trim()) {
      showMsg("Tên nhà cung cấp là trường bắt buộc", 'error');
      return;
    }

    setActionLoading(true);
    try {
      const isNew = editingSupplier.Id === 0;
      const res = await fetch("/api/suppliers", {
        method: isNew ? "POST" : "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(editingSupplier)
      });
      
      const json = await res.json();
      if (json.success) {
        showMsg(isNew ? "Đăng ký đối tác thành công" : "Cập nhật hồ sơ thành công", 'success');
        handleClear();
        loadSuppliers();
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
    if (editingSupplier.Id === 0) return;
    if (!confirm(`Xác nhận xóa hồ sơ đối tác "${editingSupplier.Name}" khỏi hệ thống?`)) return;

    setActionLoading(true);
    try {
      const res = await fetch(`/api/suppliers?id=${editingSupplier.Id}`, {
        method: "DELETE"
      });
      const json = await res.json();
      if (json.success) {
        showMsg("Đã xóa hồ sơ đối tác", 'success');
        handleClear();
        loadSuppliers();
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
    setEditingSupplier({
      Id: 0,
      Name: "",
      Phone: "",
      Email: "",
      Address: "",
      Description: "",
      CreatedDate: new Date().toISOString()
    });
  };

  const toggleSort = (col: typeof sortCol) => {
    if (sortCol === col) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortCol(col);
      setSortOrder('asc');
    }
  };

  if (loading && suppliers.length === 0) return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] gap-4 no-select uppercase italic font-black text-slate-400">
      <div className="w-12 h-12 border-4 border-[#0078D4] border-t-transparent rounded-full animate-spin" />
      <p className="text-[11px] tracking-widest">Đang tải danh mục chuỗi cung ứng...</p>
    </div>
  );

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-20 no-select">
      {/* WPF Header / Ribbon */}
      <div className="wpf-panel">
         <div className="wpf-panel-header uppercase">HỆ THỐNG QUẢN TRỊ CHUỖI CUNG ỨNG & ĐỐI TÁC (SUPPLY CHAIN HUB)</div>
         <div className="p-6 bg-white flex flex-col md:flex-row md:items-center justify-between gap-6">
            <div className="flex items-center gap-4">
               <div className="w-10 h-10 bg-[#0078D4] rounded-sm flex items-center justify-center text-white shadow-md">
                  <Factory className="w-6 h-6" />
               </div>
               <div>
                  <h2 className="text-[20px] font-black text-slate-900 tracking-tight uppercase italic leading-none">NHÀ CUNG CẤP</h2>
                  <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-widest italic tracking-tighter">Fusion ERP Supplier Registry Shell v2.5</p>
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
                    <Building className="w-4 h-4 text-[#0078D4]" /> BIÊN TẬP ĐỐI TÁC
                 </span>
                 <span className={cn(
                    "text-[9px] font-black px-2 py-0.5 rounded-sm border",
                    editingSupplier.Id === 0 ? "bg-amber-50 text-amber-600 border-amber-200" : "bg-blue-50 text-blue-600 border-blue-200"
                 )}>
                    {editingSupplier.Id === 0 ? "NEW PARTNER" : `ID: SUP-${editingSupplier.Id}`}
                 </span>
              </div>
              <form onSubmit={handleSave} className="p-6 bg-white space-y-5">
                 <div className="wpf-groupbox !mt-0">
                    <span className="wpf-groupbox-label">Thông tin định danh</span>
                    <div className="space-y-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Tên nhà cung cấp:</label>
                          <input 
                            type="text" 
                            required
                            value={editingSupplier.Name}
                            onChange={(e) => setEditingSupplier({...editingSupplier, Name: e.target.value})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-bold bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm uppercase italic"
                            placeholder="Tên công ty / NCC..." 
                          />
                       </div>
                       <div className="grid grid-cols-2 gap-4">
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Điện thoại:</label>
                             <input 
                               type="text" 
                               value={editingSupplier.Phone}
                               onChange={(e) => setEditingSupplier({...editingSupplier, Phone: e.target.value})}
                               className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm tabular-nums"
                               placeholder="09..." 
                             />
                          </div>
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Email:</label>
                             <input 
                               type="email" 
                               value={editingSupplier.Email}
                               onChange={(e) => setEditingSupplier({...editingSupplier, Email: e.target.value})}
                               className="w-full h-10 border border-[#D1D1D1] px-4 text-[12px] font-medium bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm"
                               placeholder="@mail.com" 
                             />
                          </div>
                       </div>
                    </div>
                 </div>

                 <div className="wpf-groupbox">
                    <span className="wpf-groupbox-label">Vị trí & Ghi chú</span>
                    <div className="space-y-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Địa chỉ văn phòng:</label>
                          <textarea 
                            rows={3}
                            value={editingSupplier.Address || ""}
                            onChange={(e) => setEditingSupplier({...editingSupplier, Address: e.target.value})}
                            className="w-full border border-[#D1D1D1] p-3 text-[12px] font-medium bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm resize-none italic"
                            placeholder="Số nhà, đường, khu công nghiệp..." 
                          />
                       </div>
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Mô tả nghiệp vụ:</label>
                          <input 
                            type="text" 
                            value={editingSupplier.Description || ""}
                            onChange={(e) => setEditingSupplier({...editingSupplier, Description: e.target.value})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[12px] font-medium bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm"
                            placeholder="Sản phẩm cung cấp chính..." 
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
                        editingSupplier.Id === 0 ? "btn-wpf-primary border-[#005A9E]" : "bg-amber-500 text-white border-amber-700 hover:bg-amber-600"
                      )}
                    >
                       {actionLoading ? <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" /> : (editingSupplier.Id === 0 ? <Plus className="w-5 h-5" /> : <Save className="w-5 h-5" />)}
                       {editingSupplier.Id === 0 ? "ĐĂNG KÝ ĐỐI TÁC" : "LƯU THAY ĐỔI"}
                    </button>
                    
                    <div className="grid grid-cols-2 gap-3">
                       {editingSupplier.Id !== 0 && (
                         <button 
                           type="button"
                           onClick={handleDelete}
                           disabled={actionLoading}
                           className="btn-wpf h-10 text-rose-600 border-rose-200 hover:bg-rose-50 flex items-center justify-center gap-2 text-[10px] uppercase font-black"
                         >
                            <Trash2 className="w-3.5 h-3.5" /> GỠ BỎ
                         </button>
                       )}
                       <button 
                         type="button"
                         onClick={handleClear}
                         className={cn(
                           "btn-wpf h-10 text-slate-500 border-slate-200 hover:bg-slate-50 flex items-center justify-center gap-2 text-[10px] uppercase font-black",
                           editingSupplier.Id === 0 ? "col-span-2" : "col-span-1"
                         )}
                       >
                          <RotateCcw className="w-3.5 h-3.5" /> {editingSupplier.Id === 0 ? "BIÊN TẬP LẠI" : "HỦY"}
                       </button>
                    </div>
                 </div>
              </form>
           </div>
        </div>

        {/* Right Column: Supplier Grid */}
        <div className="lg:col-span-8 space-y-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex flex-col md:flex-row md:items-center justify-between gap-4 py-3 h-auto">
                 <span className="flex items-center gap-2">
                    <FileText className="w-4 h-4" /> DANH SÁCH ĐỐI TÁC CUNG ỨNG
                 </span>
                 
                 <div className="relative group">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
                    <input 
                      type="text" 
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="h-8 border border-[#D1D1D1] pl-8 pr-4 text-[10px] font-bold text-slate-600 focus:border-[#0078D4] outline-none rounded-sm bg-[#F9F9F9] focus:bg-white w-[220px] uppercase"
                      placeholder="TÌM TÊN / SĐT / EMAIL..." 
                    />
                 </div>
              </div>
              
              <div className="overflow-x-auto">
                <table className="wpf-datagrid">
                  <thead>
                    <tr>
                      <th className="w-[80px]">MÃ</th>
                      <th className="cursor-pointer hover:bg-slate-100" onClick={() => toggleSort('Name')}>
                         NHÀ CUNG CẤP {sortCol === 'Name' && <ArrowUpDown className="w-3 h-3 inline ml-1" />}
                      </th>
                      <th>LIÊN HỆ</th>
                      <th className="max-w-[150px]">ĐỊA CHỈ</th>
                      <th className="w-[120px] text-center cursor-pointer hover:bg-slate-100" onClick={() => toggleSort('CreatedDate')}>
                         NGÀY TẠO {sortCol === 'CreatedDate' && <ArrowUpDown className="w-3 h-3 inline ml-1" />}
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredSuppliers.map((s: Supplier) => (
                      <tr 
                        key={s.Id} 
                        onClick={() => setEditingSupplier({...s})}
                        className={cn("cursor-pointer", editingSupplier.Id === s.Id ? "bg-[#E5F1FB]" : "")}
                      >
                        <td className="text-center font-bold text-slate-300 tabular-nums text-[10px]">SUP-{s.Id}</td>
                        <td>
                           <div className="flex flex-col">
                              <span className={cn("text-[13px] font-black uppercase italic leading-none mb-1", editingSupplier.Id === s.Id ? "text-[#0078D4]" : "text-slate-800")}>
                                 {s.Name}
                              </span>
                              <span className="text-[9px] font-bold text-slate-400 italic">{s.Description || "---"}</span>
                           </div>
                        </td>
                        <td>
                           <div className="flex flex-col leading-tight">
                              <span className="text-[11px] font-black text-slate-700">{s.Phone || "---"}</span>
                              <span className="text-[9px] font-medium text-slate-400">{s.Email || "---"}</span>
                           </div>
                        </td>
                        <td>
                           <p className="text-[10px] font-bold text-slate-400 line-clamp-1 italic">
                              {s.Address || "---"}
                           </p>
                        </td>
                        <td className="text-center text-[10px] font-bold text-slate-400 uppercase italic">
                           {new Date(s.CreatedDate).toLocaleDateString('vi-VN')}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              
              <div className="p-4 bg-[#F0F0F0] border-t border-[#D1D1D1] flex items-center justify-between">
                 <div className="flex items-center gap-6">
                    <div className="flex flex-col">
                       <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Mã hóa bảo mật</p>
                       <span className="text-[11px] font-black text-[#0078D4] uppercase italic">PARTNER NETWORK SECURE</span>
                    </div>
                 </div>
                 <div className="flex items-center gap-2 text-[#0078D4] bg-white px-4 py-2 border border-slate-200 rounded-sm italic text-[11px] font-black uppercase">
                    <History className="w-3.5 h-3.5" /> {suppliers.length} SUPPLIERS VERIFIED
                 </div>
              </div>
           </div>

           <div className="wpf-panel !bg-slate-800 text-white border-slate-700 p-6 shadow-md">
              <div className="flex items-center gap-4">
                 <div className="w-10 h-10 bg-white/10 rounded-sm flex items-center justify-center">
                    <TrendingUp className="w-6 h-6 text-amber-500" />
                 </div>
                 <div>
                    <h4 className="text-[14px] font-black uppercase italic leading-none mb-1">CHỈ SỐ CUNG CẦU</h4>
                    <p className="text-[9px] font-bold text-white/40 uppercase tracking-[0.2em]">Partner Reliability Index</p>
                 </div>
                 <div className="ml-auto text-right">
                    <span className="text-[24px] font-black text-emerald-400 italic">A+</span>
                    <p className="text-[8px] font-bold text-white/30 uppercase">System Rating</p>
                 </div>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
}
