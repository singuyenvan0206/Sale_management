"use client";

import { useState, useEffect, useMemo } from "react";
import { cn } from "@/lib/utils";
import { 
  Users, 
  Search, 
  RotateCcw, 
  Plus,
  Trash2,
  Edit2,
  UserCheck,
  ShieldAlert,
  Key,
  Save,
  CheckCircle2,
  AlertCircle,
  ArrowUpDown,
  Lock,
  Calendar, 
  X,
  Filter,
  UserCircle,
  BadgeCheck,
  Zap,
  Monitor,
  Shield,
  Briefcase,
  ChevronRight,
} from "lucide-react";

interface Employee {
  Id: number;
  Username: string;
  Role: string;
  EmployeeName: string;
  LastLoginDate: string;
  IsActive: boolean;
  CreatedDate: string;
}

export default function EmployeesPage() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [message, setMessage] = useState<{ text: string, type: 'success' | 'error' | null }>({ text: "", type: null });

  const [editingEmp, setEditingEmp] = useState<any>({
    Id: 0,
    Username: "",
    Password: "",
    Role: "Staff",
    EmployeeName: "",
    IsActive: true
  });

  const [searchTerm, setSearchTerm] = useState("");
  const [roleFilter, setRoleFilter] = useState("All");

  const loadEmployees = async () => {
    setLoading(true);
    try {
      const res = await fetch("/api/employees");
      const json = await res.json();
      if (json.success) setEmployees(json.data);
    } catch (e) {
      showMsg("Lỗi: Không thể truy xuất danh sách nhân sự", 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadEmployees();
  }, []);

  const showMsg = (text: string, type: 'success' | 'error') => {
    setMessage({ text, type });
    setTimeout(() => setMessage({ text: "", type: null }), 3000);
  };

  const filteredEmployees = useMemo(() => {
    if (!Array.isArray(employees)) return [];
    return employees.filter(e => {
      const matchesSearch = !searchTerm || 
        e.EmployeeName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        e.Username.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesRole = roleFilter === "All" || e.Role === roleFilter;
      return matchesSearch && matchesRole;
    });
  }, [employees, searchTerm, roleFilter]);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingEmp.Username.trim() || !editingEmp.EmployeeName.trim()) {
      showMsg("Tên và Tài khoản là các trường bắt buộc", 'error');
      return;
    }

    setActionLoading(true);
    try {
      const isNew = editingEmp.Id === 0;
      const res = await fetch("/api/employees", {
        method: isNew ? "POST" : "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(editingEmp)
      });
      
      const json = await res.json();
      if (json.success) {
        showMsg(isNew ? "Thêm nhân viên thành công" : "Cập nhật quyền hạn thành công", 'success');
        if (isNew) handleClear();
        loadEmployees();
      } else {
        showMsg(json.error || "Lỗi hệ thống", 'error');
      }
    } catch (e) {
      showMsg("Lỗi kết nối máy chủ", 'error');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDelete = async () => {
    if (editingEmp.Id === 0) return;
    if (!confirm(`Xác nhận vô hiệu hóa quyền truy cập hệ thống của "${editingEmp.EmployeeName}"?`)) return;

    setActionLoading(true);
    try {
      const res = await fetch(`/api/employees?id=${editingEmp.Id}`, {
        method: "DELETE"
      });
      const json = await res.json();
      if (json.success) {
        showMsg("Đã khóa quyền truy cập", 'success');
        handleClear();
        loadEmployees();
      } else {
        showMsg(json.error || "Thao tác thất bại", 'error');
      }
    } catch (e) {
      showMsg("Lỗi kết nối mạng", 'error');
    } finally {
      setActionLoading(false);
    }
  };

  const handleClear = () => {
    setEditingEmp({
      Id: 0,
      Username: "",
      Password: "",
      Role: "Staff",
      EmployeeName: "",
      IsActive: true
    });
  };

  if (loading && employees.length === 0) return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] gap-4 no-select uppercase italic font-black text-slate-400">
      <div className="w-12 h-12 border-4 border-[#0078D4] border-t-transparent rounded-full animate-spin" />
      <p className="text-[11px] tracking-widest">Đang kiểm tra ma trận phân quyền (ACL)...</p>
    </div>
  );

  return (
    <div className="space-y-6 animate-in fade-in duration-300 pb-20 no-select">
      {/* WPF Header / Ribbon */}
      <div className="wpf-panel">
         <div className="wpf-panel-header uppercase">HỆ THỐNG QUẢN TRỊ NHÂN SỰ & QUYỀN TRUY CẬP (ACCESS CONTROL)</div>
         <div className="p-6 bg-white flex flex-col md:flex-row md:items-center justify-between gap-6">
            <div className="flex items-center gap-4">
               <div className="w-10 h-10 bg-[#0078D4] rounded-sm flex items-center justify-center text-white shadow-md">
                  <Briefcase className="w-6 h-6" />
               </div>
               <div>
                  <h2 className="text-[20px] font-black text-slate-900 tracking-tight uppercase italic leading-none">QUẢN TRỊ NHÂN SỰ</h2>
                  <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-widest italic tracking-tighter">Fusion ERP User Identity Subsystem v2.5</p>
               </div>
            </div>
            
            {message.type && (
               <div className={cn(
                 "px-6 py-3 border rounded-sm flex items-center gap-3 animate-in fade-in zoom-in duration-300 shadow-sm",
                 message.type === 'success' ? "bg-emerald-50 text-emerald-600 border-emerald-200" : "bg-rose-50 text-rose-600 border-rose-200"
               )}>
                  {message.type === 'success' ? <CheckCircle2 className="w-4 h-4" /> : <ShieldAlert className="w-4 h-4" />}
                  <span className="text-[10px] font-black uppercase tracking-widest leading-none">{message.text}</span>
               </div>
            )}
         </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        {/* Left Column: Editor */}
        <div className="lg:col-span-4 space-y-6 lg:sticky lg:top-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex justify-between items-center text-[#333]">
                 <span className="flex items-center gap-2">
                    <Shield className="w-4 h-4 text-[#0078D4]" /> CẤP QUYỀN TRUY CẬP
                 </span>
                 <span className={cn(
                    "text-[9px] font-black px-2 py-0.5 rounded-sm border",
                    editingEmp.Id === 0 ? "bg-blue-50 text-blue-600 border-blue-200" : "bg-indigo-50 text-indigo-600 border-indigo-200"
                 )}>
                    {editingEmp.Id === 0 ? "NEW USER" : `UID: ${editingEmp.Id}`}
                 </span>
              </div>
              <form onSubmit={handleSave} className="p-6 bg-white space-y-5">
                 <div className="wpf-groupbox !mt-0">
                    <span className="wpf-groupbox-label">Danh tính nhân viên</span>
                    <div className="space-y-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Họ và tên nhân viên:</label>
                          <input 
                            type="text" 
                            required
                            value={editingEmp.EmployeeName}
                            onChange={(e) => setEditingEmp({...editingEmp, EmployeeName: e.target.value})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-bold bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm uppercase"
                            placeholder="NHẬP TÊN..." 
                          />
                       </div>
                       <div className="grid grid-cols-2 gap-4">
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Vị trí / Vai trò:</label>
                             <select 
                               value={editingEmp.Role}
                               onChange={(e) => setEditingEmp({...editingEmp, Role: e.target.value})}
                               className="w-full h-10 border border-[#D1D1D1] px-3 text-[11px] font-black bg-white focus:border-[#0078D4] outline-none rounded-sm cursor-pointer uppercase"
                             >
                                <option value="Staff">STAFF (BÁN HÀNG)</option>
                                <option value="Manager">MANAGER (TỔNG KHO)</option>
                                <option value="Admin">ADMINISTRATOR</option>
                             </select>
                          </div>
                          <div className="space-y-1">
                             <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Trạng thái:</label>
                             <select 
                               value={editingEmp.IsActive ? "true" : "false"}
                               onChange={(e) => setEditingEmp({...editingEmp, IsActive: e.target.value === "true"})}
                               className="w-full h-10 border border-[#D1D1D1] px-3 text-[11px] font-black bg-white focus:border-[#0078D4] outline-none rounded-sm cursor-pointer uppercase"
                             >
                                <option value="true">HOẠT ĐỘNG</option>
                                <option value="false">VÔ HIỆU HÓA</option>
                             </select>
                          </div>
                       </div>
                    </div>
                 </div>

                 <div className="wpf-groupbox">
                    <span className="wpf-groupbox-label">Thông tin đăng nhập</span>
                    <div className="space-y-4 pt-2">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">Tài khoản (Username):</label>
                          <input 
                            type="text" 
                            required
                            disabled={editingEmp.Id !== 0}
                            value={editingEmp.Username}
                            onChange={(e) => setEditingEmp({...editingEmp, Username: e.target.value})}
                            className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm lowercase disabled:opacity-50"
                            placeholder="username..." 
                          />
                       </div>
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest pl-1">
                             Mật khẩu {editingEmp.Id !== 0 ? "(Để trống nếu không đổi)" : "(Password):"}
                          </label>
                          <div className="relative">
                             <input 
                               type="password" 
                               required={editingEmp.Id === 0}
                               value={editingEmp.Password}
                               onChange={(e) => setEditingEmp({...editingEmp, Password: e.target.value})}
                               className="w-full h-10 border border-[#D1D1D1] px-4 text-[13px] font-black bg-[#F9F9F9] focus:bg-white focus:border-[#0078D4] outline-none rounded-sm"
                               placeholder="••••••••" 
                             />
                             <Key className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-300" />
                          </div>
                       </div>
                    </div>
                 </div>

                 <div className="flex flex-col gap-3 pt-4">
                    <button 
                      type="submit"
                      disabled={actionLoading}
                      className={cn(
                        "btn-wpf h-12 flex items-center justify-center gap-3 uppercase font-black text-[11px] border-b-4",
                        editingEmp.Id === 0 ? "btn-wpf-primary border-[#005A9E]" : "bg-indigo-600 text-white border-indigo-800 hover:bg-indigo-700"
                      )}
                    >
                       {actionLoading ? <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" /> : (editingEmp.Id === 0 ? <Plus className="w-5 h-5" /> : <Save className="w-5 h-5" />)}
                       {editingEmp.Id === 0 ? "BỔ NHIỆM NHÂN VIÊN" : "CẬP NHẬT QUYỀN HẠN"}
                    </button>
                    
                    <div className="grid grid-cols-2 gap-3">
                       {editingEmp.Id !== 0 && (
                         <button 
                           type="button"
                           onClick={handleDelete}
                           disabled={actionLoading}
                           className="btn-wpf h-10 text-rose-600 border-rose-200 hover:bg-rose-50 flex items-center justify-center gap-2 text-[10px] uppercase font-black"
                         >
                            <ShieldAlert className="w-3.5 h-3.5" /> KHÓA TÀI KHOẢN
                         </button>
                       )}
                       <button 
                         type="button"
                         onClick={handleClear}
                         className={cn(
                           "btn-wpf h-10 text-slate-500 border-slate-200 hover:bg-slate-50 flex items-center justify-center gap-2 text-[10px] uppercase font-black",
                           editingEmp.Id === 0 ? "col-span-2" : "col-span-1"
                         )}
                       >
                          <RotateCcw className="w-3.5 h-3.5" /> HỦY BỎ
                       </button>
                    </div>
                 </div>
              </form>
           </div>
        </div>

        {/* Right Column: User Grid */}
        <div className="lg:col-span-8 space-y-6">
           <div className="wpf-panel shadow-md">
              <div className="wpf-panel-header flex flex-col md:flex-row md:items-center justify-between gap-4 py-3 h-auto">
                 <span className="flex items-center gap-2">
                    <Monitor className="w-4 h-4" /> DANH SÁCH TÀI KHOẢN (USER REGISTRY)
                 </span>
                 
                 <div className="flex flex-wrap items-center gap-3">
                    <div className="relative group">
                       <Filter className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
                       <select 
                         value={roleFilter}
                         onChange={(e) => setRoleFilter(e.target.value)}
                         className="h-8 border border-[#D1D1D1] pl-8 pr-4 text-[9px] font-black uppercase tracking-widest text-[#0078D4] focus:border-[#0078D4] outline-none rounded-sm bg-white cursor-pointer"
                       >
                          <option value="All">TẤT CẢ VỊ TRÍ</option>
                          <option value="Staff">STAFF</option>
                          <option value="Manager">MANAGER</option>
                          <option value="Admin">ADMIN</option>
                       </select>
                    </div>

                    <div className="relative group">
                       <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
                       <input 
                         type="text" 
                         value={searchTerm}
                         onChange={(e) => setSearchTerm(e.target.value)}
                         className="h-8 border border-[#D1D1D1] pl-8 pr-4 text-[10px] font-bold text-slate-600 focus:border-[#0078D4] outline-none rounded-sm bg-[#F9F9F9] focus:bg-white w-[180px] uppercase"
                         placeholder="TÌM TÊN / USERNAME..." 
                       />
                    </div>
                 </div>
              </div>
              
              <div className="overflow-x-auto">
                <table className="wpf-datagrid">
                  <thead>
                    <tr>
                      <th className="w-[80px]">UID</th>
                      <th>BIÊN CHẾ / TÀI KHOẢN</th>
                      <th className="w-[120px] text-center">VAI TRÒ</th>
                      <th className="w-[120px] text-center">TRẠNG THÁI</th>
                      <th className="w-[150px] text-center">TRUY CẬP CUỐI</th>
                      <th className="text-right w-[60px]">EL</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredEmployees.map((e: Employee) => (
                      <tr 
                        key={e.Id} 
                        onClick={() => setEditingEmp({...e, Password: ""})}
                        className={cn("cursor-pointer", editingEmp.Id === e.Id ? "bg-[#E5F1FB]" : "")}
                      >
                        <td className="text-center font-bold text-slate-300 tabular-nums text-[10px]">{e.Id}</td>
                        <td>
                           <div className="flex items-center gap-3">
                              <div className="w-8 h-8 rounded-sm bg-slate-900 flex items-center justify-center text-indigo-400">
                                 <UserCircle className="w-5 h-5" />
                              </div>
                              <div className="flex flex-col">
                                 <span className={cn("text-[13px] font-black uppercase italic leading-none mb-0.5", editingEmp.Id === e.Id ? "text-[#0078D4]" : "text-slate-800")}>
                                    {e.EmployeeName}
                                 </span>
                                 <span className="text-[9px] font-bold text-slate-400">@{e.Username}</span>
                              </div>
                           </div>
                        </td>
                        <td className="text-center">
                           <span className={cn(
                               "px-2 py-0.5 rounded-sm text-[9px] font-black uppercase border",
                               e.Role === 'Admin' ? "bg-slate-900 text-amber-500 border-slate-700" : 
                               e.Role === 'Manager' ? "bg-indigo-50 text-indigo-600 border-indigo-200" : "bg-slate-50 text-slate-400 border-slate-200"
                           )}>
                              {e.Role}
                           </span>
                        </td>
                        <td className="text-center">
                           <div className="flex items-center justify-center gap-2">
                              <div className={cn(
                                 "w-2 h-2 rounded-full",
                                 e.IsActive ? "bg-emerald-500 animate-pulse" : "bg-slate-300"
                              )} />
                              <span className={cn(
                                 "text-[9px] font-black uppercase tracking-widest",
                                 e.IsActive ? "text-emerald-500" : "text-slate-400"
                              )}>
                                 {e.IsActive ? "ACTIVE" : "DISABLED"}
                              </span>
                           </div>
                        </td>
                        <td className="text-center text-[10px] font-bold text-slate-400 uppercase italic">
                           {e.LastLoginDate ? new Date(e.LastLoginDate).toLocaleDateString('vi-VN') : 'NEVER'}
                        </td>
                        <td className="text-right">
                           {editingEmp.Id === e.Id ? <div className="w-1.5 h-6 bg-[#0078D4] rounded-sm ml-auto" /> : <ChevronRight className="w-4 h-4 text-slate-100 ml-auto" />}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              
              <div className="p-4 bg-[#F0F0F0] border-t border-[#D1D1D1] flex items-center justify-between">
                 <div className="flex items-center gap-4">
                    <BadgeCheck className="w-4 h-4 text-emerald-500" />
                    <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest italic leading-none">Security Matrix Verified Shell v2.5.0</p>
                 </div>
                 <div className="flex items-center gap-2 text-indigo-600 bg-white px-4 py-2 border border-slate-200 rounded-sm italic text-[11px] font-black uppercase">
                    {employees.length} ACCOUNTS ACTIVE
                 </div>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
}
