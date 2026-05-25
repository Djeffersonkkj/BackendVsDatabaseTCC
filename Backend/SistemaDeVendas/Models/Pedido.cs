namespace SistemaDeVendas.Models;

public sealed class Pedido
{
    public int Id { get; set; }
    public byte IdMetodoPagamento { get; set; }
    public int IdCliente { get; set; }
    public int IdVendedor { get; set; }
    public DateTime DataPedido { get; set; }
    public decimal TotalPedido { get; set; }
    public decimal ValorComissao { get; set; }

    public MetodoPagamento MetodoPagamento { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public Vendedor Vendedor { get; set; } = null!;
    public ICollection<PedidoProduto> PedidoProdutos { get; set; } = new List<PedidoProduto>();
}
