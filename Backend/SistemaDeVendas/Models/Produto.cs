using System.ComponentModel.DataAnnotations;

namespace SistemaDeVendas.Models;

public sealed class Produto
{
    public int Id { get; set; }
    public byte IdCategoria { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int Estoque { get; set; }
    public decimal PrecoUnitario { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    public Categoria Categoria { get; set; } = null!;
    public ICollection<PedidoProduto> PedidoProdutos { get; set; } = new List<PedidoProduto>();
}
