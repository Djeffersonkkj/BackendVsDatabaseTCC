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

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var clienteService = scope.ServiceProvider.GetRequiredService<IClienteService>();
var produtoService = scope.ServiceProvider.GetRequiredService<IProdutoService>();

try
{
    var clientes = await clienteService.ListarClientesAsync();
    ConsoleTable.ImprimirClientes(clientes);

    Console.WriteLine();

    var produtos = await produtoService.ListarProdutosAsync();
    ConsoleTable.ImprimirProdutos(produtos.Take(10));
}
catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException or Microsoft.Data.SqlClient.SqlException)
{
    Console.WriteLine("Nao foi possivel consultar o banco de dados.");
    Console.WriteLine("Verifique se o SQL Server esta acessivel e se a connection string em appsettings.json esta correta.");
    Console.WriteLine($"Detalhe: {ex.Message}");
}
