Propósito
---------
Repositório didático com o código-fonte de um cliente RAT (Remote Access Trojan) em C# (WinForms).
Criado para demonstrar, em vídeo, o funcionamento básico de um agent/C2: comunicação TCP, execução remota de comandos,
download de arquivos e coleta de informações do sistema. Material voltado exclusivamente para fins educacionais e de pesquisa.

AVISO LEGAL (LEIA ANTES)
------------------------
- ESTE PROJETO É APENAS PARA FINS EDUCACIONAIS E DE DEMONSTRAÇÃO.
- NÃO utilize este código contra sistemas de terceiros sem AUTORIZAÇÃO EXPLÍCITA. Uso indevido é ILEGAL.
- NÃO disponibilize binários prontos em redes públicas. Mantenha apenas código-fonte e material didático.
- O autor não se responsabiliza por usos maliciosos ou danos resultantes deste código.

Conteúdo do repositório
-----------------------
/src/RATClient/       -> Código-fonte do cliente (WinForms)
  - RATClientForm.cs
  - app.manifest
  - RATClient.csproj

Pré-requisitos
--------------
- Visual Studio (recomendado) ou .NET SDK compatível.
- Máquina/VM isolada para testes (recomendado: VMs em rede interna).
- Se for usar um servidor C2 manual: netcat, listener customizado ou servidor de teste.

Antes de compilar / executar
---------------------------
1. Revise todo o código antes de compilar.
2. **Altere o IP do servidor** em `RATClientForm.cs` (variável `serverIP`) para o endereço do seu ambiente de laboratório.
   - Para exemplos locais, use `127.0.0.1` ou `0.0.0.0`.
3. NÃO execute em máquinas de produção ou com dados reais. Use VMs isoladas (NAT interno / rede privada).
4. Remova quaisquer dados sensíveis, credenciais ou IPs públicos antes de commitar.

Como compilar (resumo)
---------------------
Visual Studio:
- Abra `RATClient.csproj` no Visual Studio.
- Ajuste configurações de build (Debug/Release) conforme necessário.
- Build -> Rebuild Solution.

Formato de resposta
-------------------
- Respostas de texto são enviadas via stream TCP codificadas em UTF-8.
- Para arquivos, o conteúdo é enviado como Base64 acompanhado do nome do arquivo.

Exemplo rápido de laboratório
-----------------------------
1. Configure duas VMs isoladas (ex: Windows client + Kali server) na mesma rede interna.
2. No servidor (Kali), utilize um listener simples (ex: `nc -lvp 4444`) ou um servidor C2 custom.
3. Ajuste `serverIP` no código do client para o IP da VM servidor e compile.
4. Execute o servidor/listener primeiro, depois inicie o client compilado na VM Windows.
5. Envie comandos pelo listener e observe logs no cliente (WinForms).

Boas práticas de segurança
--------------------------
- Nunca exponha o listener para a Internet pública.
- Não compartilhe executáveis compilados publicamente.
- Documente e peça autorização explícita antes de testar em redes ou máquinas que não sejam suas.

Licença e Disclaimer
--------------------
- Este repositório inclui um DISCLAIMER com aviso explícito de uso educacional.
- Se optar por uma licença oficial, recomendo MIT + DISCLAIMER adicional no README (a licença não isenta responsabilidades legais).
- Não publique este projeto com objetivo de facilitar atividades maliciosas.

Referência / Vídeo
------------------
Projeto demonstrado e explicado no vídeo: [[AQUI]](https://youtu.be/cBqEWlhtxJM)
Vídeo por: Douglas Lockshield

Contribuição
------------
Contribuições são bem-vindas, desde que:
- Focadas em melhorar documentação, instruções de laboratório e mitigação.
- Não adicionem funcionalidades que facilitem uso malicioso (ex.: propagação automática, exploração remota, etc.).
- Incluam testes em ambiente isolado e descrições claras do objetivo.

------------------
Douglas Lockshield
