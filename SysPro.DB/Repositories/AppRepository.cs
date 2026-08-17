using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SysPro.Application.Interfaces;
using SysPro.DB.Persistence;
using SysPro.Domain.Models;

namespace SysPro.DB.Repositories;

public class AppRepository : IAppRepository
{
    readonly AppDbContext _dbContext;
    
    public AppRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> InsertAudit(ImportAuditViewModel model)
    {
        var connection = (SqlConnection)_dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();
        try
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "App.InsertImportAudit";
            cmd.Parameters.Add(new SqlParameter("@FileName",   SqlDbType.NVarChar, -1) { Value = model.FileName });
            cmd.Parameters.Add(new SqlParameter("@Considered", SqlDbType.Int)          { Value = model.Considered });
            cmd.Parameters.Add(new SqlParameter("@Applied",    SqlDbType.Int)          { Value = model.Applied });
            cmd.Parameters.Add(new SqlParameter("@Invalid",    SqlDbType.Int)          { Value = model.Invalid });
            cmd.Parameters.Add(new SqlParameter("@ProcessedUtc", SqlDbType.DateTime2)
                { Value = model.ProcessedUtc == default ? DBNull.Value : model.ProcessedUtc });

            var newId = await cmd.ExecuteScalarAsync();
            return newId is Guid id && id != Guid.Empty;
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }
}