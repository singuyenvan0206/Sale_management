"use client";

import { useState, useEffect, useMemo } from "react";
import { formatCurrency, cn } from "@/lib/utils";
import { 
  Plus, 
  Trash2,
  Edit2,
  Search,
  RotateCcw,
  CheckCircle2,
  AlertCircle
} from "lucide-react";

async function fetchInitialData() {
  const [productsRes, categoriesRes, suppliersRes] = await Promise.all([
    fetch("/api/products").then(res => res.json()), // I need to make sure I have an /api/products GET
    fetch("/api/categories").then(res => res.json()),
    fetch("/api/suppliers").then(res => res.json())
  ]);
  return { products: productsRes, categories: categoriesRes, suppliers: suppliersRes };
}

export default function ProductsPage() {
  const [products, setProducts] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [suppliers, setSuppliers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  // Form State
  const [editingProduct, setEditingProduct] = useState<any>({
    Id: 0,
    Name: "",
    Code: "",
    CategoryId: 0,
    SalePrice: 0,
    PurchasePrice: 0,
    StockQuantity: 0,
    Description: "",
    SupplierId: 0
  });

  // Filter State
  const [searchTerm, setSearchTerm] = useState("");
  const [filterCategory, setFilterCategory] = useState("0");
  const [filterSupplier, setFilterSupplier] = useState("0");
  const [filterStock, setFilterStock] = useState("All");
  const [filterPromo, setFilterPromo] = useState("All");
  const [filterPrice, setFilterPrice] = useState("All");

  const [statusText, setStatusText] = useState("Sẵn sàng");

  useEffect(() => {
    // For this demo, we'll fetch from the client
    const load = async () => {
      try {
        const res = await fetch("/api/products");
        const data = await res.json();
        if (Array.isArray(data)) setProducts(data);
        
        const catRes = await fetch("/api/categories");
        const catData = await catRes.json();
        if (Array.isArray(catData)) setCategories(catData);

        const supRes = await fetch("/api/suppliers");
        const supData = await supRes.json();
        if (Array.isArray(supData)) setSuppliers(supData);
      } catch (e) {
        console.error("Failed to load data", e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const filteredProducts = useMemo(() => {
    if (!Array.isArray(products)) return [];
    return products.filter(p => {
      const matchesSearch = !searchTerm || 
        p.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.Code.toLowerCase().includes(searchTerm.toLowerCase());
      
      const matchesCat = filterCategory === "0" || p.CategoryId.toString() === filterCategory;
      const matchesSup = filterSupplier === "0" || p.SupplierId?.toString() === filterSupplier;

      const matchesStock = filterStock === "All" || (
        filterStock === "OutOfStock" ? p.StockQuantity === 0 :
        filterStock === "LowStock" ? p.StockQuantity > 0 && p.StockQuantity < 10 :
        filterStock === "InStock" ? p.StockQuantity >= 10 : true
      );

      const matchesPrice = filterPrice === "All" || (
        filterPrice === "Under100k" ? p.SalePrice < 100000 :
        filterPrice === "100kTo500k" ? p.SalePrice >= 100000 && p.SalePrice < 500000 :
        filterPrice === "500kTo1M" ? p.SalePrice >= 500000 && p.SalePrice < 1000000 :
        filterPrice === "Over1M" ? p.SalePrice >= 1000000 : true
      );

      return matchesSearch && matchesCat && matchesSup && matchesStock && matchesPrice;
    });
  }, [products, searchTerm, filterCategory, filterSupplier, filterStock, filterPrice]);

  const handleSelectProduct = (product: any) => {
    setEditingProduct({...product});
    setStatusText(`Đang chọn: ${product.Name}`);
  };

  const handleLookupByCode = async () => {
    if (!editingProduct.Code) return;
    try {
      const res = await fetch(`/api/products/lookup?code=${editingProduct.Code}`);
      if (res.ok) {
        const p = await res.json();
        setEditingProduct({...p});
        setStatusText(`🔍 Tìm thấy: ${p.Name}. Đã tự động điền.`);
      }
    } catch (e) {
      // Not found or error
    }
  };

  const handleClear = () => {
    setEditingProduct({
      Id: 0,
      Name: "",
      Code: "",
      CategoryId: 0,
      SalePrice: 0,
      PurchasePrice: 0,
      StockQuantity: 0,
      Description: "",
      SupplierId: 0
    });
    setStatusText("Mới");
  };

  if (loading) return <div className="p-20 text-center font-black animate-pulse">ĐANG TẢI DỮ LIỆU HỆ THỐNG...</div>;

  return (
    <div className="space-y-6 animate-in fade-in duration-500 pb-20">
      {/* Header Bar - WPF style */}
      <div className="bg-[#1CB5E0] p-6 -mx-8 -mt-8 mb-8 shadow-lg">
        <h2 className="text-[24px] font-bold text-white text-center tracking-tight">
           📦 Quản Lý Sản Phẩm
        </h2>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
        {/* Left Panel - Input Form (WPF Style) */}
        <div className="lg:col-span-1">
           <div className="bg-white p-6 rounded-[10px] shadow-sm space-y-6 sticky top-4 border-t-4 border-blue-500">
              <div className="flex items-center justify-between border-b pb-3">
                 <h3 className="text-[14px] font-black text-slate-800 uppercase italic">Chi tiết sản phẩm</h3>
                 <span className="text-[10px] font-bold text-blue-500 px-2 py-0.5 bg-blue-50 rounded">
                    {editingProduct.Id === 0 ? "Tạo mới" : `ID: ${editingProduct.Id}`}
                 </span>
              </div>
              
              <div className="space-y-4">
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Tên sản phẩm *</label>
                    <input 
                      type="text" 
                      value={editingProduct.Name}
                      onChange={(e) => setEditingProduct({...editingProduct, Name: e.target.value})}
                      className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm focus:bg-white focus:ring-2 focus:ring-blue-100 outline-none transition-all font-bold" 
                      placeholder="Nhập tên..." 
                    />
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Mã sản phẩm (Lookup)</label>
                    <input 
                      type="text" 
                      value={editingProduct.Code}
                      onChange={(e) => setEditingProduct({...editingProduct, Code: e.target.value})}
                      onBlur={handleLookupByCode}
                      onKeyDown={(e) => e.key === 'Enter' && handleLookupByCode()}
                      className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-mono font-bold text-blue-600 focus:bg-white outline-none" 
                      placeholder="SP001..." 
                    />
                 </div>
                 <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                       <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Giá bán *</label>
                       <input 
                        type="number" 
                        value={editingProduct.SalePrice}
                        onChange={(e) => setEditingProduct({...editingProduct, SalePrice: Number(e.target.value)})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-black text-emerald-600" 
                       />
                    </div>
                    <div className="space-y-1.5">
                       <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Giá nhập</label>
                       <input 
                        type="number" 
                        value={editingProduct.PurchasePrice}
                        onChange={(e) => setEditingProduct({...editingProduct, PurchasePrice: Number(e.target.value)})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-black text-slate-500" 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Danh mục</label>
                    <select 
                      value={editingProduct.CategoryId}
                      onChange={(e) => setEditingProduct({...editingProduct, CategoryId: Number(e.target.value)})}
                      className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold"
                    >
                       <option value={0}>Chọn danh mục...</option>
                       {(Array.isArray(categories) ? categories : []).map((c: any) => <option key={c.Id} value={c.Id}>{c.Name}</option>)}
                    </select>
                 </div>
              </div>

              <div className="grid grid-cols-3 gap-2 pt-4">
                 <button className="bg-[#4CAF50] hover:bg-[#43a047] text-white font-black py-3 rounded-md text-[10px] uppercase tracking-tighter shadow-md active:scale-95">🌿 Thêm</button>
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

        {/* Right Panel - Product List Table */}
        <div className="lg:col-span-3">
          <div className="bg-white rounded-[10px] shadow-sm overflow-hidden min-h-[700px] flex flex-col">
            <div className="p-6 bg-[#f8f9fa]/50">
               <h3 className="text-[18px] font-black text-[#1CB5E0] uppercase tracking-tight mb-6">📦 Danh Sách Sản Phẩm</h3>
               
               {/* Filter Panel - EXACT WPF LOGIC */}
               <div className="bg-[#F8F9FA] p-5 rounded-[8px] space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                     <div className="space-y-1">
                        <span className="text-[10px] font-black text-slate-400 uppercase">Lọc Danh Mục</span>
                        <select 
                          value={filterCategory}
                          onChange={(e) => setFilterCategory(e.target.value)}
                          className="w-full h-10 bg-white rounded-md px-3 text-sm font-bold cursor-pointer outline-none shadow-sm focus:ring-2 focus:ring-blue-100"
                        >
                           <option value="0">Tất cả danh mục</option>
                           {categories.map((c: any) => <option key={c.Id} value={c.Id}>{c.Name}</option>)}
                        </select>
                     </div>
                     <div className="space-y-1">
                        <span className="text-[10px] font-black text-slate-400 uppercase">Sắp hết hàng</span>
                        <select 
                          value={filterStock}
                          onChange={(e) => setFilterStock(e.target.value)}
                          className="w-full h-10 bg-white rounded-md px-3 text-sm font-bold cursor-pointer outline-none shadow-sm"
                        >
                           <option value="All">Tất cả</option>
                           <option value="OutOfStock">Hết hàng (0)</option>
                           <option value="LowStock">Sắp hết (&lt;10)</option>
                           <option value="InStock">Còn hàng (≥10)</option>
                        </select>
                     </div>
                     <div className="md:col-span-2 flex items-end gap-2">
                        <div className="flex-1 relative">
                           <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                           <input 
                            type="text" 
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full h-10 bg-white rounded-md pl-10 pr-3 text-sm font-bold outline-none shadow-sm focus:ring-2 focus:ring-blue-100" 
                            placeholder="Tên hoặc mã sản phẩm..." 
                           />
                        </div>
                        <button className="bg-[#FF9800] hover:bg-[#f57c00] text-white font-black h-10 px-8 rounded-md text-[11px] uppercase tracking-widest whitespace-nowrap active:scale-95 shadow-md">🔍 Lọc</button>
                     </div>
                  </div>
               </div>
            </div>

            <div className="flex-1 overflow-x-auto">
              <table className="w-full text-left">
                <thead className="bg-[#f8f9fa] sticky top-0 z-10">
                  <tr className="text-slate-800 text-[11px] font-black uppercase tracking-tight">
                    <th className="px-6 py-5">ID</th>
                    <th className="px-6 py-5">📦 Tên Sản Phẩm</th>
                    <th className="px-6 py-5 text-center">🏷️ Mã SP</th>
                    <th className="px-6 py-5">📂 Danh Mục</th>
                    <th className="px-6 py-5 text-right">💰 Giá Bán</th>
                    <th className="px-6 py-5 text-center">🔥 KM%</th>
                    <th className="px-6 py-5 text-center">📊 Tồn Kho</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 italic font-medium">
                  {filteredProducts.map((p: any, idx: number) => (
                    <tr 
                      key={p.Id} 
                      onClick={() => handleSelectProduct(p)}
                      className={cn(
                        "hover:bg-blue-600 hover:text-white transition-all cursor-pointer group",
                        idx % 2 === 1 ? "bg-[#F8F9FA]" : "bg-white",
                        editingProduct.Id === p.Id ? "bg-blue-600 text-white font-black" : "text-slate-700"
                      )}
                    >
                      <td className="px-6 py-5 font-bold text-[12px] opacity-50">{p.Id}</td>
                      <td className="px-6 py-5 text-[14px] font-black uppercase tracking-tight">{p.Name}</td>
                      <td className="px-6 py-5 text-center font-mono text-[12px] opacity-70">{p.Code}</td>
                      <td className="px-6 py-5 text-[13px] uppercase">{p.CategoryName}</td>
                      <td className="px-6 py-5 text-right">
                        <span className={cn(
                          "font-black text-[15px]",
                          editingProduct.Id === p.Id ? "text-white" : "text-[#4CAF50]"
                        )}>
                          {formatCurrency(p.SalePrice)}
                        </span>
                      </td>
                      <td className="px-6 py-5 text-center">
                         {Number(p.PromoDiscountPercent) > 0 ? (
                            <span className={cn(
                              "font-black text-[12px]",
                              editingProduct.Id === p.Id ? "text-white" : "text-rose-600"
                            )}>-{p.PromoDiscountPercent}%</span>
                         ) : <span className="opacity-20">-</span>}
                      </td>
                      <td className="px-6 py-5 text-center">
                        <span className={cn(
                           "font-black text-[14px]",
                           editingProduct.Id === p.Id ? "text-white" : 
                           p.StockQuantity <= 5 ? "text-rose-600 animate-pulse" : "text-amber-500"
                        )}>{p.StockQuantity}</span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            
            {/* Pagination WPF Style */}
            <div className="p-8 bg-[#F8F9FA] flex flex-col sm:flex-row items-center justify-between gap-4">
               <span className="text-[12px] font-black text-[#1CB5E0] uppercase tracking-tighter">
                  📊 Trang: 1 / 1 • Tổng: {filteredProducts.length} sản phẩm
               </span>
               <div className="flex gap-1.5 items-center">
                  <button className="w-10 h-9 bg-[#2196F3] text-white flex items-center justify-center rounded shadow-md hover:bg-blue-600 active:scale-90 transition-all font-black text-xs">⏮️</button>
                  <button className="w-10 h-9 bg-[#2196F3] text-white flex items-center justify-center rounded shadow-md hover:bg-blue-600 active:scale-90 transition-all font-black text-xs">◀️</button>
                  <div className="w-14 h-9 bg-white rounded flex items-center justify-center text-sm font-black shadow-inner border border-slate-200">1</div>
                  <button className="w-10 h-9 bg-[#2196F3] text-white flex items-center justify-center rounded shadow-md hover:bg-blue-600 active:scale-90 transition-all font-black text-xs">▶️</button>
                  <button className="w-10 h-9 bg-[#2196F3] text-white flex items-center justify-center rounded shadow-md hover:bg-blue-600 active:scale-90 transition-all font-black text-xs">⏭️</button>
               </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
