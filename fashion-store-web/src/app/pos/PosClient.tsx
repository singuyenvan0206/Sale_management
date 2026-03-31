"use client";

import { useState, useMemo } from "react";
import { 
  Search, 
  ShoppingCart, 
  Minus, 
  Plus, 
  Trash2, 
  CreditCard, 
  Wallet, 
  User, 
  Tag, 
  Package,
  X,
  ChevronRight
} from "lucide-react";
import { formatCurrency, cn } from "@/lib/utils";

interface Product {
  Id: number;
  Name: string;
  Code: string;
  CategoryId: number;
  CategoryName: string;
  SalePrice: number;
  StockQuantity: number;
  PromoDiscountPercent?: number;
}

interface CartItem extends Product {
  quantity: number;
}

export function PosClient({ products, categories }: { products: any[], categories: any[] }) {
  const [cart, setCart] = useState<CartItem[]>([]);
  const [search, setSearch] = useState("");
  const [activeCategory, setActiveCategory] = useState("all");

  const filteredProducts = useMemo(() => {
    return products.filter(p => {
      const matchSearch = p.Name.toLowerCase().includes(search.toLowerCase()) || 
                          p.Code.toLowerCase().includes(search.toLowerCase());
      const matchCat = activeCategory === "all" || p.CategoryId.toString() === activeCategory;
      return matchSearch && matchCat;
    });
  }, [products, search, activeCategory]);

  const addToCart = (product: Product) => {
    setCart(prev => {
      const existing = prev.find(item => item.Id === product.Id);
      if (existing) {
        return prev.map(item => 
          item.Id === product.Id ? { ...item, quantity: item.quantity + 1 } : item
        );
      }
      return [...prev, { ...product, quantity: 1 }];
    });
  };

  const updateQuantity = (id: number, delta: number) => {
    setCart(prev => prev.map(item => {
      if (item.Id === id) {
        const newQty = Math.max(1, item.quantity + delta);
        return { ...item, quantity: newQty };
      }
      return item;
    }));
  };

  const removeFromCart = (id: number) => {
    setCart(prev => prev.filter(item => item.Id !== id));
  };

  const subtotal = cart.reduce((sum, item) => sum + (Number(item.SalePrice) * item.quantity), 0);
  const tax = subtotal * 0.08; // 8% Default
  const total = subtotal + tax;

  return (
    <div className="flex h-full gap-6 overflow-hidden">
      {/* Left: Product Selection */}
      <div className="flex-1 flex flex-col gap-6 overflow-hidden">
        <div className="flex items-center justify-between gap-4">
          <div className="relative flex-1 group">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400 group-focus-within:text-amber-600 transition-colors" />
            <input 
              type="text" 
              placeholder="Tìm sản phẩm (Tên, mã SP, barcode)..." 
              className="w-full bg-white border border-slate-200 rounded-2xl py-4 pl-12 pr-4 text-slate-900 focus:outline-none focus:ring-4 focus:ring-amber-500/10 focus:border-amber-500/50 transition-all font-medium shadow-sm"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <button className="bg-white hover:bg-slate-50 text-slate-600 p-4 rounded-2xl transition-all active:scale-95 border border-slate-200 shadow-sm">
            <User className="w-6 h-6" />
          </button>
        </div>

        {/* Category Tabs */}
        <div className="flex gap-2 overflow-x-auto pb-2 no-scrollbar">
          <button 
            onClick={() => setActiveCategory("all")}
            className={cn(
              "px-6 py-2.5 rounded-xl text-sm font-bold transition-all whitespace-nowrap border capitalize",
              activeCategory === "all" 
                ? "bg-amber-500 text-white border-amber-400 shadow-lg shadow-amber-500/20" 
                : "bg-white text-slate-500 border-slate-200 hover:bg-slate-50 hover:text-slate-900 shadow-sm"
            )}
          >
            Tất cả
          </button>
          {categories.map((cat: any) => (
            <button 
              key={cat.Id}
              onClick={() => setActiveCategory(cat.Id.toString())}
              className={cn(
                "px-6 py-2.5 rounded-xl text-sm font-bold transition-all whitespace-nowrap border capitalize",
                activeCategory === cat.Id.toString() 
                  ? "bg-amber-500 text-white border-amber-400 shadow-lg shadow-amber-500/20" 
                  : "bg-white text-slate-500 border-slate-200 hover:bg-slate-50 hover:text-slate-900 shadow-sm"
              )}
            >
              {cat.Name}
            </button>
          ))}
        </div>

        {/* Product Grid */}
        <div className="flex-1 overflow-y-auto pr-2 custom-scrollbar">
          <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-4 pb-8">
            {filteredProducts.map((p: any) => (
              <button 
                key={p.Id}
                onClick={() => addToCart(p)}
                className="glass-card p-4 rounded-3xl flex flex-col items-start gap-3 text-left border-transparent hover:border-amber-500/30 hover:bg-white transition-all group active:scale-95 relative overflow-hidden"
              >
                <div className="w-full aspect-square rounded-2xl bg-slate-50 flex items-center justify-center relative overflow-hidden border border-slate-100">
                  <Package className="w-12 h-12 text-slate-200 group-hover:scale-110 group-hover:text-amber-500/30 transition-all duration-500" />
                  <div className="absolute inset-0 bg-gradient-to-t from-white/80 to-transparent flex items-end p-3 opacity-0 group-hover:opacity-100 transition-opacity">
                    <span className="text-[10px] font-bold text-white uppercase tracking-widest bg-amber-500 px-2 py-1 rounded shadow-lg">Thêm vào giỏ</span>
                  </div>
                </div>
                <div className="w-full">
                  <p className="text-[10px] text-amber-600 font-bold uppercase tracking-widest mb-1">{p.CategoryName}</p>
                  <p className="text-sm font-bold text-slate-900 line-clamp-1 group-hover:text-amber-600 transition-colors uppercase">{p.Name}</p>
                  <div className="flex items-center justify-between mt-2 pt-2 border-t border-slate-100">
                    <span className="text-sm font-bold text-slate-900">{formatCurrency(p.SalePrice)}</span>
                    <span className="text-[10px] text-slate-400 font-bold">SL: {p.StockQuantity}</span>
                  </div>
                </div>
                {p.PromoDiscountPercent > 0 && (
                  <div className="absolute top-2 right-2 bg-emerald-500 text-white text-[10px] font-black px-2 py-1 rounded-lg">
                    -{p.PromoDiscountPercent}%
                  </div>
                )}
              </button>
            ))}
            {filteredProducts.length === 0 && (
              <div className="col-span-full py-20 text-center opacity-50">
                <Package className="w-16 h-16 text-slate-200 mx-auto mb-4" />
                <p className="text-slate-500 font-medium italic">Không tìm thấy sản phẩm nào...</p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Right: Cart/Checkout */}
      <div className="w-96 flex flex-col gap-6 overflow-hidden">
        <div className="flex-1 glass-card rounded-[2.5rem] flex flex-col overflow-hidden border-slate-200 shadow-2xl shadow-slate-200/50 bg-white">
          <div className="p-8 border-b border-slate-100 flex items-center justify-between bg-slate-50/50">
            <div className="flex items-center gap-3">
              <div className="p-3 bg-amber-500 rounded-2xl text-white shadow-lg shadow-amber-500/20">
                <ShoppingCart className="w-6 h-6" />
              </div>
              <h2 className="text-xl font-black text-slate-900 uppercase tracking-tight">Đơn hàng</h2>
            </div>
            <span className="bg-slate-100 text-slate-500 text-[10px] font-black px-3 py-1.5 rounded-full border border-slate-200">
              {cart.reduce((a, b) => a + b.quantity, 0)} Items
            </span>
          </div>

          <div className="flex-1 overflow-y-auto p-6 space-y-4 custom-scrollbar">
            {cart.map((item) => (
              <div key={item.Id} className="flex gap-4 p-4 rounded-2xl bg-white border border-slate-100 group hover:bg-slate-50 transition-all shadow-sm">
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-bold text-slate-900 truncate uppercase">{item.Name}</p>
                  <p className="text-[10px] text-amber-600 font-bold mb-2">Giá: {formatCurrency(item.SalePrice)}</p>
                  <div className="flex items-center gap-3">
                    <div className="flex items-center bg-slate-50 rounded-xl border border-slate-100 p-1">
                      <button 
                        onClick={() => updateQuantity(item.Id, -1)}
                        className="p-1 hover:bg-white rounded-lg text-slate-400 transition-colors shadow-sm"
                      >
                        <Minus className="w-3 h-3" />
                      </button>
                      <span className="w-8 text-center text-xs font-bold text-slate-900">{item.quantity}</span>
                      <button 
                        onClick={() => updateQuantity(item.Id, 1)}
                        className="p-1 hover:bg-white rounded-lg text-slate-400 transition-colors shadow-sm"
                      >
                        <Plus className="w-3 h-3" />
                      </button>
                    </div>
                  </div>
                </div>
                <div className="flex flex-col items-end justify-between">
                  <button 
                    onClick={() => removeFromCart(item.Id)}
                    className="p-2 hover:bg-rose-50 text-slate-300 hover:text-rose-500 rounded-xl transition-all"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                  <p className="text-sm font-black text-amber-600">{formatCurrency(Number(item.SalePrice) * item.quantity)}</p>
                </div>
              </div>
            ))}
            {cart.length === 0 && (
              <div className="h-full flex flex-col items-center justify-center text-center opacity-30 mt-20">
                <div className="p-8 border-4 border-dashed border-slate-100 rounded-[3rem] mb-6">
                  <ShoppingCart className="w-16 h-16 text-slate-300" />
                </div>
                <p className="text-slate-500 font-bold text-sm tracking-widest uppercase">Giỏ hàng trống</p>
              </div>
            )}
          </div>

          <div className="p-8 bg-slate-50 border-t border-slate-100 space-y-6">
            <div className="space-y-3">
              <div className="flex justify-between text-sm">
                <span className="text-slate-500 font-medium">Tạm tính</span>
                <span className="text-slate-900 font-bold">{formatCurrency(subtotal)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-slate-500 font-medium">Thuế (8%)</span>
                <span className="text-slate-900 font-bold">{formatCurrency(tax)}</span>
              </div>
              <div className="h-[1px] bg-slate-200" />
              <div className="flex justify-between items-end">
                <span className="text-sm text-slate-500 font-bold uppercase tracking-widest">Tổng cộng</span>
                <span className="text-3xl font-black text-amber-600">
                  {formatCurrency(total)}
                </span>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <button className="flex items-center justify-center gap-2 bg-white hover:bg-slate-50 text-slate-900 font-black py-4 rounded-2xl border border-slate-200 transition-all active:scale-95 uppercase text-xs tracking-widest shadow-sm">
                <Wallet className="w-5 h-5 text-amber-500" />
                Tiền mặt
              </button>
              <button className="flex items-center justify-center gap-2 bg-white hover:bg-slate-50 text-slate-900 font-black py-4 rounded-2xl border border-slate-200 transition-all active:scale-95 uppercase text-xs tracking-widest shadow-sm">
                <CreditCard className="w-5 h-5 text-amber-500" />
                Thẻ / Chuyển khoản
              </button>
            </div>
            
            <button className={cn(
              "w-full bg-amber-500 hover:bg-amber-600 text-white font-black py-5 rounded-[2rem] transition-all active:scale-95 flex items-center justify-center gap-3 shadow-xl shadow-amber-500/30 uppercase tracking-tighter text-lg",
              cart.length === 0 && "opacity-50 cursor-not-allowed saturate-0"
            )} disabled={cart.length === 0}>
               Xác Nhận Thanh Toán
               <ChevronRight className="w-6 h-6" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
