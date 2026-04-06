"use client";

import { useState } from "react";
import { Edit2, Trash2, AlertCircle, X, Trash } from "lucide-react";
import { useRouter } from "next/navigation";
import { cn } from "@/lib/utils";
import Link from "next/link";

export function ProductActions({ id, name }: { id: number, name: string }) {
  const router = useRouter();
  const [isDeleting, setIsDeleting] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  const handleDelete = async () => {
    setIsDeleting(true);
    try {
      const res = await fetch(`/api/products/${id}`, {
        method: "DELETE"
      });
      const json = await res.json();
      if (json.success) {
        setShowConfirm(false);
        router.refresh();
      } else {
        alert("❌ Lỗi: " + (json.error || "Thao tác thất bại"));
      }
    } catch (e) {
      alert("❌ Lỗi kết nối");
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <>
      <div className="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
        <Link href={`/products/${id}/edit`}>
          <button className="p-2.5 hover:bg-blue-50 rounded-xl text-blue-600 transition-all active:scale-90">
            <Edit2 className="w-4 h-4" />
          </button>
        </Link>
        <button 
          onClick={() => setShowConfirm(true)}
          className="p-2.5 hover:bg-rose-50 rounded-xl text-rose-500 transition-all active:scale-90"
        >
          <Trash2 className="w-4 h-4" />
        </button>
      </div>

      {showConfirm && (
        <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-[100] flex items-center justify-center p-4 animate-in fade-in duration-300">
           <div className="bg-white rounded-[32px] p-10 max-w-md w-full shadow-2xl animate-in zoom-in-95 duration-300 border border-slate-100">
              <div className="w-20 h-20 bg-rose-50 rounded-full flex items-center justify-center mx-auto mb-6 text-rose-500 border border-rose-100">
                 <AlertCircle className="w-10 h-10" />
              </div>
              <h2 className="text-2xl font-black text-slate-900 text-center uppercase tracking-tight">Xác nhận xóa?</h2>
              <p className="text-slate-500 text-center mt-4 text-[13px] font-medium leading-relaxed">
                 Bạn có chắc chắn muốn xóa sản phẩm <span className="font-black text-slate-900">{name}</span>? 
                 Hành động này không thể hoàn tác nếu sản phẩm chưa có lịch sử bán hàng.
              </p>
              
              <div className="grid grid-cols-2 gap-4 mt-10">
                 <button 
                   onClick={() => setShowConfirm(false)}
                   className="bg-slate-100 hover:bg-slate-200 text-slate-600 font-black py-4 rounded-2xl transition-all active:scale-95 uppercase text-xs tracking-widest"
                 >
                    <X className="w-4 h-4 inline-block mr-2" /> Hủy
                 </button>
                 <button 
                   onClick={handleDelete}
                   disabled={isDeleting}
                   className="bg-rose-600 hover:bg-rose-700 text-white font-black py-4 rounded-2xl transition-all active:scale-95 uppercase text-xs tracking-widest shadow-lg shadow-rose-600/30 flex items-center justify-center gap-2"
                 >
                    {isDeleting ? "..." : <Trash className="w-4 h-4" />} {isDeleting ? "Đang xóa" : "Xác nhận"}
                 </button>
              </div>
           </div>
        </div>
      )}
    </>
  );
}
