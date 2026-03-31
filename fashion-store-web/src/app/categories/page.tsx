"use client";

import { useState, useEffect, useMemo } from "react";
import { cn } from "@/lib/utils";
import { 
  Layers, 
  Search, 
  RotateCcw, 
  Plus,
  Trash2,
  Edit2,
  Tag,
  Hash
} from "lucide-react";

export default function CategoriesPage() {
  const [categories, setCategories] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  // Form State
  const [editingCategory, setEditingCategory] = useState<any>({
    Id: 0,
    Name: "",
    Description: "",
    TaxPercent: 0
  });

  // Filter State
  const [searchTerm, setSearchTerm] = useState("");

  const [statusText, setStatusText] = useState("Sẵn sàng");

  useEffect(() => {
    const load = async () => {
      try {
        const res = await fetch("/api/categories");
        const data = await res.json();
        if (Array.isArray(data)) setCategories(data);
      } catch (e) {
        console.error("Failed to load categories", e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const filteredCategories = useMemo(() => {
    if (!Array.isArray(categories)) return [];
    return categories.filter(c => {
      return !searchTerm || 
        c.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (c.Description || "").toLowerCase().includes(searchTerm.toLowerCase());
    });
  }, [categories, searchTerm]);

  const handleSelectCategory = (category: any) => {
    setEditingCategory({...category});
    setStatusText(`Đang chọn: ${category.Name}`);
  };

  const handleClear = () => {
    setEditingCategory({
      Id: 0,
      Name: "",
      Description: "",
      TaxPercent: 0
    });
    setStatusText("Mới");
  };

  if (loading) return <div className="p-20 text-center font-black animate-pulse">ĐANG TẢI DANH MỤC SẢN PHẨM...</div>;

  return (
    <div className="space-y-6 animate-in fade-in duration-500 pb-20">
      {/* Header Bar - WPF style */}
      <div className="bg-[#1CB5E0] p-6 -mx-8 -mt-8 mb-8 shadow-lg">
        <h2 className="text-[24px] font-bold text-white text-center tracking-tight">
           📂 Danh Mục Sản Phẩm
        </h2>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
        {/* Left Panel - Input Form */}
        <div className="lg:col-span-1">
           <div className="bg-white p-6 rounded-[10px] shadow-sm space-y-6 sticky top-4 border-t-4 border-amber-400">
              <div className="flex items-center justify-between border-b pb-3">
                 <h3 className="text-[14px] font-black text-slate-800 uppercase italic tracking-tight">Chi tiết danh mục</h3>
                 <span className="text-[10px] font-bold text-amber-500 px-2 py-0.5 bg-amber-50 rounded uppercase">
                    {editingCategory.Id === 0 ? "Mới" : `ID: ${editingCategory.Id}`}
                 </span>
              </div>
              
              <div className="space-y-4">
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Tên danh mục *</label>
                    <div className="relative">
                       <Tag className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                       <input 
                        type="text" 
                        value={editingCategory.Name}
                        onChange={(e) => setEditingCategory({...editingCategory, Name: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 pl-10 pr-3 text-sm font-bold focus:bg-white focus:ring-2 focus:ring-amber-100 outline-none transition-all" 
                        placeholder="Quần Áo Nam..." 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Thuế suất (%)</label>
                    <div className="relative">
                       <Hash className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                       <input 
                        type="number" 
                        value={editingCategory.TaxPercent}
                        onChange={(e) => setEditingCategory({...editingCategory, TaxPercent: Number(e.target.value)})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 pl-10 pr-3 text-sm font-bold focus:bg-white outline-none" 
                        placeholder="0..." 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Mô tả thêm</label>
                    <textarea 
                      rows={3}
                      value={editingCategory.Description || ""}
                      onChange={(e) => setEditingCategory({...editingCategory, Description: e.target.value})}
                      className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold focus:bg-white outline-none transition-all resize-none" 
                      placeholder="Ghi chú về danh mục này..." 
                    />
                 </div>
              </div>

              <div className="grid grid-cols-2 gap-2 pt-4">
                 <button className="bg-[#4CAF50] hover:bg-[#43a047] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-widest shadow-md active:scale-95 flex items-center justify-center gap-2">
                    <Plus className="w-3.5 h-3.5" /> Thêm Danh Mục
                 </button>
                 <button className="bg-[#2196F3] hover:bg-[#1e88e5] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-widest shadow-md active:scale-95 flex items-center justify-center gap-2">
                    <Edit2 className="w-3.5 h-3.5" /> Cập nhật
                 </button>
                 <button className="col-span-2 bg-[#F44336] hover:bg-[#e53935] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-[0.2em] shadow-md active:scale-95 flex items-center justify-center gap-2">
                    <Trash2 className="w-3.5 h-3.5" /> Xóa danh mục
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

        {/* Right Panel - Category List */}
        <div className="lg:col-span-3">
          <div className="bg-white rounded-[10px] shadow-sm overflow-hidden min-h-[700px] flex flex-col">
            <div className="p-6 bg-[#f8f9fa]/50 border-b border-slate-50">
               <h3 className="text-[18px] font-black text-[#1CB5E0] uppercase tracking-tight mb-6">📂 Danh Mục Đang Vận Hành</h3>
               
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
                        placeholder="Tìm kiếm danh mục nhanh..." 
                        />
                    </div>
                    <button className="bg-[#FF9800] hover:bg-[#f57c00] text-white font-black h-11 px-10 rounded-md text-[12px] uppercase tracking-widest active:scale-95 shadow-md">🔍 Tìm</button>
                  </div>
               </div>
            </div>

            <div className="flex-1 overflow-x-auto">
              <table className="w-full text-left">
                <thead className="bg-[#f8f9fa] sticky top-0 z-10 border-b border-slate-100">
                  <tr className="text-slate-800 text-[11px] font-black uppercase tracking-tight">
                    <th className="px-6 py-5"># ID</th>
                    <th className="px-6 py-5">📁 Tên Danh Mục</th>
                    <th className="px-6 py-5">📝 Mô Tả Chi Tiết</th>
                    <th className="px-6 py-5 text-center">📊 Thuế Suất</th>
                    <th className="px-6 py-5 text-center">📅 Ngày tạo</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {filteredCategories.map((c: any, idx: number) => (
                    <tr 
                      key={c.Id} 
                      onClick={() => handleSelectCategory(c)}
                      className={cn(
                        "hover:bg-blue-600 hover:text-white transition-all cursor-pointer group font-medium",
                        idx % 2 === 1 ? "bg-[#F8F9FA]" : "bg-white",
                        editingCategory.Id === c.Id ? "bg-blue-600 text-white font-black shadow-lg" : "text-slate-700"
                      )}
                    >
                      <td className="px-6 py-5 font-bold text-[12px] opacity-40">{c.Id}</td>
                      <td className="px-6 py-5 font-black uppercase tracking-tighter text-[15px]">
                         {c.Name}
                      </td>
                      <td className="px-6 py-5 text-[13px] opacity-80">{c.Description || "---"}</td>
                      <td className="px-6 py-5 text-center font-black">
                         {c.TaxPercent || 0}%
                      </td>
                      <td className="px-6 py-5 text-center text-[12px] opacity-60">
                         {new Date(c.CreatedDate).toLocaleDateString('vi-VN')}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            
            <div className="p-8 bg-[#F8F9FA] text-right">
               <span className="text-[12px] font-black text-[#1CB5E0] uppercase tracking-tighter shadow-sm p-4 bg-white rounded-lg">
                  Tổng cộng: {filteredCategories.length} danh mục
               </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
