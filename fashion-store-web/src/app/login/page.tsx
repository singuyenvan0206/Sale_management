"use client";

import { useState, useEffect } from "react";
import { signIn } from "next-auth/react";
import { useRouter } from "next/navigation";
import { User, Lock, ArrowRight, Loader2, Monitor, ShieldCheck, Power } from "lucide-react";
import { cn } from "@/lib/utils";

export default function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [currentTime, setCurrentTime] = useState(new Date());
  
  const router = useRouter();

  useEffect(() => {
    const timer = setInterval(() => setCurrentTime(new Date()), 1000);
    return () => clearInterval(timer);
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const result = await signIn("credentials", {
        username,
        password,
        redirect: false,
      });

      if (result?.error) {
        setError("AUTHENTICATION FAILED: Invalid credentials provided.");
      } else {
        router.push("/");
        router.refresh();
      }
    } catch (err) {
      setError("SYSTEM ERROR: Failed to establish secure connection.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#004275] flex items-center justify-center font-sans overflow-hidden relative no-select">
      {/* Windows 10/11-like Background Overlay */}
      <div className="absolute inset-0 bg-gradient-to-br from-[#004275] via-[#005A9E] to-[#0078D4] opacity-90" />
      <div className="absolute inset-0 bg-[url('https://images.unsplash.com/photo-1497366216548-37526070297c?q=80&w=2069&auto=format&fit=crop')] bg-cover bg-center mix-blend-overlay opacity-30 blur-sm" />

      {/* Top Right System Info */}
      <div className="absolute top-10 right-10 text-white text-right z-10 hidden md:block">
         <p className="text-[64px] font-light leading-none tracking-tighter">
            {currentTime.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false })}
         </p>
         <p className="text-[18px] font-medium opacity-80 mt-2">
            {currentTime.toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' })}
         </p>
      </div>

      {/* Main Logon UI */}
      <div className="relative z-20 w-full max-w-[360px] flex flex-col items-center animate-in fade-in zoom-in-95 duration-700">
         <div className="w-48 h-48 bg-white/10 rounded-full border border-white/20 flex items-center justify-center mb-8 backdrop-blur-md shadow-2xl overflow-hidden relative group">
            <User className="w-24 h-24 text-white opacity-90 group-hover:scale-110 transition-transform duration-700" />
            <div className="absolute inset-0 bg-white/5 opacity-0 group-hover:opacity-100 transition-opacity" />
         </div>

         <h1 className="text-white text-[28px] font-light mb-8 tracking-wide">FUSION ADMINISTRATOR</h1>

         <form onSubmit={handleSubmit} className="w-full space-y-4">
            <div className="relative group">
               <input
                 type="text"
                 required
                 placeholder="Tên đăng nhập (Username)"
                 className="w-full bg-white/10 border border-white/20 h-10 px-4 pr-10 text-white placeholder:text-white/40 focus:bg-white focus:text-slate-900 focus:outline-none transition-all rounded-sm font-medium"
                 value={username}
                 onChange={(e) => setUsername(e.target.value)}
               />
               <User className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-white/40 group-focus-within:text-slate-400 transition-colors pointer-events-none" />
            </div>

            <div className="relative group">
               <input
                 type="password"
                 required
                 placeholder="Mật khẩu (Password)"
                 className="w-full bg-white/10 border border-white/20 h-10 px-4 pr-10 text-white placeholder:text-white/40 focus:bg-white focus:text-slate-900 focus:outline-none transition-all rounded-sm font-medium"
                 value={password}
                 onChange={(e) => setPassword(e.target.value)}
               />
               <button 
                 type="submit" 
                 disabled={loading}
                 className="absolute right-0 top-0 bottom-0 w-10 bg-white/10 flex items-center justify-center text-white hover:bg-[#0078D4] disabled:opacity-50 transition-colors border-l border-white/10"
               >
                  {loading ? <Loader2 className="w-4 h-4 animate-spin" /> : <ArrowRight className="w-5 h-5" />}
               </button>
            </div>

            {error && (
              <div className="bg-rose-500/20 border border-rose-500/30 text-rose-100 px-4 py-2 rounded-sm text-[11px] font-bold text-center animate-in shake duration-500">
                {error}
              </div>
            )}
         </form>

         <div className="mt-12 flex items-center gap-6">
            <button className="flex flex-col items-center gap-2 group">
               <div className="w-10 h-10 rounded-full border border-white/20 flex items-center justify-center text-white hover:bg-white/10 transition-all active:scale-90">
                  <ShieldCheck className="w-5 h-5" />
               </div>
               <span className="text-[10px] font-bold text-white/60 group-hover:text-white transition-colors uppercase tracking-widest">Secure</span>
            </button>
            <button className="flex flex-col items-center gap-2 group">
               <div className="w-10 h-10 rounded-full border border-white/20 flex items-center justify-center text-white hover:bg-white/10 transition-all active:scale-90">
                  <Monitor className="w-5 h-5" />
               </div>
               <span className="text-[10px] font-bold text-white/60 group-hover:text-white transition-colors uppercase tracking-widest">Network</span>
            </button>
         </div>
      </div>

      {/* Bottom Footer Controls */}
      <div className="absolute bottom-10 right-10 flex items-center gap-8 z-30">
         <button className="text-white/60 hover:text-white flex items-center gap-2 group transition-all">
            <Power className="w-6 h-6 group-hover:scale-110 transition-transform" />
            <span className="text-[12px] font-bold uppercase tracking-widest hidden md:inline">Shut Down</span>
         </button>
      </div>

      <div className="absolute bottom-10 left-10 text-white/20 text-[10px] font-black uppercase tracking-[0.4em] z-0">
         Fusion ERP v2.5.0 • Desktop Authentication Subsystem
      </div>
    </div>
  );
}
