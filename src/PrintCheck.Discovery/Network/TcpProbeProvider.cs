using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using PrintCheck.Discovery;
using PrintCheck.Core;

namespace PrintCheck.Discovery.Network
{
    public class TcpProbeProvider : IPrinterDiscoveryProvider
    {
        public string ProviderName => "TCP Probe (Multi-Port)";

        public async Task<IEnumerable<DiscoveredPrinter>> DiscoverAsync()
        {
            // Ponto 4: Heartbeat inicial
            DiscoveryLogger.Log(">>> HEARTBEAT: TcpProbeProvider.DiscoverAsync() INICIADO");
            
            var discovered = new List<DiscoveredPrinter>();
            
            try
            {
                var interfaces = GetActiveInterfaces();
                if (!interfaces.Any())
                {
                    DiscoveryLogger.Log("Nenhuma interface de rede ativa encontrada. Encerrando descoberta.");
                    return discovered;
                }

                foreach (var (ni, ipInfo) in interfaces)
                {
                    try
                    {
                        DiscoveryLogger.Log($"\nProcessando interface: {ni.Name} (IP: {ipInfo.Address}, Máscara: {ipInfo.IPv4Mask})");
                        
                        var (startIp, endIp, count) = CalculateIpRange(ipInfo.Address, ipInfo.IPv4Mask);
                        DiscoveryLogger.Log($"Faixa calculada: {startIp} até {endIp} (Total: {count} hosts)");

                        // Ponto 3: Proteção contra sub-redes gigantes
                        if (count > 1024)
                        {
                            DiscoveryLogger.Log($"AVISO: Sub-rede muito grande ({count} hosts). Limitando varredura para /24 em torno do IP local para segurança.");
                            (startIp, endIp, count) = GetFallbackRange(ipInfo.Address);
                            DiscoveryLogger.Log($"Nova faixa limitada: {startIp} até {endIp} (Total: {count} hosts)");
                        }

                        var ipsToScan = GenerateIpList(startIp, endIp, ipInfo.Address);
                        DiscoveryLogger.Log($"Varrendo {ipsToScan.Count} IPs na sub-rede de {ni.Name}...");

                        if (ipsToScan.Contains("10.0.0.21"))
                            DiscoveryLogger.Log("CHECK: 10.0.0.21 ESTÁ na lista de varredura desta interface.");
                        else
                            DiscoveryLogger.Log("CHECK: 10.0.0.21 NÃO ESTÁ na lista de varredura desta interface.");

                        var results = await ScanIpsAsync(ipsToScan);
                        discovered.AddRange(results);
                    }
                    catch (Exception ex)
                    {
                        DiscoveryLogger.LogError($"Loop de Interface ({ni.Name})", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                // Ponto 2: Try/Catch Global
                DiscoveryLogger.LogError("DiscoverAsync Global", ex);
            }

            DiscoveryLogger.Log($">>> FIM DA DESCOBERTA: {discovered.Count} impressoras encontradas.");
            return discovered.GroupBy(p => p.Address).Select(g => g.First());
        }

        private List<(NetworkInterface ni, UnicastIPAddressInformation ipInfo)> GetActiveInterfaces()
        {
            var result = new List<(NetworkInterface, UnicastIPAddressInformation)>();
            DiscoveryLogger.Log("\n--- Detecção de Interfaces de Rede ---");
            
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    try
                    {
                        bool isVirtual = ni.Description.IndexOf("Virtual", StringComparison.OrdinalIgnoreCase) >= 0 || 
                                         ni.Name.IndexOf("vEthernet", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         ni.Description.IndexOf("VMware", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         ni.Description.IndexOf("VirtualBox", StringComparison.OrdinalIgnoreCase) >= 0;
                        
                        DiscoveryLogger.Log($"Interface: {ni.Name} | Desc: {ni.Description} | Tipo: {ni.NetworkInterfaceType} | Status: {ni.OperationalStatus} | Virtual: {isVirtual}");

                        if (ni.OperationalStatus == OperationalStatus.Up && 
                            ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && 
                            !isVirtual)
                        {
                            foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == AddressFamily.InterNetwork && ip.IPv4Mask != null)
                                {
                                    DiscoveryLogger.Log($"  -> IP Válido encontrado: {ip.Address} / {ip.IPv4Mask}");
                                    result.Add((ni, ip));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DiscoveryLogger.Log($"Erro ao ler interface individual: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                DiscoveryLogger.LogError("GetActiveInterfaces", ex);
            }
            return result;
        }

        private (IPAddress start, IPAddress end, long count) CalculateIpRange(IPAddress ip, IPAddress mask)
        {
            byte[] ipBytes = ip.GetAddressBytes();
            byte[] maskBytes = mask.GetAddressBytes();
            byte[] networkBytes = new byte[4];
            byte[] broadcastBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                broadcastBytes[i] = (byte)(networkBytes[i] | ~maskBytes[i]);
            }

            // Incrementar o IP da rede para começar no .1
            networkBytes[3]++;
            // Decrementar o IP de broadcast para terminar no .254
            broadcastBytes[3]--;

            long count = 1;
            for (int i = 0; i < 4; i++)
            {
                int diff = broadcastBytes[i] - networkBytes[i] + 1;
                if (diff > 0) count *= diff;
            }

            return (new IPAddress(networkBytes), new IPAddress(broadcastBytes), Math.Abs(count));
        }

        private (IPAddress start, IPAddress end, long count) GetFallbackRange(IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();
            byte[] start = new byte[] { bytes[0], bytes[1], bytes[2], 1 };
            byte[] end = new byte[] { bytes[0], bytes[1], bytes[2], 254 };
            return (new IPAddress(start), new IPAddress(end), 254);
        }

        private List<string> GenerateIpList(IPAddress start, IPAddress end, IPAddress local)
        {
            var list = new List<string>();
            try
            {
                byte[] startBytes = start.GetAddressBytes();
                byte[] endBytes = end.GetAddressBytes();
                string localStr = local.ToString();

                for (int a = startBytes[0]; a <= endBytes[0]; a++)
                {
                    for (int b = startBytes[1]; b <= endBytes[1]; b++)
                    {
                        for (int c = startBytes[2]; c <= endBytes[2]; c++)
                        {
                            for (int d = startBytes[3]; d <= endBytes[3]; d++)
                            {
                                string ip = $"{a}.{b}.{c}.{d}";
                                if (ip != localStr) list.Add(ip);
                                if (d >= 255) break;
                            }
                            if (c >= 255) break;
                        }
                        if (b >= 255) break;
                    }
                    if (a >= 255) break;
                }
            }
            catch (Exception ex)
            {
                DiscoveryLogger.LogError("GenerateIpList", ex);
            }
            return list;
        }

        private async Task<List<DiscoveredPrinter>> ScanIpsAsync(List<string> ips)
        {
            var results = new List<DiscoveredPrinter>();
            try
            {
                using (var semaphore = new SemaphoreSlim(75))
                {
                    var tasks = ips.Select(async ip =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            var printers = await ProbeIpWithMultiplePortsAsync(ip);
                            lock (results)
                            {
                                results.AddRange(printers);
                            }
                        }
                        catch (Exception ex)
                        {
                            DiscoveryLogger.Log($"Erro ao varrer IP {ip}: {ex.Message}");
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                DiscoveryLogger.LogError("ScanIpsAsync", ex);
            }
            return results;
        }

        private async Task<List<DiscoveredPrinter>> ProbeIpWithMultiplePortsAsync(string ip)
        {
            int[] ports = { 9100, 9101, 23, 515 };
            var found = new List<DiscoveredPrinter>();

            foreach (var port in ports)
            {
                try
                {
                    var printer = await ProbeIpAsync(ip, port);
                    if (printer != null)
                    {
                        found.Add(printer);
                        break; 
                    }
                }
                catch { }
            }
            return found;
        }

        private async Task<DiscoveredPrinter> ProbeIpAsync(string ip, int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    using (var cts = new CancellationTokenSource(500)) // Aumentado levemente para 500ms
                    {
                        try
                        {
                            await client.ConnectAsync(ip, port, cts.Token);
                            DiscoveryLogger.Log($"[SUCESSO] Dispositivo encontrado em {ip}:{port}");
                            return new DiscoveredPrinter
                            {
                                Name = $"Network Printer ({GetPortName(port)})",
                                Address = ip,
                                Port = port,
                                Type = ConnectionType.Network,
                                Source = $"TCP Probe ({port})",
                                IsOnline = true
                            };
                        }
                        catch (OperationCanceledException) { return null; }
                        catch (SocketException) { return null; }
                    }
                }
            }
            catch { }
            return null;
        }

        private string GetPortName(int port)
        {
            switch (port)
            {
                case 9100: return "RAW/JetDirect";
                case 9101: return "Bematech/Secondary";
                case 23: return "Telnet/Serial";
                case 515: return "LPD";
                default: return "TCP";
            }
        }
    }
}
