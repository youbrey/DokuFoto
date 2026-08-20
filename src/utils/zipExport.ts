import JSZip from 'jszip';
import saveAs from 'file-saver';
import { CSHARP_SOLUTION_STRUCTURE } from '../csharp_scaffold/solutionData';

export async function downloadCSharpSolutionZip(): Promise<void> {
  const zip = new JSZip();

  // Root folder
  const root = zip.folder('SetwanDokuFoto') || zip;

  // Add all C# files from solution structure
  for (const file of CSHARP_SOLUTION_STRUCTURE) {
    // clean path
    const relativePath = file.path.startsWith('/') ? file.path.substring(1) : file.path;
    root.file(relativePath, file.content);
  }

  // Add README.md
  root.file(
    'README.md',
    `# Setwan DokuFoto - Solusi C# .NET 8 (WPF)

Aplikasi Desktop Windows untuk Pembuatan Dokumen Kolase Foto Resmi Sekretariat DPRD Kota Bitung.

## Persyaratan Sistem
- Windows 10 / 11 x64
- .NET 8.0 SDK (Desktop Runtime)
- Visual Studio 2022 v17.8+ (dengan workload ".NET Desktop Development")

## Cara Membuka & Menjalankan di Visual Studio
1. Ekstrak file ZIP ini ke folder kerja Anda (misal: \`D:\\Projects\\SetwanDokuFoto\`).
2. Buka file \`SetwanDokuFoto.sln\` dengan Visual Studio 2022.
3. Pastikan \`KolaseFotoApp.UI\` terpilih sebagai Startup Project (Klik kanan \`KolaseFotoApp.UI\` -> *Set as Startup Project*).
4. Tekan **F5** atau klik tombol **Start (Debug/Release)**.

## Menjalankan via .NET CLI
\`\`\`bash
# Restore & Build Seluruh Solusi (8 Proyek)
dotnet restore SetwanDokuFoto.sln
dotnet build SetwanDokuFoto.sln

# Menjalankan Seluruh Unit Test (Core, DocxEngine, PrintEngine)
dotnet test SetwanDokuFoto.sln

# Menjalankan Aplikasi Desktop WPF
dotnet run --project src/KolaseFotoApp.UI/KolaseFotoApp.UI.csproj
\`\`\`
`
  );

  // Generate ZIP blob and trigger download
  const content = await zip.generateAsync({ type: 'blob' });
  saveAs(content, 'SetwanDokuFoto_CSharp_NET8_Solution.zip');
}
