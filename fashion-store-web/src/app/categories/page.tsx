"use client";

import { useState, useEffect, useMemo } from "react";
import { cn, formatCurrency } from "@/lib/utils";
import { 
  Layers, 
  Search, 
  RotateCcw, 
  Plus,
  Trash2,
  Edit2,
  Tag,
  Hash,
  AlertCircle,
  CheckCircle2,
  ArrowUpDown,
  Calendar,
  MoreVertical,
  X,
  Save,
  ChevronRight,
  History,
  TrendingUp,
  FileText
} from "lucide-react";

interface Category {
  Id: number;
  Name: string;
  Description: string;
  TaxPercent: number;
  CreatedDate: string;
}

export default function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [message, setMessage] = useState<{ text: string, type: 'success' | 'error' | null }>({ text: "", type: null });

  const [editingCategory, setEditingCategory] = useState<Category>({
    Id: 0,
    Name: "",
    Description: "",
    TaxPercent: 0,
    CreatedDate: new Date().toISOString()
  });

  const [searchTerm, setSearchTerm] = useState("");
  const [sortCol, setSortCol] = useState<'Name' | 'TaxPercent' | 'CreatedDate'>('Name');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');

  const loadCategories = async () => {
    setLoading(true);
    try {
      const res = await fetch("/api/categories");
      const json = await res.json();
      if (json.success) setCategories(json.data);
    } catch (e) {
      showMsg("Lỗi: Không thể truy xuất danh mục sản phẩm", 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCategories();
  }, []);

  const showMsg = (text: string, type: 'success' | 'error') => {
    setMessage({ text, type });
    setTimeout(() => setMessage({ text: "", type: null }), 3000);
  };

  const filteredCategories = useMemo(() => {
    if (!Array.isArray(categories)) return [];
    let result = categories.filter(c => {
      const search = searchTerm.toLowerCase();
      return !searchTerm || 
        c.Name.toLowerCase().includes(search) ||
        (c.Description || "").toLowerCase().includes(search);
    });

    return result.sort((a, b) => {
      const valA = a[sortCol];
      const valB = b[sortCol];
      if (valA < valB) return sortOrder === 'asc' ? -1 : 1;
      if (valA > valB) return sortOrder === 'asc' ? 1 : -1;
      return 0;
    });
  }, [categories, searchTerm, sortCol, sortOrder]);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingCategory.Name.trim()) {
      showMsg("Tên danh mục là trường bắt buộc", 'error');
      return;
    }

    setActionLoading(true);
    try {
      const isNew = editingCategory.Id === 0;
      const res = await fetch("/api/categories", {
        method: isNew ? "POST" : "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(editingCategory)
      });
      
      const json = await res.json();
      if (json.success) {
        showMsg(isNew ? "Thêm danh mục thành công" : "Cập nhật thành công", 'success');
        handleClear();
        loadCategories();
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
    if (editingCategory.Id === 0) return;
    if (!confirm(`Xác nhận xóa danh mục "${editingCategory.Name}"?`)) return;

    setActionLoading(true);
    try {
      const res = await fetch(`/api/categories?id=${editingCategory.Id}`, {
        method: "DELETE"
      });
      const json = await res.json();
      if (json.success) {
        showMsg("Đã xóa danh mục", 'success');
        handleClear();
        loadCategories();
      } else {
        showMsg(json.error || "Không thể thực hiện", 'error');
      }
    } catch (e) {
      showMsg("Lỗi kết nối mạng", 'error');
    } finally {
      setActionLoading(false);
    }
  };

  const handleSelectCategory = (category: Category) => {
    setEditingCategory({...category});
  };

  const handleClear = () => {
    setEditingCategory({
      Id: 0,
      Name: "",
      Description: "",
      TaxPercent: 0,
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

  if (loading && categories.length === 0) return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] gap-4 no-select uppercase italic font-black text-slate-400">
      <div className="w-12 h-12 border-4 border-[#0078D4] border-t-transparent rounded-full animate-spin" />
      <p className="text-[11px] tracking-widest">Đang tải cấu trúc danh mục ERP...</p>
    </div>
  );

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-20 no-select">
      {/* WPF Header / Ribbon */}
      <div className="wpf-panel">
         <div className="wpf-panel-header uppercase">HỆ THỐNG PHÂN LOẠI HÀNG HÓA & THUẾ SUẤT (HIERARCHY REGISTRY)</div>
         <div className="p-6 bg-white flex flex-col md:flex-row md:items-center justify-between gap-6">
            <div className="flex items-center gap-4">
               <div className="w-10 h-10 bg-[#0078D4] rounded-sm flex items-center justify-center text-white shadow-md">
                  <Layers className="w-6 h-6" />
               </div>
               <div>
                  <h2 className="text-[20px] font-black text-slate-900 tracking-tight uppercase italic leading-none">DANH MỤC SẢN PHẨM</h2>
                  <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-widest italic tracking-tighter">Fusion ERP Organizational Subsystem v2.5</p>
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
                    <Tag className="w-4 h-4 text-[#0078D4]" /> CHI TIẾT PHÂN LOẠI
                 </span>
                 <span className={cn(
                    "text-[9px] font-black px-2 py-0.5 rounded-sm border",
                    editingCategory.Id === 0 ? "bg-amber-50 text-amber-600 border-amber-200" : "bg-blue-50 text-blue-600 border-blue-200"
                 )}>
                    {editingCategory.Id === 0 ? "NEW CATEGORY" : `ID: CAT-${editingCategory.Id}`}
                 </span>
              </div>
              <form onSubmit={handleSave} className="p-6 bg-white space-y-5">
                 <div className="wpf-groupbox !mt-0">
                    <span className="wpf-groupbox-label">Cấu hình danh mục</span>
                    <div className="space-y-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Tên danh mục hàng hóa:</label>
                          <input 
                            type="text" 
                            required
                            value={editingCategory.Name}
                            onChange={(e) => setEditingCategory({...editingCategory, Name: e.target.value})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-bold bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm uppercase italic"
                            placeholder="NHẬP TÊN..." 
                          />
                       </div>
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Thuế suất mặc định (%):</label>
                          <div className="relative">
                             <input 
                               type="number" 
                               value={editingCategory.TaxPercent}
                               onChange={(e) => setEditingCategory({...editingCategory, TaxPercent: Number(e.target.value)})}
                               className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm tabular-nums"
                               placeholder="0" 
                             />
                             <Hash className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-300" />
                          </div>
                       </div>
                    </div>
                 </div>

                 <div className="wpf-groupbox">
                    <span className="wpf-groupbox-label">Mô tả hệ thống</span>
                    <div className="pt-2">
                       <textarea 
                         rows={4}
                         value={editingCategory.Description || ""}
                         onChange={(e) => setEditingCategory({...editingCategory, Description: e.target.value})}
                         className="w-full border border-[#D1D1D1] p-3 text-[12px] font-medium bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm resize-none italic"
                         placeholder="Ghi chú chi tiết về nhóm hàng..." 
                       />
                    </div>
                 </div>

                 <div className="flex flex-col gap-3 pt-4">
                    <button 
                      type="submit"
                      disabled={actionLoading}
                      className={cn(
                        "btn-wpf h-12 flex items-center justify-center gap-3 uppercase font-black text-[11px] border-b-4",
                        editingCategory.Id === 0 ? "btn-wpf-primary border-[#005A9E]" : "bg-blue-600 text-white border-blue-800 hover:bg-blue-700"
                      )}
                    >
                       {actionLoading ? <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" /> : (editingCategory.Id === 0 ? <Plus className="w-5 h-5" /> : <Save className="w-5 h-5" />)}
                       {editingCategory.Id === 0 ? "TẠO DANH MỤC MỚI" : "LƯU THAY ĐỔI"}
                    </button>
                    
                    <div className="grid grid-cols-2 gap-3">
                       {editingCategory.Id !== 0 && (
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
                           editingCategory.Id === 0 ? "col-span-2" : "col-span-1"
                         )}
                       >
                          <RotateCcw className="w-3.5 h-3.5" /> {editingCategory.Id === 0 ? "XÓA TRẮNG FORM" : "HỦY BỎ"}
                       </button>
                    </div>
                 </div>
              </form>
           </div>
        </div>

        {/* Right Column: Hierarchy Grid */}
        <div className="lg:col-span-8 space-y-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex flex-col md:flex-row md:items-center justify-between gap-4 py-3 h-auto">
                 <span className="flex items-center gap-2">
                    <FileText className="w-4 h-4" /> DANH SÁCH PHÂN LOẠI (HIERARCHY)
                 </span>
                 
                 <div className="relative group">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
                    <input 
                      type="text" 
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="h-8 border border-[#D1D1D1] pl-8 pr-4 text-[10px] font-bold text-slate-600 focus:border-[#0078D4] outline-none rounded-sm bg-[#F9F9F9] focus:bg-white w-[220px] uppercase"
                      placeholder="TÌM DANH MỤC..." 
                    />
                 </div>
              </div>
              
              <div className="overflow-x-auto">
                <table className="wpf-datagrid">
                  <thead>
                    <tr>
                      <th className="w-[80px]">MÃ</th>
                      <th className="cursor-pointer hover:bg-slate-100" onClick={() => toggleSort('Name')}>
                         TÊN DANH MỤC {sortCol === 'Name' && <ArrowUpDown className="w-3 h-3 inline ml-1" />}
                      </th>
                      <th>MÔ TẢ CHI TIẾT</th>
                      <th className="w-[100px] text-center cursor-pointer hover:bg-slate-100" onClick={() => toggleSort('TaxPercent')}>
                         THUẾ {sortCol === 'TaxPercent' && <ArrowUpDown className="w-3 h-3 inline ml-1" />}
                      </th>
                      <th className="w-[120px] text-center cursor-pointer hover:bg-slate-100" onClick={() => toggleSort('CreatedDate')}>
                         NGÀY TẠO {sortCol === 'CreatedDate' && <ArrowUpDown className="w-3 h-3 inline ml-1" />}
                      </th>
                      <th className="text-right w-[60px]">EL</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredCategories.map((c: Category) => (
                      <tr 
                        key={c.Id} 
                        onClick={() => handleSelectCategory(c)}
                        className={cn("cursor-pointer", editingCategory.Id === c.Id ? "bg-[#E5F1FB]" : "")}
                      >
                        <td className="text-center font-bold text-slate-300 tabular-nums text-[10px]">{c.Id}</td>
                        <td>
                           <span className={cn("text-[13px] font-black uppercase italic leading-none", editingCategory.Id === c.Id ? "text-[#0078D4]" : "text-slate-800")}>
                              {c.Name}
                           </span>
                        </td>
                        <td>
                           <p className="text-[10px] font-bold text-slate-400 line-clamp-1 italic max-w-[200px]">
                              {c.Description || "---"}
                           </p>
                        </td>
                        <td className="text-center">
                           <span className="px-2 py-0.5 rounded-sm bg-slate-100 border border-slate-200 text-[10px] font-black text-slate-600 tabular-nums">
                              {c.TaxPercent}%
                           </span>
                        </td>
                        <td className="text-center text-[10px] font-bold text-slate-400 uppercase italic">
                           {new Date(c.CreatedDate).toLocaleDateString('vi-VN')}
                        </td>
                        <td className="text-right">
                           {editingCategory.Id === c.Id ? <div className="w-1.5 h-6 bg-[#0078D4] rounded-sm ml-auto" /> : <ChevronRight className="w-4 h-4 text-slate-100 ml-auto" />}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              
              <div className="p-4 bg-[#F0F0F0] border-t border-[#D1D1D1] flex items-center justify-between">
                 <div className="flex items-center gap-6">
                    <div className="text-right">
                       <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Cấp độ phân loại</p>
                       <span className="text-[11px] font-black text-[#0078D4] uppercase italic">VERIFIED REGISTRY</span>
                    </div>
                 </div>
                 <div className="flex items-center gap-2 text-[#0078D4] bg-white px-4 py-2 border border-slate-200 rounded-sm italic text-[11px] font-black uppercase">
                    <History className="w-3.5 h-3.5" /> {categories.length} CATEGORIES SYNCED
                 </div>
              </div>
           </div>

           <div className="wpf-panel !bg-slate-800 text-white border-slate-700 p-6 shadow-md">
              <div className="flex items-center gap-4">
                 <div className="w-10 h-10 bg-white/10 rounded-sm flex items-center justify-center">
                    <TrendingUp className="w-6 h-6 text-amber-500" />
                 </div>
                 <div>
                    <h4 className="text-[14px] font-black uppercase italic leading-none mb-1">MẬT ĐỘ DANH MỤC</h4>
                    <p className="text-[9px] font-bold text-white/40 uppercase tracking-[0.2em]">Efficiency Analysis</p>
                 </div>
                 <div className="ml-auto text-right">
                    <span className="text-[20px] font-black text-emerald-400 italic">OPTIMAL</span>
                 </div>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
}
