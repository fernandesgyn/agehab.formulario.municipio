# Formulário dos Municípios Goianos — AGEHAB

Aplicação exclusivamente **ASP.NET Core 8 (Razor Pages)**, preparada para IIS em Windows Server e publicação em **https://painel.agehab.go.gov.br/**. Mantém o AdminLTE 4 e a identidade visual do sistema Diárias AGEHAB.

> O código está preparado para o endereço, mas o DNS, certificado HTTPS, IIS e a conexão SQL Server precisam ser configurados pela infraestrutura. Não há publicação remota automática.

## Tecnologia e estrutura

- **ASP.NET Core 8 + Razor Pages**, C# e Bootstrap/AdminLTE 4.
- **SQL Server** via Microsoft.Data.SqlClient (ADO.NET parametrizado).
- Banco inicial em database/schema.sql, com 246 municípios do IBGE e 16 campos quantitativos. Sem EF Core/migrations.
- Código em src/MunicipioPortal; testes em tests/PortalSmokeTests.
- A origem do formulário é a planilha Forms para Municípios Goianos.xlsx: município, cadastro/seleção de famílias, visitas sociais, TS pré e pós ocupação; efetivos/terceirizados/temporários/comissionados em cada bloco; programa habitacional próprio e quatro alternativas.

## Banco de dados SQL Server

Execute **uma única vez** database/schema.sql no SQL Server Management Studio em uma base nova. O script cria AGEHAB_FORMULARIO_MUNICIPIOS, dbo.Municipios e dbo.RespostasMunicipais. Se já tiver instalado a versão PHP anterior, o esquema foi preservado e **não deve ser recriado**.

Crie uma conta SQL da aplicação com permissões mínimas de SELECT nas duas tabelas e INSERT em dbo.RespostasMunicipais, sem CREATE/ALTER/DROP. O app não consulta o banco de RH do Diárias.

## Configuração de ambiente (não versionar segredos)

Configure no servidor IIS, fora do repositório, estas variáveis de ambiente da aplicação:

    ConnectionStrings__Municipios=Server=SERVIDOR,1433;Database=AGEHAB_FORMULARIO_MUNICIPIOS;User Id=USUARIO;Password=SENHA;Encrypt=True;TrustServerCertificate=False;

Use certificado válido e confiável para conexões SQL criptografadas. Excepcionalmente em homologação interna, sem certificado SQL válido, ajuste Encrypt/TrustServerCertificate de forma consciente. Não exponha o SQL Server à internet.

Para gerar hash de senha de administração, execute localmente:

    dotnet run --project src/MunicipioPortal/MunicipioPortal.csproj -- --gerar-hash-admin

Informe a senha no terminal (mínimo de 12 caracteres) e configure o hash no servidor:

    Admin__PasswordHash=HASH_GERADO

A autenticação administrativa usa hash ASP.NET Core Identity e cookie. As contas municipais **não são autenticadas** nessa primeira versão.

## Desenvolvimento e testes

Requer SDK **.NET 8**. Na raiz do Git:

    dotnet restore src/MunicipioPortal/MunicipioPortal.csproj
    dotnet run --project src/MunicipioPortal/MunicipioPortal.csproj --urls http://localhost:5080

Formulário: http://localhost:5080/; administração: http://localhost:5080/acesso. Sem conexão SQL, o formulário aparece, mas não é possível concluir envios.

Smoke tests sem SQL:

    dotnet run --project tests/PortalSmokeTests/PortalSmokeTests.csproj

O GitHub Actions compila, executa os testes e publica em diretório de artefato.

## Publicação IIS: painel.agehab.go.gov.br

1. Criar registro DNS A ou CNAME de **painel.agehab.go.gov.br** para o servidor/reverse proxy corporativo autorizado.
2. Instalar ASP.NET Core **8 Hosting Bundle** no Windows Server com IIS e reiniciar serviço IIS após instalação.
3. Compilar em Windows no diretório do repositório:

       dotnet publish src/MunicipioPortal/MunicipioPortal.csproj -c Release -o C:\Publicacao\painel-agehab

4. Criar site no IIS com diretório físico da publicação; application pool dedicado, **.NET CLR Version: No Managed Code**, modo integrado e identidade de menores privilégios.
5. Em **Bindings**, configurar HTTPS, porta 443, host name **painel.agehab.go.gov.br**, SNI e certificado válido para o domínio. Configurar também redirecionamento HTTP → HTTPS quando apropriado.
6. Injetar as variáveis ConnectionStrings__Municipios e Admin__PasswordHash no ambiente do site (com acesso restrito), reciclar o App Pool e garantir firewall permitindo o IIS alcançar o SQL Server.
7. Validar a raiz, /acesso e /administracao com banco de **homologação** antes de apontar usuários finais.

O .NET SDK gera automaticamente o web.config para o módulo ASP.NET Core no dotnet publish; **não usar web.config legado do PHP**. A configuração de Production restringe o host a painel.agehab.go.gov.br, habilita HSTS e exige HTTPS.

## Endereços

| Rota | Função |
| --- | --- |
| / | Questionário público |
| /enviado | Protocolo de confirmação |
| /acesso | Login administrativo |
| /administracao | Listagem, totais e exportação CSV |
| /administracao/resposta/{id} | Consulta detalhada autenticada |

## Regras e limitações

- Lista **246 municípios**, com código IBGE.
- Quatro grupos de quatro quantitativos, aceitando inteiros de 0 a 100.000; total por bloco exibido automaticamente.
- Questão 6 obrigatória, com detalhamento obrigatório de nome e benefício quando selecionado programa municipal próprio.
- Gravação parametrizada em SQL Server, validação no servidor, proteção antiforgery, uma única resposta por município, protocolo por ID e exportação CSV protegida.
- Formulário municipal é público e **não valida quem está respondendo em nome do município**. Antes de divulgar oficialmente, considere convites individualizados ou autenticação institucional; um terceiro pode registrar dados incorretos e bloquear novo envio.
- Administração usa uma senha compartilhada, não autenticação individual no Active Directory.
- DNS, IIS, certificado e conexão SQL precisam ser implantados e testados no ambiente AGEHAB. O workflow de CI não acessa esse servidor.
