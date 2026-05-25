using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SistemaDeVendas.Data;
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

builder.Services.AddDbContext<SistemaDeVendasDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var clienteService = scope.ServiceProvider.GetRequiredService<IClienteService>();
var produtoService = scope.ServiceProvider.GetRequiredService<IProdutoService>();
var relatorioService = scope.ServiceProvider.GetRequiredService<IRelatorioService>();

try
{
    var clientes = await clienteService.ListarClientesAsync();
    ConsoleTable.ImprimirClientes(clientes);

    Console.WriteLine();

    var produtos = await produtoService.ListarProdutosAsync();
    ConsoleTable.ImprimirProdutos(produtos.Take(10));

    Console.WriteLine();

    var dataInicio = new DateTime(2024, 1, 1);
    var dataFim = new DateTime(2027, 1, 1);

    var vendasPorPeriodo = await relatorioService.ObterVendasPorPeriodoAsync(dataInicio, dataFim);
    ConsoleTable.ImprimirResumoRelatorio("Relatorio de vendas por periodo", vendasPorPeriodo);
    ConsoleTable.ImprimirVendasPorPeriodo(vendasPorPeriodo.Registros);

    Console.WriteLine();

    var rankingVendedores = await relatorioService.ObterRankingVendedoresAsync(dataInicio, dataFim, pagina: 1, tamanhoPagina: 10);
    ConsoleTable.ImprimirResumoRelatorio("Ranking de vendedores", rankingVendedores);
    ConsoleTable.ImprimirRankingVendedores(rankingVendedores.Registros);

    Console.WriteLine();

    var produtosMaisVendidos = await relatorioService.ObterProdutosMaisVendidosAsync(dataInicio, dataFim, pagina: 1, tamanhoPagina: 10);
    ConsoleTable.ImprimirResumoRelatorio("Produtos mais vendidos", produtosMaisVendidos);
    ConsoleTable.ImprimirProdutosMaisVendidos(produtosMaisVendidos.Registros);

    Console.WriteLine();

    var clientesQueMaisCompraram = await relatorioService.ObterClientesQueMaisCompraramAsync(dataInicio, dataFim, pagina: 1, tamanhoPagina: 10);
    ConsoleTable.ImprimirResumoRelatorio("Clientes que mais compraram", clientesQueMaisCompraram);
    ConsoleTable.ImprimirClientesQueMaisCompraram(clientesQueMaisCompraram.Registros);

    Console.WriteLine();

    var relatorioConsolidado = await relatorioService.ObterRelatorioConsolidadoAsync(dataInicio, dataFim, pagina: 1, tamanhoPagina: 20);
    ConsoleTable.ImprimirResumoRelatorio("Relatorio geral consolidado", relatorioConsolidado);
    ConsoleTable.ImprimirRelatorioConsolidado(relatorioConsolidado.Registros);
}
catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException or Microsoft.Data.SqlClient.SqlException)
{
    Console.WriteLine("Nao foi possivel consultar o banco de dados.");
    Console.WriteLine("Verifique se o SQL Server esta acessivel e se a connection string em appsettings.json esta correta.");
    Console.WriteLine($"Detalhe: {ex.Message}");
}
