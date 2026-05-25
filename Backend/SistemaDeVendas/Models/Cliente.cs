namespace SistemaDeVendas.Models;

public sealed class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;

    public ICollection<EnderecoCliente> EnderecosClientes { get; set; } = new List<EnderecoCliente>();
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
