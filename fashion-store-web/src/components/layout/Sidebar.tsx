"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";

const menuItems = [
  { name: "🏠 Trang Chủ", href: "/" },
  { name: "📦 Sản Phẩm", href: "/products" },
  { name: "📂 Danh Mục", href: "/categories" },
  { name: "🏭 Nhà Cung Cấp", href: "/suppliers" },
  { name: "🎟️ Mã Giảm Giá", href: "/vouchers" },
  { name: "🎁 Chương Trình KM", href: "/promotions" },
  { name: "🔎 Tìm Kiếm Sản Phẩm", href: "/search" },
  { name: "👥 Khách Hàng", href: "/customers" },
  { name: "🧾 Hóa Đơn", href: "/history" },
  { name: "📊 Báo Cáo", href: "/stats" },
  { name: "👤 Quản Lý Người Dùng", href: "/employees" },
  { name: "⚙️ Cài Đặt", href: "/settings" },
];

export function Sidebar() {
  const pathname = usePathname();

  return (
    <aside className="w-[240px] h-full bg-[#1E293B] flex flex-col no-print shrink-0 overflow-hidden shadow-2xl relative z-40">
      {/* Sidebar Header - WPF Accent */}
      <div className="bg-[#0F172A] p-6 pb-8 flex flex-col items-center justify-center border-b border-white/5 rounded-tr-[20px]">
        <div className="text-[24px] mb-2 drop-shadow-lg">🎛️</div>
        <h2 className="text-[#F8FAFC] font-bold text-[18px] uppercase tracking-tight text-center">
          QUẢN LÝ
        </h2>
        <p className="text-[#94A3B8] text-[12px] font-medium uppercase tracking-widest mt-1 opacity-80">
          Hệ thống POS
        </p>
      </div>

      <nav className="flex-1 mt-5 overflow-y-auto custom-scrollbar-dark pb-20">
        <ul className="space-y-[2px]">
          {menuItems.map((item) => {
            const isActive = pathname === item.href;
            return (
              <li key={item.href}>
                <Link
                  href={item.href}
                  className={cn(
                    "flex items-center gap-0 py-[10px] transition-all duration-150 group relative",
                    isActive 
                      ? "bg-[#2563EB] text-white font-bold" 
                      : "text-[#F8FAFC] hover:bg-[#263445]"
                  )}
                >
                  {/* Left accent bar - WPF ListBoxItem Style */}
                  <div className={cn(
                    "absolute left-0 w-[6px] h-full rounded-r-[3px] transition-all",
                    isActive ? "bg-[#60A5FA]" : "bg-transparent group-hover:bg-slate-600"
                  )} />
                  
                  <div className="ml-[14px] flex items-center justify-start w-full px-[14px]">
                    <span className="text-[15px] font-semibold tracking-tight">
                      {item.name}
                    </span>
                  </div>
                </Link>
              </li>
            );
          })}
        </ul>
      </nav>

      {/* Footer Branding */}
      <div className="p-4 bg-[#0F172A] text-center border-t border-white/5">
        <span className="text-[10px] font-black text-slate-500 uppercase tracking-widest opacity-40 hover:opacity-100 transition-opacity">
          © 2026 FASHION STORE v2.4.0
        </span>
      </div>
    </aside>
  );
}
