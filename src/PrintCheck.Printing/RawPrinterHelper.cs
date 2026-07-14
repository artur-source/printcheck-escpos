using System;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using PrintCheck.Discovery;

namespace PrintCheck.Printing
{
    public class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        public static bool SendBytesToPrinter(string szPrinterName, byte[] bytes)
        {
            DiscoveryLogger.Log($"[SPOOLER SEND] Iniciando envio para '{szPrinterName}' ({bytes.Length} bytes)");
            IntPtr pBytes = Marshal.AllocCoTaskMem(bytes.Length);
            Marshal.Copy(bytes, 0, pBytes, bytes.Length);
            bool success = false;
            try
            {
                if (OpenPrinter(szPrinterName, out IntPtr hPrinter, IntPtr.Zero))
                {
                    DOCINFOA di = new DOCINFOA { pDocName = "PrintCheck Test", pDataType = "RAW" };
                    if (StartDocPrinter(hPrinter, 1, di))
                    {
                        success = WritePrinter(hPrinter, pBytes, bytes.Length, out _);
                        EndDocPrinter(hPrinter);
                    }
                    else
                    {
                        DiscoveryLogger.Log($"[WIN32 ERROR] StartDocPrinter falhou. Código: {Marshal.GetLastWin32Error()}");
                    }
                    ClosePrinter(hPrinter);
                }
                else
                {
                    DiscoveryLogger.Log($"[WIN32 ERROR] OpenPrinter falhou para '{szPrinterName}'. Código: {Marshal.GetLastWin32Error()}");
                }
            }
            catch (Exception ex)
            {
                DiscoveryLogger.LogError("SendBytesToPrinter (Spooler)", ex);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pBytes);
            }
            return success;
        }

        public static bool SendBytesToTcpPrinter(string ip, int port, byte[] bytes)
        {
            DiscoveryLogger.Log($"[TCP SEND START] Destino: {ip}:{port} | Tamanho: {bytes.Length} bytes");
            
            if (string.IsNullOrEmpty(ip))
            {
                DiscoveryLogger.Log("[TCP SEND ERROR] IP da impressora está nulo ou vazio.");
                return false;
            }

            try
            {
                using (var client = new TcpClient())
                {
                    DiscoveryLogger.Log($"[TCP CONNECTING] Tentando conectar a {ip}:{port}...");
                    
                    // Usando ConnectAsync com Task.Wait para capturar exceções de forma síncrona no contexto do worker thread
                    var connectTask = client.ConnectAsync(ip, port);
                    if (connectTask.Wait(4000)) // Aumentado para 4 segundos
                    {
                        if (client.Connected)
                        {
                            DiscoveryLogger.Log($"[TCP CONNECTED] Conexão estabelecida com {ip}:{port}. Obtendo stream...");
                            using (var stream = client.GetStream())
                            {
                                DiscoveryLogger.Log($"[TCP WRITING] Escrevendo {bytes.Length} bytes no stream...");
                                stream.Write(bytes, 0, bytes.Length);
                                DiscoveryLogger.Log("[TCP FLUSHING] Executando Flush...");
                                stream.Flush();
                                DiscoveryLogger.Log("[TCP SEND SUCCESS] Envio finalizado com sucesso.");
                                return true;
                            }
                        }
                        else
                        {
                            DiscoveryLogger.Log("[TCP SEND ERROR] Conectado é falso após Wait bem sucedido.");
                            return false;
                        }
                    }
                    else
                    {
                        DiscoveryLogger.Log($"[TCP SEND ERROR] Timeout de 4s ao conectar em {ip}:{port}");
                        return false;
                    }
                }
            }
            catch (AggregateException ae)
            {
                foreach (var inner in ae.InnerExceptions)
                {
                    DiscoveryLogger.LogError("SendBytesToTcpPrinter (Aggregate)", inner);
                }
                return false;
            }
            catch (Exception ex)
            {
                DiscoveryLogger.LogError("SendBytesToTcpPrinter (General)", ex);
                return false;
            }
        }
    }
}
