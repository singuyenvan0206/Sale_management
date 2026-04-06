"use client";

import { Download } from "lucide-react";

interface ExportButtonProps {
  data: any[];
}

export default function ExportButton({ data }: ExportButtonProps) {
  const handleExport = () => {
    if (data.length === 0) return;

    const headers = ["Invoice Number", "Date", "Customer", "Total", "Method", "Status"];
    const rows = data.map(inv => [
      inv.InvoiceNumber || `INV-${inv.Id}`,
      new Date(inv.CreatedDate).toLocaleString(),
      inv.CustomerName || "Khách vãng lai",
      inv.FinalAmount,
      inv.PaymentMethod || "Cash",
      inv.Status
    ]);

    const csvContent = [
      headers.join(","),
      ...rows.map(r => r.join(","))
    ].join("\n");

    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.setAttribute("href", url);
    link.setAttribute("download", `Fusion_Invoices_${new Date().toISOString().split('T')[0]}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <button 
      onClick={handleExport}
      className="bg-white border border-slate-200 rounded-2xl px-8 py-4 text-[10px] font-black uppercase tracking-widest text-slate-400 hover:text-slate-900 hover:border-slate-400 transition-all shadow-sm active:scale-95 flex items-center gap-3"
    >
      <Download className="w-4 h-4" /> EXPORT REPORT (CSV)
    </button>
  );
}
