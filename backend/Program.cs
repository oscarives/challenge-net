using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var historyRepository = db.GetService<IHistoryRepository>();
    var dbConnection = db.Database.GetDbConnection();
    dbConnection.Open();

    var hasLegacySchema =
        TableExists(dbConnection, "Pacientes") &&
        TableExists(dbConnection, "Turnos") &&
        TableExists(dbConnection, "Medicos") &&
        TableExists(dbConnection, "Sucursales");

    if (hasLegacySchema && !historyRepository.Exists())
    {
        db.Database.ExecuteSqlRaw(historyRepository.GetCreateScript());
        var efVersion = typeof(DbContext).Assembly.GetName().Version?.ToString() ?? "9.0.0";

        foreach (var migrationId in db.Database.GetMigrations())
        {
            db.Database.ExecuteSqlRaw(
                "INSERT OR IGNORE INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ({0}, {1});",
                migrationId,
                efVersion);
        }
    }

    db.Database.Migrate();
    dbConnection.Close();
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
