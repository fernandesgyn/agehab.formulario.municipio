using Microsoft.Data.SqlClient;

namespace MunicipioPortal.Services;

public sealed class SqlServerNaoConfiguradoException() :
    InvalidOperationException("ConnectionStrings:Municipios não foi configurada no ambiente.");

public static class SqlServerConnectionFactory
{
    public static SqlConnection Criar(IConfiguration configuracao)
    {
        string? connectionString = configuracao.GetConnectionString("Municipios");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new SqlServerNaoConfiguradoException();

        // A senha jamais é registrada em logs, telas ou arquivos do repositório.
        return new SqlConnection(connectionString);
    }
}
