using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using TurnosMedicos.Data;
using TurnosMedicos.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=turnos.db"));
builder.Services.Configure<NoShowPenaltySettings>(builder.Configuration.GetSection("NoShowPenalty"));
builder.Services.AddScoped<INoShowPenaltyEvaluator, NoShowPenaltyEvaluator>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();
const string LegacyBaselineMigrationId = "20260519144134_InitialSchemaNoShow";

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupMigrations");
    var historyRepository = db.GetService<IHistoryRepository>();
    var migrations = db.Database.GetMigrations().ToList();

    if (!migrations.Contains(LegacyBaselineMigrationId))
        throw new InvalidOperationException($"No se encontró la migración baseline requerida: {LegacyBaselineMigrationId}.");

    if (historyRepository.Exists())
    {
        db.Database.Migrate();
    }
    else
    {
        var dbConnection = db.Database.GetDbConnection();
        dbConnection.Open();

        var hasLegacySchema =
            TableExists(dbConnection, "Pacientes") &&
            TableExists(dbConnection, "Turnos") &&
            TableExists(dbConnection, "Medicos") &&
            TableExists(dbConnection, "Sucursales");

        if (!hasLegacySchema)
        {
            logger.LogInformation("Base sin esquema legado detectada. Se aplica flujo normal de migraciones.");
            dbConnection.Close();
            db.Database.Migrate();
        }
        else if (IsLegacySchemaCompatible(dbConnection))
        {
            logger.LogWarning("Esquema legado compatible detectado sin historial EF. Se registrará baseline y se continuarán migraciones.");
            RegisterLegacyBaselineHistory(db, historyRepository, LegacyBaselineMigrationId);
            dbConnection.Close();
            db.Database.Migrate();
        }
        else
        {
            dbConnection.Close();
            logger.LogError("Esquema legado inconsistente detectado. Se aborta arranque para evitar drift de migraciones.");
            throw new InvalidOperationException("Esquema legado inconsistente: no es seguro registrar baseline automáticamente. Revise columnas/tablas requeridas por baseline.");
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.MapControllers();
app.Run();

static bool TableExists(System.Data.Common.DbConnection connection, string tableName)
{
    using var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name = @name;";
    var parameter = cmd.CreateParameter();
    parameter.ParameterName = "@name";
    parameter.Value = tableName;
    cmd.Parameters.Add(parameter);

    var result = cmd.ExecuteScalar();
    return Convert.ToInt32(result) > 0;
}

static bool TableHasColumns(System.Data.Common.DbConnection connection, string tableName, params string[] columns)
{
    var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    using var cmd = connection.CreateCommand();
    cmd.CommandText = $"PRAGMA table_info(\"{tableName}\");";
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        existing.Add(reader.GetString(1));
    }

    return columns.All(existing.Contains);
}

static bool IsLegacySchemaCompatible(System.Data.Common.DbConnection connection)
{
    // Flexibilidad: no exigir columnas legadas removidas (Bloqueado/NoShowCount),
    // pero sí exigir las columnas activas requeridas por la versión actual.
    return
        TableHasColumns(connection, "Pacientes", "Id", "NombreCompleto", "DNI", "Email", "Telefono", "FechaBloqueo", "createdAt", "isActive") &&
        TableHasColumns(connection, "Turnos", "Id", "PacienteId", "MedicoId", "FechaHora", "Estado", "FechaCreacion", "Motivo", "AusenciaPenalizada") &&
        TableHasColumns(connection, "Medicos", "Id", "NombreCompleto", "Especialidad", "SucursalId") &&
        TableHasColumns(connection, "Sucursales", "Id", "Nombre", "Direccion");
}

static void RegisterLegacyBaselineHistory(AppDbContext db, IHistoryRepository historyRepository, string baselineMigrationId)
{
    db.Database.ExecuteSqlRaw(historyRepository.GetCreateScript());
    var efVersion = typeof(DbContext).Assembly.GetName().Version?.ToString() ?? "9.0.0";

    db.Database.ExecuteSqlRaw(
        "INSERT OR IGNORE INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ({0}, {1});",
        baselineMigrationId,
        efVersion);
}
