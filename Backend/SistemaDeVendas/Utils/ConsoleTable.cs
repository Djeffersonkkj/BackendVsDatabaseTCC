using SistemaDeVendas.DTOs;

namespace SistemaDeVendas.Utils;

// Camada Utils: reúne auxiliares pequenos e sem regra de negócio para uso pela aplicação console.
public static class ConsoleTable
{
    public static void ImprimirClientes(IEnumerable<ClienteResumoDto> clientes)
    {
        Console.WriteLine("Clientes cadastrados");
        Console.WriteLine("Id | Nome | Email | Enderecos | Pedidos");

        foreach (var cliente in clientes)
        {
            Console.WriteLine($"{cliente.Id} | {cliente.Nome} | {cliente.Email} | {cliente.TotalEnderecos} | {cliente.TotalPedidos}");
        }
    }

    public static void ImprimirProdutos(IEnumerable<ProdutoResumoDto> produtos)
    {
        Console.WriteLine("Produtos cadastrados");
        Console.WriteLine("Id | Nome | Categoria | Estoque | Preco");

        foreach (var produto in produtos)
        {
            Console.WriteLine($"{produto.Id} | {produto.Nome} | {produto.Categoria} | {produto.Estoque} | {produto.PrecoUnitario:C}");
        }
    }
}
