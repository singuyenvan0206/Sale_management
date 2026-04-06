"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";
import { signOut } from "next-auth/react";
import { 
  LayoutDashboard, 
  ShoppingBag, 
  Package, 
  Users, 
  History, 
  Settings, 
  LogOut,
  ChevronLeft,
  ChevronRight,
  Database,
  BarChart3,
  Ticket,
  Zap,
  Tag,
  Warehouse,
  Briefcase
} from "lucide-react";
import { useState } from "react";

const menuItems = [
  { id: "dashboard", label: "Dashboard", icon: LayoutDashboard, href: "/stats" },
  { id: "pos", label: "Point of Sale", icon: ShoppingBag, href: "/pos" },
  { id: "products", label: "Product Management", icon: Package, href: "/products" },
  { id: "inventory", label: "Warehouse & Stock", icon: Warehouse, href: "/inventory" },
  { id: "categories", label: "Product Categories", icon: Tag, href: "/categories" },
  { id: "history", label: "Transaction History", icon: History, href: "/history" },
  { id: "customers", label: "CRM (Customers)", icon: Users, href: "/customers" },
  { id: "vouchers", label: "Discount Vouchers", icon: Ticket, href: "/vouchers" },
  { id: "promotions", label: "Active Promotions", icon: Zap, href: "/promotions" },
  { id: "team", label: "Team Management", icon: Briefcase, href: "/employees" },
  { id: "settings", label: "System Config", icon: Settings, href: "/settings" },
];

export default function Sidebar() {
  const pathname = usePathname();
  const [collapsed, setCollapsed] = useState(false);

  return (
    <aside className={cn(
      "h-screen bg-[#F0F0F0] border-r border-[#D1D1D1] flex flex-col transition-all duration-200 z-50 no-select sticky top-0",
      collapsed ? "w-[64px]" : "w-[260px]"
    )}>
      {/* Title Bar / Header Area */}
      <div className="h-[48px] flex items-center justify-between px-4 border-b border-[#D1D1D1] bg-white group hover:bg-[#F3F9FF]">
        {!collapsed && (
          <div className="flex items-center gap-3">
             <div className="w-6 h-6 bg-[#0078D4] rounded-sm flex items-center justify-center text-white font-black text-xs">F</div>
             <span className="text-[14px] font-bold text-[#333] tracking-tight uppercase tracking-[0.1em]">Fusion Store</span>
          </div>
        )}
        <button 
          onClick={() => setCollapsed(!collapsed)}
          className="p-1.5 hover:bg-[#E5F1FB] rounded-sm transition-colors text-slate-500"
        >
          {collapsed ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
        </button>
      </div>

      {/* Navigation Groups */}
      <div className="flex-1 py-4 overflow-y-auto custom-scrollbar">
        {!collapsed && (
           <p className="px-5 text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-3 italic">Navigation Menu</p>
        )}
        <nav className="px-2 space-y-0.5">
          {menuItems.map((item) => {
            const isActive = pathname === item.href;
            return (
              <Link
                key={item.id}
                href={item.href}
                className={cn(
                  "flex items-center gap-3 px-3 py-2 text-[13px] font-medium transition-all group rounded-sm border border-transparent",
                  isActive 
                    ? "bg-[#DDEBFA] text-[#0078D4] border-[#CCE4F7] shadow-sm font-semibold" 
                    : "text-slate-600 hover:bg-[#E5F1FB] hover:text-[#0078D4] hover:border-[#D1D1D1]"
                )}
              >
                <item.icon className={cn(
                  "w-5 h-5 flex-shrink-0",
                  isActive ? "text-[#0078D4]" : "text-slate-400 group-hover:text-[#0078D4]"
                )} />
                {!collapsed && <span>{item.label}</span>}
                {isActive && !collapsed && <div className="ml-auto w-1 h-4 bg-[#0078D4] rounded-full" />}
              </Link>
            );
          })}
        </nav>
      </div>

      {/* Footer / Logout Section */}
      <div className="p-2 border-t border-[#D1D1D1] bg-[#F5F5F5]">
        <button 
          onClick={() => signOut()}
          className="w-full flex items-center gap-3 px-3 py-2 text-[13px] font-medium text-rose-600 hover:bg-rose-50 rounded-sm transition-colors group"
        >
          <LogOut size={18} className="text-rose-500 group-hover:rotate-12 transition-transform" />
          {!collapsed && <span>Đăng xuất Hệ thống</span>}
        </button>
        {!collapsed && (
          <div className="mt-2 px-3 py-2 bg-white/50 border border-slate-200 rounded-sm">
             <p className="text-[9px] font-bold text-slate-400 uppercase">Version Control</p>
             <p className="text-[10px] font-black text-slate-500 italic uppercase">Enterprise v2.5.0</p>
          </div>
        )}
      </div>

      <style jsx global>{`
        .custom-scrollbar::-webkit-scrollbar {
          width: 4px;
        }
        .custom-scrollbar::-webkit-scrollbar-track {
          background: transparent;
        }
        .custom-scrollbar::-webkit-scrollbar-thumb {
          background: #D1D1D1;
          border-radius: 10px;
        }
      `}</style>
    </aside>
  );
}
