using SistemaDeVendas.DTOs;

namespace SistemaDeVendas.Utils;

// Camada Utils: reúne auxiliares pequenos e sem regra de negócio para uso pela aplicação console.
public static class ConsoleTable
{
    public static void ImprimirClientes(IEnumerable<ClienteResumoDto> clientes)
    {
        Console.WriteLine("Clientes cadastrados");
        Console.WriteLine("Id | Nome | Email | Enderecos | Pedidos");

        foreach (var cliente in clientes)
        {
            Console.WriteLine($"{cliente.Id} | {cliente.Nome} | {cliente.Email} | {cliente.TotalEnderecos} | {cliente.TotalPedidos}");
        }
    }

    public static void ImprimirProdutos(IEnumerable<ProdutoResumoDto> produtos)
    {
        Console.WriteLine("Produtos cadastrados");
        Console.WriteLine("Id | Nome | Categoria | Estoque | Preco");

        foreach (var produto in produtos)
        {
            Console.WriteLine($"{produto.Id} | {produto.Nome} | {produto.Categoria} | {produto.Estoque} | {produto.PrecoUnitario:C}");
        }
    }

    public static void ImprimirResumoRelatorio<T>(string titulo, RelatorioExecucaoDto<T> relatorio)
    {
        Console.WriteLine(titulo);
        Console.WriteLine($"Tempo da consulta: {relatorio.TempoConsulta.TotalMilliseconds:N2} ms");
        Console.WriteLine($"Registros retornados: {relatorio.QuantidadeRegistros}");
        Console.WriteLine($"Memoria utilizada: {relatorio.MemoriaUtilizadaBytes / 1024.0:N2} KB");
    }

    public static void ImprimirResultadoPedido(string titulo, ExecucaoOperacaoDto<RegistrarPedidoResultadoDto> execucao)
    {
        Console.WriteLine(titulo);
        Console.WriteLine($"Tempo de execucao: {execucao.TempoExecucao.TotalMilliseconds:N2} ms");
        Console.WriteLine($"Tempo processamento/regra: {execucao.TempoProcessamento.TotalMilliseconds:N2} ms");
        Console.WriteLine($"Tempo persistencia/banco: {execucao.TempoPersistencia.TotalMilliseconds:N2} ms");
        Console.WriteLine($"Id pedido: {execucao.Resultado.IdPedido}");
        Console.WriteLine($"Registros afetados: {execucao.Resultado.RegistrosAfetados}");
        Console.WriteLine($"Total pedido: {execucao.Resultado.TotalPedido:C}");
        Console.WriteLine($"Comissao: {execucao.Resultado.ValorComissao:C}");
        Console.WriteLine($"Mensagem: {execucao.Resultado.Mensagem}");
    }

    public static void ImprimirComparacao(ComparacaoTempoDto comparacao)
    {
        Console.WriteLine();
        Console.WriteLine(comparacao.Operacao);
        Console.WriteLine($"Tempo backend: {comparacao.TempoBackend.TotalMilliseconds:N2} ms");
        Console.WriteLine($"Tempo procedure: {comparacao.TempoProcedure.TotalMilliseconds:N2} ms");
        Console.WriteLine($"Quantidade/registros backend: {comparacao.RegistrosBackend}");
        Console.WriteLine($"Quantidade/registros procedure: {comparacao.RegistrosProcedure}");
        Console.WriteLine($"Diferenca percentual: {comparacao.DiferencaPercentual:N2}%");
        Console.WriteLine($"Analise: {comparacao.AnaliseTextual}");
    }

