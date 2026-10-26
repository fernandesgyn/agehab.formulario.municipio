# Formulário dos Municípios Goianos — AGEHAB

Aplicação PHP 8.4, AdminLTE 4.10 e SQL Server para levantamento de profissionais e programas habitacionais dos municípios de Goiás. O leiaute reutiliza a identidade visual e o CSS do repositório de Diárias AGEHAB: https://github.com/fernandesgyn/diarias

## Questões implementadas

1. Identificação: seleção dos 246 municípios de Goiás (código IBGE).
2. Profissionais no cadastro e seleção de famílias: efetivos, terceirizados, contrato temporário e cargo em comissão.
3. Profissionais nas visitas sociais e elaboração de relatório social: as mesmas quatro categorias.
4. Profissionais no Trabalho Social (TS), fase pré-ocupacional: as mesmas quatro categorias.
5. Profissionais no Trabalho Social (TS), fase pós-ocupacional: as mesmas quatro categorias.
6. Programa habitacional próprio: quatro alternativas da planilha, com campo obrigatório para identificar nome e benefício do programa próprio quando a resposta for "Sim".

Os campos numéricos exigem inteiro entre 0 e 100.000 (incluindo zero); os totais por área são calculados dinamicamente. A gravação impede duplicidade de município por meio de chave única. Correções dependem de intervenção do responsável pela base: o formulário público não sobrescreve respostas.

## Pré-requisitos

- PHP 8.4 ou superior com extensão pdo_sqlsrv habilitada.
- Microsoft ODBC Driver for SQL Server 17 ou 18.
- Microsoft SQL Server 2019 ou superior.
- IIS com PHP instalado ou servidor PHP embutido para desenvolvimento.
- Navegador com acesso aos CDNs de Bootstrap 5, AdminLTE 4 e Bootstrap Icons.
- Em produção, configure HTTPS, permissões mínimas no banco e backups.

## Provisionamento inicial do SQL Server

Abra database/schema.sql no SQL Server Management Studio ou execute no sqlcmd. O script cria o banco AGEHAB_FORMULARIO_MUNICIPIOS, a tabela de municípios com 246 registros e dbo.RespostasMunicipais com 16 campos quantitativos, opção de programa, detalhes e data de envio. O arquivo é o esquema final para uma base nova: não há migrations nem criação de tabelas na execução da aplicação.

Crie um usuário SQL Server próprio para a aplicação, com somente SELECT sobre dbo.Municipios e SELECT/INSERT sobre dbo.RespostasMunicipais; conceda também permissão de SELECT necessária para obter OUTPUT INSERTED.Id. Não utilize as credenciais administrativas nem as credenciais do banco de RH.

## Configuração

Copie .env.example para .env e altere host, porta, nome do banco, usuário e senha. O .env está no .gitignore e é bloqueado da navegação por router.php e web.config.

Para habilitar a área administrativa, execute no terminal:

    php -r "echo password_hash('TROQUE_A_SENHA', PASSWORD_DEFAULT), PHP_EOL;"

Cole o hash produzido em ADMIN_PASSWORD_HASH do arquivo .env. Não armazene a senha em texto puro no Git.

No terminal, na pasta do projeto, execute:

    php -S localhost:80 router.php

Se estiver usando Windows e um PHP específico:

    "C:\CAMINHO\php.exe" -S localhost:80 router.php

Acesse http://localhost/. O router.php impede acesso direto às pastas internas e aos arquivos de configuração quando usado o servidor PHP embutido. Em IIS, web.config protege as pastas internas e arquivos sensíveis.

## Páginas

- index.php: questionário público.
- salvar.php: valida e grava a resposta usando PDO e parâmetros SQL.
- concluido.php: confirmação do envio e protocolo.
- admin.php: visão administrativa protegida por senha, com total de municípios e resumo.
- resposta.php: consulta detalhada da resposta (restrita).
- exportar.php: exportação de CSV em UTF-8, protegido por sessão administrativa.
- tests/questionnaire.php: testes sem banco de dados.

Para testar:

    php -d zend.assertions=1 -d assert.exception=1 tests/questionnaire.php

O GitHub Actions executa os testes e a verificação de sintaxe dos arquivos PHP.

## Segurança e limitações

O formulário **não autentica o respondente municipal**: a identidade de quem responde em nome do município precisa ser verificada fora da aplicação. Para uso oficial, recomenda-se autenticação institucional ou convites individuais por município antes da ampla divulgação. Uma única resposta é permitida por município; envios indevidos devem ser resolvidos administrativamente. A autenticação administrativa utiliza senha com hash no .env e sessão segura; não use HTTP aberto em produção.

Não foram executados testes de integração com o SQL Server real, que depende de infraestrutura e credenciais do servidor.

## Fonte da lista de municípios

Catálogo de municípios com códigos IBGE, filtrados pela UF 52 (GO), extraído de https://github.com/kelvins/Municipios-Brasileiros. A API de localidades do IBGE está documentada em https://servicodados.ibge.gov.br/api/docs/localidades. Os nomes foram incorporados ao código para que o formulário não dependa de conexão externa ao carregar a lista.
