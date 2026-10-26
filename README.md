# Agência Goiana de Habitação - AGEHAB

Formulário para municípios goianos, implementado em ASP.NET Core 8 / Razor Pages, com leiaute AdminLTE do projeto Diárias AGEHAB e persistência em Microsoft SQL Server, sem PHP ou MySQL. Hospedagem prevista: https://painel.agehab.go.gov.br/ (IIS / Windows Server).

**Não existe área administrativa.** As respostas são consultadas diretamente no SQL Server.

## Funcionalidades

- Seleção dos 246 municípios do Estado de Goiás (código IBGE).
- Quatro blocos de profissionais por vínculo (efetivos, terceirizados, temporários e comissionados).
- Questão sobre programa habitacional próprio, com detalhamento obrigatório quando afirmativo.
- Validação de números inteiros de 0 a 100.000 e totais automáticos.
- Inserção parametrizada no SQL Server e confirmação com protocolo somente após salvar.
- Uma única resposta por município (restrição UNIQUE no banco).
- Apenas as rotas /, /enviado e /erro. Não existem rotas de administração.

## Requisitos

- SDK .NET 8 para desenvolvimento; ASP.NET Core 8 Hosting Bundle no servidor IIS.
- SQL Server 2019 ou posterior com o banco e tabelas provisionados.
- Certificado TLS/HTTPS e DNS para painel.agehab.go.gov.br.
- Acesso do navegador aos CDNs de Bootstrap, AdminLTE e Bootstrap Icons.

## 1. Instalação do banco SQL Server

Em um **banco novo**, execute uma única vez database/schema.sql no SSMS ou sqlcmd. Cria AGEHAB_FORMULARIO_MUNICIPIOS, dbo.Municipios e dbo.RespostasMunicipais, com as 246 cidades.

Se já tiver criado a base da implementação anterior, **não reaplique o script** nem exclua seus dados. Esta versão não executa migrations, CREATE TABLE nem ALTER TABLE em requisições web.

Configure uma identidade SQL com SELECT nas duas tabelas e INSERT em dbo.RespostasMunicipais; não conceda direitos de DDL ao usuário da aplicação.

## 2. Configuração indispensável da conexão no IIS

O arquivo versionado src/MunicipioPortal/appsettings.json mantém **ConnectionStrings:Municipios vazio de propósito**, para não publicar senhas no Git. Sem configurar esta chave no servidor IIS, o envio apresentará **BD-CONFIG**.

Defina a variável de ambiente **ConnectionStrings__Municipios** no processo do ASP.NET Core/IIS.

Exemplo para autenticação SQL e certificado SQL Server confiável:

    Server=SERVIDOR_SQL,1433;Database=AGEHAB_FORMULARIO_MUNICIPIOS;User Id=USUARIO_APLICACAO;Password=SENHA;Encrypt=True;TrustServerCertificate=False;

Exemplo para autenticação integrada Windows: a identidade do Application Pool deve possuir acesso ao banco SQL Server.

    Server=SERVIDOR_SQL,1433;Database=AGEHAB_FORMULARIO_MUNICIPIOS;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;

Configure essa variável no ambiente do Application Pool ou por método seguro adotado pela infraestrutura. Recicle o pool. Se o IIS iniciar a aplicação por identidade diferente da conta usada no teste de terminal, especialmente com Integrated Security, o resultado poderá ser diferente.

Não coloque senhas reais no repositório nem em arquivos públicos. Prefira certificado SQL Server confiável. Se a instalação usar instância/porta diferente, ajuste o Server de acordo com a configuração real. Não exponha SQL Server à internet.

### Diagnóstico do SQL Server (somente linha de comando)

    dotnet run --project src/MunicipioPortal/MunicipioPortal.csproj -- --verificar-sqlserver

Execute no servidor com a conexão configurada no ambiente da execução. O comando **não cria tabelas, não insere dados e não precisa da área administrativa**. Confere conexão, duas tabelas, colunas obrigatórias, 246 municípios e permissão INSERT. A saída usa códigos sem exibir senhas.

Para um teste local em PowerShell com autenticação integrada, no mesmo terminal:

    $env:ConnectionStrings__Municipios = 'Server=SERVIDOR_SQL,1433;Database=AGEHAB_FORMULARIO_MUNICIPIOS;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;'
    dotnet run --project src/MunicipioPortal/MunicipioPortal.csproj -- --verificar-sqlserver

**A execução local não substitui o teste sob a identidade do IIS.**

### Mensagens da aplicação

- BD-CONFIG: ConnectionStrings__Municipios está vazia. Configure no IIS e recicle.
- BD-CONEXAO: SQL Server, porta, DNS ou firewall não acessível.
- BD-ACESSO: login SQL recusado ou banco inacessível para a conta.
- BD-ESTRUTURA: tabelas/colunas ausentes; conferir database/schema.sql.
- BD-PERMISSAO: faltam direitos INSERT.
- BD-TEMPO: operação ultrapassou limite de tempo.
- BD-OPERACAO: falha restante; confira logs ASP.NET Core no servidor.

A página exibe uma mensagem segura e preserva o preenchimento; os logs incluem a exceção técnica. Um envio malsucedido nunca gera protocolo. Duplicatas mostram erro no município, sem revelar dados de respostas existentes.

## 3. Execução local

    dotnet restore src/MunicipioPortal/MunicipioPortal.csproj
    dotnet run --project src/MunicipioPortal/MunicipioPortal.csproj --urls http://localhost:5080

Acesse http://localhost:5080/. A página será exibida mesmo quando a conexão não estiver configurada, mas só gravará depois da conexão e do banco estarem prontos.

Teste sem banco:

    dotnet run --project tests/PortalSmokeTests/PortalSmokeTests.csproj

## 4. Implantação IIS — painel.agehab.go.gov.br

1. Provisionar DNS e certificado HTTPS para painel.agehab.go.gov.br.
2. Instalar ASP.NET Core 8 Hosting Bundle no Windows Server.
3. Compilar: dotnet publish src/MunicipioPortal/MunicipioPortal.csproj -c Release -o C:\Publicacao\painel-agehab
4. Criar site IIS, com pool dedicado e .NET CLR Version: No Managed Code.
5. Criar binding https/443 para painel.agehab.go.gov.br (SNI e certificado válido).
6. Configurar ConnectionStrings__Municipios e as permissões no SQL Server. Reciclar o pool.
7. Executar teste de homologação usando município ainda não respondido e consultar a tabela.

O web.config é gerado automaticamente pelo dotnet publish. Não reutilize o arquivo do antigo sistema PHP.

## 5. Consultar respostas diretamente no SQL Server

Execute no SSMS:

    USE AGEHAB_FORMULARIO_MUNICIPIOS;
    SELECT R.*, M.Nome AS NomeMunicipio
    FROM dbo.RespostasMunicipais AS R
    JOIN dbo.Municipios AS M ON M.CodigoIBGE = R.CodigoIBGE
    ORDER BY M.Nome;

Nenhuma integração com o banco ERP_RH_AGEHAB é exigida.

## Segurança

O formulário não autentica quem responde em nome do município. Antes de enviar publicamente para órgãos municipais, recomenda-se validar a identidade do respondente ou controlar convites, pois o primeiro envio bloqueará novos envios do mesmo município.

Os testes automatizados incluem compilação, verificação do catálogo, códigos de diagnóstico e publicação para IIS, mas não podem comprovar acesso ao SQL Server real sem credenciais, infraestrutura e um ambiente de homologação.
