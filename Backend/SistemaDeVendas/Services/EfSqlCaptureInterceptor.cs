using System.Data.Common;
using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SistemaDeVendas.Services;

public sealed class EfSqlCaptureInterceptor : DbCommandInterceptor
{
    private readonly string outputDirectory = Path.Combine(AppContext.BaseDirectory, "SqlCapturado");

    public bool SalvarEmArquivo { get; set; } = true;

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        await RegistrarAsync(command, eventData, cancellationToken);
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await RegistrarAsync(command, eventData, cancellationToken);
        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        await RegistrarAsync(command, eventData, cancellationToken);
        return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private async Task RegistrarAsync(DbCommand command, CommandExecutedEventData eventData, CancellationToken cancellationToken)
    {
        if (!SalvarEmArquivo)
        {
            return;
        }

        Directory.CreateDirectory(outputDirectory);

        var builder = new StringBuilder();
        builder.AppendLine($"-- EF Core SQL capturado em {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        builder.AppendLine($"-- CommandId: {eventData.CommandId}");
        builder.AppendLine($"-- Tempo ate envio ao banco: {eventData.Duration.TotalMilliseconds:N2} ms");
        builder.AppendLine("-- Parametros:");

        foreach (DbParameter parameter in command.Parameters)
        {
            builder.AppendLine($"-- {parameter.ParameterName} = {parameter.Value} ({parameter.DbType})");
        }

        builder.AppendLine();
        builder.AppendLine(command.CommandText);
        builder.AppendLine();
        builder.AppendLine("GO");

        var fileName = $"ef-sql-{DateTime.Now:yyyyMMdd-HHmmss-fff}-{Process.GetCurrentProcess().Id}.sql";
        await File.AppendAllTextAsync(Path.Combine(outputDirectory, fileName), builder.ToString(), cancellationToken);
    }
}
