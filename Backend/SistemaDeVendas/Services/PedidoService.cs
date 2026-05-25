using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SistemaDeVendas.BusinessRules;
using SistemaDeVendas.Data;
using SistemaDeVendas.DTOs;
using SistemaDeVendas.Interfaces;
using SistemaDeVendas.Models;

namespace SistemaDeVendas.Services;

public sealed class PedidoService(SistemaDeVendasDbContext context) : IPedidoService
{
    public async Task<ExecucaoOperacaoDto<RegistrarPedidoResultadoDto>> RegistrarPedidoAsync(
        RegistrarPedidoDto pedido,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var tempoProcessamento = Stopwatch.StartNew();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            PedidoBusinessRules.ValidarPedido(pedido);

            var clienteExiste = await context.Clientes.AnyAsync(cliente => cliente.Id == pedido.IdCliente, cancellationToken);
            if (!clienteExiste)
            {
                throw new InvalidOperationException(PedidoBusinessRules.MensagemClienteNaoExiste);
            }

            var metodoPagamentoExiste = await context.MetodosPagamento
                .AnyAsync(metodo => metodo.Id == pedido.IdMetodoPagamento, cancellationToken);
            if (!metodoPagamentoExiste)
            {
                throw new InvalidOperationException(PedidoBusinessRules.MensagemMetodoPagamentoNaoExiste);
            }

            var vendedor = await context.Vendedores
                .SingleOrDefaultAsync(vendedor => vendedor.Id == pedido.IdVendedor, cancellationToken)
                ?? throw new InvalidOperationException(PedidoBusinessRules.MensagemVendedorNaoExiste);

            var idsProdutos = pedido.Itens.Select(item => item.IdProduto).Distinct().ToList();
            var produtos = await context.Produtos
                .Where(produto => idsProdutos.Contains(produto.Id))
                .ToDictionaryAsync(produto => produto.Id, cancellationToken);

            if (produtos.Count != idsProdutos.Count)
            {
                throw new InvalidOperationException(PedidoBusinessRules.MensagemProdutosNaoExistem);
            }

            var novoPedido = new Pedido
            {
                IdMetodoPagamento = pedido.IdMetodoPagamento,
                IdCliente = pedido.IdCliente,
                IdVendedor = pedido.IdVendedor,
                DataPedido = pedido.DataPedido
            };

            foreach (var item in pedido.Itens)
            {
                if (!produtos.TryGetValue(item.IdProduto, out var produto))
                {
                    throw new InvalidOperationException(PedidoBusinessRules.MensagemProdutosNaoExistem);
                }

                if (produto.Estoque < item.Quantidade)
                {
                    throw new InvalidOperationException(PedidoBusinessRules.MensagemEstoqueInsuficiente);
                }

                var subTotal = PedidoBusinessRules.CalcularSubTotal(produto.PrecoUnitario, item.Quantidade, item.Desconto);

                produto.Estoque -= item.Quantidade;
                novoPedido.TotalPedido += subTotal;
                novoPedido.PedidoProdutos.Add(new PedidoProduto
                {
                    IdProduto = item.IdProduto,
                    Quantidade = item.Quantidade,
                    Desconto = item.Desconto,
                    PrecoUnitario = produto.PrecoUnitario,
                    SubTotal = subTotal
                });
            }

            novoPedido.ValorComissao = PedidoBusinessRules.CalcularComissao(novoPedido.TotalPedido, vendedor.Comissao);

            context.Pedidos.Add(novoPedido);
            tempoProcessamento.Stop();

            // Controle otimista: o EF inclui RowVersion no WHERE do UPDATE de Produto.
            // Se outra transacao alterou o estoque depois da leitura, nenhum registro e atualizado e
            // DbUpdateConcurrencyException sinaliza conflito. A procedure usa UPDLOCK, que e pessimista:
            // bloqueia a linha antes de atualizar. O modelo otimista reduz tempo de bloqueio e costuma escalar
            // melhor com baixa disputa; em alta disputa pode exigir retry e gerar mais falhas concorrentes.
            var persistencia = Stopwatch.StartNew();
            var registrosAfetados = await context.SaveChangesAsync(cancellationToken);
            persistencia.Stop();
            await transaction.CommitAsync(cancellationToken);

            stopwatch.Stop();
            return new ExecucaoOperacaoDto<RegistrarPedidoResultadoDto>(
                new RegistrarPedidoResultadoDto(
                    novoPedido.Id,
                    registrosAfetados,
                    novoPedido.TotalPedido,
                    novoPedido.ValorComissao,
                    PedidoBusinessRules.MensagemPedidoBackendSucesso),
                stopwatch.Elapsed,
                tempoProcessamento.Elapsed,
                persistencia.Elapsed);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new InvalidOperationException(PedidoBusinessRules.MensagemConcorrenciaEstoque, ex);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<RegistrarPedidoDto> CriarPedidoExemploAsync(CancellationToken cancellationToken = default)
    {
        var cliente = await context.Clientes.AsNoTracking().OrderBy(cliente => cliente.Id).FirstAsync(cancellationToken);
        var vendedor = await context.Vendedores.AsNoTracking().OrderBy(vendedor => vendedor.Id).FirstAsync(cancellationToken);
        var metodoPagamento = await context.MetodosPagamento.AsNoTracking().OrderBy(metodo => metodo.Id).FirstAsync(cancellationToken);
        var produtos = await context.Produtos
            .AsNoTracking()
            .Where(produto => produto.Estoque >= 4)
            .OrderBy(produto => produto.Id)
            .Take(3)
            .Select(produto => new PedidoItemDto(produto.Id, 1, 0))
            .ToListAsync(cancellationToken);

        if (produtos.Count == 0)
        {
            throw new InvalidOperationException("Nao ha produtos com estoque suficiente para criar pedido de exemplo.");
        }

        return new RegistrarPedidoDto(
            metodoPagamento.Id,
            cliente.Id,
            vendedor.Id,
            new DateTime(2026, 1, 15, 10, 0, 0),
            produtos);
    }
}
