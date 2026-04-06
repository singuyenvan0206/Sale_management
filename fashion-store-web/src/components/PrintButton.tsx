"use client";

import { Printer } from "lucide-react";

export default function PrintButton() {
  return (
    <button 
      onClick={() => window.print()}
      className="btn-wpf btn-wpf-primary h-12 px-10 flex items-center justify-center gap-3 uppercase font-black text-[12px] no-print"
    >
      <Printer className="w-5 h-5" />
      IN HÓA ĐƠN (PRINT)
    </button>
  );
}
