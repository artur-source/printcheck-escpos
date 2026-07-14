using System;
using System.Collections.Generic;
using System.Text;
using PrintCheck.Core;

namespace PrintCheck.Protocol
{
    public class EscPosBuilder
    {
        private List<byte> _buffer = new List<byte>();
        private Encoding _encoding;

        public EscPosBuilder(string encodingName = "cp850")
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try { _encoding = Encoding.GetEncoding(encodingName); }
            catch { _encoding = Encoding.GetEncoding(850); }
            Initialize();
        }

        public void Initialize() => _buffer.AddRange(new byte[] { 0x1B, 0x40 });
        public void SetAlignment(int align) => _buffer.AddRange(new byte[] { 0x1B, 0x61, (byte)align });
        public void SetBold(bool on) => _buffer.AddRange(new byte[] { 0x1B, 0x45, (byte)(on ? 1 : 0) });
        public void SetFontSize(int size) => _buffer.AddRange(new byte[] { 0x1D, 0x21, (byte)size });
        public void PrintText(string text) => _buffer.AddRange(_encoding.GetBytes(text));
        public void PrintLine(string text = "") => PrintText(text + "\n");
        public void DrawSeparator(char c = '=') => PrintLine(new string(c, 48));
        public void Cut() => _buffer.AddRange(new byte[] { 0x1D, 0x56, 0x01 });

        // QR Code removido na versão 2.0.1 para distribuição final

        public byte[] Build() => _buffer.ToArray();

        public static byte[] GetTestCoupon(DiscoveredPrinter printer, string encodingName)
        {
            var b = new EscPosBuilder(encodingName);
            b.SetAlignment(1);
            b.PrintLine("╔════════════════════════════════╗");
            b.PrintLine("║       PrintCheck v2.0          ║");
            b.PrintLine("║      Discovery Edition         ║");
            b.PrintLine("╚════════════════════════════════╝");
            b.SetFontSize(0x11); b.SetBold(true); b.PrintLine("PrintCheck"); b.SetFontSize(0); b.SetBold(false);
            b.DrawSeparator();
            
            b.SetAlignment(0);
            b.SetBold(true); b.PrintLine("[INFO CONEXAO]"); b.SetBold(false);
            b.PrintLine($"Impressora: {printer.Name}");
            b.PrintLine($"Tipo: {printer.Type}");
            b.PrintLine($"Endereco: {printer.Address}");
            b.PrintLine($"Fonte: {printer.Source}");
            b.PrintLine($"Timestamp: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            b.DrawSeparator();

            b.SetAlignment(1);
            b.SetBold(true); b.PrintLine("[TESTE DE IMPRESSAO FINALIZADO]"); b.SetBold(false);
            b.PrintLine("Comunicacao estabelecida com sucesso.");
            b.PrintLine();
            
            b.DrawSeparator();
            b.PrintLine("by artur-source");
            b.Cut();
            return b.Build();
        }
    }
}
