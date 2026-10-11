# Rapid Clicker

Aplicativo leve para Windows que automatiza cliques do botão esquerdo do mouse na posição atual do cursor. O controle é feito por atalhos globais, permitindo iniciar ou pausar a automação sem sair do aplicativo em uso.

## Recursos

- Intervalo de clique ajustável entre 1 e 10.000 milissegundos.
- Estimativa de cliques por segundo exibida em tempo real.
- Atalho global `F6` para iniciar ou pausar.
- Atalho global `F7` para encerrar o aplicativo.
- Operação pela bandeja do sistema ao minimizar.
- Executável autocontido para Windows 64 bits.

## Download e uso

Baixe `RapidClicker.exe`, abra o arquivo e escolha o intervalo desejado. Posicione o cursor sobre a área que receberá os cliques e pressione `F6`.

Pressione `F6` novamente para pausar ou `F7` para encerrar. O programa também pode ser controlado pelo ícone na bandeja do sistema.

> Alguns aplicativos e jogos podem bloquear entradas simuladas ou restringir automação. Use o programa somente quando isso for permitido pelas regras do serviço ou aplicativo.

## Compilação

### Pré-requisitos

- Windows 10 ou superior
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Executar em desenvolvimento

```powershell
dotnet run --project .\RapidClicker.csproj
```

### Publicar executável único

```powershell
dotnet publish .\RapidClicker.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

O arquivo publicado estará em:

```text
bin\Release\net8.0-windows\win-x64\publish\RapidClicker.exe
```

## Tecnologias

- C#
- .NET 8
- Windows Forms
- Windows API (`SendInput` e atalhos globais)

## Licença

Este projeto é distribuído sob a [Licença MIT](LICENSE).
