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
  AlertCircle,
  Database,
  Cpu,
  Monitor,
  ChevronRight,
  FileText,
  Warehouse,
  History,
  TrendingUp,
  X  
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
        const json = await res.json();
        if (json.success && Array.isArray(json.data)) {
          setProducts(json.data);
        } else {
          console.error("API error:", json.error);
        }
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

  if (loading) return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] gap-4 no-select uppercase italic font-black text-slate-400">
      <div className="w-12 h-12 border-4 border-[#0078D4] border-t-transparent rounded-full animate-spin" />
      <p className="text-[11px] tracking-widest">Đang tải cơ sở dữ liệu tra cứu...</p>
    </div>
  );

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-20 no-select">
      {/* WPF Header / Ribbon */}
      <div className="wpf-panel">
         <div className="wpf-panel-header uppercase">HỆ THỐNG TRA CỨU SẢN PHẨM & TỒN KHO (QUERY SUBSYSTEM)</div>
         <div className="p-6 bg-white flex flex-col md:flex-row md:items-center justify-between gap-6">
            <div className="flex items-center gap-4">
               <div className="w-10 h-10 bg-[#0078D4] rounded-sm flex items-center justify-center text-white shadow-md">
                  <Search className="w-6 h-6" />
               </div>
               <div>
                  <h2 className="text-[20px] font-black text-slate-900 tracking-tight uppercase italic leading-none">TRA CỨU HÀNG HÓA</h2>
                  <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-widest italic tracking-tighter">Fusion ERP Global Search Shell v2.5</p>
               </div>
            </div>
            
            <div className="flex items-center gap-2 text-emerald-600 bg-white px-4 py-2 border border-slate-200 rounded-sm italic text-[11px] font-black uppercase">
               <Database className="w-3.5 h-3.5" /> {products.length} RECORDS INDEXED
            </div>
         </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        {/* Left Column: Search Interface */}
        <div className="lg:col-span-4 space-y-6 lg:sticky lg:top-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex items-center gap-2">
                 <Monitor className="w-4 h-4 text-[#0078D4]" /> NHẬP LIỆU TRA CỨU
              </div>
              <div className="p-6 bg-white space-y-6">
                 <div className="wpf-groupbox !mt-0">
                    <span className="wpf-groupbox-label">Từ khóa hệ thống</span>
                    <div className="pt-2">
                       <div className="relative">
                          <input 
                            type="text" 
                            autoFocus
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full h-12 border border-[#D1D1D1] pl-10 pr-4 text-[15px] font-black bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm uppercase italic tabular-nums"
                            placeholder="MÃ SP / TÊN SẢN PHẨM..." 
                          />
                          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-300" />
                       </div>
                    </div>
                 </div>

                 <div className="space-y-2">
                    <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest italic border-b border-slate-100 pb-1">KẾT QUẢ GỢI Ý (MATCHES)</p>
                    <div className="space-y-1">
                       {results.map((p: any) => (
                         <button 
                           key={p.Id} 
                           onClick={() => { setSelectedProduct(p); setSearchTerm(""); }}
                           className="w-full h-12 flex items-center justify-between px-3 hover:bg-[#E5F1FB] border border-transparent hover:border-[#CCE4F7] transition-all group text-left rounded-sm"
                         >
                            <div className="flex items-center gap-3">
                               <Package className="w-4 h-4 text-slate-300 group-hover:text-[#0078D4]" />
                               <div className="flex flex-col">
                                  <span className="text-[12px] font-black uppercase text-slate-800 leading-none mb-0.5">{p.Name}</span>
                                  <span className="text-[9px] font-bold text-slate-400">{p.Code}</span>
                               </div>
                            </div>
                            <ChevronRight className="w-4 h-4 text-slate-200 group-hover:text-[#0078D4]" />
                         </button>
                       ))}
                       {searchTerm && results.length === 0 && (
                         <div className="py-8 text-center bg-slate-50 border border-dashed border-slate-200 rounded-sm italic text-[11px] font-bold text-slate-400 uppercase">
                            Không tìm thấy dữ liệu match.
                         </div>
                       )}
                       {!searchTerm && (
                         <div className="py-20 text-center flex flex-col items-center gap-3 opacity-20">
                            <Monitor className="w-12 h-12" />
                            <p className="text-[10px] font-black uppercase italic tracking-widest">SẴN SÀNG TRA CỨU</p>
                         </div>
                       )}
                    </div>
                 </div>
              </div>
           </div>
        </div>

        {/* Right Column: Query Results */}
        <div className="lg:col-span-8 space-y-6">
           {selectedProduct ? (
             <div className="space-y-6 animate-in fade-in slide-in-from-right-4 duration-300">
                <div className="wpf-panel shadow-md border-emerald-200">
                   <div className="wpf-panel-header !bg-[#0078D4] !text-white flex items-center justify-between">
                      <span className="flex items-center gap-2">
                         <FileText className="w-4 h-4" /> THÔNG TIN TRUY VẤN CHI TIẾT
                      </span>
                      <span className="text-[10px] font-black border border-white/20 px-2 py-0.5 rounded-sm">
                         SYSLOG: OK
                      </span>
                   </div>
                   <div className="p-8 bg-white grid grid-cols-1 md:grid-cols-2 gap-8">
                      {/* Sub-Group: Identity */}
                      <div className="wpf-groupbox !mt-0">
                         <span className="wpf-groupbox-label">Định danh sản phẩm</span>
                         <div className="space-y-4 pt-2">
                            <div className="flex items-center gap-4">
                               <div className="p-4 bg-slate-100 rounded-sm">
                                  <Package className="w-8 h-8 text-[#0078D4]" />
                               </div>
                               <div>
                                  <h3 className="text-[20px] font-black text-slate-900 uppercase italic leading-none">{selectedProduct.Name}</h3>
                                  <p className="text-[11px] font-black text-blue-600 mt-2 uppercase tracking-widest border-b border-blue-100 inline-block">{selectedProduct.Code}</p>
                               </div>
                            </div>
                            <div className="grid grid-cols-2 gap-4 pt-2">
                               <div className="flex flex-col">
                                  <span className="text-[9px] font-black text-slate-400 uppercase tracking-widest">Danh mục</span>
                                  <span className="text-[13px] font-black text-slate-800 uppercase">{selectedProduct.CategoryName || "---"}</span>
                               </div>
                               <div className="flex flex-col">
                                  <span className="text-[9px] font-black text-slate-400 uppercase tracking-widest">Đơn vị</span>
                                  <span className="text-[13px] font-black text-slate-800 uppercase">{selectedProduct.PurchaseUnit || "Cái/Chiếc"}</span>
                               </div>
                            </div>
                         </div>
                      </div>

                      {/* Sub-Group: Commercial */}
                      <div className="wpf-groupbox !mt-0 !border-emerald-200">
                         <span className="wpf-groupbox-label !text-emerald-600 !bg-white">Thông số vận hành</span>
                         <div className="space-y-4 pt-2">
                            <div className="grid grid-cols-2 gap-6">
                               <div className="flex flex-col">
                                  <span className="text-[9px] font-black text-slate-400 uppercase tracking-widest">Giá niêm yết</span>
                                  <span className="text-[22px] font-black text-slate-900 italic tabular-nums">{formatCurrency(selectedProduct.SalePrice || 0)}</span>
                               </div>
                               <div className="flex flex-col">
                                  <span className="text-[9px] font-black text-slate-400 uppercase tracking-widest">Tồn hiện tại</span>
                                  <span className={cn(
                                     "text-[22px] font-black italic tabular-nums",
                                     selectedProduct.StockQuantity > 10 ? "text-emerald-600" : "text-rose-600"
                                  )}>
                                     {selectedProduct.StockQuantity} <span className="text-[10px] uppercase font-black not-italic opacity-40">UNIT</span>
                                  </span>
                               </div>
                            </div>
                            <div className="flex items-center gap-2 pt-2 text-emerald-600">
                               <CheckCircle2 className="w-4 h-4" />
                               <span className="text-[10px] font-black uppercase tracking-widest">SẴN SÀNG CHO GIAO DỊCH LẺ</span>
                            </div>
                         </div>
                      </div>
                   </div>

                   <div className="px-8 pb-8 flex gap-3">
                      <button className="btn-wpf btn-wpf-primary h-12 flex-1 flex items-center justify-center gap-3 text-[11px] font-black uppercase border-b-4 border-[#005A9E]">
                         <ShoppingCart className="w-5 h-5" /> THÊM VÀO GIỎ HÀNG (POS)
                      </button>
                      <button 
                        onClick={() => setSelectedProduct(null)}
                        className="btn-wpf h-12 w-12 flex items-center justify-center text-slate-400 border-slate-200 hover:bg-slate-50"
                      >
                         <X className="w-5 h-5" />
                      </button>
                   </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                   <div className="wpf-panel p-6 bg-slate-900 border-slate-700 text-white">
                      <div className="flex items-center gap-4 mb-4">
                         <div className="w-10 h-10 bg-white/10 rounded-sm flex items-center justify-center border border-white/20">
                            <Warehouse className="w-6 h-6 text-[#0078D4]" />
                         </div>
                         <div>
                            <h4 className="text-[14px] font-black uppercase italic leading-none mb-1">KIỂM SOÁT KHO</h4>
                            <p className="text-[9px] font-bold text-white/40 uppercase tracking-[0.2em]">Storage Location Insight</p>
                         </div>
                      </div>
                      <p className="text-[12px] font-medium italic text-slate-300 mb-4 leading-relaxed uppercase tracking-tight">
                         VỊ TRÍ: KHU VỰC TRƯNG BÀY (A03)
                         <br/>
                         PHÂN LÔ: LÔ 24 - KỆ CHÍNH
                         <br/>
                         TRẠNG THÁI: KIỂM KÊ HOÀN TẤT
                      </p>
                   </div>

                   <div className="wpf-panel p-6 bg-white border-slate-200">
                      <div className="flex items-center gap-4 mb-4">
                         <div className="w-10 h-10 bg-slate-100 rounded-sm flex items-center justify-center border border-slate-200">
                            <History className="w-6 h-6 text-slate-400" />
                         </div>
                         <div>
                            <h4 className="text-[14px] font-black uppercase italic leading-none mb-1 text-slate-800">NHẬT KÝ TRA CỨU</h4>
                            <p className="text-[9px] font-bold text-slate-400 uppercase tracking-[0.2em]">Recent Access Log</p>
                         </div>
                      </div>
                      <div className="space-y-2">
                         <div className="flex justify-between text-[11px] font-bold text-slate-400 uppercase">
                            <span>Last Query:</span>
                            <span>{new Date().toLocaleTimeString()}</span>
                         </div>
                         <div className="flex justify-between text-[11px] font-bold text-slate-400 uppercase">
                            <span>User:</span>
                            <span className="text-[#0078D4]">ADMIN-POS-01</span>
                         </div>
                      </div>
                   </div>
                </div>
             </div>
           ) : (
             <div className="wpf-panel h-[500px] flex flex-col items-center justify-center opacity-10 grayscale no-select">
                <Search className="w-32 h-32 mb-6" />
                <h3 className="text-[28px] font-black uppercase tracking-[0.5em] italic">READY FOR QUERY</h3>
                <p className="text-[12px] font-bold mt-4 uppercase tracking-widest italic">Nhập dữ liệu vào khung tra cứu bên trái để bắt đầu truy vấn.</p>
             </div>
           )}
        </div>
      </div>
    </div>
  );
}
