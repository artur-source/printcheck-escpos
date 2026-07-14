using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrintCheck.Core;
using PrintCheck.Discovery;
using PrintCheck.Discovery.Network;
using PrintCheck.Protocol;
using PrintCheck.Printing;

namespace PrintCheck.UI
{
    public partial class Form1 : Form
    {
        private ComboBox cbPrinters;
        private ComboBox cbEncoding;
        private Button btnPrint;
        private Button btnDiscover;
        private ListBox logBox;
        private List<DiscoveredPrinter> discoveredPrinters = new List<DiscoveredPrinter>();

        public Form1()
        {
            InitializeComponentManual();
            
            // Título limpo para produção
            this.Text = "PrintCheck v2.0.1";
            
            DiscoveryLogger.Log("Aplicação iniciada - Versão 2.0.1");

            _ = RunDiscoveryAsync(); // Iniciar descoberta ao abrir
        }

        private void InitializeComponentManual()
        {
            this.Text = "PrintCheck v2.0 - Discovery Edition";
            this.Size = new Size(550, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(244, 247, 246);

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(41, 128, 185) };
            pnlHeader.Controls.Add(new Label { Text = "PrintCheck 2.0", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.White, Location = new Point(25, 20), AutoSize = true });
            pnlHeader.Controls.Add(new Label { Text = "Discovery Engine Ativo", Font = new Font("Segoe UI", 9), ForeColor = Color.White, Location = new Point(27, 58), AutoSize = true });

            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            
            Label lblP = new Label { Text = "IMPRESSORAS ENCONTRADAS:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(25, 20), AutoSize = true };
            cbPrinters = new ComboBox { Location = new Point(25, 45), Width = 380, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            btnDiscover = new Button { Text = "🔍", Location = new Point(415, 44), Size = new Size(45, 28), BackColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDiscover.Click += async (s, e) => await RunDiscoveryAsync();

            Label lblE = new Label { Text = "ENCODING:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(25, 90), AutoSize = true };
            cbEncoding = new ComboBox { Location = new Point(25, 115), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cbEncoding.Items.AddRange(new string[] { "cp850", "cp858", "latin-1", "utf-8" });
            cbEncoding.SelectedIndex = 0;

            btnPrint = new Button { Text = "IMPRIMIR TESTE COMPLETO", Location = new Point(25, 170), Size = new Size(435, 60), BackColor = Color.FromArgb(41, 128, 185), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            btnPrint.Click += BtnPrint_Click;

            logBox = new ListBox { Location = new Point(25, 250), Size = new Size(435, 200), Font = new Font("Consolas", 9), BackColor = Color.White };

            pnlMain.Controls.AddRange(new Control[] { lblP, cbPrinters, btnDiscover, lblE, cbEncoding, btnPrint, logBox });
            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlHeader);
        }

        private async Task RunDiscoveryAsync()
        {
            btnDiscover.Enabled = false;
            Log("Iniciando Discovery Engine...");
            cbPrinters.Items.Clear();
            discoveredPrinters.Clear();

            var providers = new List<IPrinterDiscoveryProvider> { new WindowsSpoolerProvider(), new TcpProbeProvider() };
            foreach (var provider in providers)
            {
                Log($"Executando {provider.ProviderName}...");
                var found = await provider.DiscoverAsync();
                foreach (var p in found)
                {
                    discoveredPrinters.Add(p);
                    cbPrinters.Items.Add(p.ToString());
                }
            }

            if (cbPrinters.Items.Count > 0) cbPrinters.SelectedIndex = 0;
            Log($"Descoberta finalizada. {discoveredPrinters.Count} dispositivos encontrados.");
            btnDiscover.Enabled = true;
        }

        private void Log(string msg)
        {
            if (logBox.InvokeRequired) { logBox.Invoke(new Action(() => Log(msg))); return; }
            logBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
            logBox.TopIndex = logBox.Items.Count - 1;
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (cbPrinters.SelectedIndex < 0)
            {
                DiscoveryLogger.Log("AVISO: Nenhuma impressora selecionada no ComboBox.");
                return;
            }

            var printer = discoveredPrinters[cbPrinters.SelectedIndex];
            string enc = cbEncoding.SelectedItem.ToString();

            DiscoveryLogger.Log($"[UI SELECTION] Impressora: {printer.Name} | Source: {printer.Source} | IP: {printer.Address}");
            Log($"Imprimindo em {printer.Name} ({printer.Source})...");

            Task.Run(() => {
                try {
                    byte[] payload = EscPosBuilder.GetTestCoupon(printer, enc);
                    bool ok = false;
                    string target = "";

                    // Ponto 1 e 2: Ramificar lógica de envio
                    if (printer.Source.Contains("TCP Probe"))
                    {
                        target = $"{printer.Address}:{printer.Port}";
                        ok = RawPrinterHelper.SendBytesToTcpPrinter(printer.Address, printer.Port, payload);
                    }
                    else
                    {
                        target = "Spooler";
                        ok = RawPrinterHelper.SendBytesToPrinter(printer.Name, payload);
                    }

                    if (ok)
                    {
                        Log($"SUCESSO: Enviado para {target}.");
                    }
                    else
                    {
                        Log($"ERRO: Falha ao enviar para {target}.");
                        DiscoveryLogger.Log($"[UI ALERT] O envio para {target} falhou. Verifique as mensagens acima no log para detalhes técnicos.");
                    }
                } catch (Exception ex) { 
                    Log($"FALHA: {ex.Message}");
                    DiscoveryLogger.LogError("BtnPrint_Click Task", ex);
                }
            });
        }
    }
}
