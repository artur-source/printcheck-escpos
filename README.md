# PrintCheck ESC/POS 🖨️

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![License](https://img.shields.io/github/license/artur-source/printcheck-escpos?style=for-the-badge&color=blue)
![GitHub Stars](https://img.shields.io/github/stars/artur-source/printcheck-escpos?style=for-the-badge&logoColor=white)
![GitHub Forks](https://img.shields.io/github/forks/artur-source/printcheck-escpos?style=for-the-badge&logoColor=white)

**PrintCheck ESC/POS** é uma ferramenta utilitária robusta, desenvolvida em .NET 8, projetada para **diagnóstico e teste de impressoras térmicas** que operam com o protocolo ESC/POS. Facilita a validação de comunicações e layouts de impressão através da descoberta automática de dispositivos na rede (via TCP Probe) e no sistema local (via Windows Spooler).

## ✨ Funcionalidades Principais

*   **Descoberta Automática:** Localiza impressoras térmicas conectadas via USB (Windows Spooler) ou Rede (TCP/IP).
*   **Suporte Multi-Encoding:** Permite testes de impressão com diversos encodings, incluindo `cp850`, `cp858`, `latin-1` e `utf-8`.
*   **Protocolo ESC/POS:** Implementação clara para comandos essenciais como inicialização, alinhamento, negrito, ajuste de tamanho de fonte e corte de papel.
*   **Interface Intuitiva:** Oferece uma UI moderna baseada em Windows Forms para operação simplificada.
*   **Logs Técnicos:** Fornece rastreamento detalhado do processo de descoberta e envio de dados para depuração.

## 🚀 Como Começar

### Pré-requisitos

Certifique-se de ter os seguintes softwares instalados:

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   Sistema Operacional Windows 10/11 (necessário para a execução da interface gráfica)

### 📥 Download e Execução

1.  **Clone o repositório:**

    ```bash
    git clone https://github.com/artur-source/printcheck-escpos.git
    cd printcheck-escpos
    ```

2.  **Navegue até a pasta do projeto:**

    ```bash
    cd src/PrintCheck.UI
    ```

3.  **Execute a aplicação:**

    ```bash
    dotnet run
    ```

    Alternativamente, você pode abrir o projeto no Visual Studio e executá-lo.

## 📂 Estrutura do Projeto

O projeto é organizado em módulos para facilitar a manutenção e escalabilidade:

*   `PrintCheck.Core`: Contém modelos de dados e lógica de negócios fundamental.
*   `PrintCheck.Discovery`: Responsável pela descoberta de impressoras na rede e no sistema.
*   `PrintCheck.Printing`: Gerencia a comunicação e o envio de comandos ESC/POS para a impressora.
*   `PrintCheck.Protocol`: Implementa a lógica para a construção de comandos ESC/POS.
*   `PrintCheck.UI`: A interface gráfica do usuário (Windows Forms).

## 🤝 Contribuição

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues ou enviar pull requests. Por favor, siga as diretrizes de código existentes.

## 📄 Licença

Este projeto está licenciado sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.
