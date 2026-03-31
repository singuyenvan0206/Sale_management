import type { Metadata } from "next";
import "./globals.css";
import { Sidebar } from "@/components/layout/Sidebar";
import Header from "@/components/layout/Header";
import { Providers } from "@/components/providers";
import { cn } from "@/lib/utils";

export const metadata: Metadata = {
  title: "Fashion Store Management Dashboard",
  description: "Bản Web cao cấp cho hệ thống quản lý Fashion Store",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi" className="h-full">
      <body className={cn("font-sans bg-[#F5F5F5] text-slate-900 h-full antialiased flex flex-col")}>
        <Providers>
          <Header />
          <div className="flex flex-1 overflow-hidden">
            <Sidebar />
            <main className="flex-1 overflow-y-auto bg-[#F5F5F5] relative">
              <div className="p-8 relative z-10">
                {children}
              </div>
            </main>
          </div>
        </Providers>
      </body>
    </html>
  );
}
