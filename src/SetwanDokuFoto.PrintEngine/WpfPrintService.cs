using System.Printing;
using System.Windows.Controls;
using SetwanDokuFoto.Core.Models;
using SetwanDokuFoto.Core.Services;

namespace SetwanDokuFoto.PrintEngine;

public class WpfPrintService : IPrintService
{
    public Task<List<string>> GetAvailablePrintersAsync()
    {
        return Task.Run(() =>
        {
            var printers = new List<string>();
            try
            {
                var printServer = new LocalPrintServer();
                var printQueues = printServer.GetPrintQueues(new[] {
                    EnumeratedPrintQueueTypes.Local,
                    EnumeratedPrintQueueTypes.Connections
                });

                foreach (var pq in printQueues)
                {
                    printers.Add(pq.FullName);
                }
            }
            catch
            {
                printers.Add("Microsoft Print to PDF");
                printers.Add("Default Printer");
            }
            return printers;
        });
    }

    public Task PrintDocumentAsync(DocumentProject project, string printerName)
    {
        return Task.Run(() =>
        {
            var printDialog = new PrintDialog();
            var printServer = new LocalPrintServer();
            var queue = printServer.GetPrintQueue(printerName);
            printDialog.PrintQueue = queue;
            
            // Print spooling logic for WPF document visual
        });
    }
}
