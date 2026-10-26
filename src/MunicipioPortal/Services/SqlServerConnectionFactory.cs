using Microsoft.Data.SqlClient;

namespace MunicipioPortal.Services;

public sealed class SqlServerNaoConfiguradoException : InvalidOperationException
{
    public SqlServerNaoConfiguradoException()
        : base("ConnectionStrings:Municipios não foi configurada no ambiente.") { }
}

public static class SqlServerConnectionFactory
{
    public static SqlConnection Criar(IConfiguration configuracao)
    {
        string? connectionString = configuracao.GetConnectionString("Municipios");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new SqlServerNaoConfiguradoException();

        // Credenciais nunca são incluídas nas mensagens de resposta.
        return new SqlConnection(connectionString);
    }
}
