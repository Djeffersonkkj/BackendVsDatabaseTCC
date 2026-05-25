namespace SistemaDeVendas.Models;

public sealed class Categoria
{
    public byte Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
