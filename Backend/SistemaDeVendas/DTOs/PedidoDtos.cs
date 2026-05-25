namespace SistemaDeVendas.DTOs;

public sealed record PedidoItemDto(
    int IdProduto,
    short Quantidade,
    decimal Desconto);

public sealed record RegistrarPedidoDto(
    byte IdMetodoPagamento,
    int IdCliente,
    int IdVendedor,
    DateTime DataPedido,
    IReadOnlyList<PedidoItemDto> Itens);

public sealed record RegistrarPedidoResultadoDto(
    int IdPedido,
    int RegistrosAfetados,
    decimal TotalPedido,
    decimal ValorComissao,
    string Mensagem);

public sealed record ExecucaoOperacaoDto<T>(
    T Resultado,
    TimeSpan TempoExecucao,
    TimeSpan TempoProcessamento = default,
    TimeSpan TempoPersistencia = default);

public sealed record ComparacaoTempoDto(
    string Operacao,
    TimeSpan TempoBackend,
    TimeSpan TempoProcedure,
    int RegistrosBackend,
    int RegistrosProcedure)
{
    public double DiferencaPercentual
    {
        get
        {
            if (TempoBackend.TotalMilliseconds <= 0)
            {
                return 0;
            }

            return ((TempoProcedure.TotalMilliseconds - TempoBackend.TotalMilliseconds) / TempoBackend.TotalMilliseconds) * 100;
        }
    }

    public string AnaliseTextual
    {
        get
        {
            if (Math.Abs(DiferencaPercentual) < 5)
            {
                return "Os tempos ficaram muito proximos; a diferenca pode estar dentro da variacao normal de execucao.";
            }

            return DiferencaPercentual < 0
                ? "A Stored Procedure foi mais rapida nesta execucao, concentrando trabalho e trafego no SQL Server."
                : "A regra no backend foi mais rapida nesta execucao, mantendo a logica em C# e reduzindo acoplamento ao banco.";
        }
    }
}
