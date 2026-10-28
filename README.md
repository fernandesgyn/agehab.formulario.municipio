# Formulário AGEHAB — Municípios Goianos

Sistema **ASP.NET Core 8 (Razor Pages)**, AdminLTE e **Microsoft SQL Server**. Endereço previsto: https://painel.agehab.go.gov.br/. O formulário tem 8 questões e lista os 246 municípios de Goiás. Não existe área administrativa; consultas são feitas no SSMS.

## Banco de dados: esquema definitivo e sem migrations

O projeto possui **apenas um script de criação**, database/schema.sql. O arquivo contém tudo que uma instalação nova necessita:

- CREATE DATABASE AGEHAB_FORMULARIO_MUNICIPIOS.
- CREATE TABLE dbo.Municipios com os 246 municípios e códigos IBGE.
- CREATE TABLE dbo.RespostasMunicipais com **todas as colunas atuais** das questões 1 a 8, restrições de validade e índice por data de envio.

**Não existem migrations, tabelas V2, verificações condicionais de criação ou atualizações automáticas de esquema.** O aplicativo jamais executa CREATE/ALTER/DROP, nem na inicialização nem durante o envio. O script deve ser executado **uma única vez em uma instalação limpa** (SSMS/sqlcmd). Não há a necessidade de executar instruções adicionais de criação de tabelas ou migrations após ele.

### Atenção para instalações já existentes

**Não execute database/schema.sql sobre uma instalação que já tenha o banco AGEHAB_FORMULARIO_MUNICIPIOS.** O script contém CREATE DATABASE/CREATE TABLE normais e falhará em objetos existentes. **Não apague nem recrie um banco com respostas reais sem antes fazer backup e aprovar a estratégia de preservação de dados.** Esta versão substitui a nomenclatura dbo.RespostasMunicipaisV2 por dbo.RespostasMunicipais e usa um esquema diferente do formulário antigo. Portanto, os bancos já provisionados anteriormente **não são compatíveis automaticamente** com o novo código.

Para uma nova implantação sem conversão, use um ambiente SQL Server sem essa base ou uma instância nova, execute schema.sql e só então aponte ConnectionStrings__Municipios para ela. Se a infraestrutura precisar **conservar respostas históricas no mesmo banco**, será necessária uma decisão administrativa de conversão/arquivamento **manual e específica**, fora deste projeto (nenhuma migração é fornecida e nenhum dado é alterado automaticamente). **Atualizar apenas o executável no IIS sem adequar o banco causará erro de gravação.**

### Configuração de conexão

Defina no processo ASP.NET Core/IIS a variável de ambiente ConnectionStrings__Municipios (não publique senhas no GitHub). Exemplos:

Autenticação SQL com certificado confiável:

    Server=SERVIDOR_SQL,1433;Database=AGEHAB_FORMULARIO_MUNICIPIOS;User Id=USUARIO;Password=SENHA;Encrypt=True;TrustServerCertificate=False;

Autenticação integrada com conta Windows autorizada no SQL Server:

    Server=SERVIDOR_SQL,1433;Database=AGEHAB_FORMULARIO_MUNICIPIOS;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;

A conta da aplicação precisa de permissão INSERT em dbo.RespostasMunicipais e SELECT em dbo.Municipios. O app não altera tabelas. Conceda as permissões por procedimento próprio do DBA.

## Questões implementadas

| Questão | Conteúdo |
| --- | --- |
| 1 | Nome do Município (246 opções) |
| 2 | Profissionais no cadastro/seleção por vínculo (2.1–2.4) e assistentes sociais com registro no CRESS (2.5) |
| 3 | Assistentes sociais nas visitas/relatórios por vínculo (3.1–3.4) e quantos atuaram exclusivamente na seleção (3.5) |
| 4 | Profissionais no Trabalho Social pós-ocupacional, por vínculo (4.1–4.4) |
| 5 | Assistentes sociais coordenadores com CRESS, áreas afins e administrativos (5.1–5.3) |
| 6 | Estrutura organizacional (9 opções; outra estrutura requer detalhamento) |
| 7 | Programa habitacional próprio ou ações com Estado/União (4 opções; programa próprio requer detalhamento) |
| 8 | Acesso a serviços e infraestrutura: 13 itens com resposta obrigatória Sim/Não |

Os 17 campos numéricos são obrigatórios, entre 0 e 100.000, inclusive zero quando não existirem profissionais. O total 2.5 não pode superar a soma 2.1–2.4 e o total 3.5 não pode superar a soma 3.1–3.4. A gravação é parametrizada, protege CSRF e gera protocolo somente após INSERT confirmado. Uma resposta por código IBGE, controlada no SQL Server.

## Como executar no Windows

Requer o SDK **.NET 8** e acesso à conexão SQL Server configurada. Na raiz do repositório:

    dotnet restore src/MunicipioPortal/MunicipioPortal.csproj
    dotnet run --project src/MunicipioPortal/MunicipioPortal.csproj --urls http://localhost:5080

Abra http://localhost:5080/.

Para testar as definições do questionário e a estrutura do script sem SQL Server:

    dotnet run --project tests/PortalSmokeTests/PortalSmokeTests.csproj

Para verificar uma conexão SQL Server já configurada, sem gravar:

    dotnet run --project src/MunicipioPortal/MunicipioPortal.csproj -- --verificar-sqlserver

Esse comando apenas consulta a conexão, o catálogo, as colunas da tabela dbo.RespostasMunicipais e as permissões, mas **não cria nem altera objetos**.

## Publicar no IIS

1. **Antes de publicar**, configure o banco novo executando database/schema.sql no SSMS (somente instalação limpa) e as permissões da identidade do portal.
2. Instale ASP.NET Core 8 Hosting Bundle no servidor IIS, se necessário.
3. Gere o pacote: dotnet publish src/MunicipioPortal/MunicipioPortal.csproj -c Release -o C:\Publicacao\painel-agehab
4. Crie/atualize o site IIS com pool .NET CLR Version: No Managed Code e diretório apontando para a publicação.
5. Configure DNS e HTTPS para painel.agehab.go.gov.br, certificado válido e ConnectionStrings__Municipios.
6. Confirme que o banco apontado pela conexão é o **novo esquema definitivo**. Execute o diagnóstico e teste uma submissão autorizada.

O pipeline GitHub Actions compila, testa o esquema em nível de texto e gera pacote IIS, mas não testa a conexão com o SQL Server da AGEHAB.

## Consultar todos os municípios e respostas tabuladas

Execute database/consultas.sql no SSMS. A consulta usa dbo.Municipios LEFT JOIN dbo.RespostasMunicipais para incluir os 246 municípios, mesmo os pendentes, e devolve respostas em colunas.

Uma consulta rápida apenas aos respondentes:

    USE AGEHAB_FORMULARIO_MUNICIPIOS;
    SELECT M.Nome AS Municipio, R.*
    FROM dbo.RespostasMunicipais AS R
    JOIN dbo.Municipios AS M ON M.CodigoIBGE = R.CodigoIBGE
    ORDER BY M.Nome;

## Segurança

O formulário não autentica individualmente os representantes municipais. A unicidade por município não garante autoria da resposta. Para utilização oficial aberta, considere validar o responsável ou distribuir convites individuais. Nenhum dado existente é apagado pelos arquivos publicados neste repositório.
