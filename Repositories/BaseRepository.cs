using Dapper;
using Nconnect.Settings;
using Npgsql;
// using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Nconnect.Repositories;

public class BaseRepository
{
    protected readonly IConfiguration _configuration;
    private readonly string ConnectionString;

    protected BaseRepository(IConfiguration config)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        _configuration = config;
        this.ConnectionString = _configuration.GetSection(nameof(PostgresSettings)).Get<PostgresSettings>()!.ConnectionString;
    }

    public NpgsqlConnection NewConnection => new NpgsqlConnection(ConnectionString);

    // public enum LegacySchema { SCHOOLNEW, COLLEGENEW, PARENTLOGIN, MASTERS, PAYROLLNEW, STUDENTINFO, STUDENTATT, STUDENTEXAM, STUDENTFEE }
    // public async Task<OracleConnection> LegacyConnection(LegacySchema? schema = null)
    // {
    //     var con = new OracleConnection(_configuration
    //     .GetSection(nameof(OracleSettings)).Get<OracleSettings>()!.ConnectionString);

    //     if (con.State == ConnectionState.Closed)
    //         con.Open();

    //     if (schema.HasValue)
    //     {
    //         if (schema == LegacySchema.SCHOOLNEW)
    //             await con.QueryAsync($@"ALTER SESSION SET CURRENT_SCHEMA = SCHOOLNEW");
    //         else if (schema == LegacySchema.COLLEGENEW)
    //             await con.QueryAsync($@"ALTER SESSION SET CURRENT_SCHEMA = COLLEGENEW");
    //         else if (schema == LegacySchema.MASTERS)
    //             await con.QueryAsync($@"ALTER SESSION SET CURRENT_SCHEMA = MASTERS");
    //         else if (schema == LegacySchema.PAYROLLNEW)
    //             await con.QueryAsync($@"ALTER SESSION SET CURRENT_SCHEMA = PAYROLLNEW");
    //         else if (schema == LegacySchema.STUDENTATT)
    //             await con.QueryAsync($@"ALTER SESSION SET CURRENT_SCHEMA = STUDENTATT");
    //         else if (schema == LegacySchema.STUDENTEXAM)
    //             await con.QueryAsync($@"ALTER SESSION SET CURRENT_SCHEMA = STUDENTEXAM");
    //         else
    //             await con.QueryAsync($@"ALTER SESSION SET CURRENT_SCHEMA = PARENTLOGIN");
    //     }

    //     return con;
    // }

    public async Task<T> RunInTransaction<T>(Func<NpgsqlConnection, NpgsqlTransaction, Task<T>> Run, ILogger logger = null)
    {
        using (var con = NewConnection)
        {
            await con.OpenAsync();

            using (var tran = await con.BeginTransactionAsync())
            {
                try
                {
                    var res = await Run(con, tran);
                    await tran.CommitAsync();
                    return res;
                }
                catch (Exception e)
                {
                    if (logger is not null)
                    {
                        logger.LogError(e, "Rolling back transaction:");
                    }
                    else
                    {
                        Console.WriteLine("Rolling back transaction:");
                        Console.WriteLine(e);
                    }

                    await tran.RollbackAsync();
                    throw;
                }
                finally
                {
                    await con.CloseAsync();
                }
            }
        }
    }
}
