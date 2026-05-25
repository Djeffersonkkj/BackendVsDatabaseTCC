namespace SistemaDeVendas.DTOs;

// Camada DTOs: define objetos leves para transportar dados de consultas sem expor entidades inteiras.
public sealed record ClienteResumoDto(
    int Id,
    string Nome,
    string Email,
    int TotalEnderecos,
    int TotalPedidos);
