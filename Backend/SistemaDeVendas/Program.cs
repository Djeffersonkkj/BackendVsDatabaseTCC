using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SistemaDeVendas.Data;
using SistemaDeVendas.Interfaces;
using SistemaDeVendas.Repositories;
using SistemaDeVendas.Services;
using SistemaDeVendas.Utils;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("SistemaDeVendas")
    ?? throw new InvalidOperationException("Connection string 'SistemaDeVendas' nao configurada.");

builder.Services.AddSingleton<EfSqlCaptureInterceptor>();
builder.Services.AddDbContext<SistemaDeVendasDbContext>((serviceProvider, options) =>
    options
        .UseSqlServer(connectionString)
        .AddInterceptors(serviceProvider.GetRequiredService<EfSqlCaptureInterceptor>()));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IProcedureService, ProcedureService>();
builder.Services.AddScoped<IBenchmarkService, BenchmarkService>();
builder.Services.AddScoped<ISqlComparacaoService, SqlComparacaoService>();

using var host = builder.Build();

while (true)
{
    Console.WriteLine();
    Console.WriteLine("Comparacao Backend x Stored Procedure");
    Console.WriteLine("[1] Executar regra via backend");
    Console.WriteLine("[2] Executar regra via procedure");
    Console.WriteLine("[3] Executar benchmark reproduzivel");
    Console.WriteLine("[4] Salvar SQL gerado pelo EF Core");
    Console.WriteLine("[0] Sair");
    Console.Write("Escolha: ");

    var opcao = Console.ReadLine();
    if (opcao == "0")
    {
        Console.Clear();
        Console.WriteLine("Sistema finalizado.");
        break;
    }

    try
    {
        await using var scope = host.Services.CreateAsyncScope();
        var pedidoService = scope.ServiceProvider.GetRequiredService<IPedidoService>();
        var relatorioService = scope.ServiceProvider.GetRequiredService<IRelatorioService>();
        var procedureService = scope.ServiceProvider.GetRequiredService<IProcedureService>();
        var benchmarkService = scope.ServiceProvider.GetRequiredService<IBenchmarkService>();
        var sqlComparacaoService = scope.ServiceProvider.GetRequiredService<ISqlComparacaoService>();

        var dataInicio = new DateTime(2024, 1, 1);
        var dataFim = new DateTime(2027, 1, 1);

        switch (opcao)
        {
            case "1":
                await ExecutarBackendAsync(pedidoService, relatorioService, dataInicio, dataFim);
                break;
            case "2":
                await ExecutarProcedureAsync(pedidoService, procedureService, dataInicio, dataFim);
                break;
            case "3":
                ConsoleTable.ImprimirBenchmark(await benchmarkService.ExecutarComparacaoAsync(execucoes: 10));
                break;
            case "4":
                var path = await sqlComparacaoService.SalvarSqlEfCoreAsync(dataInicio, dataFim);
                Console.WriteLine($"SQL do EF Core salvo em: {path}");
                Console.WriteLine("A captura em tempo de execucao tambem registra SQL, parametros e duracao em SqlCapturado.");
                break;
            default:
                Console.WriteLine("Opcao invalida.");
                break;
        }
    }
    catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException or Microsoft.Data.SqlClient.SqlException)
    {
        Console.WriteLine("Nao foi possivel executar a operacao no banco de dados.");
        Console.WriteLine("Verifique se o SQL Server esta acessivel, se a connection string esta correta e se as procedures foram criadas.");
        Console.WriteLine($"Detalhe: {ex.Message}");
    }
}

static async Task ExecutarBackendAsync(
    IPedidoService pedidoService,
    IRelatorioService relatorioService,
    DateTime dataInicio,
    DateTime dataFim)
{
    Console.WriteLine();
    Console.WriteLine("Execucao via backend/EF Core");

    var pedido = await pedidoService.CriarPedidoExemploAsync();
    var resultadoPedido = await pedidoService.RegistrarPedidoAsync(pedido);
    ConsoleTable.ImprimirResultadoPedido("RegistrarPedido via backend", resultadoPedido);

    var consolidado = await relatorioService.ObterRelatorioConsolidadoAsync(dataInicio, dataFim, pagina: 1, tamanhoPagina: 20);
    ConsoleTable.ImprimirResumoRelatorio("Relatorio consolidado via backend", consolidado);
    ConsoleTable.ImprimirRelatorioConsolidado(consolidado.Registros);
}

static async Task ExecutarProcedureAsync(
    IPedidoService pedidoService,
    IProcedureService procedureService,
    DateTime dataInicio,
    DateTime dataFim)
{
    Console.WriteLine();
    Console.WriteLine("Execucao via Stored Procedure");

    var pedido = await pedidoService.CriarPedidoExemploAsync();
    var resultadoPedido = await procedureService.RegistrarPedidoAsync(pedido);
    ConsoleTable.ImprimirResultadoPedido("RegistrarPedido via procedure", resultadoPedido);

    var consolidado = await procedureService.ObterRelatorioConsolidadoAsync(dataInicio, dataFim, pagina: 1, tamanhoPagina: 20);
    ConsoleTable.ImprimirResumoRelatorio("Relatorio consolidado via procedure", consolidado);
    ConsoleTable.ImprimirRelatorioConsolidado(consolidado.Registros);
}
