"use client";

import { useState, useEffect } from "react";
import { cn } from "@/lib/utils";
import { 
  Settings, 
  Save, 
  Building, 
  Phone, 
  MapPin, 
  CreditCard, 
  CheckCircle2, 
  AlertCircle,
  Smartphone,
  Globe,
  Briefcase,
  User,
  ShieldCheck,
  Zap,
  Info,
  Database,
  Cpu,
  Lock,
  FileText
} from "lucide-react";

export default function SettingsPage() {
  const [settings, setSettings] = useState<any>({
    StoreName: "",
    StoreAddress: "",
    StorePhone: "",
    BankId: "",
    AccountNumber: "",
    AccountName: ""
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<{ text: string, type: 'success' | 'error' | null }>({ text: "", type: null });

  useEffect(() => {
    const loadSettings = async () => {
      try {
        const res = await fetch("/api/settings");
        const json = await res.json();
        if (json.success) {
          setSettings(json.data);
        } else {
          showMsg(json.error || "Lỗi cấu hình", 'error');
        }
      } catch (e) {
        showMsg("Lỗi: Không thể truy xuất cấu hình hệ thống", 'error');
      } finally {
        setLoading(false);
      }
    };
    loadSettings();
  }, []);

  const showMsg = (text: string, type: 'success' | 'error') => {
    setMessage({ text, type });
    setTimeout(() => setMessage({ text: "", type: null }), 3000);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      const res = await fetch("/api/settings", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(settings)
      });
      const json = await res.json();
      if (json.success) {
        showMsg("Cấu hình hệ thống đã được cập nhật thành công", 'success');
      } else {
        showMsg(json.error || "Lỗi khi ghi dữ liệu cấu hình", 'error');
      }
    } catch (e) {
      showMsg("Lỗi kết nối máy chủ", 'error');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] gap-4 no-select uppercase italic font-black text-slate-400">
      <div className="w-12 h-12 border-4 border-[#0078D4] border-t-transparent rounded-full animate-spin" />
      <p className="text-[11px] tracking-widest">Đang tải cấu trúc hạ tầng hệ thống...</p>
    </div>
  );

  return (
    <div className="max-w-5xl mx-auto space-y-6 animate-in fade-in duration-300 pb-20 no-select">
      {/* WPF Header / Ribbon */}
      <div className="wpf-panel">
         <div className="wpf-panel-header uppercase">CẤU HÌNH HỆ THỐNG & TÙY CHỌN VẬN HÀNH (OPTIONS PANEL)</div>
         <div className="p-6 bg-white flex flex-col md:flex-row md:items-center justify-between gap-6">
            <div className="flex items-center gap-4">
               <div className="w-10 h-10 bg-slate-800 rounded-sm flex items-center justify-center text-white shadow-md">
                  <Cpu className="w-6 h-6" />
               </div>
               <div>
                  <h2 className="text-[20px] font-black text-slate-900 tracking-tight uppercase italic leading-none">THIẾT LẬP HỆ THỐNG</h2>
                  <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-widest italic tracking-tighter">Fusion ERP CORE Framework v2.5.0-STABLE</p>
               </div>
            </div>
            
            {message.type && (
               <div className={cn(
                 "px-6 py-3 border rounded-sm flex items-center gap-3 animate-in fade-in zoom-in duration-300 shadow-sm",
                 message.type === 'success' ? "bg-emerald-50 text-emerald-600 border-emerald-200" : "bg-rose-50 text-rose-600 border-rose-200"
               )}>
                  {message.type === 'success' ? <CheckCircle2 className="w-4 h-4" /> : <AlertCircle className="w-4 h-4" />}
                  <span className="text-[10px] font-black uppercase tracking-widest leading-none">{message.text}</span>
               </div>
            )}
         </div>
      </div>

      <form onSubmit={handleSave} className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Left Column: Store Details */}
        <div className="lg:col-span-7 space-y-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex items-center gap-2">
                 <Building className="w-4 h-4 text-[#0078D4]" /> DANH TÍNH DOANH NGHIỆP
              </div>
              <div className="p-6 bg-white space-y-5">
                 <div className="wpf-groupbox !mt-0">
                    <span className="wpf-groupbox-label">Thông tin cơ bản (In trên hóa đơn)</span>
                    <div className="space-y-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Tên thương hiệu / Cửa hàng:</label>
                          <input 
                            type="text" 
                            required
                            value={settings.StoreName || ""}
                            onChange={(e) => setSettings({...settings, StoreName: e.target.value})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm uppercase italic"
                            placeholder="Tên doanh nghiệp..." 
                          />
                       </div>
                       <div className="grid grid-cols-2 gap-4">
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Hotline CSKH:</label>
                             <input 
                               type="text" 
                               value={settings.StorePhone || ""}
                               onChange={(e) => setSettings({...settings, StorePhone: e.target.value})}
                               className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm tabular-nums"
                               placeholder="09..." 
                             />
                          </div>
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Website URL:</label>
                             <input 
                               type="text" 
                               value={settings.StoreWebsite || ""}
                               onChange={(e) => setSettings({...settings, StoreWebsite: e.target.value})}
                               className="w-full h-10 border border-[#D1D1D1] px-4 text-[12px] font-medium bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm"
                               placeholder="www.yoursite.com" 
                             />
                          </div>
                       </div>
                    </div>
                 </div>

                 <div className="wpf-groupbox">
                    <span className="wpf-groupbox-label">Địa điểm vận hành</span>
                    <div className="pt-2">
                       <textarea 
                         rows={3}
                         value={settings.StoreAddress || ""}
                         onChange={(e) => setSettings({...settings, StoreAddress: e.target.value})}
                         className="w-full border border-[#D1D1D1] p-3 text-[12px] font-medium bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm resize-none italic"
                         placeholder="Số nhà, đường, quận, thành phố..." 
                       />
                    </div>
                 </div>
              </div>
           </div>

           <div className="wpf-panel p-6 bg-[#F0F0FF] border-blue-200">
              <div className="flex gap-4">
                 <div className="p-2 bg-blue-600 text-white rounded-sm shadow-sm">
                    <Info className="w-5 h-5" />
                 </div>
                 <div>
                    <h4 className="text-[12px] font-black text-blue-900 uppercase italic mb-1">KIỂM SOÁT HẠ TẦNG (AUDIT LOG)</h4>
                    <p className="text-[10px] font-bold text-blue-700/60 leading-relaxed uppercase tracking-tight">
                       Mọi thay đổi trên Options Panel này sẽ được lưu lại trong nhật ký hệ thống. Nội dung thay đổi sẽ tự động đồng bộ lên App Mobile và máy in hóa đơn (Thermal Printer) sau 30 giây.
                    </p>
                 </div>
              </div>
           </div>
        </div>

        {/* Right Column: Financials & Save */}
        <div className="lg:col-span-5 space-y-6">
           <div className="wpf-panel shadow-md border-amber-200">
              <div className="wpf-panel-header !bg-amber-500 !text-white flex items-center gap-2">
                 <CreditCard className="w-4 h-4" /> CẤU HÌNH THANH TOÁN (FINANCIAL GATEWAY)
              </div>
              <div className="p-6 bg-white space-y-5">
                 <div className="wpf-groupbox !mt-0 !border-amber-200">
                    <span className="wpf-groupbox-label !text-amber-600 !bg-white">Cổng VietQR POS</span>
                    <div className="space-y-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Ngân hàng đích (BANK-ID):</label>
                          <select 
                            value={settings.BankId || ""}
                            onChange={(e) => setSettings({...settings, BankId: e.target.value})}
                            className="w-full h-10 border border-[#D1D1D1] px-3 text-[11px] font-black bg-white focus:border-amber-500 outline-none rounded-sm cursor-pointer uppercase"
                          >
                             <option value="MB">MBBank (NGÂN HÀNG QUÂN ĐỘI)</option>
                             <option value="VCB">VIETCOMBANK (NGOẠI THƯƠNG)</option>
                             <option value="TCB">TECHCOMBANK (KỸ THƯƠNG)</option>
                             <option value="ACB">ACB BANK (Á CHÂU)</option>
                             <option value="VPB">VPBANK (THỊNH VƯỢNG)</option>
                          </select>
                       </div>
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Số tài khoản hưởng thụ:</label>
                          <input 
                            type="text" 
                            required
                            value={settings.AccountNumber || ""}
                            onChange={(e) => setSettings({...settings, AccountNumber: e.target.value})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-amber-500 outline-none rounded-sm tabular-nums"
                            placeholder="STK..." 
                          />
                       </div>
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Tên chủ tài khoản (In hoa không dấu):</label>
                          <input 
                            type="text" 
                            required
                            value={settings.AccountName || ""}
                            onChange={(e) => setSettings({...settings, AccountName: e.target.value.toUpperCase()})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-amber-500 outline-none rounded-sm uppercase tabular-nums"
                            placeholder="ACCOUNT NAME..." 
                          />
                       </div>
                    </div>
                 </div>

                 <div className="mt-4 p-4 border border-dashed border-amber-200 rounded-sm bg-amber-50/30 flex flex-col items-center">
                    <p className="text-[9px] font-black text-amber-600 uppercase tracking-widest mb-3 italic">POS QR LIVE PREVIEW</p>
                    <div className="bg-white p-2 border border-slate-200 shadow-sm rounded-sm">
                       <img 
                         src={`https://img.vietqr.io/image/${settings.BankId}-${settings.AccountNumber}-compact.png?amount=0&addInfo=POS&accountName=${encodeURIComponent(settings.AccountName || "")}`}
                         alt="VietQR POS"
                         className="w-24 h-24 object-contain opacity-40 grayscale contrast-125"
                       />
                    </div>
                    <p className="text-[8px] font-bold text-slate-400 mt-2 uppercase">Chỉ dùng để kiểm tra cấu trúc QR</p>
                 </div>
              </div>
           </div>

           <button 
             type="submit"
             disabled={saving}
             className={cn(
               "btn-wpf btn-wpf-primary w-full h-14 flex items-center justify-center gap-4 text-[13px] font-black uppercase tracking-widest border-b-6 border-[#005A9E]",
               saving && "opacity-80 cursor-wait"
             )}
           >
              {saving ? <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" /> : <ShieldCheck className="w-6 h-6" />}
              {saving ? "ĐANG LƯU CẤU HÌNH..." : "LƯU CẤU HÌNH VÀ TÁI KHỞI ĐỘNG CỔNG"}
           </button>

           <div className="wpf-panel p-4 bg-slate-800 text-white border-slate-700">
              <div className="flex items-center gap-3">
                 <Database className="w-4 h-4 text-emerald-400" />
                 <div className="flex flex-col">
                    <span className="text-[10px] font-black uppercase leading-none mb-1">Database Connectivity</span>
                    <span className="text-[9px] font-bold text-emerald-400 tracking-widest">CONNECTED TO POSTGRESQL HOST:127.0.0.1</span>
                 </div>
              </div>
           </div>
        </div>
      </form>
    </div>
  );
}
