namespace SistemaDeVendas.Models;

public sealed class MetodoPagamento
{
    public byte Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
