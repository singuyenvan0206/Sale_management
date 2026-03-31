"use client";

import { useState, useEffect } from "react";
import { cn } from "@/lib/utils";
import { 
  Settings, 
  Store, 
  ShieldCheck, 
  Printer, 
  Database, 
  Save, 
  RotateCcw,
  Info,
  Smartphone,
  Mail,
  MapPin,
  Globe,
  CreditCard
} from "lucide-react";

export default function SettingsPage() {
  const [settings, setSettings] = useState<any>({
    StoreName: "Fashion Store",
    Address: "123 Đường ABC, Hà Nội",
    Phone: "0123 456 789",
    Email: "contact@fashionstore.com",
    Website: "www.fashionstore.com",
    TaxId: "0011223344",
    PrinterName: "XPrinter XP-N160II",
    PrinterPaperSize: "80mm",
    BackupInterval: "Daily"
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    const load = async () => {
      try {
        const res = await fetch("/api/settings");
        if (res.ok) {
          const data = await res.json();
          if (Object.keys(data).length > 0) setSettings(data);
        }
      } catch (e) {
        console.error("Failed to load settings", e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const handleSave = async () => {
    setSaving(true);
    try {
      const res = await fetch("/api/settings", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(settings)
      });
      if (res.ok) {
        alert("Cấu hình hệ thống đã được cập nhật thành công!");
      }
    } catch (e) {
      alert("Lỗi khi cập nhật cấu hình.");
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div className="p-20 text-center font-black animate-pulse">ĐANG TẢI CẤU HÌNH HỆ THỐNG...</div>;

  return (
    <div className="space-y-6 animate-in fade-in duration-500 pb-20 max-w-5xl mx-auto">
      {/* Header Bar - WPF style */}
      <div className="bg-[#1CB5E0] p-6 -mx-8 -mt-8 mb-8 shadow-lg">
        <h2 className="text-[24px] font-bold text-white text-center tracking-tight">
           ⚙️ Cấu Hình Hệ Thống & Cửa Hàng
        </h2>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
         {/* Sidebar Tabs - WPF style */}
         <div className="md:col-span-1 space-y-2">
            <button className="w-full flex items-center gap-3 p-4 bg-white rounded-lg shadow-sm font-black text-[#1CB5E0] border-l-4 border-[#1CB5E0] text-[13px] uppercase tracking-widest text-left">
               <Store className="w-4 h-4" /> Thông tin cửa hàng
            </button>
            <button className="w-full flex items-center gap-3 p-4 bg-white/50 hover:bg-white rounded-lg text-slate-500 font-bold text-[13px] uppercase tracking-widest text-left transition-all">
               <Printer className="w-4 h-4" /> Máy in & Hóa đơn
            </button>
            <button className="w-full flex items-center gap-3 p-4 bg-white/50 hover:bg-white rounded-lg text-slate-500 font-bold text-[13px] uppercase tracking-widest text-left transition-all">
               <ShieldCheck className="w-4 h-4" /> Bảo mật & Phân quyền
            </button>
            <button className="w-full flex items-center gap-3 p-4 bg-white/50 hover:bg-white rounded-lg text-slate-500 font-bold text-[13px] uppercase tracking-widest text-left transition-all">
               <Database className="w-4 h-4" /> Sao lưu & Phục hồi
            </button>
         </div>

         {/* Content - WPF style Form */}
         <div className="md:col-span-2 space-y-6">
            <div className="bg-white p-8 rounded-xl shadow-sm border border-slate-100 space-y-8">
               {/* Store Section */}
               <div className="space-y-6">
                  <div className="flex items-center gap-2 border-b pb-4">
                     <Info className="w-5 h-5 text-[#1CB5E0]" />
                     <h3 className="text-[15px] font-black text-slate-800 uppercase tracking-tighter shadow-sm bg-blue-50 px-3 py-1 rounded">Hồ sơ kinh doanh</h3>
                  </div>
                  
                  <div className="space-y-4">
                     <div className="space-y-1.5">
                        <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Tên thương hiệu *</label>
                        <input 
                          type="text" 
                          value={settings.StoreName}
                          onChange={(e) => setSettings({...settings, StoreName: e.target.value})}
                          className="w-full bg-[#f8f9fa] rounded-md py-3 px-4 text-sm font-black text-slate-700 focus:bg-white focus:ring-2 focus:ring-blue-100 outline-none transition-all uppercase tracking-tight" 
                        />
                     </div>
                     <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-1.5">
                           <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Số điện thoại</label>
                           <div className="relative">
                              <Smartphone className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                              <input 
                               type="text" 
                               value={settings.Phone}
                               onChange={(e) => setSettings({...settings, Phone: e.target.value})}
                               className="w-full bg-[#f8f9fa] rounded-md py-2.5 pl-10 pr-3 text-sm font-bold" 
                              />
                           </div>
                        </div>
                        <div className="space-y-1.5">
                           <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest"> Mã số thuế</label>
                           <div className="relative">
                              <CreditCard className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                              <input 
                               type="text" 
                               value={settings.TaxId}
                               onChange={(e) => setSettings({...settings, TaxId: e.target.value})}
                               className="w-full bg-[#f8f9fa] rounded-md py-2.5 pl-10 pr-3 text-sm font-bold" 
                              />
                           </div>
                        </div>
                     </div>
                     <div className="space-y-1.5">
                        <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Địa chỉ trụ sở</label>
                        <textarea 
                           rows={2}
                           value={settings.Address}
                           onChange={(e) => setSettings({...settings, Address: e.target.value})}
                           className="w-full bg-[#f8f9fa] rounded-md py-3 px-4 text-sm font-bold resize-none" 
                        />
                     </div>
                  </div>
               </div>

               {/* Printer Section */}
               <div className="space-y-6 pt-4 border-t">
                  <div className="flex items-center gap-2 border-b pb-4">
                     <Printer className="w-5 h-5 text-[#1CB5E0]" />
                     <h3 className="text-[15px] font-black text-slate-800 uppercase tracking-tighter shadow-sm bg-blue-50 px-3 py-1 rounded">Cấu hình in ấn (80mm)</h3>
                  </div>
                  
                  <div className="grid grid-cols-2 gap-4">
                     <div className="space-y-1.5">
                        <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Tên máy in mặc định</label>
                        <input 
                          type="text" 
                          value={settings.PrinterName}
                          onChange={(e) => setSettings({...settings, PrinterName: e.target.value})}
                          className="w-full bg-[#f8f9fa] rounded-md py-2.5 px-3 text-sm font-bold" 
                        />
                     </div>
                     <div className="space-y-1.5">
                        <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Khổ giấy</label>
                        <select 
                          value={settings.PrinterPaperSize}
                          onChange={(e) => setSettings({...settings, PrinterPaperSize: e.target.value})}
                          className="w-full bg-[#f8f9fa] rounded-md py-2.5 px-3 text-sm font-black"
                        >
                           <option value="80mm">Khổ K80 (Tiêu chuẩn)</option>
                           <option value="58mm">Khổ K58 (Mini)</option>
                           <option value="A4">Khổ A4 (Báo cáo)</option>
                        </select>
                     </div>
                  </div>
               </div>

               <div className="pt-8 border-t flex flex-col md:flex-row gap-3">
                  <button 
                    disabled={saving}
                    onClick={handleSave}
                    className="flex-1 bg-[#4CAF50] hover:bg-[#43a047] text-white font-black py-4 rounded-lg text-[13px] uppercase tracking-[0.2em] shadow-lg active:scale-95 flex items-center justify-center gap-2 transition-all disabled:opacity-50"
                  >
                     <Save className="w-5 h-5" /> {saving ? "ĐANG LƯU..." : "LƯU CẤU HÌNH"}
                  </button>
                  <button className="px-8 bg-slate-100 hover:bg-slate-200 text-slate-500 font-black py-4 rounded-lg text-[13px] uppercase tracking-widest active:scale-95 flex items-center justify-center gap-2 transition-all">
                     <RotateCcw className="w-5 h-5" /> KHÔI PHỤC
                  </button>
               </div>
            </div>

            {/* System Info Cards */}
            <div className="grid grid-cols-2 gap-4">
               <div className="bg-slate-900 p-6 rounded-xl text-white">
                  <p className="text-[9px] font-black text-slate-500 uppercase tracking-[0.2em]">Phiên bản phần mềm</p>
                  <p className="text-lg font-black mt-1">v2.4.0 (Enterprise)</p>
               </div>
               <div className="bg-white p-6 rounded-xl border border-slate-100">
                  <p className="text-[9px] font-black text-slate-400 uppercase tracking-[0.2em]">Trạng thái máy chủ</p>
                  <p className="text-lg font-black mt-1 text-emerald-500 flex items-center gap-2">
                     <span className="w-2 h-2 bg-emerald-500 rounded-full animate-pulse" /> Đang hoạt động
                  </p>
               </div>
            </div>
         </div>
      </div>
    </div>
  );
}
