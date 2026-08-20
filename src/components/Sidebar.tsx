import React from 'react';
import {
  LayoutGrid,
  Image as ImageIcon,
  Type,
  Landmark,
  Sliders,
  Code2,
  ChevronRight,
  ChevronLeft,
} from 'lucide-react';

export type SidebarTab = 'templates' | 'media' | 'text' | 'margins' | 'csharp';

interface SidebarProps {
  activeTab: SidebarTab;
  onChangeTab: (tab: SidebarTab) => void;
  isOpen: boolean;
  onToggleOpen: () => void;
  children: React.ReactNode;
}

export const Sidebar: React.FC<SidebarProps> = ({
  activeTab,
  onChangeTab,
  isOpen,
  onToggleOpen,
  children,
}) => {
  const navItems = [
    {
      id: 'templates' as SidebarTab,
      label: 'Kisi Kolase',
      icon: LayoutGrid,
      desc: 'Pilih template layout',
    },
    {
      id: 'media' as SidebarTab,
      label: 'Media Foto',
      icon: ImageIcon,
      desc: 'Impor & kelola foto',
    },
    {
      id: 'text' as SidebarTab,
      label: 'Teks & Judul',
      icon: Type,
      desc: 'Judul, waktu & ttd',
    },
    {
      id: 'margins' as SidebarTab,
      label: 'Kertas & Margin',
      icon: Sliders,
      desc: 'Ukuran F4/A4 & batas',
    },
    {
      id: 'csharp' as SidebarTab,
      label: 'Solusi C# .NET',
      icon: Code2,
      desc: 'Arsitektur WPF Fase 1',
    },
  ];

  return (
    <aside className="flex z-20 flex-shrink-0 h-full select-none">
      {/* 1. Primary Left Icon Rail */}
      <div className="w-18 bg-slate-900 border-r border-slate-800 flex flex-col items-center py-3 gap-1 z-20">
        {navItems.map((item) => {
          const Icon = item.icon;
          const isActive = activeTab === item.id && isOpen;
          return (
            <button
              key={item.id}
              onClick={() => {
                if (activeTab === item.id && isOpen) {
                  // Toggle drawer if same tab clicked
                  // onToggleOpen();
                } else {
                  onChangeTab(item.id);
                  if (!isOpen) onToggleOpen();
                }
              }}
              className={`w-14 py-2.5 px-1 rounded-xl flex flex-col items-center gap-1 transition-all text-center relative ${
                isActive
                  ? 'bg-sky-600/20 text-sky-400 border border-sky-500/40 shadow-inner'
                  : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/60'
              }`}
              title={item.desc}
            >
              <Icon className="w-5 h-5" />
              <span className="text-[10px] font-bold leading-tight line-clamp-1">{item.label}</span>
              {isActive && (
                <div className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-6 bg-sky-400 rounded-r-full" />
              )}
            </button>
          );
        })}

        {/* Bottom Toggle Drawer Button */}
        <div className="mt-auto pt-2 border-t border-slate-800 w-full flex justify-center">
          <button
            onClick={onToggleOpen}
            className="p-2 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition"
            title={isOpen ? 'Tutup Panel Samping' : 'Buka Panel Samping'}
          >
            {isOpen ? <ChevronLeft className="w-5 h-5" /> : <ChevronRight className="w-5 h-5" />}
          </button>
        </div>
      </div>

      {/* 2. Secondary Expanded Drawer Panel */}
      {isOpen && (
        <div className="w-80 md:w-96 bg-slate-900 border-r border-slate-800 text-slate-100 flex flex-col h-full shadow-2xl overflow-hidden z-10 animate-in slide-in-from-left-2 duration-150">
          <div className="flex-1 overflow-y-auto">{children}</div>
        </div>
      )}
    </aside>
  );
};
