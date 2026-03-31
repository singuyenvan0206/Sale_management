"use client";

import { useState, useEffect, useMemo } from "react";
import { cn } from "@/lib/utils";
import { 
  UserCircle, 
  Search, 
  RotateCcw, 
  ShieldCheck,
  UserPlus,
  Trash2,
  Lock,
  UserCog
} from "lucide-react";

export default function UserManagementPage() {
  const [users, setUsers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  // Form State
  const [editingUser, setEditingUser] = useState<any>({
    Id: 0,
    Username: "",
    Password: "",
    EmployeeName: "",
    Role: "Cashier"
  });

  // Filter State
  const [searchTerm, setSearchTerm] = useState("");
  const [filterRole, setFilterRole] = useState("All");

  const [statusText, setStatusText] = useState("Sẵn sàng");

  useEffect(() => {
    const load = async () => {
      try {
        const res = await fetch("/api/users");
        const data = await res.json();
        if (Array.isArray(data)) setUsers(data);
      } catch (e) {
        console.error("Failed to load users", e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const filteredUsers = useMemo(() => {
    if (!Array.isArray(users)) return [];
    return users.filter(u => {
      const matchesSearch = !searchTerm || 
        u.Username.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (u.EmployeeName || "").toLowerCase().includes(searchTerm.toLowerCase());
      
      const matchesRole = filterRole === "All" || u.Role === filterRole;

      return matchesSearch && matchesRole;
    });
  }, [users, searchTerm, filterRole]);

  const handleSelectUser = (user: any) => {
    setEditingUser({...user, Password: ""}); // Don't show password
    setStatusText(`Đang chọn: ${user.Username}`);
  };

  const handleClear = () => {
    setEditingUser({
      Id: 0,
      Username: "",
      Password: "",
      EmployeeName: "",
      Role: "Cashier"
    });
    setStatusText("Mới");
  };

  if (loading) return <div className="p-20 text-center font-black animate-pulse">ĐANG TẢI DANH SÁCH NGƯỜI DÙNG...</div>;

  return (
    <div className="space-y-6 animate-in fade-in duration-500 pb-20">
      {/* Header Bar - WPF style */}
      <div className="bg-[#1CB5E0] p-6 -mx-8 -mt-8 mb-8 shadow-lg">
        <h2 className="text-[24px] font-bold text-white text-center tracking-tight">
           👤 Quản Lý Người Dùng
        </h2>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
        {/* Left Panel - Input Form */}
        <div className="lg:col-span-1">
           <div className="bg-white p-6 rounded-[10px] shadow-sm space-y-6 sticky top-4 border-t-4 border-blue-600">
              <div className="flex items-center justify-between border-b pb-3">
                 <h3 className="text-[14px] font-black text-slate-800 uppercase italic tracking-tight">Tài khoản hệ thống</h3>
                 <span className="text-[10px] font-bold text-blue-600 px-2 py-0.5 bg-blue-50 rounded uppercase">
                    {editingUser.Role || "Người dùng"}
                 </span>
              </div>
              
              <div className="space-y-4">
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Tên đăng nhập *</label>
                    <div className="relative">
                       <UserCircle className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                       <input 
                        type="text" 
                        disabled={editingUser.Id !== 0}
                        value={editingUser.Username}
                        onChange={(e) => setEditingUser({...editingUser, Username: e.target.value})}
                        className={cn(
                          "w-full bg-[#f8f9fa] rounded-md py-2 pl-10 pr-3 text-sm font-bold focus:bg-white focus:ring-2 focus:ring-blue-100 outline-none transition-all",
                          editingUser.Id !== 0 && "opacity-50 cursor-not-allowed"
                        )}
                        placeholder="admin..." 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Mật khẩu *</label>
                    <div className="relative">
                       <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                       <input 
                        type="password" 
                        value={editingUser.Password}
                        onChange={(e) => setEditingUser({...editingUser, Password: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 pl-10 pr-3 text-sm font-bold focus:bg-white outline-none" 
                        placeholder="********" 
                       />
                    </div>
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Tên nhân viên</label>
                    <input 
                      type="text" 
                      value={editingUser.EmployeeName || ""}
                      onChange={(e) => setEditingUser({...editingUser, EmployeeName: e.target.value})}
                      className="w-full bg-[#f8f9fa] rounded-md py-2 px-3 text-sm font-bold focus:bg-white outline-none" 
                      placeholder="Nguyễn Văn A..." 
                    />
                 </div>
                 <div className="space-y-1.5">
                    <label className="text-[11px] font-black text-slate-400 uppercase tracking-widest">Vai trò quyền hạn</label>
                    <div className="relative">
                       <ShieldCheck className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 pointer-events-none" />
                       <select 
                        value={editingUser.Role}
                        onChange={(e) => setEditingUser({...editingUser, Role: e.target.value})}
                        className="w-full bg-[#f8f9fa] rounded-md py-2 pl-10 pr-3 text-sm font-bold cursor-pointer outline-none appearance-none"
                       >
                          <option value="Cashier">Thu ngân</option>
                          <option value="Manager">Quản lý</option>
                          <option value="Admin">Quản trị viên</option>
                       </select>
                    </div>
                 </div>
              </div>

              <div className="grid grid-cols-2 gap-2 pt-4">
                 <button className="bg-[#4CAF50] hover:bg-[#43a047] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-widest shadow-md active:scale-95 flex items-center justify-center gap-2">
                    <UserPlus className="w-3.5 h-3.5" /> Thêm mới
                 </button>
                 <button className="bg-[#2196F3] hover:bg-[#1e88e5] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-widest shadow-md active:scale-95 flex items-center justify-center gap-2">
                    <UserCog className="w-3.5 h-3.5" /> Cập nhật
                 </button>
                 <button className="col-span-2 bg-[#F44336] hover:bg-[#e53935] text-white font-black py-4 rounded-md text-[10px] uppercase tracking-[0.2em] shadow-md active:scale-95 flex items-center justify-center gap-2">
                    <Trash2 className="w-3.5 h-3.5" /> Xóa tài khoản
                 </button>
              </div>
              <button 
                onClick={handleClear}
                className="w-full bg-[#9E9E9E] hover:bg-[#757575] text-white font-black py-3 rounded-md text-[10px] uppercase tracking-widest flex items-center justify-center gap-2 active:scale-95 transition-all"
              >
                 <RotateCcw className="w-3.5 h-3.5" /> Làm mới Form
              </button>
              <div className="text-[10px] font-bold text-slate-400 italic text-center pt-2">
                 Trạng thái: {statusText}
              </div>
           </div>
        </div>

        {/* Right Panel - User List */}
        <div className="lg:col-span-3">
          <div className="bg-white rounded-[10px] shadow-sm overflow-hidden min-h-[700px] flex flex-col">
            <div className="p-6 bg-[#f8f9fa]/50">
               <h3 className="text-[18px] font-black text-[#1CB5E0] uppercase tracking-tight mb-6">👤 Danh Sách Người Dùng Hệ Thống</h3>
               
               {/* Filter Panel */}
               <div className="bg-[#F8F9FA] p-5 rounded-[8px] space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                     <div className="space-y-1">
                        <span className="text-[10px] font-black text-slate-400 uppercase">Lọc Vai Trò</span>
                        <select 
                          value={filterRole}
                          onChange={(e) => setFilterRole(e.target.value)}
                          className="w-full h-10 bg-white rounded-md px-3 text-sm font-bold cursor-pointer outline-none shadow-sm focus:ring-2 focus:ring-blue-100"
                        >
                           <option value="All">Tất cả vai trò</option>
                           <option value="Admin">Quản trị viên</option>
                           <option value="Manager">Quản lý</option>
                           <option value="Cashier">Thu ngân</option>
                        </select>
                     </div>
                     <div className="md:col-span-3 flex items-end gap-2">
                        <div className="flex-1 relative">
                           <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                           <input 
                            type="text" 
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full h-10 bg-white rounded-md pl-10 pr-3 text-sm font-bold outline-none shadow-sm focus:ring-2 focus:ring-blue-100" 
                            placeholder="Tìm kiếm theo Tên nhân viên hoặc Username..." 
                           />
                        </div>
                        <button className="bg-[#FF9800] hover:bg-[#f57c00] text-white font-black h-10 px-8 rounded-md text-[11px] uppercase tracking-widest whitespace-nowrap active:scale-95 shadow-md">🔍 Lọc</button>
                     </div>
                  </div>
               </div>
            </div>

            <div className="flex-1 overflow-x-auto">
              <table className="w-full text-left">
                <thead className="bg-[#f8f9fa] sticky top-0 z-10 border-b border-slate-100">
                  <tr className="text-slate-800 text-[11px] font-black uppercase tracking-tight">
                    <th className="px-6 py-5">ID</th>
                    <th className="px-6 py-5">👤 Tài khoản (Username)</th>
                    <th className="px-6 py-5">📛 Tên nhân viên</th>
                    <th className="px-6 py-5">🛡️ Vai trò</th>
                    <th className="px-6 py-5 text-center">📅 Ngày tạo</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {filteredUsers.map((u: any, idx: number) => (
                    <tr 
                      key={u.Id} 
                      onClick={() => handleSelectUser(u)}
                      className={cn(
                        "hover:bg-blue-600 hover:text-white transition-all cursor-pointer group font-medium",
                        idx % 2 === 1 ? "bg-[#F8F9FA]" : "bg-white",
                        editingUser.Id === u.Id ? "bg-blue-600 text-white font-black shadow-lg" : "text-slate-700"
                      )}
                    >
                      <td className="px-6 py-5 font-bold text-[12px] opacity-40">{u.Id}</td>
                      <td className="px-6 py-5 font-black uppercase tracking-tighter text-[14px]">
                         {u.Username}
                      </td>
                      <td className="px-6 py-5 font-bold">{u.EmployeeName || "---"}</td>
                      <td className="px-6 py-5">
                         <span className={cn(
                            "px-3 py-1 rounded text-[10px] font-black uppercase tracking-widest",
                            u.Role === 'Admin' ? "bg-rose-100 text-rose-600" : 
                            u.Role === 'Manager' ? "bg-blue-100 text-blue-600" : "bg-slate-100 text-slate-600"
                         )}>
                            {u.Role === 'Admin' ? 'Quản trị viên' : u.Role === 'Manager' ? 'Quản lý' : 'Thu ngân'}
                         </span>
                      </td>
                      <td className="px-6 py-5 text-center text-[12px] opacity-60">
                         {new Date(u.CreatedDate).toLocaleDateString('vi-VN')}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            
            <div className="p-8 bg-[#F8F9FA] flex flex-col sm:flex-row items-center justify-between gap-4">
               <span className="text-[12px] font-black text-[#1CB5E0] uppercase tracking-tighter">
                  📊 Hiển thị {filteredUsers.length} tài khoản hệ thống
               </span>
               <div className="flex gap-2">
                  <div className="w-10 h-8 bg-white border border-slate-200 rounded flex items-center justify-center text-[12px] font-black shadow-sm">1</div>
               </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