    public static void ImprimirBenchmark(BenchmarkResultadoDto resultado)
    {
        Console.WriteLine();
        Console.WriteLine("Benchmark reproduzivel");
        Console.WriteLine($"Execucoes medidas: {resultado.Cenario.Execucoes}");
        Console.WriteLine($"Tempo conexao backend: {resultado.TempoConexaoBackend.TotalMilliseconds:N2} ms");
        Console.WriteLine($"Tempo conexao procedure: {resultado.TempoConexaoProcedure.TotalMilliseconds:N2} ms");
        Console.WriteLine($"Estrategia: {resultado.EstrategiaReproducibilidade}");

        foreach (var operacao in resultado.Operacoes)
        {
            Console.WriteLine();
            Console.WriteLine(operacao.Operacao);
            Console.WriteLine($"Equivalencia de resultado: {(operacao.ResultadosEquivalentes ? "sim" : "nao")}");
            Console.WriteLine($"Registros backend/procedure: {operacao.RegistrosBackend}/{operacao.RegistrosProcedure}");
            Console.WriteLine($"Media backend: {operacao.Backend.MediaMs:N2} ms");
            Console.WriteLine($"Media procedure: {operacao.Procedure.MediaMs:N2} ms");
            Console.WriteLine($"Mediana backend/procedure: {operacao.Backend.MedianaMs:N2}/{operacao.Procedure.MedianaMs:N2} ms");
            Console.WriteLine($"Min backend/procedure: {operacao.Backend.MinimoMs:N2}/{operacao.Procedure.MinimoMs:N2} ms");
            Console.WriteLine($"Max backend/procedure: {operacao.Backend.MaximoMs:N2}/{operacao.Procedure.MaximoMs:N2} ms");
            Console.WriteLine($"Desvio padrao backend/procedure: {operacao.Backend.DesvioPadraoMs:N2}/{operacao.Procedure.DesvioPadraoMs:N2} ms");
            Console.WriteLine($"Diferenca percentual media: {operacao.DiferencaPercentual:N2}%");

            if (operacao.ProcessamentoBackend is not null && operacao.PersistenciaBackend is not null
                && operacao.ProcessamentoProcedure is not null && operacao.PersistenciaProcedure is not null)
            {
                Console.WriteLine($"Processamento medio backend/procedure: {operacao.ProcessamentoBackend.MediaMs:N2}/{operacao.ProcessamentoProcedure.MediaMs:N2} ms");
                Console.WriteLine($"Persistencia medio backend/procedure: {operacao.PersistenciaBackend.MediaMs:N2}/{operacao.PersistenciaProcedure.MediaMs:N2} ms");
            }
        }
    }

    public static void ImprimirVendasPorPeriodo(IEnumerable<VendasPorPeriodoDto> registros)
    {
        Console.WriteLine("Ano | Mes | Total vendido | Pedidos | Ticket medio | Comissao");

        foreach (var registro in registros)
        {
            Console.WriteLine($"{registro.Ano} | {registro.Mes:00} | {registro.TotalVendido:C} | {registro.QuantidadePedidos} | {registro.TicketMedio:C} | {registro.TotalComissao:C}");
        }
    }

    public static void ImprimirRankingVendedores(IEnumerable<RankingVendedorDto> registros)
    {
        Console.WriteLine("Id | Vendedor | Total vendido | Pedidos | Comissao");

        foreach (var registro in registros)
        {
            Console.WriteLine($"{registro.IdVendedor} | {registro.Vendedor} | {registro.TotalVendido:C} | {registro.QuantidadePedidos} | {registro.ComissaoTotal:C}");
        }
    }

    public static void ImprimirProdutosMaisVendidos(IEnumerable<ProdutoMaisVendidoDto> registros)
    {
        Console.WriteLine("Id | Produto | Categoria | Quantidade | Faturamento | Desconto medio");

        foreach (var registro in registros)
        {
            Console.WriteLine($"{registro.IdProduto} | {registro.Produto} | {registro.Categoria} | {registro.QuantidadeVendida} | {registro.Faturamento:C} | {registro.MediaDesconto:N2}%");
        }
    }

    public static void ImprimirClientesQueMaisCompraram(IEnumerable<ClienteMaisComprouDto> registros)
    {
        Console.WriteLine("Id | Cliente | Total gasto | Pedidos | Ticket medio");

        foreach (var registro in registros)
        {
            Console.WriteLine($"{registro.IdCliente} | {registro.Cliente} | {registro.TotalGasto:C} | {registro.QuantidadePedidos} | {registro.TicketMedio:C}");
        }
    }

    public static void ImprimirRelatorioConsolidado(IEnumerable<RelatorioConsolidadoDto> registros)
    {
        Console.WriteLine("Categoria | Metodo | Vendedor | Pedidos | Itens | Total vendido | Comissao | Desconto medio");

        foreach (var registro in registros)
        {
            Console.WriteLine($"{registro.Categoria} | {registro.MetodoPagamento} | {registro.Vendedor} | {registro.QuantidadePedidos} | {registro.QuantidadeItensVendidos} | {registro.TotalVendido:C} | {registro.TotalComissao:C} | {registro.MediaDesconto:N2}%");
        }
    }
}
