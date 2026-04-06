"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { Search, Filter, ArrowUpDown } from "lucide-react";
import { cn } from "@/lib/utils";

interface Category {
  Id: number;
  Name: string;
}

export function ProductFilterSection({ categories }: { categories: Category[] }) {
  const router = useRouter();
  const searchParams = useSearchParams();

  const q = searchParams.get("q") || "";
  const category = searchParams.get("category") || "all";
  const status = searchParams.get("status") || "all";
  const sort = searchParams.get("sort") || "date";
  const order = searchParams.get("order") || "desc";

  const updateFilters = (updates: Record<string, string>) => {
    const params = new URLSearchParams(searchParams.toString());
    Object.entries(updates).forEach(([key, value]) => {
      if (value === "all" || value === "") {
        params.delete(key);
      } else {
        params.set(key, value);
      }
    });
    router.push(`/products?${params.toString()}`);
  };

  return (
    <div className="wpf-groupbox mb-6">
      <span className="wpf-groupbox-label">BỘ LỌC TÌM KIẾM HỆ THỐNG</span>
      
      <div className="flex flex-col lg:flex-row gap-6 items-end">
        <div className="flex-1 space-y-1">
          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Tìm kiếm theo tên / mã SKU:</label>
          <div className="relative group">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 group-focus-within:text-[#0078D4] transition-colors" />
            <input 
              type="text"
              defaultValue={q}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  updateFilters({ q: (e.target as HTMLInputElement).value });
                }
              }}
              className="w-full h-10 border border-[#D1D1D1] pl-10 pr-4 text-[13px] bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm font-bold placeholder:text-slate-300 placeholder:font-normal"
              placeholder="Nhập nội dung tìm kiếm..."
            />
          </div>
        </div>

        <div className="flex flex-wrap gap-4">
           <div className="space-y-1">
              <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Danh mục:</label>
              <select 
                value={category}
                onChange={(e) => updateFilters({ category: e.target.value })}
                className="h-10 border border-[#D1D1D1] px-4 text-[12px] font-black bg-white rounded-sm outline-none min-w-[160px] cursor-pointer hover:bg-[#F3F9FF]"
              >
                 <option value="all">TẤT CẢ DANH MỤC</option>
                 {categories.map((c) => <option key={c.Id} value={c.Id.toString()}>{c.Name.toUpperCase()}</option>)}
              </select>
           </div>

           <div className="space-y-1">
              <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Trạng thái:</label>
              <select 
                value={status}
                onChange={(e) => updateFilters({ status: e.target.value })}
                className="h-10 border border-[#D1D1D1] px-4 text-[12px] font-black bg-white rounded-sm outline-none min-w-[140px] cursor-pointer hover:bg-[#F3F9FF]"
              >
                 <option value="all">TẤT CẢ STATUS</option>
                 <option value="active">ĐANG KINH DOANH</option>
                 <option value="inactive">NGỪNG KINH DOANH</option>
              </select>
           </div>

           <div className="space-y-1">
              <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Sắp xếp:</label>
              <select 
                value={sort}
                onChange={(e) => updateFilters({ sort: e.target.value })}
                className="h-10 border border-[#D1D1D1] px-4 text-[12px] font-black bg-white rounded-sm outline-none min-w-[140px] cursor-pointer hover:bg-[#F3F9FF]"
              >
                 <option value="date">NGÀY TẠO</option>
                 <option value="name">TÊN SẢN PHẨM</option>
                 <option value="price">GIÁ BÁN NIÊM YẾT</option>
                 <option value="stock">LƯỢNG TỒN KHO</option>
              </select>
           </div>
           
           <button 
             type="button" 
             className={cn(
               "btn-wpf h-10 w-10 flex items-center justify-center",
               order === 'asc' ? "bg-[#DDEBFA] border-[#0078D4] text-[#0078D4]" : ""
             )}
             onClick={() => {
               const newOrder = order === 'asc' ? 'desc' : 'asc';
               updateFilters({ order: newOrder });
             }}
           >
              <ArrowUpDown className="w-4 h-4" />
           </button>
        </div>
      </div>
    </div>
  );
}
