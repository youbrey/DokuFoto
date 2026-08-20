import React from 'react';
import {
  FileText,
  Printer,
  Eye,
  Download,
  Settings,
  Code2,
  Plus,
  Save,
  RotateCcw,
  Sparkles,
  Layers,
} from 'lucide-react';
import { DocumentProject, PaperSizeType } from '../types';
import { PAPER_DIMENSIONS } from '../utils/constants';

interface NavbarProps {
  project: DocumentProject;
  onUpdateProject: (updated: Partial<DocumentProject>) => void;
  onOpenDocxExport: () => void;
  onOpenPrintPreview: () => void;
  onOpenPaperModal: () => void;
  onToggleCSharpView: () => void;
  isCSharpView: boolean;
  onNewProject: () => void;
  isExportingDocx: boolean;
  onOpenAutoCollage?: () => void;
}

export const Navbar: React.FC<NavbarProps> = ({
  project,
  onUpdateProject,
  onOpenDocxExport,
  onOpenPrintPreview,
  onOpenPaperModal,
  onToggleCSharpView,
  isCSharpView,
  onNewProject,
  isExportingDocx,
  onOpenAutoCollage,
}) => {
  return (
    <header className="bg-slate-900 border-b border-slate-800 text-white select-none z-30 sticky top-0">
      <div className="max-w-7xl mx-auto px-4 py-2.5 flex items-center justify-between gap-4">
        {/* Left Branding */}
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-sky-600 via-blue-600 to-indigo-700 flex items-center justify-center shadow-lg shadow-sky-950/50 border border-sky-400/20">
            <FileText className="w-5 h-5 text-white" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <span className="font-extrabold text-base tracking-tight text-white flex items-center gap-1.5">
                Setwan <span className="text-sky-400">DokuFoto</span>
              </span>
              <span className="text-[10px] font-semibold tracking-wide uppercase px-2 py-0.5 rounded-full bg-sky-950/80 text-sky-300 border border-sky-800/80">
                DPRD Kota Bitung
              </span>
            </div>
            <p className="text-xs text-slate-400 font-medium">
              Sistem Pembuat Dokumen Kolase Foto Kegiatan Resmi (.docx)
            </p>
          </div>
        </div>

        {/* Center: Title & Quick Paper Specs */}
        <div className="hidden lg:flex items-center gap-2 bg-slate-950/70 border border-slate-800 rounded-xl px-3 py-1.5 shadow-inner max-w-md w-full">
          <input
            type="text"
            value={project.title}
            onChange={(e) => onUpdateProject({ title: e.target.value })}
            placeholder="Judul Dokumen Dokumentasi..."
            className="bg-transparent text-xs font-semibold text-slate-200 focus:outline-none focus:text-white w-full truncate"
            title="Klik untuk mengubah nama dokumen"
          />
          <button
            onClick={onOpenPaperModal}
            className="flex-shrink-0 text-[11px] font-medium text-slate-400 hover:text-sky-300 bg-slate-800/80 hover:bg-slate-800 px-2 py-0.5 rounded-md transition-colors flex items-center gap-1 border border-slate-700/50"
            title="Ubah Ukuran Kertas & Margin"
          >
            <span>{project.paperSize}</span>
            <span className="text-slate-500">|</span>
            <span className="capitalize">{project.orientation}</span>
          </button>
        </div>

        {/* Right Action Buttons */}
        <div className="flex items-center gap-2">
          {/* Auto Kolase Feature */}
          {onOpenAutoCollage && (
            <button
              onClick={onOpenAutoCollage}
              className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-extrabold text-white bg-gradient-to-r from-sky-600 via-indigo-600 to-purple-600 hover:from-sky-500 hover:to-purple-500 shadow-md shadow-sky-900/40 border border-sky-400/40 transition active:scale-95"
              title="Studio Auto Kisi Kolase Foto Otomatis"
            >
              <Sparkles className="w-3.5 h-3.5 text-amber-300 animate-pulse" />
              <span>Auto Kolase</span>
            </button>
          )}

          {/* Paper Settings */}
          <button
            onClick={onOpenPaperModal}
            className="hidden sm:inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold text-slate-300 hover:text-white bg-slate-800/80 hover:bg-slate-800 border border-slate-700 transition"
            title="Atur Kertas & Margin"
          >
            <Settings className="w-3.5 h-3.5 text-slate-400" />
            <span>Kertas & Margin</span>
          </button>

          {/* Pratinjau Cetak */}
          <button
            onClick={onOpenPrintPreview}
            className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold text-slate-200 hover:text-white bg-slate-800 hover:bg-slate-700 border border-slate-600/70 transition shadow-sm"
            title="Pratinjau sebelum mencetak"
          >
            <Eye className="w-3.5 h-3.5 text-emerald-400" />
            <span>Pratinjau Cetak</span>
          </button>

          {/* Export to .DOCX */}
          <button
            onClick={onOpenDocxExport}
            disabled={isExportingDocx}
            className="inline-flex items-center gap-1.5 px-3.5 py-1.5 rounded-lg text-xs font-bold text-white bg-gradient-to-r from-sky-600 to-blue-600 hover:from-sky-500 hover:to-blue-500 shadow-md shadow-sky-900/40 border border-sky-400/30 transition disabled:opacity-50"
            title="Ekspor langsung ke format Microsoft Word .docx asli"
          >
            <Download className="w-3.5 h-3.5 text-white" />
            <span>{isExportingDocx ? 'Membuat .docx...' : 'Ekspor .DOCX'}</span>
          </button>

          {/* C# Solution Explorer Toggle */}
          <button
            onClick={onToggleCSharpView}
            className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold border transition ${
              isCSharpView
                ? 'bg-purple-950 text-purple-200 border-purple-600 shadow-inner'
                : 'bg-slate-800/80 text-purple-300 border-purple-800/50 hover:bg-purple-950/40'
            }`}
            title="Lihat Arsitektur C# .NET 8 WPF (Fase 1)"
          >
            <Code2 className="w-3.5 h-3.5 text-purple-400" />
            <span className="hidden md:inline">C# .NET WPF</span>
          </button>
        </div>
      </div>
    </header>
  );
};
