using System.Text.Json;
using Microsoft.Data.Sqlite;
using SetwanDokuFoto.Core.Models;
using SetwanDokuFoto.Core.Services;

namespace SetwanDokuFoto.Data;

public class SqliteProjectRepository : IProjectRepository
{
    private readonly string _connectionString;

    public SqliteProjectRepository(string dbPath = "setwan_dokufoto.db")
    {
        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Projects (
                Id TEXT PRIMARY KEY,
                Title TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                JsonData TEXT NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }

    public async Task SaveProjectAsync(DocumentProject project, string filePath)
    {
        var json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Projects (Id, Title, UpdatedAt, JsonData)
            VALUES ($id, $title, $updatedAt, $jsonData)
            ON CONFLICT(Id) DO UPDATE SET
                Title = excluded.Title,
                UpdatedAt = excluded.UpdatedAt,
                JsonData = excluded.JsonData;
        ";
        command.Parameters.AddWithValue("$id", project.Id);
        command.Parameters.AddWithValue("$title", project.Title);
        command.Parameters.AddWithValue("$updatedAt", DateTime.UtcNow.ToString("o"));
        command.Parameters.AddWithValue("$jsonData", json);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<DocumentProject> LoadProjectAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<DocumentProject>(json) ?? new DocumentProject();
    }
}
