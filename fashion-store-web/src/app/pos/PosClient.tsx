"use client";

import { useState, useMemo, useEffect } from "react";
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
  ChevronRight,
  CheckCircle2,
  Printer,
  History,
  LayoutGrid,
  Maximize2
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
}

interface CartItem extends Product {
  quantity: number;
  promoDiscount: number; // Discount per unit
}

export function PosClient({ products, categories, customers, promotions }: { products: any[], categories: any[], customers: any[], promotions: any[] }) {
  const [cart, setCart] = useState<CartItem[]>([]);
  const [search, setSearch] = useState("");
  const [activeCategory, setActiveCategory] = useState("all");
  const [selectedCustomerId, setSelectedCustomerId] = useState<number>(1);
  const [voucherCode, setVoucherCode] = useState("");
  const [appliedVoucher, setAppliedVoucher] = useState<any>(null);
  const [isProcessing, setIsProcessing] = useState(false);
  const [isValidatingVoucher, setIsValidatingVoucher] = useState(false);
  const [paymentMethod, setPaymentMethod] = useState<'CASH' | 'TRANSFER'>('CASH');
  const [showSuccessModal, setShowSuccessModal] = useState(false);
  const [lastInvoice, setLastInvoice] = useState<any>(null);
  const [settings, setSettings] = useState<any>({
    BankId: 'MB',
    AccountNumber: '020120248888',
    AccountName: 'FASHION STORE'
  });

  useEffect(() => {
    fetch("/api/settings")
      .then(res => res.json())
      .then(json => {
        if (json.success) {
          setSettings((prev: any) => ({ ...prev, ...json.data }));
        }
      })
      .catch(e => console.error("Settings fetch failed", e));
  }, []);

  const getProductPromo = (product: Product) => {
    const prodPromo = promotions.find(p => p.RequiredProductId === product.Id);
    if (prodPromo) return prodPromo;
    return promotions.find(p => p.TargetCategoryId === product.CategoryId);
  };

  const calculatePromoDiscount = (product: Product) => {
    const promo = getProductPromo(product);
    if (!promo) return 0;
    return promo.DiscountPercent > 0 
      ? (Number(product.SalePrice) * Number(promo.DiscountPercent) / 100)
      : Number(promo.DiscountValue);
  };

  const filteredProducts = useMemo(() => {
    return products.filter(p => {
      const matchSearch = p.Name?.toLowerCase().includes(search.toLowerCase()) || 
                          p.Code?.toLowerCase().includes(search.toLowerCase());
      const matchCat = activeCategory === "all" || p.CategoryId?.toString() === activeCategory;
      return matchSearch && matchCat;
    });
  }, [products, search, activeCategory]);

  const addToCart = (product: Product) => {
    const promoDiscount = calculatePromoDiscount(product);
    setCart(prev => {
      const existing = prev.find(item => item.Id === product.Id);
      if (existing) {
        return prev.map(item => 
          item.Id === product.Id ? { ...item, quantity: item.quantity + 1 } : item
        );
      }
      return [...prev, { ...product, quantity: 1, promoDiscount }];
    });
  };

  const updateQuantity = (id: number, delta: number) => {
    setCart(prev => prev.map(item => {
      if (item.Id === id) return { ...item, quantity: Math.max(1, item.quantity + delta) };
      return item;
    }));
  };

  const removeFromCart = (id: number) => {
    setCart(prev => prev.filter(item => item.Id !== id));
  };

  const totalOriginalPrice = cart.reduce((sum, item) => sum + (Number(item.SalePrice) * item.quantity), 0);
  const totalPromoDiscount = cart.reduce((sum, item) => sum + (Number(item.promoDiscount) * item.quantity), 0);
  const subtotal = totalOriginalPrice - totalPromoDiscount;
  
  const voucherDiscount = appliedVoucher ? (
    appliedVoucher.DiscountType === '%' 
      ? (subtotal * Number(appliedVoucher.Value) / 100)
      : Number(appliedVoucher.Value)
  ) : 0;

  const totalDiscount = totalPromoDiscount + voucherDiscount; 
  const taxableAmount = Math.max(0, subtotal - voucherDiscount);
  const tax = taxableAmount * 0.08; 
  const total = taxableAmount + tax;

  const applyVoucher = async () => {
    if (!voucherCode.trim()) return;
    setIsValidatingVoucher(true);
    try {
      const res = await fetch(`/api/vouchers/validate?code=${voucherCode.trim()}`);
      const json = await res.json();
      if (json.success) {
        const voucher = json.data;
        if (subtotal < Number(voucher.MinInvoiceAmount)) {
          alert(`❌ Đơn tối thiểu ${formatCurrency(voucher.MinInvoiceAmount)} req.`);
        } else {
          setAppliedVoucher(voucher);
        }
      } else {
        alert("❌ " + json.error);
      }
    } catch (e) {
      alert("❌ Lỗi kết nối");
    } finally {
      setIsValidatingVoucher(false);
    }
  };

  const handleCheckout = async () => {
    if (cart.length === 0) return;
    setIsProcessing(true);
    
    try {
      const res = await fetch("/api/pos/checkout", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          customerId: selectedCustomerId,
          items: cart,
          subtotal,
          tax,
          total,
          voucherId: appliedVoucher?.Id,
          discountAmount: totalDiscount,
          paymentMethod
        })
      });
      
      const json = await res.json();
      if (json.success) {
        setLastInvoice({ ...json.data, total, items: cart });
        setShowSuccessModal(true);
        setCart([]);
        setAppliedVoucher(null);
        setVoucherCode("");
      } else {
        alert("❌ Lỗi: " + json.error);
      }
    } catch (error) {
      alert("❌ Lỗi kết nối hệ thống.");
    } finally {
      setIsProcessing(false);
    }
  };

  const qrUrl = lastInvoice ? `https://img.vietqr.io/image/${settings.BankId}-${settings.AccountNumber}-compact.png?amount=${Math.round(lastInvoice.total)}&addInfo=${lastInvoice.invoiceNumber}&accountName=${encodeURIComponent(settings.AccountName)}` : '';

  return (
    <div className="flex h-screen -m-8 bg-[#F3F3F3] overflow-hidden no-select">
      {/* Left Column: Product Management Interface */}
      <div className="flex-1 flex flex-col bg-[#F9F9F9] border-r border-[#D1D1D1]">
         {/* WPF Top Search / Ribbon Area */}
         <div className="bg-white border-b border-[#D1D1D1] px-6 py-4 space-y-4 shadow-sm">
            <div className="flex items-center gap-4">
               <div className="relative flex-1">
                  <div className="absolute left-3 top-1/2 -translate-y-1/2 flex items-center gap-2">
                     <Search className="w-4 h-4 text-slate-400" />
                  </div>
                  <input 
                    type="text" 
                    placeholder="TRA CỨU SẢN PHẨM (Tên, Mã, Barcode)..." 
                    className="w-full h-10 border border-[#D1D1D1] pl-10 pr-4 text-[13px] bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] focus:ring-4 focus:ring-[#0078D4]/5 transition-all outline-none rounded-sm font-bold uppercase tracking-tight"
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                  />
               </div>
               <div className="flex items-center gap-2 border-l border-[#D1D1D1] pl-4">
                  <span className="text-[11px] font-bold text-slate-400 uppercase">Khách hàng:</span>
                  <select 
                    className="h-10 border border-[#D1D1D1] px-4 text-[12px] font-black bg-white rounded-sm outline-none min-w-[200px] cursor-pointer hover:bg-[#F3F9FF]"
                    value={selectedCustomerId}
                    onChange={(e) => setSelectedCustomerId(Number(e.target.value))}
                  >
                    {customers.map(c => <option key={c.Id} value={c.Id}>{c.Name.toUpperCase()}</option>)}
                  </select>
               </div>
            </div>

            <div className="flex items-center gap-2 overflow-x-auto pb-1 no-scrollbar">
               <button 
                 onClick={() => setActiveCategory("all")}
                 className={cn(
                   "btn-wpf h-8 flex items-center gap-2 min-w-[120px] uppercase tracking-widest text-[10px] font-black",
                   activeCategory === "all" ? "bg-[#0078D4] text-white border-[#005A9E]" : ""
                 )}
               >
                 TẤT CẢ (ALL)
               </button>
               {categories.map((cat: any) => (
                 <button 
                   key={cat.Id}
                   onClick={() => setActiveCategory(cat.Id.toString())}
                   className={cn(
                     "btn-wpf h-8 flex items-center gap-2 min-w-[120px] uppercase tracking-widest text-[10px] font-black",
                     activeCategory === cat.Id.toString() ? "bg-[#0078D4] text-white border-[#005A9E]" : ""
                   )}
                 >
                   {cat.Name}
                 </button>
               ))}
            </div>
         </div>

         {/* Product Grid Area */}
         <div className="flex-1 overflow-y-auto p-6 bg-[#F3F3F3]">
            <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-3">
               {filteredProducts.map((p: any) => (
                 <button 
                   key={p.Id}
                   onClick={() => addToCart(p)}
                   className="bg-white border border-[#D1D1D1] p-3 text-left hover:border-[#0078D4] hover:bg-[#F3F9FF] transition-all group flex flex-col h-full rounded-[2px]"
                 >
                   <div className="aspect-square bg-[#F9F9F9] border border-[#EEEEEE] mb-3 flex items-center justify-center relative overflow-hidden">
                      <Package className="w-8 h-8 text-slate-200 group-hover:text-[#0078D4]/20 transition-all duration-300" />
                      <div className="absolute top-2 right-2">
                         <span className={cn(
                           "text-[9px] font-black px-1.5 py-0.5 rounded-sm border",
                           p.StockQuantity > 10 ? "bg-emerald-50 text-emerald-600 border-emerald-200" : "bg-rose-50 text-rose-600 border-rose-200"
                         )}>
                           X{p.StockQuantity}
                         </span>
                      </div>
                   </div>
                   <p className="text-[9px] font-bold text-[#0078D4] uppercase tracking-tighter mb-1">{p.CategoryName}</p>
                   <h4 className="text-[12px] font-bold text-slate-800 uppercase line-clamp-2 leading-tight mb-2 flex-1">{p.Name}</h4>
                   <div className="flex items-center justify-between mt-auto pt-2 border-t border-[#F0F0F0]">
                      <span className="text-[13px] font-black text-slate-900">{formatCurrency(p.SalePrice)}</span>
                      <div className="w-6 h-6 bg-slate-100 rounded-sm flex items-center justify-center group-hover:bg-[#0078D4] group-hover:text-white transition-colors">
                        <Plus className="w-3 h-3" />
                      </div>
                   </div>
                 </button>
               ))}
            </div>
         </div>

         {/* Bottom Status Bar */}
         <div className="h-8 bg-[#0078D4] border-t border-[#005A9E] flex items-center justify-between px-6 text-white text-[10px] font-bold tracking-widest uppercase">
            <div className="flex items-center gap-6 italic">
               <span>TRẠNG THÁI: KẾT NỐI</span>
               <span className="border-l border-white/20 pl-6">SERVER: PRIMARY NODE-01</span>
            </div>
            <div className="flex items-center gap-2">
               <div className="w-2 h-2 rounded-full bg-white animate-pulse" />
               FUSION ERP VERSION 2.5
            </div>
         </div>
      </div>

      {/* Right Column: Order Summary / Checkout Interface */}
      <div className="w-[480px] bg-white border-l border-[#D1D1D1] flex flex-col shadow-2xl">
         <div className="p-4 bg-[#F0F0F0] border-b border-[#D1D1D1] flex items-center justify-between">
            <div className="flex items-center gap-3">
               <div className="w-8 h-8 bg-slate-800 rounded-sm flex items-center justify-center text-white">
                  <ShoppingCart className="w-4 h-4" />
               </div>
               <h3 className="text-[13px] font-black text-slate-800 uppercase tracking-tighter">HÓA ĐƠN HIỆN TẠI</h3>
            </div>
            <div className="flex items-center gap-2">
               <button onClick={() => setCart([])} className="btn-wpf text-rose-600 hover:bg-rose-50 border-rose-200">
                  <Trash2 className="w-3.5 h-3.5" />
               </button>
               <div className="bg-white border border-[#D1D1D1] px-3 py-1 text-[11px] font-bold text-[#0078D4]">
                  {cart.length} DÒNG
               </div>
            </div>
         </div>

         <div className="flex-1 overflow-y-auto px-4 py-2 bg-white">
            <table className="wpf-datagrid border-none">
               <thead>
                  <tr className="border-b border-[#D1D1D1]">
                     <th className="bg-white border-none py-3 text-[#333]">SẢN PHẨM</th>
                     <th className="bg-white border-none py-3 w-[80px] text-center">SL</th>
                     <th className="bg-white border-none py-3 w-[120px] text-right">TỔNG</th>
                  </tr>
               </thead>
               <tbody>
                  {cart.map((item) => (
                    <tr key={item.Id} className="group border-b border-[#F0F0F0] hover:bg-[#F9FBFE]">
                       <td className="py-3 pr-4 border-none">
                          <div className="flex flex-col">
                             <span className="text-[12px] font-bold text-slate-800 uppercase leading-tight mb-1">{item.Name}</span>
                             <span className="text-[10px] font-medium text-slate-400 italic">HÀNG CHÍNH HÃNG • {item.Code}</span>
                          </div>
                       </td>
                       <td className="py-3 border-none">
                          <div className="flex items-center justify-center gap-1">
                             <button onClick={() => updateQuantity(item.Id, -1)} className="w-6 h-6 border border-[#D1D1D1] rounded-sm hover:bg-[#E5F1FB] transition-colors"><Minus className="w-3 h-3 mx-auto" /></button>
                             <span className="w-6 text-center text-[12px] font-black">{item.quantity}</span>
                             <button onClick={() => updateQuantity(item.Id, 1)} className="w-6 h-6 border border-[#D1D1D1] rounded-sm hover:bg-[#E5F1FB] transition-colors"><Plus className="w-3 h-3 mx-auto" /></button>
                          </div>
                       </td>
                       <td className="py-3 text-right border-none font-bold text-slate-900 tabular-nums">
                          {formatCurrency(Number(item.SalePrice) * item.quantity)}
                       </td>
                    </tr>
                  ))}
                  {cart.length === 0 && (
                    <tr>
                       <td colSpan={3} className="py-20 text-center border-none">
                          <ShoppingCart className="w-12 h-12 text-slate-100 mx-auto mb-4" />
                          <p className="text-[11px] font-bold text-slate-300 uppercase tracking-widest italic font-bold">Vui lòng chọn sản phẩm</p>
                       </td>
                    </tr>
                  )}
               </tbody>
            </table>
         </div>

         {/* Summary & Checkout Section */}
         <div className="bg-[#F9F9F9] border-t border-[#D1D1D1] p-6 space-y-6 shadow-top shadow-sm">
            <div className="grid grid-cols-1 gap-3">
               <div className="flex items-center gap-2">
                  <input 
                    type="text" 
                    placeholder="MÃ ƯU ĐÃI (VOUCHER)..." 
                    className="flex-1 h-10 border border-[#D1D1D1] px-4 text-[11px] font-black bg-white rounded-sm outline-none uppercase tracking-widest"
                    value={voucherCode}
                    onChange={(e) => setVoucherCode(e.target.value)}
                  />
                  <button 
                    onClick={applyVoucher}
                    className="btn-wpf h-10 px-6 bg-slate-800 text-white font-black hover:bg-black"
                  >
                    NHẬP
                  </button>
               </div>

               <div className="bg-white border border-[#D1D1D1] p-4 space-y-2">
                  <div className="flex justify-between text-[11px] font-bold text-slate-400">
                     <span>CỘNG TIỀN HÀNG:</span>
                     <span>{formatCurrency(subtotal)}</span>
                  </div>
                  {totalDiscount > 0 && (
                    <div className="flex justify-between text-[11px] font-bold text-rose-500">
                       <span>CHIẾT KHẤU / GIẢM GIÁ:</span>
                       <span>-{formatCurrency(totalDiscount)}</span>
                    </div>
                  )}
                  <div className="flex justify-between text-[11px] font-bold text-slate-400">
                     <span>THUẾ GIÁ TRỊ GIA TĂNG (8%):</span>
                     <span>{formatCurrency(tax)}</span>
                  </div>
                  <div className="h-[1px] bg-[#E5E5E5] my-2" />
                  <div className="flex justify-between items-end">
                     <span className="text-[13px] font-black text-[#333] uppercase italic">TỔNG THANH TOÁN:</span>
                     <span className="text-[32px] font-black text-[#0078D4] leading-none tracking-tighter italic">{formatCurrency(total)}</span>
                  </div>
               </div>

               <div className="grid grid-cols-2 gap-2">
                  <button 
                    onClick={() => setPaymentMethod('CASH')}
                    className={cn(
                      "btn-wpf h-14 flex flex-col items-center justify-center gap-1",
                      paymentMethod === 'CASH' ? "bg-[#DDEBFA] border-[#0078D4] text-[#0078D4]" : ""
                    )}
                  >
                     <Wallet className="w-5 h-5" />
                     <span className="text-[10px] font-black uppercase">TIỀN MẶT</span>
                  </button>
                  <button 
                    onClick={() => setPaymentMethod('TRANSFER')}
                    className={cn(
                      "btn-wpf h-14 flex flex-col items-center justify-center gap-1",
                      paymentMethod === 'TRANSFER' ? "bg-[#DDEBFA] border-[#0078D4] text-[#0078D4]" : ""
                    )}
                  >
                     <CreditCard className="w-5 h-5" />
                     <span className="text-[10px] font-black uppercase">CHUYỂN KHOẢN</span>
                  </button>
               </div>

               <button 
                 onClick={handleCheckout}
                 disabled={cart.length === 0 || isProcessing}
                 className="w-full h-18 bg-[#0078D4] hover:bg-[#005A9E] text-white font-black text-[14px] uppercase tracking-widest shadow-xl flex items-center justify-center gap-4 disabled:opacity-30 disabled:grayscale transition-all rounded-sm border-b-4 border-[#004578] active:translate-y-1 active:border-b-0"
               >
                  {isProcessing ? "DANG XỬ LÝ..." : "XÁC NHẬN THANH TOÁN (F12)"}
                  <ChevronRight className="w-6 h-6" />
               </button>
            </div>
         </div>
      </div>

      {/* Success Modal - WPF Styled Dialog */}
      {showSuccessModal && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-6 no-select">
           <div className="absolute inset-0 bg-slate-900/40 backdrop-blur-[2px]" />
           <div className="bg-white border border-[#D1D1D1] w-full max-w-2xl relative shadow-2xl animate-in zoom-in-95 duration-300 rounded-sm">
              {/* Fake Windows Title Bar for Modal */}
              <div className="h-10 bg-[#0078D4] text-white flex items-center justify-between px-4">
                 <span className="text-[12px] font-bold uppercase tracking-widest">Hoàn Tất Giao Dịch • Fusion ERP</span>
                 <button onClick={() => setShowSuccessModal(false)} className="hover:bg-rose-500 w-8 h-8 flex items-center justify-center transition-colors">
                    <X className="w-4 h-4" />
                 </button>
              </div>

              <div className="p-10 space-y-10">
                 <div className="flex items-center gap-6">
                    <div className="w-16 h-16 bg-emerald-50 border border-emerald-200 flex items-center justify-center text-emerald-600">
                       <CheckCircle2 className="w-10 h-10" />
                    </div>
                    <div>
                       <h3 className="text-[24px] font-black text-slate-900 tracking-tighter uppercase italic leading-none mb-2">THANH TOÁN THÀNH CÔNG!</h3>
                       <p className="text-[11px] font-bold text-slate-400 uppercase tracking-widest">SỐ HÓA ĐƠN: {lastInvoice?.invoiceNumber}</p>
                    </div>
                 </div>

                 <div className="grid grid-cols-2 gap-10">
                    <div className="space-y-6">
                       <div className="bg-[#F9F9F9] border border-[#D1D1D1] p-6 space-y-4">
                          <div className="flex justify-between text-[11px] font-bold text-slate-400 uppercase">
                             <span>Tổng cộng:</span>
                             <span className="text-slate-900 text-[18px] font-black">{formatCurrency(lastInvoice?.total)}</span>
                          </div>
                       </div>
                       
                       <div className="flex flex-col gap-3">
                          <button onClick={() => window.print()} className="btn-wpf btn-wpf-primary flex items-center justify-center gap-3 h-12 uppercase font-black text-[12px]">
                             <Printer className="w-5 h-5" /> IN HÓA ĐƠN
                          </button>
                          <button onClick={() => setShowSuccessModal(false)} className="btn-wpf h-12 uppercase font-black text-[12px] text-slate-400">
                             TIẾP TỤC BÁN HÀNG
                          </button>
                       </div>
                    </div>

                    <div className={cn(
                      "flex flex-col items-center justify-center transition-all duration-700 p-4 border border-dashed border-[#D1D1D1] rounded-sm",
                      paymentMethod === 'TRANSFER' ? "opacity-100" : "opacity-0 invisible"
                    )}>
                       <p className="text-[10px] font-black text-slate-300 uppercase tracking-[0.3em] mb-4">VietQR Fast Payment</p>
                       <img src={qrUrl} alt="Payment QR" className="w-[180px] h-[180px] bg-white p-2 border border-[#EEEEEE]" />
                       <p className="text-[12px] font-black text-[#0078D4] mt-4 uppercase tracking-tighter">{settings.AccountName}</p>
                       <p className="text-[11px] font-bold text-slate-400 uppercase">{settings.BankId} BANKING</p>
                    </div>
                 </div>
              </div>
           </div>
        </div>
      )}
    </div>
  );
}
