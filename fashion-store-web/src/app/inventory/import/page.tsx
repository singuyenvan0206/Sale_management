import { query } from "@/lib/db";
import { formatCurrency, cn } from "@/lib/utils";
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
  AlertCircle
} from "lucide-react";

async function getProcurementData() {
  const suppliers = await query("SELECT Id, Name FROM suppliers WHERE IsActive = 1");
  const products = await query("SELECT Id, Name, Code, StockQuantity, PurchasePrice FROM products WHERE IsActive = 1");
  const recentImports = await query(`
    SELECT sm.*, p.Name as ProductName, p.Code as ProductCode 
    FROM stockmovements sm 
    JOIN products p ON sm.ProductId = p.Id 
    WHERE sm.MovementType = 'Import' 
    ORDER BY sm.CreatedDate DESC 
    LIMIT 5
  `);

  return {
    suppliers: suppliers as any[],
    products: products as any[],
    recentImports: recentImports as any[]
  };
}

export default async function ImportInventoryPage() {
  const { suppliers, products, recentImports } = await getProcurementData();

  return (
    <div className="space-y-8 animate-in fade-in duration-1000">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold tracking-tight text-slate-900 mb-1">
            Nhập Kho <span className="amber-gradient-text">Hàng Hóa</span>
          </h2>
          <p className="text-slate-500">Lập phiếu nhập hàng và cập nhật số lượng tồn kho từ nhà cung cấp.</p>
        </div>
        <div className="flex gap-3">
          <button className="bg-white hover:bg-slate-50 text-slate-600 font-bold px-6 py-3 rounded-2xl transition-all border border-slate-200 shadow-sm flex items-center gap-2 active:scale-95">
            <History className="w-5 h-5" />
            Lịch Sử Nhập
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
        {/* Main Entry Form */}
        <div className="lg:col-span-8 space-y-6">
           <div className="glass-card p-10 rounded-[2.5rem] border-slate-200 bg-white/50 shadow-2xl shadow-slate-200/50">
              <div className="flex items-center gap-4 mb-10 pb-6 border-b border-slate-100">
                 <div className="p-4 bg-amber-500 rounded-2xl text-white shadow-lg shadow-amber-500/20">
                    <FilePlus className="w-8 h-8" />
                 </div>
                 <div>
                    <h3 className="text-xl font-black text-slate-900 uppercase">Phiếu Nhập Hàng Mới</h3>
                    <p className="text-xs font-bold text-slate-400 mt-1 uppercase tracking-widest">Tạo mới chứng từ nhập kho</p>
                 </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                 <div className="space-y-3">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest flex items-center gap-2">
                       <Truck className="w-3 h-3 text-amber-500" /> Nhà cung cấp
                    </label>
                    <select className="w-full bg-slate-50 border border-slate-200 rounded-2xl py-4 px-5 text-sm text-slate-900 font-bold focus:outline-none focus:ring-4 focus:ring-amber-500/10 focus:border-amber-500/50 transition-all appearance-none cursor-pointer">
                       <option value="">Chọn nhà cung cấp...</option>
                       {suppliers.map((s) => (
                         <option key={s.Id} value={s.Id}>{s.Name}</option>
                       ))}
                    </select>
                 </div>
                 <div className="space-y-3">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest flex items-center gap-2">
                       <Calendar className="w-3 h-3 text-amber-500" /> Ngày nhập hàng
                    </label>
                    <input type="date" defaultValue={new Date().toISOString().split('T')[0]} className="w-full bg-slate-50 border border-slate-200 rounded-2xl py-4 px-5 text-sm text-slate-900 font-bold focus:outline-none focus:ring-4 focus:ring-amber-500/10 focus:border-amber-500/50 transition-all cursor-pointer" />
                 </div>
              </div>

              <div className="mt-12 space-y-6">
                 <div className="flex items-center justify-between pb-4 border-b border-slate-50">
                    <h4 className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Danh sách mặt hàng</h4>
                    <button className="text-[10px] font-black text-amber-600 uppercase tracking-widest flex items-center gap-1.5 hover:text-amber-700 transition-colors">
                       <Plus className="w-4 h-4" /> Thêm nhanh
                    </button>
                 </div>
                 
                 <div className="space-y-4">
                    <div className="p-10 border-4 border-dashed border-slate-100 rounded-3xl flex flex-col items-center justify-center text-center">
                       <div className="w-16 h-16 bg-slate-50 rounded-full flex items-center justify-center mb-4">
                          <Package className="w-8 h-8 text-slate-200" />
                       </div>
                       <p className="text-sm font-bold text-slate-400 tracking-tight">Chưa có sản phẩm nào được thêm vào phiếu</p>
                       <button className="mt-6 px-8 py-3 bg-slate-900 text-white rounded-xl text-xs font-black uppercase tracking-widest shadow-xl shadow-slate-900/10 hover:bg-slate-800 transition-all active:scale-95">
                          Quét Mã Barcode
                       </button>
                    </div>
                 </div>
              </div>

              <div className="mt-12 p-8 bg-slate-900 rounded-[2.5rem] flex items-center justify-between">
                 <div className="flex items-center gap-4">
                    <div className="p-3 bg-amber-500 rounded-xl text-slate-900">
                       <DollarSign className="w-6 h-6" />
                    </div>
                    <div>
                       <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tổng tiền nhập hàng</p>
                       <p className="text-2xl font-black text-white">{formatCurrency(0)}</p>
                    </div>
                 </div>
                 <button className="px-10 py-5 bg-amber-500 hover:bg-amber-600 text-slate-900 font-black rounded-[1.8rem] transition-all shadow-xl shadow-amber-500/20 active:scale-95 flex items-center gap-3 uppercase tracking-tighter text-sm">
                    Lưu Phiếu & Cập Nhật Kho
                    <ArrowRight className="w-5 h-5" />
                 </button>
              </div>
           </div>
        </div>

        {/* Recent Movements & Quick View */}
        <div className="lg:col-span-4 space-y-8">
           <div className="glass-card p-8 rounded-[2.5rem] border-slate-200">
              <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest flex items-center gap-2 mb-8">
                 <History className="w-4 h-4 text-amber-500" /> Nhập kho gần đây
              </h3>
              <div className="space-y-6">
                 {recentImports.map((sm, idx) => (
                   <div key={idx} className="flex gap-4 group">
                      <div className="w-[2px] bg-slate-100 group-hover:bg-amber-500 transition-all relative">
                         <div className="absolute top-0 left-1/2 -translate-x-1/2 w-2 h-2 rounded-full bg-slate-200 group-hover:bg-amber-500 group-hover:scale-125 transition-all" />
                      </div>
                      <div className="flex-1 pb-6">
                         <p className="text-xs font-black text-slate-900 uppercase tracking-tight group-hover:text-amber-600 transition-colors">{sm.ProductName}</p>
                         <div className="flex items-center justify-between mt-1">
                            <span className="text-[10px] font-bold text-slate-400 uppercase">+{sm.Quantity} {sm.ReferenceType || "Đơn vị"}</span>
                            <span className="text-[8px] font-black text-slate-300 uppercase">{new Date(sm.CreatedDate).toLocaleDateString('vi-VN')}</span>
                         </div>
                      </div>
                   </div>
                 ))}
              </div>
              <button className="w-full mt-4 py-4 bg-slate-50 hover:bg-slate-100 text-slate-500 font-black rounded-2xl text-[10px] uppercase tracking-widest transition-all border border-slate-100">
                 Xem tất cả lịch sử
              </button>
           </div>

           <div className="glass-card p-8 rounded-[2.5rem] bg-amber-500 text-slate-900 relative overflow-hidden group">
              <div className="absolute top-0 right-0 w-32 h-32 bg-white/20 blur-3xl rounded-full -mr-16 -mt-16 group-hover:bg-white/30 transition-all" />
              <h3 className="text-lg font-black uppercase tracking-tight relative z-10 flex items-center gap-2">
                 <AlertCircle className="w-5 h-5" /> Cần chú ý
              </h3>
              <div className="mt-8 space-y-4 relative z-10">
                 <div className="p-4 bg-white/40 rounded-2xl border border-white/20 backdrop-blur-md">
                    <p className="text-[10px] font-black uppercase tracking-widest opacity-60">Sắp hết hàng</p>
                    <p className="text-xl font-black mt-1">12 Mặt hàng</p>
                 </div>
                 <div className="p-4 bg-white/40 rounded-2xl border border-white/20 backdrop-blur-md">
                    <p className="text-[10px] font-black uppercase tracking-widest opacity-60">Đang về kho</p>
                    <p className="text-xl font-black mt-1">2 Chuyến hàng</p>
                 </div>
              </div>
              <button className="mt-6 w-full py-4 bg-slate-900 text-white font-black rounded-[1.5rem] transition-all active:scale-95 text-[10px] uppercase tracking-widest shadow-xl shadow-slate-900/10 flex items-center justify-center gap-2">
                 Dự báo nhập hàng <ChevronRight className="w-4 h-4" />
              </button>
           </div>
        </div>
      </div>
    </div>
  );
}
