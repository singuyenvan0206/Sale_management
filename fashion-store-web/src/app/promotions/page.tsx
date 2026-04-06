"use client";

import { useState, useEffect, useMemo } from "react";
import { cn, formatCurrency } from "@/lib/utils";
import { 
  Zap, 
  Search, 
  Plus, 
  Trash2,
  Calendar,
  Tag,
  Package,
  Layers,
  CheckCircle2,
  Info,
  Clock,
  ChevronRight,
  BarChart3,
  Monitor,
  Database,
  History,
  TrendingUp,
  FileText
} from "lucide-react";

export default function PromotionsPage() {
  const [promotions, setPromotions] = useState<any[]>([]);
  const [products, setProducts] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");

  const [editingPromo, setEditingPromo] = useState<any>({
    Id: 0,
    Name: "",
    Type: "FlashSale",
    DiscountPercent: 0,
    DiscountValue: 0,
    StartDate: new Date().toISOString().split('T')[0],
    EndDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    TargetCategoryId: 0,
    RequiredProductId: 0,
    IsActive: true
  });

  useEffect(() => {
    const load = async () => {
      try {
        const [promoRes, prodRes, catRes] = await Promise.all([
          fetch("/api/promotions").then(res => res.json()),
          fetch("/api/products").then(res => res.json()),
          fetch("/api/categories").then(res => res.json())
        ]);
        
        if (promoRes.success) setPromotions(promoRes.data);
        if (prodRes.success) setProducts(prodRes.data);
        if (catRes.success) setCategories(catRes.data);
      } catch (e) {
        console.error("Failed to load promotion data", e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const filteredPromos = useMemo(() => {
    return promotions.filter(p => p.Name.toLowerCase().includes(searchTerm.toLowerCase()));
  }, [promotions, searchTerm]);

  const handleSelect = (p: any) => {
    setEditingPromo({
      ...p,
      StartDate: new Date(p.StartDate).toISOString().split('T')[0],
      EndDate: new Date(p.EndDate).toISOString().split('T')[0]
    });
  };

  const handleClear = () => {
    setEditingPromo({
      Id: 0,
      Name: "",
      Type: "FlashSale",
      DiscountPercent: 0,
      DiscountValue: 0,
      StartDate: new Date().toISOString().split('T')[0],
      EndDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
      TargetCategoryId: 0,
      RequiredProductId: 0,
      IsActive: true
    });
  };

  const handleSave = async () => {
    if (!editingPromo.Name) return alert("Vui lòng nhập tên chiến dịch");
    const isNew = editingPromo.Id === 0;
    try {
      const res = await fetch("/api/promotions", {
        method: isNew ? "POST" : "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(editingPromo)
      });
      const json = await res.json();
      if (json.success) {
        handleClear();
        const refresh = await fetch("/api/promotions").then(res => res.json());
        if (refresh.success) setPromotions(refresh.data);
      } else {
        alert("❌ Lỗi: " + json.error);
      }
    } catch (e) {
      alert("❌ Lỗi kết nối");
    }
  };

  const handleDelete = async () => {
    if (editingPromo.Id === 0) return;
    if (!confirm("Xác nhận gỡ bỏ chiến dịch này khỏi hệ thống?")) return;
    try {
      const res = await fetch(`/api/promotions?id=${editingPromo.Id}`, { method: "DELETE" });
      const json = await res.json();
      if (json.success) {
        handleClear();
        const refresh = await fetch("/api/promotions").then(res => res.json());
        if (refresh.success) setPromotions(refresh.data);
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
      <p className="text-[11px] tracking-widest">Đang tải cấu trúc chiến dịch Marketing...</p>
    </div>
  );

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-20 no-select">
      {/* WPF Header / Ribbon */}
      <div className="wpf-panel">
         <div className="wpf-panel-header uppercase">HỆ THỐNG QUẢN LÝ CHIẾN DỊCH & ƯU ĐÃI (CAMPAIGN ENGINE)</div>
         <div className="p-6 bg-white flex flex-col md:flex-row md:items-center justify-between gap-6">
            <div className="flex items-center gap-4">
               <div className="w-10 h-10 bg-[#0078D4] rounded-sm flex items-center justify-center text-white shadow-md">
                  <Zap className="w-6 h-6 fill-white" />
               </div>
               <div>
                  <h2 className="text-[20px] font-black text-slate-900 tracking-tight uppercase italic leading-none">QUẢN TRỊ KHUYẾN MÃI</h2>
                  <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-widest italic tracking-tighter">Fusion ERP Marketing Service v2.5</p>
               </div>
            </div>
            
            <div className="flex items-center gap-4 bg-amber-50 border border-amber-200 px-6 py-2 shadow-sm rounded-sm">
               <div className="flex items-center gap-3">
                  <BarChart3 className="w-4 h-4 text-amber-600" />
                  <span className="text-[11px] font-black uppercase text-amber-700 tracking-widest tabular-nums">{promotions.filter(p => new Date(p.EndDate) > new Date()).length} EVENTS ON-AIR</span>
               </div>
            </div>
         </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        {/* Left Column: Form Editor */}
        <div className="lg:col-span-4 space-y-6 lg:sticky lg:top-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex items-center justify-between">
                 <span className="flex items-center gap-2">
                    <Tag className="w-4 h-4 text-[#0078D4]" /> THÔNG SỐ CHIẾN DỊCH
                 </span>
                 <span className={cn(
                    "text-[9px] font-black px-2 py-0.5 rounded-sm border",
                    editingPromo.Id === 0 ? "bg-amber-50 text-amber-600 border-amber-200" : "bg-blue-50 text-blue-600 border-blue-200"
                 )}>
                    {editingPromo.Id === 0 ? "NEW EVENT" : `ID: PRM-${editingPromo.Id}`}
                 </span>
              </div>
              <div className="p-6 bg-white space-y-5">
                 <div className="wpf-groupbox !mt-0">
                    <span className="wpf-groupbox-label">Định dạng sự kiện</span>
                    <div className="space-y-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Tên chiến dịch quảng bá:</label>
                          <input 
                            type="text" 
                            required
                            value={editingPromo.Name}
                            onChange={(e) => setEditingPromo({...editingPromo, Name: e.target.value.toUpperCase()})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm uppercase tracking-widest italic"
                            placeholder="NHẬP TÊN SỰ KIỆN..." 
                          />
                       </div>
                       <div className="grid grid-cols-2 gap-4">
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Chiết khấu (%):</label>
                             <input 
                               type="number" 
                               value={editingPromo.DiscountPercent}
                               onChange={(e) => setEditingPromo({...editingPromo, DiscountPercent: Number(e.target.value)})}
                               className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-rose-500 outline-none rounded-sm tabular-nums text-rose-500"
                             />
                          </div>
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Giảm tiền mặt:</label>
                             <input 
                               type="number" 
                               value={editingPromo.DiscountValue}
                               onChange={(e) => setEditingPromo({...editingPromo, DiscountValue: Number(e.target.value)})}
                               className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-emerald-500 outline-none rounded-sm tabular-nums text-emerald-600"
                             />
                          </div>
                       </div>
                    </div>
                 </div>

                 <div className="wpf-groupbox">
                    <span className="wpf-groupbox-label">Hiệu lực thời gian</span>
                    <div className="grid grid-cols-2 gap-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Bắt đầu:</label>
                          <input 
                            type="date" 
                            value={editingPromo.StartDate}
                            onChange={(e) => setEditingPromo({...editingPromo, StartDate: e.target.value})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[11px] font-bold bg-[#F9F9F9] rounded-sm"
                          />
                       </div>
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Kết thúc:</label>
                          <input 
                            type="date" 
                            value={editingPromo.EndDate}
                            onChange={(e) => setEditingPromo({...editingPromo, EndDate: e.target.value})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[11px] font-bold bg-[#F9F9F9] rounded-sm"
                          />
                       </div>
                    </div>
                 </div>

                 <div className="wpf-groupbox">
                    <span className="wpf-groupbox-label">Đối tượng tác động</span>
                    <div className="space-y-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Theo danh mục sản phẩm:</label>
                          <select 
                            value={editingPromo.TargetCategoryId}
                            onChange={(e) => setEditingPromo({...editingPromo, TargetCategoryId: Number(e.target.value)})}
                            className="w-full h-10 border border-[#D1D1D1] px-3 text-[11px] font-black bg-white focus:border-[#0078D4] outline-none rounded-sm cursor-pointer uppercase"
                          >
                             <option value={0}>TOÀN BỘ CỬA HÀNG (GLOBAL)</option>
                             {categories.map((c: any) => <option key={c.Id} value={c.Id}>{c.Name.toUpperCase()}</option>)}
                          </select>
                       </div>
                    </div>
                 </div>

                 <div className="flex flex-col gap-3 pt-4">
                    <button 
                      onClick={handleSave}
                      className="btn-wpf btn-wpf-primary h-12 flex items-center justify-center gap-3 uppercase font-black text-[11px] border-b-4 border-[#005A9E]"
                    >
                       <Zap className="w-5 h-5 fill-white" />
                       {editingPromo.Id === 0 ? "KÍCH HOẠT CHIẾN DỊCH" : "LƯU CẬP NHẬT TRẠNG THÁI"}
                    </button>
                    
                    <div className="grid grid-cols-2 gap-3">
                       {editingPromo.Id !== 0 && (
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
                           editingPromo.Id === 0 ? "col-span-2" : "col-span-1"
                         )}
                       >
                          <RotateCcw className="w-3.5 h-3.5" /> {editingPromo.Id === 0 ? "XÓA TRẮNG FORM" : "HỦY BỎ"}
                       </button>
                    </div>
                 </div>
              </div>
              <div className="p-4 bg-[#F0F0FF] border-t border-[#D1D1D1] flex items-start gap-3 text-blue-800">
                 <Info className="w-4 h-4 shrink-0 mt-0.5" />
                 <p className="text-[10px] font-bold italic uppercase leading-relaxed">Giá sản phẩm tại POS sẽ tự động cập nhật theo cấu hình On-Air này.</p>
              </div>
           </div>
        </div>

        {/* Right Column: Registry List */}
        <div className="lg:col-span-8 space-y-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex flex-col md:flex-row md:items-center justify-between gap-4 py-3 h-auto">
                 <span className="flex items-center gap-2">
                    <FileText className="w-4 h-4" /> CAMPAIGN REGISTRY DATABASE
                 </span>
                 
                 <div className="relative group">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
                    <input 
                      type="text" 
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="h-8 border border-[#D1D1D1] pl-8 pr-4 text-[10px] font-bold text-slate-600 focus:border-[#0078D4] outline-none rounded-sm bg-[#F9F9F9] focus:bg-white w-[220px] uppercase"
                      placeholder="TRA CỨU CHIẾN DỊCH..." 
                    />
                 </div>
              </div>
              
              <div className="overflow-x-auto">
                <table className="wpf-datagrid">
                  <thead>
                    <tr>
                      <th className="w-[180px]">CHIẾN DỊCH QUẢNG BÁ</th>
                      <th className="text-right w-[120px]">MỨC ƯU ĐÃI</th>
                      <th className="text-center w-[140px]">PHẠM VI ÁP DỤNG</th>
                      <th className="text-center w-[120px]">HẾT HẠN</th>
                      <th className="text-center w-[100px]">STATUS</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredPromos.map((p: any) => {
                       const isLive = new Date(p.EndDate) > new Date();
                       return (
                         <tr 
                           key={p.Id} 
                           onClick={() => handleSelect(p)}
                           className={cn("cursor-pointer", editingPromo.Id === p.Id ? "bg-[#E5F1FB]" : "")}
                         >
                           <td>
                              <span className={cn("text-[13px] font-black uppercase italic tracking-tighter leading-none", editingPromo.Id === p.Id ? "text-[#0078D4]" : "text-slate-800")}>
                                 {p.Name}
                              </span>
                           </td>
                           <td className="text-right">
                              <span className="text-[14px] font-black text-rose-500 tabular-nums italic">
                                 {p.DiscountPercent > 0 ? `-${p.DiscountPercent}%` : `-${formatCurrency(p.DiscountValue)}`}
                              </span>
                           </td>
                           <td className="text-center">
                              {p.TargetCategoryId ? (
                                 <span className="flex items-center justify-center gap-1.5 text-[9px] font-black text-slate-400 uppercase tracking-widest bg-slate-100/50 py-1 rounded-sm border border-slate-100">
                                    <Layers className="w-3 h-3" /> CATEGORY
                                 </span>
                              ) : (
                                 <span className="text-[9px] font-black text-slate-300 uppercase tracking-widest italic">STORE WIDE</span>
                              )}
                           </td>
                           <td className="text-center text-[10px] font-bold text-slate-400 uppercase italic">
                              {new Date(p.EndDate).toLocaleDateString('vi-VN')}
                           </td>
                           <td className="text-center">
                              <div className={cn(
                                "inline-flex items-center gap-1.5 px-2 py-0.5 rounded-sm text-[8px] font-black uppercase tracking-widest border",
                                isLive && p.IsActive ? "bg-emerald-50 text-emerald-600 border-emerald-200" : "bg-slate-50 text-slate-300 border-slate-200"
                              )}>
                                 {isLive && p.IsActive && <div className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />}
                                 {isLive && p.IsActive ? "BROADCASTING" : "OFF-AIR"}
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
                       <span className="text-[11px] font-black text-[#0078D4] uppercase italic">MARKETING HUB v2.5</span>
                    </div>
                 </div>
                 <div className="flex items-center gap-2 text-emerald-600 bg-white px-4 py-2 border border-slate-200 rounded-sm italic text-[11px] font-black uppercase">
                    <Database className="w-3.5 h-3.5" /> {promotions.length} EVENTS LOADED
                 </div>
              </div>
           </div>

           <div className="wpf-panel p-6 bg-slate-100 border-slate-200 shadow-md">
              <div className="flex items-center gap-4">
                 <div className="w-10 h-10 bg-[#0078D4] rounded-sm flex items-center justify-center">
                    <TrendingUp className="w-6 h-6 text-white" />
                 </div>
                 <div>
                    <h4 className="text-[14px] font-black uppercase italic leading-none mb-1 text-slate-800">TỈ LỆ CHUYỂN ĐỔI</h4>
                    <p className="text-[9px] font-bold text-slate-400 uppercase tracking-[0.2em]">Campaign Performance Index</p>
                 </div>
                 <div className="ml-auto text-right">
                    <span className="text-[20px] font-black text-[#0078D4] italic tabular-nums">92.4%</span>
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
