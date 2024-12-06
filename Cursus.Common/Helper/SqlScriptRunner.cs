using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cursus.Common
{
    public class SqlScriptRunner
    {
        private readonly string _connectionString;
        private readonly ILogger<SqlScriptRunner> _logger;

        public SqlScriptRunner(IConfiguration configuration, ILogger<SqlScriptRunner> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task ExecuteAllSqlScriptsAsync(string directoryPath)
        {
            _logger.LogInformation("Starting execution of SQL scripts from directory: {Directory}", directoryPath);

            if (!Directory.Exists(directoryPath))
            {
                _logger.LogError("Directory not found: {Directory}", directoryPath);
                return;
            }

            var sqlFiles = Directory.GetFiles(directoryPath, "*.sql");
            if (sqlFiles.Length == 0)
            {
                _logger.LogWarning("No SQL files found in directory: {Directory}", directoryPath);
                return;
            }

            foreach (var filePath in sqlFiles)
            {
                try
                {
                    var script = await File.ReadAllTextAsync(filePath);
                    _logger.LogInformation("Executing script: {FileName}", Path.GetFileName(filePath));
                    await ExecuteSqlScriptAsync(script);
                    _logger.LogInformation("Successfully executed script: {FileName}", Path.GetFileName(filePath));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing script {FileName}: {Message}", Path.GetFileName(filePath), ex.Message);
                }
            }
        }

        private async Task ExecuteSqlScriptAsync(string script)
        {
            // Tách script thành các câu lệnh SQL
            var statements = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            foreach (var statement in statements)
            {
                if (string.IsNullOrWhiteSpace(statement)) continue;

                try
                {
                    _logger.LogDebug("Executing SQL statement: {Statement}", statement.Substring(0, Math.Min(100, statement.Length)) + "...");

                    await using var command = new SqlCommand(statement, connection)
                    {
                        CommandTimeout = 120 // Tăng thời gian chờ lên 120 giây
                    };
                    await command.ExecuteNonQueryAsync();

                    _logger.LogInformation("Successfully executed SQL statement.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing statement: {Message}", ex.Message);
                }
            }
        }
    }
}
