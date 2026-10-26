namespace MunicipioPortal.Services;

/// <summary>
/// Erros exibidos sem revelar servidor, usuário, senha, texto SQL ou detalhes internos.
/// Informações técnicas completas são registradas somente no logger do servidor.
/// </summary>
public static class SqlServerFailure
{
    public static (string Codigo, string Mensagem) Classificar(int numero)
        => numero switch
        {
            2601 or 2627 => ("BD-DUPLICIDADE", "Este município já possui uma resposta registrada."),
            18456 or 4060 or 18452 => ("BD-ACESSO",
                "O SQL Server recusou o acesso ao banco de dados. A equipe de TI deve verificar as credenciais e o banco configurado."),
            208 or 207 => ("BD-ESTRUTURA",
                "As tabelas ou colunas do formulário não foram encontradas no SQL Server. É necessário conferir a instalação do banco."),
            229 or 230 => ("BD-PERMISSAO",
                "A conta da aplicação não tem permissão suficiente para gravar no SQL Server."),
            -2 => ("BD-TEMPO",
                "A conexão com o SQL Server excedeu o tempo limite."),
            26 or 40 or 53 or 11001 or 10060 or 10061 => ("BD-CONEXAO",
                "Não foi possível estabelecer conexão com o SQL Server. Confira rede, servidor e porta."),
            _ => ("BD-OPERACAO",
                "O SQL Server não conseguiu registrar os dados. A equipe de TI deve conferir os logs do servidor.")
        };
}
