# Levantamento habitacional dos municípios goianos — AGEHAB

Sistema **ASP.NET Core 8 (Razor Pages)** com identidade visual AGEHAB/AdminLTE e banco de dados **SQL Server**, preparado para IIS em https://painel.agehab.go.gov.br/.

**A aplicação não possui área administrativa.** As respostas são consultadas no SSMS utilizando os scripts de consulta do repositório.

## Questionário (versão 2)

| Questão | Dados | Tipo |
| --- | --- | --- |
| 1 | Nome do município (246 municípios goianos) | Lista suspensa, código IBGE |
| 2 | Profissionais no cadastro/seleção de famílias (2.1–2.4) e quantos são assistentes sociais com CRESS ativo (2.5) | 5 inteiros |
| 3 | Assistentes sociais em visitas/relatórios por vínculo (3.1–3.4) e quantos atuaram exclusivamente na seleção (3.5) | 5 inteiros |
| 4 | Profissionais no Trabalho Social pós-ocupacional por vínculo (4.1–4.4) | 4 inteiros |
| 5 | Coordenação por assistentes sociais com CRESS, profissionais de áreas afins e administrativos (5.1–5.3) | 3 inteiros |
| 6 | Estrutura organizacional de Habitação e Trabalho Social | Lista suspensa com 9 opções; descrição se Outra estrutura |
| 7 | Programa Habitacional próprio ou atuação com Estado/União | 4 alternativas; detalhamento obrigatório se programa próprio |
| 8 | Educação, saúde, lazer, CRAS/CREAS, comércio, reciclagem, limpeza, asfalto, iluminação, posto policial | 13 respostas Sim/Não |

**Validação:** os 17 campos de quantidades são obrigatórios, inteiros de 0 a 100.000 (zero significa ausência). O item 2.5 não pode exceder a soma de 2.1–2.4, nem o item 3.5 a soma de 3.1–3.4. Os 13 itens da questão 8 são obrigatórios, sem resposta padrão; o município seleciona Sim ou Não em cada item. Os detalhes de programa próprio e de Outra estrutura são exigidos apenas quando pertinentes.

Todos os dados são validados novamente no servidor. A gravação é parametrizada no SQL Server e fornece um protocolo. Uma única resposta por município **na versão 2**.

## Alterações no banco: importante antes de publicar

A estrutura foi modificada **substancialmente** em relação às perguntas anteriores. Para impedir perda de dados, a versão nova grava em uma tabela separada:

- **Nova:** dbo.RespostasMunicipaisV2 — contém integralmente questões 1 a 8 novas.
- **Antiga:** dbo.RespostasMunicipais — mantém os registros do questionário anterior e não é modificada pela migração.
- Compartilhada: dbo.Municipios — contém os 246 municípios do Estado de Goiás.

### Servidor SQL Server já utilizado pelo projeto

**Execute uma vez** database/migracao-v2.sql no SSMS, conectado ao SQL Server correto. O script usa o banco AGEHAB_FORMULARIO_MUNICIPIOS e cria somente dbo.RespostasMunicipaisV2 se ainda não existir. **Não execute database/schema.sql em uma instância que já possua o banco.**

### Instalação do zero

Se ainda não existe esse banco, execute database/schema.sql. O script cria a base, os 246 municípios e a nova dbo.RespostasMunicipaisV2. Não cria a tabela de respostas antiga.

### Conceder permissão para a aplicação

A identidade utilizada pelo IIS precisa ter INSERT em dbo.RespostasMunicipaisV2, além de SELECT no catálogo de municípios, de acordo com a configuração de segurança local. Exemplo ilustrativo no SSMS, substituindo o usuário pelo nome real:

    USE AGEHAB_FORMULARIO_MUNICIPIOS;
    GRANT INSERT ON dbo.RespostasMunicipaisV2 TO [USUARIO_APLICACAO];
    GRANT SELECT ON dbo.Municipios TO [USUARIO_APLICACAO];

