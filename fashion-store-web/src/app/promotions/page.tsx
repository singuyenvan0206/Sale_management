"use client";

import { useState, useEffect, useMemo } from "react";
import { cn } from "@/lib/utils";
import { 
  Zap, 
  Search, 
  RotateCcw, 
  Plus, 
  Trash2,
  Edit2,
  Calendar,
  Tag,
  Package,
  Layers
} from "lucide-react";

export default function PromotionsPage() {
  const [promotions, setPromotions] = useState<any[]>([]);
  const [products, setProducts] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  // Form State
  const [editingPromo, setEditingPromo] = useState<any>({
    Id: 0,
    Name: "",
    Type: "FlashSale",
    DiscountPercent: 0,
    DiscountAmount: 0,
    StartDate: new Date().toISOString().split('T')[0],
    EndDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    TargetCategoryId: 0,
    RequiredProductId: 0
  });

  // Filter State
  const [searchTerm, setSearchTerm] = useState("");

  const [statusText, setStatusText] = useState("Sẵn sàng");

  useEffect(() => {
    const load = async () => {
      try {
        const [promoRes, prodRes, catRes] = await Promise.all([
          fetch("/api/promotions").then(res => res.json()),
          fetch("/api/products").then(res => res.json()),
          fetch("/api/categories").then(res => res.json())
        ]);
        if (Array.isArray(promoRes)) setPromotions(promoRes);
        if (Array.isArray(prodRes)) setProducts(prodRes);
        if (Array.isArray(catRes)) setCategories(catRes);
      } catch (e) {
        console.error("Failed to load promotion data", e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const filteredPromos = useMemo(() => {
    if (!Array.isArray(promotions)) return [];
    return promotions.filter(p => p.Name.toLowerCase().includes(searchTerm.toLowerCase()));
  }, [promotions, searchTerm]);

  const handleSelectPromo = (promo: any) => {
    setEditingPromo({
      ...promo,
      StartDate: new Date(promo.StartDate).toISOString().split('T')[0],
      EndDate: new Date(promo.EndDate).toISOString().split('T')[0]
    });
    setStatusText(`Đang chọn: ${promo.Name}`);
  };

  const handleClear = () => {
    setEditingPromo({
      Id: 0,
      Name: "",
      Type: "FlashSale",
      DiscountPercent: 0,
      DiscountAmount: 0,
      StartDate: new Date().toISOString().split('T')[0],
      EndDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
      TargetCategoryId: 0,
      RequiredProductId: 0
    });
    setStatusText("Mới");
  };

  if (loading) return <div className="p-20 text-center font-black animate-pulse">ĐANG TẢI CHƯƠNG TRÌNH KHUYẾN MÃI...</div>;

  return (
    <div className="space-y-6 animate-in fade-in duration-500 pb-20">
      <div className="bg-[#1CB5E0] p-6 -mx-8 -mt-8 mb-8 shadow-lg">
        <h2 className="text-[24px] font-bold text-white text-center tracking-tight">
           🎁 Chương Trình Khuyến Mãi
        </h2>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
        {/* Left Panel */}
        <div className="lg:col-span-1">
           <div className="bg-white p-6 rounded-[10px] shadow-sm space-y-6 sticky top-4 border-t-4 border-amber-500">
              <div className="flex items-center justify-between border-b pb-3">
                 <h3 className="text-[14px] font-black text-slate-800 uppercase italic">Thiết lập chiến dịch</h3>
                 <span className="text-[10px] font-bold text-amber-500 px-2 py-0.5 bg-amber-50 rounded uppercase">
                    {editingPromo.Id === 0 ? "Mới" : `ID: ${editingPromo.Id}`}
                 </span>
              </div>
              
              <div className="space-y-4">
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Tên chiến dịch *</label>
                    <input 
                      type="text" 
                      value={editingPromo.Name}
                      onChange={(e) => setEditingPromo({...editingPromo, Name: e.target.value})}
                      className="w-full bg-[#f8f9fa] rounded-md py-3 px-3 text-sm font-black focus:bg-white focus:ring-2 focus:ring-amber-100 outline-none transition-all" 
                      placeholder="Xả Kho Mùa Hè..." 
                    />
                 </div>
                 <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                       <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Giảm %</label>
                       <input 
                         type="number" 
                         value={editingPromo.DiscountPercent}
                         onChange={(e) => setEditingPromo({...editingPromo, DiscountPercent: Number(e.target.value)})}
                         className="w-full bg-[#f8f9fa] rounded-md py-2.5 px-3 text-sm font-black text-rose-600" 
                        />
                    </div>
                    <div className="space-y-1.5">
                       <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Giảm Tiền (đ)</label>
                       <input 
                         type="number" 
                         value={editingPromo.DiscountAmount}
                         onChange={(e) => setEditingPromo({...editingPromo, DiscountAmount: Number(e.target.value)})}
                         className="w-full bg-[#f8f9fa] rounded-md py-2.5 px-3 text-sm font-black text-emerald-600" 
                        />
                    </div>
                 </div>
                 <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                       <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Áp dụng từ</label>
                       <input 
                        type="date" 
                        value={editingPromo.StartDate}
                        onChange={(e) => setEditingPromo({...editingPromo, StartDate: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold" 
                       />
                    </div>
                    <div className="space-y-1.5">
                       <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Đến ngày</label>
                       <input 
                        type="date" 
                        value={editingPromo.EndDate}
                        onChange={(e) => setEditingPromo({...editingPromo, EndDate: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold" 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5 border-t pt-4">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Áp dụng cho Danh mục</label>
                    <select 
                      value={editingPromo.TargetCategoryId}
                      onChange={(e) => setEditingPromo({...editingPromo, TargetCategoryId: Number(e.target.value)})}
                      className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold"
                    >
                       <option value={0}>Tất cả danh mục (Default)</option>
                       {categories.map((c: any) => <option key={c.Id} value={c.Id}>{c.Name}</option>)}
                    </select>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Hoặc riêng Sản phẩm</label>
                    <select 
                      value={editingPromo.RequiredProductId}
                      onChange={(e) => setEditingPromo({...editingPromo, RequiredProductId: Number(e.target.value)})}
                      className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold"
                    >
                       <option value={0}>Không áp dụng cho sp lẻ</option>
                       {products.slice(0, 50).map((p: any) => <option key={p.Id} value={p.Id}>{p.Name}</option>)}
                    </select>
                 </div>
              </div>

              <div className="grid grid-cols-3 gap-2 pt-4">
                 <button className="bg-[#4CAF50] hover:bg-[#43a047] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-widest shadow-md">🌿 Thêm</button>
                 <button className="bg-[#2196F3] hover:bg-[#1e88e5] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-widest shadow-md">📝 Sửa</button>
                 <button className="bg-[#F44336] hover:bg-[#e53935] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-widest shadow-md">🗑️ Xóa</button>
              </div>
              <button onClick={handleClear} className="w-full bg-[#9E9E9E] text-white font-black py-3 rounded-md text-[10px] uppercase tracking-widest flex items-center justify-center gap-2">
                 <RotateCcw className="w-3.5 h-3.5" /> Làm mới
              </button>
           </div>
        </div>

        {/* Right Panel */}
        <div className="lg:col-span-3">
          <div className="bg-white rounded-[10px] shadow-sm overflow-hidden min-h-[700px] flex flex-col">
            <div className="p-6 bg-[#f8f9fa]/50">
               <h3 className="text-[18px] font-black text-[#1CB5E0] uppercase tracking-tight mb-6">🎁 Các Chiến Dịch Đang Chạy</h3>
               
               <div className="bg-[#F8F9FA] p-5 rounded-[8px]">
                  <div className="flex items-center gap-2">
                    <div className="flex-1 relative">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                        <input 
                        type="text" 
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        className="w-full h-11 bg-white rounded-md pl-10 pr-3 text-sm font-bold outline-none shadow-sm" 
                        placeholder="Tìm kiếm chiến dịch..." 
                        />
                    </div>
                    <button className="bg-[#FF9800] hover:bg-[#f57c00] text-white font-black h-11 px-10 rounded-md text-[11px] uppercase tracking-widest shadow-md">🔍 Lọc</button>
                  </div>
               </div>
            </div>

            <div className="flex-1 overflow-x-auto">
              <table className="w-full text-left">
                <thead className="bg-[#f8f9fa] sticky top-0 z-10 border-b border-slate-100">
                  <tr className="text-slate-800 text-[11px] font-black uppercase tracking-tight">
                    <th className="px-6 py-5"># ID</th>
                    <th className="px-6 py-5">🔥 Tên Chiến Dịch</th>
                    <th className="px-6 py-5 text-right">💰 Ưu Đãi</th>
                    <th className="px-6 py-5">🎯 Đối Tượng</th>
                    <th className="px-6 py-5 text-center">📅 Thời Hạn</th>
                    <th className="px-6 py-5 text-center">Trạng thái</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 italic">
                  {filteredPromos.map((p: any, idx: number) => {
                    const isActive = new Date(p.EndDate) > new Date();
                    return (
                      <tr 
                        key={p.Id} 
                        onClick={() => handleSelectPromo(p)}
                        className={cn(
                          "hover:bg-[#FFC107] hover:text-[#000] transition-all cursor-pointer group font-medium",
                          idx % 2 === 1 ? "bg-[#F8F9FA]" : "bg-white",
                          editingPromo.Id === p.Id ? "bg-[#FFC107] text-[#000] font-black shadow-lg" : "text-slate-700"
                        )}
                      >
                        <td className="px-6 py-5 font-bold text-[12px] opacity-40">{p.Id}</td>
                        <td className="px-6 py-5">
                            <span className="text-[14px] font-black uppercase tracking-tighter">{p.Name}</span>
                        </td>
                        <td className="px-6 py-5 text-right">
                           <span className={cn(
                              "font-black text-[15px]",
                              editingPromo.Id === p.Id ? "text-black" : "text-rose-600"
                           )}>
                              {p.DiscountPercent > 0 ? `-${p.DiscountPercent}%` : `-${new Intl.NumberFormat('vi-VN').format(p.DiscountAmount)}đ`}
                           </span>
                        </td>
                        <td className="px-6 py-5">
                           <div className="flex flex-col gap-0.5 text-[11px]">
                              {p.TargetCategoryId ? (
                                <span className="flex items-center gap-1 font-bold"><Layers className="w-3 h-3" /> {categories.find(c => c.Id === p.TargetCategoryId)?.Name || "Danh mục"}</span>
                              ) : p.RequiredProductId ? (
                                <span className="flex items-center gap-1 font-bold"><Package className="w-3 h-3" /> {products.find(prod => prod.Id === p.RequiredProductId)?.Name?.slice(0, 20) || "Sản phẩm"}...</span>
                              ) : <span className="opacity-50">Toàn cửa hàng</span>}
                           </div>
                        </td>
                        <td className="px-6 py-5 text-center text-[10px]">
                           <div className="flex flex-col">
                              <span>Từ: {new Date(p.StartDate).toLocaleDateString('vi-VN')}</span>
                              <span>Đến: {new Date(p.EndDate).toLocaleDateString('vi-VN')}</span>
                           </div>
                        </td>
                        <td className="px-6 py-5 text-center">
                           <div className={cn(
                              "inline-block w-3 h-3 rounded-full shadow-sm",
                              isActive ? "bg-emerald-500 animate-pulse" : "bg-slate-300"
                           )} />
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
            
            <div className="p-8 bg-[#F8F9FA] text-right">
               <span className="text-[12px] font-black text-[#1CB5E0] uppercase tracking-tighter bg-white shadow-sm p-4 rounded-xl">
                  Tổng cộng: {filteredPromos.length} chiến dịch
               </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
