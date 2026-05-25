namespace SistemaDeVendas.Models;

public sealed class Vendedor
{
    public int Id { get; set; }
    public byte IdSituacao { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public decimal Comissao { get; set; }

    public Situacao Situacao { get; set; } = null!;
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
