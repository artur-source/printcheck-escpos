using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using Microsoft.Win32;
using PrintCheck.Core;

namespace PrintCheck.Discovery
{
    public class WindowsSpoolerProvider : IPrinterDiscoveryProvider
    {
        public string ProviderName => "Windows Spooler";

        public async Task<IEnumerable<DiscoveredPrinter>> DiscoverAsync()
        {
            return await Task.Run(() => {
                var list = new List<DiscoveredPrinter>();
                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    var info = GetInfoFromRegistry(printerName);
                    list.Add(new DiscoveredPrinter {
                        Name = printerName,
                        Address = info.Address,
                        Type = info.Type,
                        Port = info.Port,
                        Source = "Windows",
                        IsOnline = true
                    });
                }
                return list;
            });
        }

        private (string Address, ConnectionType Type, int Port) GetInfoFromRegistry(string printerName)
        {
            try {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Print\Printers\" + printerName))
                {
                    if (key != null) {
                        string port = key.GetValue("Port")?.ToString() ?? "";
                        if (port.Contains(".")) return (port.Split(':')[0], ConnectionType.Network, 9100);
                        if (port.StartsWith("COM")) return (port, ConnectionType.Serial, 0);
                        if (port.StartsWith("USB")) return (port, ConnectionType.USB, 0);
                    }
                }
            } catch { }
            return ("Local", ConnectionType.Local, 0);
        }
    }
}
