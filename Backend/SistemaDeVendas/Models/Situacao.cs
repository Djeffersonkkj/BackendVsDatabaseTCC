namespace SistemaDeVendas.Models;

public sealed class Situacao
{
    public byte Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    public ICollection<Vendedor> Vendedores { get; set; } = new List<Vendedor>();
}
