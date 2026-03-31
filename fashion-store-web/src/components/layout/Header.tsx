"use client";

import { useSession, signOut } from "next-auth/react";
import { LogOut, Bell, User, AlertTriangle } from "lucide-react";
import { cn } from "@/lib/utils";

export default function Header() {
  const { data: session } = useSession();

  return (
    <header className="h-[80px] bg-[#1CB5E0] px-8 flex items-center justify-between no-print shadow-md relative z-50">
      <div className="flex items-center gap-6">
        <h1 className="text-[24px] font-bold text-white tracking-tight">
          🏪 Hệ Thống Quản Lý POS
        </h1>
        <div className="h-6 w-[1px] bg-[#B3E5FC] hidden md:block" />
        <div className="hidden md:flex items-center gap-2 text-[#B3E5FC] font-bold text-[16px]">
          <span>{session?.user?.name || "Người dùng"}</span>
          <span className="opacity-50 mx-1">|</span>
          <span className="uppercase text-[11px] bg-white/10 px-2 py-0.5 rounded-lg border border-white/10 text-white font-black">
            {session?.user?.role || "Nhân viên"}
          </span>
        </div>
      </div>

      <div className="flex items-center gap-4">
        <button className="bg-[#FFC107] hover:bg-[#FFB300] text-white font-bold h-[40px] px-4 rounded-lg transition-all flex items-center gap-2 shadow-sm text-[12px] group active:scale-95">
          <span className="text-[14px]">⚠️</span>
          <span>Sắp hết</span>
          <span className="bg-[#DC2626] text-white text-[12px] px-2 py-0.5 rounded-full min-w-[30px] font-bold text-center">
            5
          </span>
        </button>

        <button 
          onClick={() => signOut()}
          className="bg-[#FC5C7D] hover:bg-[#f34a6d] text-white font-bold h-[35px] px-4 rounded-lg transition-all flex items-center gap-2 shadow-sm text-[13px] active:scale-95"
        >
          <span className="text-[14px]">🚪</span>
          <span>Đăng Xuất</span>
        </button>
      </div>
    </header>
  );
}
