using System;
using System.Collections.Generic;

namespace PrintCheck.Core
{
    public enum ConnectionType { Network, USB, Serial, Parallel, Local, Unknown }

    public class DiscoveredPrinter
    {
        public string Name { get; set; }
        public ConnectionType Type { get; set; }
        public string Address { get; set; } // IP ou PortName
        public int Port { get; set; }       // Porta TCP (9100)
        public string Source { get; set; }  // "Windows Spooler", "TCP Probe", "SDK Epson", etc.
        public bool IsOnline { get; set; }

        public override string ToString() => $"[{Source}] {Name} ({Address})";
    }

    public interface IPrinterDiscoveryProvider
    {
        string ProviderName { get; }
        Task<IEnumerable<DiscoveredPrinter>> DiscoverAsync();
    }
}
