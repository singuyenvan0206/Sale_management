"use client";

import { useState, useEffect, useMemo } from "react";
import { cn, formatCurrency } from "@/lib/utils";
import { 
  Search, 
  Package, 
  Tag, 
  Layers, 
  Eye, 
  ShoppingCart,
  ArrowRight,
  Filter,
  CheckCircle2,
  AlertCircle
} from "lucide-react";

export default function SearchPage() {
  const [products, setProducts] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedProduct, setSelectedProduct] = useState<any>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const res = await fetch("/api/products");
        const data = await res.json();
        setProducts(data);
      } catch (e) {
        console.error("Failed to load products", e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const results = useMemo(() => {
    if (!searchTerm) return [];
    return products.filter(p => 
      p.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      p.Code.toLowerCase().includes(searchTerm.toLowerCase())
    ).slice(0, 10);
  }, [products, searchTerm]);

  if (loading) return <div className="p-20 text-center font-black animate-pulse">ĐANG KHỞI TẠO HỆ THỐNG TRA CỨU...</div>;

  return (
    <div className="space-y-6 animate-in fade-in duration-500 pb-20 max-w-6xl mx-auto">
      {/* Header Bar - WPF style */}
      <div className="bg-[#1CB5E0] p-6 -mx-8 -mt-8 mb-8 shadow-lg">
        <h2 className="text-[24px] font-bold text-white text-center tracking-tight">
           🔎 Tra Cứu Sản Phẩm & Tồn Kho
        </h2>
      </div>

      {/* Main Search Area */}
      <div className="bg-white p-12 rounded-2xl shadow-sm border border-slate-100 flex flex-col items-center gap-8">
         <div className="text-center space-y-2">
            <h3 className="text-[18px] font-black text-slate-800 uppercase tracking-tighter">Bạn đang tìm kiếm sản phẩm nào?</h3>
            <p className="text-[12px] font-bold text-slate-400 uppercase italic">Nhập tên hoặc mã sản phẩm để bắt đầu tra cứu nhanh</p>
         </div>

         <div className="w-full max-w-3xl relative">
            <Search className="absolute left-6 top-1/2 -translate-y-1/2 w-6 h-6 text-[#1CB5E0]" />
            <input 
              type="text" 
              autoFocus
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full h-20 bg-[#F8F9FA] rounded-[60px] pl-16 pr-8 text-[20px] font-black placeholder:text-slate-300 outline-none focus:ring-4 focus:ring-blue-100 transition-all shadow-inner uppercase tracking-tight" 
              placeholder="VÍ DỤ: ÁO SƠ MI, SP001..." 
            />
            {searchTerm && (
               <div className="absolute top-full left-0 right-0 z-50 mt-4 bg-white rounded-2xl shadow-2xl border border-slate-100 overflow-hidden divide-y divide-slate-50 animate-in slide-in-from-top-4">
                  {results.map((p: any) => (
                    <button 
                      key={p.Id} 
                      onClick={() => { setSelectedProduct(p); setSearchTerm(""); }}
                      className="w-full p-6 hover:bg-[#1CB5E0] hover:text-white transition-all flex items-center justify-between text-left group"
                    >
                       <div className="flex items-center gap-4">
                          <div className="w-12 h-12 bg-slate-50 group-hover:bg-white/20 rounded-xl flex items-center justify-center font-black text-[#1CB5E0] group-hover:text-white transition-colors">
                             <Package className="w-6 h-6" />
                          </div>
                          <div>
                             <p className="text-[14px] font-black uppercase tracking-tighter">{p.Name}</p>
                             <p className="text-[10px] font-bold opacity-50 uppercase tracking-widest">{p.Code}</p>
                          </div>
                       </div>
                       <div className="text-right">
                          <p className="text-[16px] font-black">{formatCurrency(p.SalePrice)}</p>
                          <p className="text-[10px] font-bold opacity-70">Tồn kho: {p.StockQuantity}</p>
                       </div>
                    </button>
                  ))}
                  {results.length === 0 && (
                    <div className="p-8 text-center text-slate-400 font-bold italic">Không tìm thấy sản phẩm nào...</div>
                  )}
               </div>
            )}
         </div>
      </div>

      {/* Result Detail View */}
      {selectedProduct ? (
         <div className="grid grid-cols-1 md:grid-cols-2 gap-6 animate-in zoom-in-95 duration-300">
            {/* Left Detail */}
            <div className="bg-white p-10 rounded-2xl shadow-sm space-y-8">
               <div className="flex items-center gap-4 border-b pb-6">
                  <div className="p-4 bg-blue-50 text-[#1CB5E0] rounded-[20px]">
                     <Package className="w-8 h-8" />
                  </div>
                  <div>
                     <h3 className="text-[24px] font-black text-slate-900 uppercase tracking-tight leading-none">{selectedProduct.Name}</h3>
                     <p className="text-[14px] font-bold text-[#1CB5E0] mt-2 uppercase tracking-[0.2em]">{selectedProduct.Code}</p>
                  </div>
               </div>

               <div className="grid grid-cols-2 gap-8">
                  <div className="space-y-1">
                     <p className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Giá bán lẻ</p>
                     <p className="text-[28px] font-black text-slate-900">{formatCurrency(selectedProduct.SalePrice)}</p>
                  </div>
                  <div className="space-y-1">
                     <p className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Tồn kho hiện hữu</p>
                     <p className={cn(
                        "text-[28px] font-black",
                        selectedProduct.StockQuantity > 10 ? "text-emerald-500" : "text-rose-600"
                     )}>
                        {selectedProduct.StockQuantity} <span className="text-[12px] opacity-40 uppercase">Sản phẩm</span>
                     </p>
                  </div>
               </div>

               <div className="space-y-4 pt-6 border-t font-medium text-slate-600">
                  <div className="flex items-center justify-between text-[13px]">
                     <span className="flex items-center gap-2 uppercase tracking-widest font-black text-slate-400"><Layers className="w-3.5 h-3.5" /> Danh mục</span>
                     <span className="font-black text-slate-900 uppercase">{selectedProduct.CategoryName || "Đang cập nhật"}</span>
                  </div>
                  <div className="flex items-center justify-between text-[13px]">
                     <span className="flex items-center gap-2 uppercase tracking-widest font-black text-slate-400"><Tag className="w-3.5 h-3.5" /> Đơn vị tính</span>
                     <span className="font-bold">{selectedProduct.PurchaseUnit || "Sản phẩm"}</span>
                  </div>
               </div>

               <div className="pt-8">
                  <button className="w-full h-16 bg-[#1CB5E0] hover:bg-[#1e90ff] text-white font-black rounded-xl text-[14px] uppercase tracking-widest shadow-lg active:scale-95 flex items-center justify-center gap-3 transition-all">
                     <ShoppingCart className="w-5 h-5" /> Thêm vào đơn hàng
                  </button>
               </div>
            </div>

            {/* Right Status */}
            <div className="space-y-6">
               <div className="bg-white p-8 rounded-2xl shadow-sm border-t-4 border-emerald-500">
                  <div className="flex items-start gap-4">
                     <div className="p-3 bg-emerald-50 text-emerald-600 rounded-xl">
                        <CheckCircle2 className="w-6 h-6" />
                     </div>
                     <div>
                        <h4 className="text-[15px] font-black text-slate-900 uppercase tracking-tight">Trạng thái kinh doanh</h4>
                        <p className="text-[13px] text-slate-500 mt-1">Sản phẩm đang được bán tại chi nhánh mặc định và có đủ số lượng đáp ứng đơn hàng lẻ.</p>
                     </div>
                  </div>
               </div>

               <div className="bg-slate-900 p-8 rounded-2xl text-white relative overflow-hidden group">
                  <div className="absolute top-0 right-0 w-32 h-32 bg-[#1CB5E0]/10 rounded-full blur-3xl -mr-16 -mt-16" />
                  <p className="text-[10px] font-black text-slate-500 uppercase tracking-[0.2em]">Thông tin bổ sung</p>
                  <p className="mt-4 text-[13px] font-medium italic text-slate-300 leading-relaxed">
                     Lần cuối cập nhật giá: {new Date().toLocaleDateString('vi-VN')}
                     <br/>
                     Vị trí kho: Khu vực A - Kệ 03
                  </p>
                  <button className="mt-8 flex items-center gap-2 text-[11px] font-black uppercase text-[#1CB5E0] hover:translate-x-2 transition-all">
                     Xem lịch sử giao dịch <ArrowRight className="w-4 h-4" />
                  </button>
               </div>
            </div>
         </div>
      ) : (
         <div className="py-32 text-center opacity-10 animate-pulse">
            <Package className="w-32 h-32 mx-auto mb-4" />
            <h3 className="text-[24px] font-black uppercase tracking-[0.5em]">KẾT QUẢ SẼ HIỆN TẠI ĐÂY</h3>
         </div>
      )}
    </div>
  );
}
