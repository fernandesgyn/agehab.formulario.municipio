-- Questões 1 a 8, uma linha por município, incluindo os não respondentes.
-- Execute no SQL Server Management Studio após aplicar migracao-v2.sql.
USE AGEHAB_FORMULARIO_MUNICIPIOS;
GO

SELECT
    M.CodigoIBGE,
    M.Nome AS Municipio,
    CASE WHEN R.Id IS NULL THEN N'PENDENTE' ELSE N'RESPONDIDO' END AS Situacao,
    R.Id AS Protocolo,
    R.CriadoEm AS DataEnvio,
    -- Equipe e vínculos (questões 2, 3, 4 e 5)
    R.Q2_EFETIVOS AS [Q2_EFETIVOS],
    R.Q2_TERCEIRIZADOS AS [Q2_TERCEIRIZADOS],
    R.Q2_TEMPORARIOS AS [Q2_TEMPORARIOS],
    R.Q2_COMISSIONADOS AS [Q2_COMISSIONADOS],
    R.Q2_CRESS AS [Q2_CRESS],
    R.Q3_EFETIVOS AS [Q3_EFETIVOS],
    R.Q3_TERCEIRIZADOS AS [Q3_TERCEIRIZADOS],
    R.Q3_TEMPORARIOS AS [Q3_TEMPORARIOS],
    R.Q3_COMISSIONADOS AS [Q3_COMISSIONADOS],
    R.Q3_EXCLUSIVOS AS [Q3_EXCLUSIVOS],
    R.Q4_EFETIVOS AS [Q4_EFETIVOS],
    R.Q4_TERCEIRIZADOS AS [Q4_TERCEIRIZADOS],
    R.Q4_TEMPORARIOS AS [Q4_TEMPORARIOS],
    R.Q4_COMISSIONADOS AS [Q4_COMISSIONADOS],
    R.Q5_CRESS_COORDENACAO AS [Q5_CRESS_COORDENACAO],
    R.Q5_AREAS_AFINS AS [Q5_AREAS_AFINS],
    R.Q5_ADMINISTRATIVOS AS [Q5_ADMINISTRATIVOS],
    -- Estrutura organizacional (questão 6)
    CASE R.Q6_ESTRUTURA
        WHEN 'SECRETARIA_EXCLUSIVA' THEN N'Secretaria Municipal exclusiva de Habitação'
        WHEN 'SECRETARIA_MISTA' THEN N'Secretaria Municipal que acumula Habitação e outras políticas públicas'
        WHEN 'SUPERINTENDENCIA' THEN N'Superintendência ou órgão equivalente'
        WHEN 'DIRETORIA' THEN N'Diretoria'
        WHEN 'GERENCIA_COORDENACAO' THEN N'Gerência ou Coordenação'
        WHEN 'DEPARTAMENTO_SETOR' THEN N'Departamento ou Setor'
        WHEN 'VINCULADA_OUTRA' THEN N'A política habitacional está vinculada a outra Secretaria'
        WHEN 'SEM_ESTRUTURA' THEN N'Não existe estrutura específica para a área habitacional'
        WHEN 'OUTRA' THEN N'Outra estrutura'
    END AS [6 - Estrutura],
    R.Q6_OUTRA_DESCRICAO AS [6 - Outra estrutura],
    -- Programa habitacional (questão 7)
    CASE R.Q7_PROGRAMA
        WHEN 'PROPRIO' THEN N'Sim (programa próprio)'
        WHEN 'ESTADUAL' THEN N'Somente Pra Ter Onde Morar'
        WHEN 'FEDERAL' THEN N'Somente Minha Casa Minha Vida'
        WHEN 'AMBOS' THEN N'Ambos os programas'
    END AS [7 - Programa],
    R.Q7_PROGRAMA_DETALHES AS [7 - Programa próprio e benefício],
    -- Serviços e infraestrutura (questão 8)
    CASE R.Q8_01 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.1 CMEI ate 1 km],
    CASE R.Q8_02 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.2 Ensino Fundamental I ate 1,5 km],
    CASE R.Q8_03 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.3 Ensino Fundamental II ate 1,5 km],
    CASE R.Q8_04 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.4 Esporte e lazer ate 1,5 km],
    CASE R.Q8_05 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.5 CRAS ou CREAS ate 2 km],
    CASE R.Q8_06 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.6 Posto de Saude ate 2,5 km],
    CASE R.Q8_07 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.7 Comercio cotidiano ate 1 km],
    CASE R.Q8_08 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.8 Comercio e servicos eventuais ate 1,5 km],
    CASE R.Q8_09 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.9 Coleta de materiais reciclaveis],
    CASE R.Q8_10 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.10 Limpeza urbana],
    CASE R.Q8_11 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.11 Asfaltamento],
    CASE R.Q8_12 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.12 Iluminacao publica],
    CASE R.Q8_13 WHEN 1 THEN N'Sim' WHEN 0 THEN N'Não' END AS [8.13 Posto policial]
FROM dbo.Municipios AS M
LEFT JOIN dbo.RespostasMunicipaisV2 AS R ON R.CodigoIBGE = M.CodigoIBGE
ORDER BY M.Nome;
GO

-- A versão anterior, se existir, permanece em dbo.RespostasMunicipais.
