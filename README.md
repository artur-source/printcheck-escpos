# PrintCheck ESC/POS 🖨️

**PrintCheck ESC/POS** é uma ferramenta utilitária robusta desenvolvida em .NET 8 para diagnóstico e teste de impressoras térmicas que utilizam o protocolo ESC/POS. Ela permite a descoberta automática de dispositivos na rede (via TCP Probe) e no sistema local (via Windows Spooler), facilitando a validação de comunicações e layouts de impressão.

## ✨ Funcionalidades

- **Descoberta Automática:** Localiza impressoras térmicas conectadas via USB (Windows Spooler) ou Rede (TCP/IP).
- **Suporte Multi-Encoding:** Teste de impressão com suporte a diferentes encodings (`cp850`, `cp858`, `latin-1`, `utf-8`).
- **Protocolo ESC/POS:** Implementação limpa para comandos de inicialização, alinhamento, negrito, tamanho de fonte e corte de papel.
- **Interface Intuitiva:** UI moderna em Windows Forms para fácil operação.
- **Logs Técnicos:** Rastreamento detalhado do processo de descoberta e envio de dados.

## 🚀 Como Começar

### Pré-requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11 (para execução da UI)

### Instalação e Execução

1. Clone o repositório:
   ```bash
   git clone https://github.com/artur-source/printcheck-escpos.git
   cd printcheck-escpos
   ```

2. Para compilar e rodar rapidamente, utilize o script fornecido:
   ```bash
   ./build_e_rodar.bat
   ```

Ou manualmente via terminal:
```bash
dotnet run --project src/PrintCheck.csproj
```

## 📂 Estrutura do Projeto

- `src/PrintCheck.Core/`: Modelos de dados e definições fundamentais.
- `src/PrintCheck.Discovery/`: Mecanismos de descoberta de impressoras (Rede e Spooler).
- `src/PrintCheck.Protocol/`: Builder de comandos ESC/POS.
- `src/PrintCheck.Printing/`: Helpers para envio de dados brutos (Raw Printing).
- `src/PrintCheck.UI/`: Interface gráfica do usuário.

## 🛠️ Rebranding

Este projeto foi originalmente desenvolvido por **CI Informática** e agora é mantido como um projeto de código aberto por **artur-source**.

## 📄 Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.
