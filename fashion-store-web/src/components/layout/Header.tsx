"use client";

import { useSession, signOut } from "next-auth/react";
import { LogOut, Bell, User, Package } from "lucide-react";
import { cn } from "@/lib/utils";

export default function Header() {
  const { data: session } = useSession();

  return (
    <header className="h-[80px] bg-white/80 backdrop-blur-xl px-10 flex items-center justify-between no-print shadow-[0_1px_40px_rgba(0,0,0,0.03)] border-b border-slate-200/60 sticky top-0 z-50">
      <div className="flex items-center gap-10">
        <div className="flex items-center gap-4">
           <div className="w-10 h-10 bg-slate-900 rounded-xl flex items-center justify-center text-blue-500 shadow-xl rotate-3">
              <Package className="w-6 h-6" />
           </div>
           <h1 className="text-[20px] font-black text-slate-900 tracking-tighter uppercase italic leading-none">
             FUSION <span className="text-blue-600">SYSTEM</span>
           </h1>
        </div>
        
        <div className="h-8 w-[1px] bg-slate-200 hidden lg:block" />
        
        <div className="hidden lg:flex items-center gap-6">
           <div className="flex items-center gap-3">
              <div className="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center border border-slate-200 shadow-inner">
                 <User className="w-4 h-4 text-slate-400" />
              </div>
              <div className="flex flex-col">
                 <span className="text-[13px] font-black text-slate-900 leading-none uppercase tracking-tight">{session?.user?.name || "ADMINISTRATOR"}</span>
                 <span className="text-[9px] font-black text-blue-500 uppercase tracking-widest mt-1 opacity-80 italic italic-accent">
                   {session?.user?.role || "MASTER ACCESS"}
                 </span>
              </div>
           </div>
        </div>
      </div>

      <div className="flex items-center gap-6">
        <div className="flex items-center gap-2">
           <button className="p-3 hover:bg-slate-50 text-slate-400 hover:text-slate-900 rounded-2xl transition-all relative">
              <Bell className="w-5 h-5" />
              <div className="absolute top-2.5 right-2.5 w-2 h-2 bg-rose-500 rounded-full border-2 border-white" />
           </button>
        </div>

        <button 
          onClick={() => signOut()}
          className="bg-slate-900 hover:bg-black text-white font-black h-[48px] px-8 rounded-2xl transition-all flex items-center gap-4 shadow-2xl shadow-slate-900/20 active:scale-95 text-[11px] uppercase tracking-widest border-b-4 border-slate-700 leading-none"
        >
          <LogOut className="w-4 h-4 text-blue-500" />
          ĐĂNG XUẤT
        </button>
      </div>
    </header>
  );
}
