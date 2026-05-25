namespace SistemaDeVendas.DTOs;

public sealed record ProdutoResumoDto(
    int Id,
    string Nome,
    string Categoria,
    int Estoque,
    decimal PrecoUnitario);
