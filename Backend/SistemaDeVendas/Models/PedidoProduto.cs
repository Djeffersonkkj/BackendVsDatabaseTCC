namespace SistemaDeVendas.Models;

public sealed class PedidoProduto
{
    public int Id { get; set; }
    public int IdProduto { get; set; }
    public int IdPedido { get; set; }
    public short Quantidade { get; set; }
    public decimal Desconto { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal SubTotal { get; set; }

    public Produto Produto { get; set; } = null!;
    public Pedido Pedido { get; set; } = null!;
}
