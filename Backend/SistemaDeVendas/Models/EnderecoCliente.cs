namespace SistemaDeVendas.Models;

public sealed class EnderecoCliente
{
    public int Id { get; set; }
    public int IdCliente { get; set; }
    public int IdEndereco { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public Endereco Endereco { get; set; } = null!;
}
