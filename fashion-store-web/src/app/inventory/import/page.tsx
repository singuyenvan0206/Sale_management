"use client";

import { useState, useEffect, useMemo } from "react";
import { cn, formatCurrency } from "@/lib/utils";
import { 
  FilePlus, 
  Search, 
  Plus, 
  Truck, 
  Package, 
  Calendar, 
  DollarSign, 
  ArrowRight,
  TrendingDown,
  Clock,
  History,
  Layers,
  ChevronRight,
  AlertCircle,
  X,
  PlusCircle,
  Trash2,
  CheckCircle2,
  ArrowLeft
} from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";

interface Product {
  Id: number;
  Name: string;
  Code: string;
  StockQuantity: number;
  PurchasePrice: number;
}

interface Supplier {
  Id: number;
  Name: string;
}

interface ImportItem {
  ProductId: number;
  ProductName: string;
  ProductCode: string;
  Quantity: number;
  PurchasePrice: number;
}

export default function ImportInventoryPage() {
  const router = useRouter();
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  
  // Selection & Form State
  const [selectedSupplierId, setSelectedSupplierId] = useState<string>("");
  const [importItems, setImportItems] = useState<ImportItem[]>([]);
  const [notes, setNotes] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [message, setMessage] = useState<{ text: string, type: 'success' | 'error' | null }>({ text: "", type: null });

  useEffect(() => {
    const loadInitialData = async () => {
      try {
        const [suppRes, prodRes] = await Promise.all([
          fetch("/api/suppliers").then(res => res.json()),
          fetch("/api/products").then(res => res.json())
        ]);
        
        if (suppRes.success) setSuppliers(suppRes.data);
        if (prodRes.success) setProducts(prodRes.data);
      } catch (e) {
        showMsg("Không thể tải danh sách dữ liệu", 'error');
      } finally {
        setLoading(false);
      }
    };
    loadInitialData();
  }, []);

  const showMsg = (text: string, type: 'success' | 'error') => {
    setMessage({ text, type });
    setTimeout(() => setMessage({ text: "", type: null }), 3000);
  };

  const filteredProducts = useMemo(() => {
    if (!searchTerm) return [];
    const search = searchTerm.toLowerCase();
    return products.filter(p => 
      p.Name.toLowerCase().includes(search) || 
      p.Code.toLowerCase().includes(search)
    ).slice(0, 5); // Limit suggestions
  }, [products, searchTerm]);

  const addProduct = (p: Product) => {
    const existing = importItems.find(item => item.ProductId === p.Id);
    if (existing) {
      setImportItems(importItems.map(item => 
        item.ProductId === p.Id ? { ...item, Quantity: item.Quantity + 1 } : item
      ));
    } else {
      setImportItems([...importItems, {
        ProductId: p.Id,
        ProductName: p.Name,
        ProductCode: p.Code,
        Quantity: 1,
        PurchasePrice: p.PurchasePrice
      }]);
    }
    setSearchTerm("");
  };

  const updateQuantity = (id: number, qty: number) => {
    if (qty < 1) return;
    setImportItems(importItems.map(item => 
      item.ProductId === id ? { ...item, Quantity: qty } : item
    ));
  };

  const updatePrice = (id: number, price: number) => {
    setImportItems(importItems.map(item => 
      item.ProductId === id ? { ...item, PurchasePrice: price } : item
    ));
  };

  const removeItem = (id: number) => {
    setImportItems(importItems.filter(item => item.ProductId !== id));
  };

  const totalAmount = importItems.reduce((acc, curr) => acc + (curr.Quantity * curr.PurchasePrice), 0);

  const handleSave = async () => {
    if (!selectedSupplierId) {
      showMsg("Vui lòng chọn nhà cung cấp", 'error');
      return;
    }
    if (importItems.length === 0) {
      showMsg("Chưa có mặt hàng nào để nhập", 'error');
      return;
    }

    setSaving(true);
    try {
      const res = await fetch("/api/inventory/import", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          SupplierId: selectedSupplierId,
          Items: importItems,
          Notes: notes
        })
      });
      
      const json = await res.json();
      if (json.success) {
        showMsg("Đã nhập kho thành công", 'success');
        setTimeout(() => router.push("/inventory"), 1000);
      } else {
        showMsg(json.error || "Có lỗi xảy ra", 'error');
      }
    } catch (e) {
      showMsg("Lỗi kết nối máy chủ", 'error');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] gap-6 animate-in fade-in duration-700">
      <div className="w-20 h-20 border-t-4 border-amber-600 rounded-full animate-spin border-opacity-30 border-l-4 border-amber-600" />
      <p className="text-[12px] font-black text-slate-400 uppercase tracking-[0.3em] animate-pulse">Initializing Procurement Module...</p>
    </div>
  );

  return (
    <div className="space-y-12 animate-in fade-in slide-in-from-bottom-6 duration-1000 pb-20 max-w-7xl mx-auto">
      {/* Header Panel */}
      <div className="flex flex-col md:flex-row md:items-center justify-between no-print gap-8 relative z-10">
        <div className="flex items-center gap-6">
          <Link href="/inventory" className="p-4 bg-white hover:bg-slate-50 border border-slate-200 rounded-[20px] transition-all shadow-sm active:scale-90 group">
            <ArrowLeft className="w-6 h-6 text-slate-400 group-hover:text-slate-900 transition-colors" />
          </Link>
          <div>
            <p className="text-[10px] font-black text-amber-500 uppercase tracking-[0.4em] mb-1 pl-1">Procurement Management</p>
            <h2 className="text-[32px] font-black tracking-tighter text-slate-900 uppercase italic leading-none flex items-center gap-4">
              <div className="w-1.5 h-10 bg-slate-900 rounded-full" />
              PHIẾU NHẬP KHO
            </h2>
          </div>
        </div>
        
        {message.type && (
           <div className={cn(
             "px-8 py-4 rounded-2xl flex items-center gap-4 animate-in slide-in-from-right-4 duration-500 shadow-xl border-b-4",
             message.type === 'success' ? "bg-emerald-50 text-emerald-600 border-white" : "bg-rose-50 text-rose-600 border-white"
           )}>
              {message.type === 'success' ? <CheckCircle2 className="w-5 h-5" /> : <AlertCircle className="w-5 h-5" />}
              <span className="text-xs font-black uppercase tracking-widest">{message.text}</span>
           </div>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-10">
        {/* Main Document Body */}
        <div className="lg:col-span-8">
           <div className="bg-white rounded-[40px] p-12 border border-slate-200/60 shadow-2xl relative overflow-hidden group min-h-[700px] flex flex-col">
              <div className="absolute top-0 right-0 w-64 h-64 bg-amber-500/5 blur-[100px] -mr-32 -mt-32 rounded-full" />
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-10 mb-12 pb-10 border-b border-slate-50 relative z-10">
                 <div className="space-y-4">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] ml-1 flex items-center gap-2">
                       <Truck className="w-3.5 h-3.5 text-amber-500" /> Chọn nhà cung cấp đối tác
                    </label>
                    <select 
                      value={selectedSupplierId}
                      onChange={(e) => setSelectedSupplierId(e.target.value)}
                      className="w-full bg-slate-50 border-none rounded-3xl py-5 px-8 text-[15px] font-black text-slate-900 focus:ring-4 focus:ring-amber-500/10 transition-all appearance-none cursor-pointer shadow-inner uppercase italic"
                    >
                       <option value="">-- CHỌN NHÀ CUNG CẤP --</option>
                       {suppliers.map((s) => (
                         <option key={s.Id} value={s.Id}>{s.Name.toUpperCase()}</option>
                       ))}
                    </select>
                 </div>
                 <div className="space-y-4">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] ml-1 flex items-center gap-2">
                       <Calendar className="w-3.5 h-3.5 text-amber-500" /> Ngày lập phiếu
                    </label>
                    <div className="bg-slate-50 rounded-3xl py-5 px-8 text-[15px] font-black text-slate-400 flex items-center justify-between shadow-inner">
                       {new Date().toLocaleDateString('vi-VN')}
                       <Clock className="w-4 h-4 opacity-30" />
                    </div>
                 </div>
              </div>

              {/* Product Selector Search */}
              <div className="mb-10 relative z-10">
                 <div className="relative group/search">
                    <Search className="absolute left-6 top-1/2 -translate-y-1/2 w-6 h-6 text-slate-300 group-focus-within/search:text-amber-500 transition-colors" />
                    <input 
                      type="text" 
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="w-full bg-white border-2 border-slate-100 rounded-[28px] py-6 pl-16 pr-8 text-[15px] font-black text-slate-900 focus:ring-4 focus:ring-amber-500/5 focus:border-amber-500/50 transition-all placeholder:text-slate-200 shadow-sm" 
                      placeholder="QUÉT MÃ HOẶC TÌM TÊN SẢN PHẨM NHẬP KHO..." 
                    />
                    {searchTerm && (
                       <button onClick={() => setSearchTerm("")} className="absolute right-6 top-1/2 -translate-y-1/2 p-2 hover:bg-slate-50 rounded-full transition-colors">
                          <X className="w-4 h-4 text-slate-300" />
                       </button>
                    )}
                 </div>

                 {filteredProducts.length > 0 && (
                   <div className="absolute top-full left-0 right-0 mt-3 bg-white rounded-3xl border border-slate-200 shadow-2xl p-4 space-y-2 z-20 animate-in slide-in-from-top-4 duration-300">
                      {filteredProducts.map(p => (
                         <div 
                           key={p.Id} 
                           onClick={() => addProduct(p)}
                           className="flex items-center justify-between p-4 hover:bg-slate-50 rounded-2xl cursor-pointer transition-colors group"
                         >
                            <div className="flex items-center gap-4">
                               <div className="w-10 h-10 bg-slate-900 rounded-xl flex items-center justify-center text-[10px] font-black text-amber-500">
                                  {p.Code}
                               </div>
                               <div>
                                  <p className="text-[14px] font-black text-slate-900 uppercase tracking-tight leading-none">{p.Name}</p>
                                  <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase">Giá gốc: {formatCurrency(p.PurchasePrice)}</p>
                               </div>
                            </div>
                            <PlusCircle className="w-6 h-6 text-slate-200 group-hover:text-amber-500 transition-colors" />
                         </div>
                      ))}
                   </div>
                 )}
              </div>

              <div className="flex-1 space-y-6 relative z-10">
                 <div className="flex items-center justify-between pb-4 border-b border-slate-50">
                    <h4 className="text-[10px] font-black text-slate-300 uppercase tracking-[0.3em]">Danh mục hàng hóa chuẩn bị nhập</h4>
                    <span className="text-[10px] font-black text-amber-600 bg-amber-50 px-4 py-1.5 rounded-full uppercase tracking-widest">{importItems.length} SKUs SELECTED</span>
                 </div>
                 
                 <div className="space-y-4">
                    {importItems.map((item) => (
                       <div key={item.ProductId} className="flex flex-col md:flex-row md:items-center justify-between p-6 bg-slate-50/50 rounded-[32px] border border-slate-100 group/row hover:bg-white hover:shadow-xl transition-all gap-6">
                          <div className="flex items-center gap-5 md:w-[40%]">
                             <div className="w-14 h-14 bg-white rounded-2xl flex items-center justify-center text-slate-200 border border-slate-100 group-hover/row:scale-110 transition-transform duration-500">
                                <Package className="w-7 h-7" />
                             </div>
                             <div>
                                <p className="text-[15px] font-black text-slate-900 uppercase tracking-tight leading-none mb-1.5">{item.ProductName}</p>
                                <p className="text-[10px] font-bold text-slate-300 uppercase tracking-widest leading-none">CODE: {item.ProductCode}</p>
                             </div>
                          </div>
                          
                          <div className="flex flex-1 items-center justify-between gap-8">
                             <div className="flex items-center gap-3">
                                <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest text-center">SỐ LƯỢNG</p>
                                <div className="flex items-center gap-1 bg-white p-1.5 rounded-2xl border border-slate-100 shadow-sm">
                                   <button 
                                     onClick={() => updateQuantity(item.ProductId, item.Quantity - 1)}
                                     className="w-10 h-10 flex items-center justify-center text-slate-400 hover:text-slate-900 transition-colors"
                                   >
                                      <p className="text-xl font-black">-</p>
                                   </button>
                                   <input 
                                     type="number" 
                                     value={item.Quantity}
                                     onChange={(e) => updateQuantity(item.ProductId, parseInt(e.target.value) || 1)}
                                     className="w-12 text-center bg-transparent border-none text-[15px] font-black text-slate-900 focus:ring-0 tabular-nums"
                                   />
                                   <button 
                                     onClick={() => updateQuantity(item.ProductId, item.Quantity + 1)}
                                     className="w-10 h-10 flex items-center justify-center text-slate-400 hover:text-slate-900 transition-colors"
                                   >
                                      <p className="text-xl font-black">+</p>
                                   </button>
                                </div>
                             </div>

                             <div className="flex-1 max-w-[150px]">
                                <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">GIÁ NHẬP (VNĐ)</p>
                                <input 
                                  type="number" 
                                  value={item.PurchasePrice}
                                  onChange={(e) => updatePrice(item.ProductId, parseInt(e.target.value) || 0)}
                                  className="w-full bg-white border border-slate-100 rounded-2xl py-3 px-4 text-sm font-black text-slate-900 focus:ring-4 focus:ring-amber-500/10 transition-all shadow-inner"
                                />
                             </div>

                             <button 
                               onClick={() => removeItem(item.ProductId)}
                               className="p-4 bg-rose-50 text-rose-300 hover:text-rose-500 hover:bg-rose-100 rounded-[22px] transition-all"
                             >
                                <Trash2 className="w-5 h-5" />
                             </button>
                          </div>
                       </div>
                    ))}
                    
                    {importItems.length === 0 && (
                      <div className="py-32 flex flex-col items-center justify-center text-center opacity-10 space-y-6">
                         <div className="w-32 h-32 border-4 border-dashed border-slate-900 rounded-[40px] flex items-center justify-center group-hover:rotate-12 transition-transform duration-1000">
                            <Layers className="w-12 h-12" />
                         </div>
                         <h4 className="text-[13px] font-black tracking-[0.4em] uppercase">Document data empty</h4>
                      </div>
                    )}
                 </div>
              </div>

              <div className="mt-12 p-10 bg-slate-900 rounded-[40px] flex items-center justify-between no-print shadow-2xl relative overflow-hidden group/footer">
                 <div className="absolute top-0 right-0 w-32 h-32 bg-amber-500/10 blur-3xl -mr-16 -mt-16 group-hover/footer:bg-amber-500/20 transition-all duration-700" />
                 <div className="flex items-center gap-6 relative z-10">
                    <div className="p-4 bg-amber-500 rounded-[24px] text-slate-900 shadow-2xl rotate-3">
                       <DollarSign className="w-8 h-8" />
                    </div>
                    <div>
                       <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1.5">TỔNG GIÁ TRỊ NHẬP KHO</p>
                       <p className="text-[36px] font-black text-white italic tracking-tighter leading-none">{formatCurrency(totalAmount)}</p>
                    </div>
                 </div>
                 <button 
                   onClick={handleSave}
                   disabled={saving}
                   className="px-12 py-6 bg-amber-500 hover:bg-amber-600 text-slate-900 font-black rounded-[32px] transition-all shadow-2xl active:scale-95 flex items-center gap-4 uppercase tracking-[0.1em] text-[15px] relative z-10 italic border-b-6 border-amber-700"
                 >
                    {saving ? "SAVING PROCUREMENT..." : "LƯU PHIẾU NGAY"}
                    {saving ? <Clock className="w-6 h-6 animate-spin" /> : <ChevronRight className="w-6 h-6" />}
                 </button>
              </div>
           </div>
        </div>

        {/* Info & Side Log */}
        <div className="lg:col-span-4 space-y-8 relative z-10 no-print">
           <div className="bg-white p-10 rounded-[40px] border border-slate-200 shadow-xl relative overflow-hidden group">
              <div className="absolute top-0 right-0 w-24 h-24 bg-amber-500/5 blur-2xl -mr-12 -mt-12" />
              <h3 className="text-[12px] font-black text-slate-900 uppercase tracking-widest flex items-center gap-3 mb-10 italic">
                 <FilePlus className="w-4 h-4 text-amber-500" /> GHI CHÚ CHỨNG TỪ
              </h3>
              <textarea 
                 rows={6}
                 value={notes}
                 onChange={(e) => setNotes(e.target.value)}
                 className="w-full bg-slate-50/50 border-none rounded-[32px] p-8 text-sm font-black text-slate-900 focus:ring-4 focus:ring-amber-500/10 transition-all resize-none shadow-inner italic"
                 placeholder="Lý do nhập hàng, chứng từ vận đơn đi kèm..."
              />
           </div>

           <div className="bg-slate-900 p-10 rounded-[40px] text-white shadow-2xl relative overflow-hidden group">
              <div className="absolute bottom-0 right-0 w-32 h-32 bg-amber-500/10 blur-[60px] -mr-16 -mb-16 rounded-full" />
              <h3 className="text-[12px] font-black uppercase tracking-widest flex items-center gap-3 mb-10 italic">
                 <AlertCircle className="w-4 h-4 text-amber-500" /> STOCK AUDIT
              </h3>
              <div className="space-y-6">
                 <div className="p-6 bg-white/5 rounded-[24px] border border-white/5 hover:border-amber-500/30 transition-all">
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-2 leading-none">ITEMS UNDER STOCK</p>
                    <p className="text-[28px] font-black text-white italic tracking-tighter leading-none">12 SKU</p>
                 </div>
                 <div className="p-6 bg-white/5 rounded-[24px] border border-white/5 hover:border-amber-500/30 transition-all">
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-2 leading-none">PENDING IMPORTS</p>
                    <p className="text-[28px] font-black text-white italic tracking-tighter leading-none">{importItems.length} SKU</p>
                 </div>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
}