**ATENÇÃO:** os comandos GRANT apenas funcionam com usuário previamente existente no banco e precisam ser executados pelo DBA. Não publique credenciais no repositório.

## Configuração SQL Server

Configure a variável de ambiente ConnectionStrings__Municipios no processo ASP.NET Core (IIS). O appsettings.json permanece sem credenciais.

Exemplo para login SQL com TLS válido:

    Server=SERVIDOR_SQL,1433;Database=AGEHAB_FORMULARIO_MUNICIPIOS;User Id=USUARIO;Password=SENHA;Encrypt=True;TrustServerCertificate=False;

Exemplo para conta de serviço Windows com autenticação integrada:

    Server=SERVIDOR_SQL,1433;Database=AGEHAB_FORMULARIO_MUNICIPIOS;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;

A conta Windows do Application Pool precisa ter permissão no SQL Server se usar Integrated Security. Em produção, use certificado SQL confiável, segredos em configuração protegida e não exponha a porta SQL publicamente.

### Diagnóstico da versão 2, sem gravação

Na raiz do projeto e com ConnectionStrings__Municipios configurada no ambiente:

    dotnet run --project src/MunicipioPortal/MunicipioPortal.csproj -- --verificar-sqlserver

O comando verifica a nova tabela, todas as colunas exigidas, os 246 municípios e a permissão INSERT. **Não envia nenhum formulário nem altera dados.** Um BD-ESTRUTURA indica que provavelmente ainda falta executar database/migracao-v2.sql.

## Execução local

    dotnet restore src/MunicipioPortal/MunicipioPortal.csproj
    dotnet run --project src/MunicipioPortal/MunicipioPortal.csproj --urls http://localhost:5080

Página: http://localhost:5080/.

Testes de validação e estrutura sem banco externo:

    dotnet run --project tests/PortalSmokeTests/PortalSmokeTests.csproj

## Publicação no IIS (painel.agehab.go.gov.br)

1. Execute database/migracao-v2.sql no SQL Server existente **antes** de substituir a versão publicada no IIS.
2. Conceda INSERT à identidade SQL do portal em dbo.RespostasMunicipaisV2.
3. Instale o ASP.NET Core 8 Hosting Bundle no servidor Windows com IIS.
4. Gere o pacote: dotnet publish src/MunicipioPortal/MunicipioPortal.csproj -c Release -o C:\Publicacao\painel-agehab
5. Publique o pacote no site do IIS, usando pool .NET CLR Version: No Managed Code.
6. Configure certificado HTTPS, domínio painel.agehab.go.gov.br e ConnectionStrings__Municipios.
7. Execute o diagnóstico e realize uma submissão autorizada de homologação.

O pipeline GitHub Actions também gera um artefato IIS com web.config e os assets estáticos. A implantação real, certificado e DNS são administrados pela infraestrutura.

## Consultas SQL

**Consulta completa tabulada, 246 linhas (incluindo pendentes):** execute database/consultas-v2.sql no SSMS.

Consulta básica de todas as respostas da versão nova:

    USE AGEHAB_FORMULARIO_MUNICIPIOS;
    SELECT M.Nome AS Municipio, R.*
      FROM dbo.RespostasMunicipaisV2 AS R
      JOIN dbo.Municipios AS M ON M.CodigoIBGE = R.CodigoIBGE
     ORDER BY M.Nome;

As respostas da versão anterior continuam na tabela dbo.RespostasMunicipais. **Não misture os quantitativos antigos e novos:** a questão 4 e a questão 5 tiveram seus significados alterados.

## Privacidade e limites

O formulário não exige identificação individual de representantes municipais. A restrição de uma resposta por município não garante a identidade de quem respondeu. Antes de uso oficial público, considere distribuição autenticada ou convites individualizados.

A compilação e os testes automatizados não substituem um teste de integração com o SQL Server real e a configuração de IIS da AGEHAB.
