import type { Metadata } from "next";
import { Inter, Be_Vietnam_Pro } from "next/font/google";
import "./globals.css";
import Sidebar from "@/components/layout/Sidebar";
import { Providers } from "@/components/providers";
import { cn } from "@/lib/utils";
import { UserMenu } from "@/components/layout/UserMenu";

const inter = Inter({ subsets: ["latin", "vietnamese"], variable: "--font-inter" });
const beVietnam = Be_Vietnam_Pro({ 
  weight: ["100", "300", "400", "500", "700", "800", "900"],
  subsets: ["latin", "vietnamese"], 
  variable: "--font-be-vietnam" 
});

export const metadata: Metadata = {
  title: "Fusion Fashion | Hệ Thống Quản Trị Cao Cấp",
  description: "Trình quản lý bán hàng chuẩn Enterprise cho chuỗi Fashion Store",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi" className={cn("h-full", inter.variable, beVietnam.variable)}>
      <body className={cn("font-sans bg-[#F5F5F5] text-slate-900 h-full antialiased flex flex-col selection:bg-blue-100")}>
        <Providers>
          <div className="flex min-h-screen bg-[#F3F3F3]">
            <Sidebar />
            <main className="flex-1 flex flex-col min-h-screen">
              {/* WPF Title Bar / Header */}
              <header className="h-[48px] bg-white border-b border-[#D1D1D1] flex items-center justify-between px-6 sticky top-0 z-40 no-select">
                 <div className="flex items-center gap-4">
                    <div className="h-4 w-1 bg-[#0078D4] rounded-full" />
                    <span className="text-[11px] font-black text-slate-400 uppercase tracking-[0.2em] italic">Enterprise Resource Planning</span>
                 </div>
                 <div className="flex items-center gap-6">
                    <UserMenu />
                    <div className="flex items-center gap-1 border-l border-slate-200 pl-6 h-6">
                       <button className="w-8 h-8 hover:bg-[#E5F1FB] rounded-sm flex items-center justify-center transition-colors">
                          <div className="w-3 h-0.5 bg-slate-400" />
                       </button>
                       <button className="w-8 h-8 hover:bg-[#E5F1FB] rounded-sm flex items-center justify-center transition-colors">
                          <div className="w-3 h-3 border-2 border-slate-400 rounded-sm" />
                       </button>
                       <button className="w-8 h-8 hover:bg-rose-500 rounded-sm flex items-center justify-center transition-colors group">
                          <div className="relative w-3 h-3">
                             <div className="absolute top-1.5 w-3 h-0.5 bg-slate-400 rotate-45 group-hover:bg-white" />
                             <div className="absolute top-1.5 w-3 h-0.5 bg-slate-400 -rotate-45 group-hover:bg-white" />
                          </div>
                       </button>
                    </div>
                 </div>
              </header>

              {/* Application View Surface */}
              <div className="p-8 overflow-y-auto">
                 {children}
              </div>
            </main>
          </div>
        </Providers>
      </body>
    </html>
  );
}
