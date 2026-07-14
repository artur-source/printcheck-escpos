using System;
using System.IO;
using System.Windows.Forms;

namespace PrintCheck.Discovery
{
    public static class DiscoveryLogger
    {
        public static readonly string LogFilePath;

        static DiscoveryLogger()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                LogFilePath = Path.Combine(baseDir, "discovery_log.txt");
                
                File.WriteAllText(LogFilePath, $"--- LOG INICIALIZADO EM {DateTime.Now} ---\nCaminho: {LogFilePath}\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERRO AO INICIALIZAR LOG: {ex.Message}", "PrintCheck Debug");
                LogFilePath = "discovery_log.txt"; // Fallback
            }
        }

        public static void Log(string message)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}";
            try
            {
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Sem MessageBox aqui para evitar loop se o log falhar no clique
                Console.WriteLine(logEntry);
            }
        }

        public static void LogError(string context, Exception ex)
        {
            Log($"[ERRO EXCEÇÃO] {context}: {ex.Message}");
            if (ex.StackTrace != null) Log($"StackTrace: {ex.StackTrace}");
        }
    }
}
