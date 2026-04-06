"use client";

import { signOut, useSession } from "next-auth/react";
import { LogOut, User } from "lucide-react";
import { cn } from "@/lib/utils";

export function UserMenu() {
  const { data: session } = useSession();

  return (
    <div className="flex items-center gap-6">
      <div className="flex items-center gap-2 group cursor-pointer">
        <div className="w-8 h-8 rounded-full bg-slate-100 border border-slate-200 flex items-center justify-center text-[10px] font-black text-[#0078D4] shadow-sm group-hover:bg-[#0078D4] group-hover:text-white transition-colors uppercase">
          {session?.user?.name ? session.user.name.substring(0, 2) : "AD"}
        </div>
        <div className="flex flex-col">
          <span className="text-[11px] font-bold text-slate-600 uppercase leading-none">
            {session?.user?.name || "Administrator"}
          </span>
          <span className="text-[9px] font-bold text-slate-400 uppercase tracking-widest mt-1">
             {session?.user?.role || "System Master"}
          </span>
        </div>
      </div>
      
      <div className="h-6 w-[1px] bg-slate-200 mx-2" />
      
      <button 
        onClick={() => signOut()}
        className="flex items-center gap-2 px-4 h-8 bg-white hover:bg-rose-50 border border-slate-200 text-rose-500 rounded-sm transition-all active:scale-95 text-[10px] font-black uppercase tracking-widest shadow-sm hover:border-rose-200"
      >
        <LogOut className="w-3.5 h-3.5" />
        ĐĂNG XUẤT
      </button>
    </div>
  );
}
