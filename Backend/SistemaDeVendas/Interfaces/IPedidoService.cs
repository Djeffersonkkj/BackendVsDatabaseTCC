using SistemaDeVendas.DTOs;

namespace SistemaDeVendas.Interfaces;

public interface IPedidoService
{
    Task<ExecucaoOperacaoDto<RegistrarPedidoResultadoDto>> RegistrarPedidoAsync(
        RegistrarPedidoDto pedido,
        CancellationToken cancellationToken = default);

    Task<RegistrarPedidoDto> CriarPedidoExemploAsync(CancellationToken cancellationToken = default);
}
