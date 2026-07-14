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

### 📥 Download

Você pode obter o código-fonte do projeto de duas maneiras:

1.  **Clonar o Repositório (Recomendado):**
    ```bash
    git clone https://github.com/artur-source/printcheck-escpos.git
    cd printcheck-escpos
    ```

2.  **Baixar como ZIP:**
    Acesse a página do repositório no GitHub ([https://github.com/artur-source/printcheck-escpos](https://github.com/artur-source/printcheck-escpos)), clique no botão verde "Code" e selecione "Download ZIP". Após baixar, extraia o conteúdo para uma pasta de sua preferência.

### 🛠️ Compilação e Execução

Após baixar o projeto, siga um dos métodos abaixo para compilar e executar:

1.  **Usando o Script `build_e_rodar.bat` (Windows):**
    Este script automatiza a compilação e execução do projeto. Basta executá-lo na raiz do diretório do projeto:
    ```bash
    ./build_e_rodar.bat
    ```

2.  **Manualmente via Terminal:**
    Abra um terminal na raiz do projeto e execute os seguintes comandos:
    ```bash
    dotnet build src/PrintCheck.csproj
    dotnet run --project src/PrintCheck.csproj
    ```
    Isso compilará o projeto e iniciará a aplicação.

### 🖥️ Tutorial de Uso da Interface

Após iniciar o **PrintCheck ESC/POS**, você verá uma interface simples com as seguintes opções:

1.  **Descoberta de Impressoras:**
    *   Clique no botão "🔍" (lupa) ao lado do campo "IMPRESSORAS ENCONTRADAS:".
    *   A aplicação iniciará o processo de descoberta, procurando impressoras via rede (TCP/IP) e no spooler do Windows.
    *   As impressoras encontradas serão listadas no campo "IMPRESSORAS ENCONTRADAS:".
    *   O "Log Box" na parte inferior exibirá o progresso e os resultados da descoberta.

2.  **Seleção de Impressora e Encoding:**
    *   Selecione a impressora desejada na lista suspensa "IMPRESSORAS ENCONTRADAS:".
    *   Escolha o `ENCODING` apropriado para sua impressora (ex: `cp850` é comum no Brasil) na lista suspensa abaixo.

3.  **Impressão de Teste:**
    *   Clique no botão "IMPRIMIR TESTE COMPLETO".
    *   A aplicação enviará um cupom de teste ESC/POS para a impressora selecionada.
    *   O "Log Box" informará se a impressão foi bem-sucedida ou se ocorreu algum erro.

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
