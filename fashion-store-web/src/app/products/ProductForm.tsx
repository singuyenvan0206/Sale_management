"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { 
  Save, 
  X, 
  Package, 
  Tag, 
  DollarSign, 
  Database, 
  AlertCircle,
  CheckCircle2,
  ChevronLeft,
  Settings
} from "lucide-react";
import { cn, formatCurrency } from "@/lib/utils";
import Link from "next/link";

interface ProductFormProps {
  initialData?: any;
  categories: any[];
  isEdit?: boolean;
}

export function ProductForm({ initialData, categories, isEdit }: ProductFormProps) {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [status, setStatus] = useState<{ type: "success" | "error" | null, message: string }>({ type: null, message: "" });

  const [formData, setFormData] = useState({
    Id: initialData?.Id || 0,
    Name: initialData?.Name || "",
    Code: initialData?.Code || "",
    CategoryId: initialData?.CategoryId || (categories[0]?.Id || 0),
    SalePrice: initialData?.SalePrice || 0,
    PurchasePrice: initialData?.PurchasePrice || 0,
    StockQuantity: initialData?.StockQuantity || 0,
    MinStockLevel: initialData?.MinStockLevel || 10,
    Description: initialData?.Description || "",
    IsActive: initialData?.IsActive !== undefined ? initialData.IsActive : true
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setStatus({ type: null, message: "" });

    try {
      const url = isEdit ? `/api/products/${formData.Id}` : "/api/products";
      const method = isEdit ? "PUT" : "POST";

      const res = await fetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(formData)
      });

      const result = await res.json();

      if (result.success) {
        setStatus({ type: "success", message: result.message || (isEdit ? "Cập nhật sản phẩm thành công!" : "Thêm sản phẩm mới thành công!") });
        setTimeout(() => {
          router.push("/products");
          router.refresh();
        }, 1500);
      } else {
        setStatus({ type: "error", message: result.error || "Có lỗi xảy ra khi lưu dữ liệu." });
      }
    } catch (error) {
      setStatus({ type: "error", message: "Lỗi kết nối máy chủ." });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-5xl mx-auto space-y-8 animate-in fade-in slide-in-from-bottom-8 duration-700 pb-20">
      {/* breadcrumbs */}
      <div className="flex items-center gap-2 text-[10px] font-black text-slate-400 uppercase tracking-widest no-print mb-4">
        <Link href="/products" className="hover:text-amber-500 transition-colors">Sản phẩm</Link>
        <ChevronLeft className="w-3 h-3 rotate-180" />
        <span className="text-slate-900">{isEdit ? "Chỉnh sửa" : "Thêm mới"}</span>
      </div>

      <div className="flex flex-col md:flex-row md:items-end justify-between gap-6">
        <div>
          <h2 className="text-[32px] font-black tracking-tighter text-slate-900 uppercase italic leading-none">
             {isEdit ? "✏️ CẬP NHẬT SẢN PHẨM" : "📦 THÊM MỚI SẢN PHẨM"}
          </h2>
          <div className="w-20 h-2 bg-amber-500 rounded-full mt-4" />
        </div>
      </div>

      {status.type && (
        <div className={cn(
          "p-6 rounded-3xl flex items-center gap-4 animate-in zoom-in-95 duration-300 shadow-xl",
          status.type === "success" ? "bg-emerald-50 text-emerald-800 border border-emerald-100" : "bg-rose-50 text-rose-800 border border-rose-100"
        )}>
          {status.type === "success" ? <CheckCircle2 className="w-8 h-8 text-emerald-500" /> : <AlertCircle className="w-8 h-8 text-rose-500" />}
          <div>
             <p className="font-black text-sm uppercase tracking-tight">{status.type === "success" ? "Thành công" : "Thất bại"}</p>
             <p className="text-[13px] font-medium opacity-80">{status.message}</p>
          </div>
        </div>
      )}

      <form onSubmit={handleSubmit} className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Left Column: Essential Info */}
        <div className="lg:col-span-2 space-y-8">
          <div className="bg-white rounded-[32px] p-10 border border-slate-200/60 shadow-sm relative overflow-hidden group">
            <div className="absolute top-0 right-0 w-64 h-64 bg-amber-500/5 blur-[100px] rounded-full -mr-32 -mt-32" />
            
            <h3 className="text-sm font-black text-slate-400 uppercase tracking-[0.2em] mb-10 flex items-center gap-3">
              <Package className="w-4 h-4 text-amber-500" />
              Thông tin cơ bản
            </h3>

            <div className="space-y-8 relative z-10">
              <div className="space-y-3">
                <label className="text-[11px] font-black text-slate-500 uppercase tracking-widest pl-1">Tên sản phẩm *</label>
                <input 
                  required
                  type="text" 
                  value={formData.Name}
                  onChange={e => setFormData({...formData, Name: e.target.value})}
                  className="w-full bg-slate-50 border-none rounded-2xl py-5 px-6 text-sm font-bold text-slate-900 focus:ring-4 focus:ring-amber-500/10 transition-all placeholder:text-slate-300"
                  placeholder="Vd: Áo Sơ Mi Lụa Premium..."
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div className="space-y-3">
                  <label className="text-[11px] font-black text-slate-500 uppercase tracking-widest pl-1">Mã sản phẩm (SKU) *</label>
                  <div className="relative group">
                    <Tag className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 group-focus-within:text-amber-500 transition-colors" />
                    <input 
                      required
                      type="text" 
                      value={formData.Code}
                      onChange={e => setFormData({...formData, Code: e.target.value.toUpperCase()})}
                      className="w-full bg-slate-50 border-none rounded-2xl py-5 pl-12 pr-6 text-sm font-black text-blue-600 focus:ring-4 focus:ring-amber-500/10 transition-all uppercase"
                      placeholder="ASM-001..."
                    />
                  </div>
                </div>

                <div className="space-y-3">
                  <label className="text-[11px] font-black text-slate-500 uppercase tracking-widest pl-1">Danh mục *</label>
                  <div className="relative group">
                    <Settings className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                    <select 
                      required
                      value={formData.CategoryId}
                      onChange={e => setFormData({...formData, CategoryId: Number(e.target.value)})}
                      className="w-full bg-slate-50 border-none rounded-2xl py-5 pl-12 pr-6 text-sm font-bold text-slate-900 focus:ring-4 focus:ring-amber-500/10 transition-all appearance-none cursor-pointer"
                    >
                      {categories.map(c => (
                        <option key={c.Id} value={c.Id}>{c.Name}</option>
                      ))}
                    </select>
                  </div>
                </div>
              </div>

              <div className="space-y-3">
                <label className="text-[11px] font-black text-slate-500 uppercase tracking-widest pl-1">Mô tả sản phẩm</label>
                <textarea 
                  rows={4}
                  value={formData.Description}
                  onChange={e => setFormData({...formData, Description: e.target.value})}
                  className="w-full bg-slate-50 border-none rounded-2xl py-5 px-6 text-sm font-medium text-slate-900 focus:ring-4 focus:ring-amber-500/10 transition-all resize-none"
                  placeholder="Thông tin chi tiết về chất liệu, kiểu dáng..."
                />
              </div>
            </div>
          </div>

          <div className="bg-white rounded-[32px] p-10 border border-slate-200/60 shadow-sm relative overflow-hidden group">
            <h3 className="text-sm font-black text-slate-400 uppercase tracking-[0.2em] mb-10 flex items-center gap-3">
              <DollarSign className="w-4 h-4 text-emerald-500" />
              Thiết lập giá & Tài chính
            </h3>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-10">
               <div className="space-y-3">
                  <label className="text-[11px] font-black text-slate-500 uppercase tracking-widest pl-1">Giá nhập (VNĐ)</label>
                  <input 
                    type="number" 
                    value={formData.PurchasePrice}
                    onChange={e => setFormData({...formData, PurchasePrice: Number(e.target.value)})}
                    className="w-full bg-slate-50 border-none rounded-2xl py-5 px-6 text-xl font-black text-slate-400 focus:ring-4 focus:ring-amber-500/10 transition-all"
                  />
                  <p className="text-[10px] font-bold text-slate-400 px-1 italic">Giá gốc nhập từ nhà cung cấp</p>
               </div>

               <div className="space-y-3">
                  <label className="text-[11px] font-black text-slate-500 uppercase tracking-widest pl-1">Giá bán lẻ (VNĐ) *</label>
                  <input 
                    required
                    type="number" 
                    value={formData.SalePrice}
                    onChange={e => setFormData({...formData, SalePrice: Number(e.target.value)})}
                    className="w-full bg-emerald-50 border-emerald-100 rounded-2xl py-5 px-6 text-2xl font-black text-emerald-700 focus:bg-white focus:ring-4 focus:ring-emerald-500/10 transition-all"
                  />
                  <div className="flex justify-between items-center px-1">
                     <p className="text-[10px] font-black text-emerald-600 uppercase">Lợi nhuận dự kiến:</p>
                     <p className="text-[10px] font-black text-emerald-600 uppercase">
                        {formatCurrency(formData.SalePrice - formData.PurchasePrice)} 
                        ({formData.SalePrice > 0 ? ((formData.SalePrice - formData.PurchasePrice) / formData.SalePrice * 100).toFixed(0) : 0}%)
                     </p>
                  </div>
               </div>
            </div>
          </div>
        </div>

        {/* Right Column: Inventory & Status */}
        <div className="space-y-8">
          <div className="bg-slate-900 rounded-[32px] p-8 text-white shadow-2xl shadow-slate-900/40 relative overflow-hidden group">
            <div className="absolute bottom-0 left-0 w-full h-1 bg-amber-500 shadow-[0_0_20px_rgba(245,158,11,0.5)]" />
            <h3 className="text-sm font-black text-white/40 uppercase tracking-[0.2em] mb-10 flex items-center gap-3">
              <Database className="w-4 h-4 text-amber-500" />
              Kho hàng & Tồn kho
            </h3>

            <div className="space-y-10">
               <div className="space-y-3">
                  <label className="text-[11px] font-black text-white/60 uppercase tracking-widest">Số lượng tồn kho</label>
                  <input 
                    type="number" 
                    value={formData.StockQuantity}
                    onChange={e => setFormData({...formData, StockQuantity: Number(e.target.value)})}
                    className="w-full bg-white/5 border border-white/10 rounded-2xl py-5 px-6 text-3xl font-black text-white focus:bg-white/10 focus:ring-4 focus:ring-amber-500/20 outline-none transition-all"
                  />
               </div>

               <div className="space-y-3">
                  <label className="text-[11px] font-black text-white/60 uppercase tracking-widest">Ngưỡng tồn an toàn</label>
                  <input 
                    type="number" 
                    value={formData.MinStockLevel}
                    onChange={e => setFormData({...formData, MinStockLevel: Number(e.target.value)})}
                    className="w-full bg-white/5 border border-white/10 rounded-2xl py-4 px-6 text-xl font-black text-amber-500 focus:bg-white/10 outline-none transition-all"
                  />
                  <p className="text-[10px] font-bold text-white/30 italic">Hệ thống sẽ cảnh báo khi tồn kho xuống dưới mức này</p>
               </div>
            </div>
          </div>

          <div className="bg-white rounded-[32px] p-8 border border-slate-200/60 shadow-sm space-y-8">
             <div className="flex items-center justify-between">
                <div className="space-y-1">
                   <p className="text-[11px] font-black text-slate-500 uppercase tracking-widest">Trạng thái bán</p>
                   <p className="text-[10px] font-bold text-slate-400 italic">Cho phép bán ngay lập tức</p>
                </div>
                <button 
                  type="button"
                  onClick={() => setFormData({...formData, IsActive: !formData.IsActive})}
                  className={cn(
                    "w-14 h-8 rounded-full p-1 transition-all duration-500",
                    formData.IsActive ? "bg-emerald-500 shadow-lg shadow-emerald-500/30" : "bg-slate-200"
                  )}
                >
                  <div className={cn(
                    "w-6 h-6 bg-white rounded-full shadow-md transition-all duration-500 transform",
                    formData.IsActive ? "translate-x-6" : "translate-x-0"
                  )} />
                </button>
             </div>

             <div className="pt-8 border-t border-slate-50 space-y-4">
                <button 
                  type="submit"
                  disabled={loading}
                  className="w-full bg-slate-900 hover:bg-black text-white font-black py-5 rounded-2xl transition-all shadow-xl active:scale-95 flex items-center justify-center gap-3 disabled:opacity-50 uppercase tracking-widest text-sm"
                >
                  {loading ? <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" /> : <Save className="w-5 h-5 text-amber-500" />}
                  {isEdit ? "Cập nhật sản phẩm" : "Lưu sản phẩm"}
                </button>

                <Link href="/products" className="w-full bg-white hover:bg-slate-50 text-slate-500 font-black py-5 rounded-2xl border border-slate-200 transition-all flex items-center justify-center gap-3 uppercase tracking-widest text-sm">
                   <X className="w-5 h-5" />
                   Hủy bỏ & Quay lại
                </Link>
             </div>
          </div>
        </div>
      </form>
    </div>
  );
}
