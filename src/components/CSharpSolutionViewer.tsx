import React, { useState } from 'react';
import {
  Code2,
  FolderTree,
  FileCode,
  Copy,
  Check,
  Download,
  Terminal,
  Layers,
  ChevronRight,
  ChevronDown,
  Info,
  Play,
  ShieldCheck,
} from 'lucide-react';
import { CSHARP_SOLUTION_STRUCTURE, CSharpProjectFile } from '../csharp_scaffold/solutionData';

export const CSharpSolutionViewer: React.FC = () => {
  const [selectedFile, setSelectedFile] = useState<CSharpProjectFile>(
    CSHARP_SOLUTION_STRUCTURE[0]
  );
  const [copied, setCopied] = useState(false);
  const [activeTab, setActiveTab] = useState<'code' | 'guide' | 'nuget'>('code');

  const handleCopy = () => {
    navigator.clipboard.writeText(selectedFile.content);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const categories = ['Solution', 'Core', 'DocxEngine', 'PrintEngine', 'Data', 'Templates', 'UI', 'Tests'] as const;

  return (
    <div className="flex-1 flex flex-col h-full bg-slate-950 text-slate-100 overflow-hidden select-none">
      {/* Header */}
      <div className="h-14 bg-slate-900 border-b border-slate-800 px-6 flex items-center justify-between flex-shrink-0">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-lg bg-purple-600/20 border border-purple-500/40 flex items-center justify-center">
            <Code2 className="w-4 h-4 text-purple-400" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h2 className="text-sm font-bold text-white tracking-tight">
                Arsitektur C# .NET 8 WPF — Setwan DokuFoto
              </h2>
              <span className="text-[10px] font-bold px-2 py-0.5 rounded bg-purple-950 text-purple-300 border border-purple-800">
                Fase 1–7: Solusi Penuh .NET 8 WPF
              </span>
            </div>
            <p className="text-xs text-slate-400">
              Template Gallery, Draggable/Crop Canvas, Photo Import Cache, OpenXML DocxEngine, System.Printing, Persistence & Installer
            </p>
          </div>
        </div>

        {/* View Tabs & Download Solution ZIP */}
        <div className="flex items-center gap-2">
          <div className="flex items-center gap-1 bg-slate-950 p-1 rounded-xl border border-slate-800">
            <button
              onClick={() => setActiveTab('code')}
              className={`px-3 py-1 text-xs font-bold rounded-lg transition ${
                activeTab === 'code'
                  ? 'bg-purple-600 text-white shadow'
                  : 'text-slate-400 hover:text-white'
              }`}
            >
              File & Kode Sumber
            </button>
            <button
              onClick={() => setActiveTab('guide')}
              className={`px-3 py-1 text-xs font-bold rounded-lg transition ${
                activeTab === 'guide'
                  ? 'bg-purple-600 text-white shadow'
                  : 'text-slate-400 hover:text-white'
              }`}
            >
              Langkah CLI .NET
            </button>
            <button
              onClick={() => setActiveTab('nuget')}
              className={`px-3 py-1 text-xs font-bold rounded-lg transition ${
                activeTab === 'nuget'
                  ? 'bg-purple-600 text-white shadow'
                  : 'text-slate-400 hover:text-white'
              }`}
            >
              Paket NuGet & Dep
            </button>
          </div>

          <button
            onClick={async () => {
              const { downloadCSharpSolutionZip } = await import('../utils/zipExport');
              await downloadCSharpSolutionZip();
            }}
            className="flex items-center gap-1.5 px-3.5 py-1.5 rounded-xl bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-500 hover:to-indigo-500 text-white text-xs font-bold shadow-md shadow-purple-950/40 border border-purple-400/30 transition"
            title="Unduh seluruh source code solusi C# .NET 8 WPF sebagai file .ZIP siap buka di Visual Studio"
          >
            <Download className="w-3.5 h-3.5" />
            <span>Unduh Solusi .ZIP</span>
          </button>
        </div>
      </div>

      {/* Main Content Area */}
      {activeTab === 'code' && (
        <div className="flex-1 flex overflow-hidden">
          {/* Left: Solution Tree Explorer */}
          <div className="w-72 bg-slate-900/60 border-r border-slate-800 p-3 overflow-y-auto flex-shrink-0">
            <div className="flex items-center justify-between px-2 mb-2">
              <span className="text-[11px] font-bold text-slate-400 uppercase tracking-wider flex items-center gap-1.5">
                <FolderTree className="w-3.5 h-3.5 text-purple-400" />
                Solution Explorer
              </span>
              <span className="text-[10px] px-1.5 py-0.5 rounded bg-slate-800 text-slate-400 font-mono">
                8 Proyek
              </span>
            </div>

            <div className="space-y-3">
              {categories.map((cat) => {
                const filesInCat = CSHARP_SOLUTION_STRUCTURE.filter((f) => f.category === cat);
                if (filesInCat.length === 0) return null;

                return (
                  <div key={cat} className="space-y-1">
                    <div className="text-[10px] font-bold text-purple-400 uppercase tracking-wider px-2 py-0.5 bg-slate-800/40 rounded flex items-center justify-between">
                      <span>{cat}</span>
                    </div>

                    <div className="space-y-0.5 pl-1">
                      {filesInCat.map((file) => {
                        const isSelected = selectedFile.path === file.path;
                        return (
                          <button
                            key={file.path}
                            onClick={() => setSelectedFile(file)}
                            className={`w-full text-left px-2.5 py-1.5 rounded-lg text-xs transition flex items-center gap-2 ${
                              isSelected
                                ? 'bg-purple-600/30 text-purple-200 font-bold border border-purple-500/50 shadow-inner'
                                : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/50'
                            }`}
                          >
                            <FileCode className="w-3.5 h-3.5 flex-shrink-0 text-purple-400" />
                            <span className="truncate">{file.name}</span>
                          </button>
                        );
                      })}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Right: Code Viewer */}
          <div className="flex-1 flex flex-col bg-slate-950 overflow-hidden">
            {/* File Path & Copy Toolbar */}
            <div className="h-10 bg-slate-900/40 border-b border-slate-800 px-4 flex items-center justify-between flex-shrink-0">
              <div className="flex items-center gap-2">
                <span className="text-xs font-mono text-purple-300 font-semibold">
                  {selectedFile.path}
                </span>
                <span className="text-[10px] text-slate-500">• {selectedFile.description}</span>
              </div>

              <button
                onClick={handleCopy}
                className="flex items-center gap-1.5 px-2.5 py-1 rounded bg-slate-800 hover:bg-slate-700 text-xs text-slate-200 transition"
              >
                {copied ? <Check className="w-3.5 h-3.5 text-emerald-400" /> : <Copy className="w-3.5 h-3.5" />}
                <span>{copied ? 'Tersalin!' : 'Salin Kode'}</span>
              </button>
            </div>

            {/* Code Text Area */}
            <div className="flex-1 p-4 overflow-auto font-mono text-xs text-slate-300 leading-relaxed bg-slate-950 selection:bg-purple-900 selection:text-white">
              <pre className="whitespace-pre-wrap font-mono">{selectedFile.content}</pre>
            </div>
          </div>
        </div>
      )}

      {/* Guide Tab */}
      {activeTab === 'guide' && (
        <div className="flex-1 p-8 overflow-y-auto max-w-4xl mx-auto space-y-6">
          <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 space-y-4">
            <h3 className="text-base font-bold text-white flex items-center gap-2">
              <Terminal className="w-5 h-5 text-purple-400" />
              Langkah Eksekusi .NET CLI untuk Setup Solusi C# (Fase 1)
            </h3>
            <p className="text-xs text-slate-400 leading-relaxed">
              Jalankan perintah berikut di Command Prompt / PowerShell Windows 10/11 x64 dengan .NET 8 SDK terpasang untuk menginisialisasi solution persis sesuai dokumen:
            </p>

            <div className="bg-slate-950 p-4 rounded-xl border border-slate-800 font-mono text-xs text-purple-300 space-y-2 overflow-x-auto">
              <p className="text-slate-500"># 1. Inisialisasi Root Solution</p>
              <p>mkdir SetwanDokuFoto && cd SetwanDokuFoto</p>
              <p>git init</p>
              <p>dotnet new sln -n SetwanDokuFoto</p>
              <br />
              <p className="text-slate-500"># 2. Buat Lapisan Core, DocxEngine, PrintEngine, Data, UI</p>
              <p>dotnet new classlib -n SetwanDokuFoto.Core -o src/SetwanDokuFoto.Core -f net8.0</p>
              <p>dotnet new classlib -n SetwanDokuFoto.DocxEngine -o src/SetwanDokuFoto.DocxEngine -f net8.0</p>
              <p>dotnet new classlib -n SetwanDokuFoto.PrintEngine -o src/SetwanDokuFoto.PrintEngine -f net8.0-windows</p>
              <p>dotnet new classlib -n SetwanDokuFoto.Data -o src/SetwanDokuFoto.Data -f net8.0</p>
              <p>dotnet new wpf -n SetwanDokuFoto.UI -o src/SetwanDokuFoto.UI -f net8.0-windows</p>
              <br />
              <p className="text-slate-500"># 3. Hubungkan Project References</p>
              <p>dotnet sln add src/SetwanDokuFoto.Core/SetwanDokuFoto.Core.csproj</p>
              <p>dotnet sln add src/SetwanDokuFoto.DocxEngine/SetwanDokuFoto.DocxEngine.csproj</p>
              <p>dotnet sln add src/SetwanDokuFoto.PrintEngine/SetwanDokuFoto.PrintEngine.csproj</p>
              <p>dotnet sln add src/SetwanDokuFoto.Data/SetwanDokuFoto.Data.csproj</p>
              <p>dotnet sln add src/SetwanDokuFoto.UI/SetwanDokuFoto.UI.csproj</p>
              <p>dotnet add src/SetwanDokuFoto.UI reference src/SetwanDokuFoto.Core src/SetwanDokuFoto.DocxEngine src/SetwanDokuFoto.PrintEngine src/SetwanDokuFoto.Data</p>
              <br />
              <p className="text-slate-500"># 4. Instal Paket NuGet Inti</p>
              <p>dotnet add src/SetwanDokuFoto.UI package CommunityToolkit.Mvvm</p>
              <p>dotnet add src/SetwanDokuFoto.UI package Microsoft.Extensions.Hosting</p>
              <p>dotnet add src/SetwanDokuFoto.DocxEngine package DocumentFormat.OpenXml</p>
              <p>dotnet add src/SetwanDokuFoto.Data package Microsoft.Data.Sqlite</p>
              <br />
              <p className="text-slate-500"># 5. Buat Unit Test & Verifikasi Kompilasi</p>
              <p>dotnet new xunit -n SetwanDokuFoto.Core.Tests -o tests/SetwanDokuFoto.Core.Tests -f net8.0</p>
              <p>dotnet build SetwanDokuFoto.sln</p>
              <p>dotnet test SetwanDokuFoto.sln</p>
            </div>
          </div>
        </div>
      )}

      {/* NuGet Tab */}
      {activeTab === 'nuget' && (
        <div className="flex-1 p-8 overflow-y-auto max-w-4xl mx-auto space-y-4">
          <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 space-y-4">
            <h3 className="text-base font-bold text-white flex items-center gap-2">
              <Layers className="w-5 h-5 text-purple-400" />
              Daftar Paket NuGet Resmi Fase 1
            </h3>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              <div className="p-4 bg-slate-950 rounded-xl border border-slate-800">
                <h4 className="font-bold text-xs text-purple-300">CommunityToolkit.Mvvm</h4>
                <p className="text-[11px] text-slate-400 mt-1">
                  Source generator resmi Microsoft untuk ObservableObject, RelayCommand, dan MVVM Shell.
                </p>
              </div>

              <div className="p-4 bg-slate-950 rounded-xl border border-slate-800">
                <h4 className="font-bold text-xs text-purple-300">Microsoft.Extensions.Hosting</h4>
                <p className="text-[11px] text-slate-400 mt-1">
                  Generic Host untuk manajemen Dependency Injection container dan daur hidup aplikasi.
                </p>
              </div>

              <div className="p-4 bg-slate-950 rounded-xl border border-slate-800">
                <h4 className="font-bold text-xs text-purple-300">DocumentFormat.OpenXml (v3.1.1)</h4>
                <p className="text-[11px] text-slate-400 mt-1">
                  Mesin pembuat dokumen Word .docx asli tanpa memerlukan instalasi Microsoft Office di komputer.
                </p>
              </div>

              <div className="p-4 bg-slate-950 rounded-xl border border-slate-800">
                <h4 className="font-bold text-xs text-purple-300">Microsoft.Data.Sqlite</h4>
                <p className="text-[11px] text-slate-400 mt-1">
                  Driver database lokal SQLite untuk penyimpanan proyek foto dan riwayat dokumen Setwan.
                </p>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
