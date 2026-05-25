using SistemaDeVendas.DTOs;

namespace SistemaDeVendas.BusinessRules;

public static class PedidoBusinessRules
{
    public const string MensagemPedidoVazio = "O pedido precisa ter pelo menos um item.";
    public const string MensagemQuantidadeInvalida = "A quantidade do item deve ser maior que zero.";
    public const string MensagemDescontoInvalido = "O desconto deve ficar entre 0 e 100 porcento.";
    public const string MensagemProdutoDuplicado = "O mesmo produto nao deve ser informado mais de uma vez no pedido.";
    public const string MensagemClienteNaoExiste = "Cliente informado nao existe.";
    public const string MensagemMetodoPagamentoNaoExiste = "Metodo de pagamento informado nao existe.";
    public const string MensagemVendedorNaoExiste = "Vendedor informado nao existe.";
    public const string MensagemProdutosNaoExistem = "Um ou mais produtos informados nao existem.";
    public const string MensagemEstoqueInsuficiente = "Estoque insuficiente para um ou mais produtos.";
    public const string MensagemConcorrenciaEstoque = "Nao foi possivel registrar o pedido porque o estoque foi alterado por outra transacao. Tente novamente.";
    public const string MensagemPedidoBackendSucesso = "Pedido registrado via backend com sucesso.";
    public const string MensagemPedidoProcedureSucesso = "Pedido registrado via Stored Procedure com sucesso.";

    public static void ValidarPedido(RegistrarPedidoDto pedido)
    {
        if (pedido.Itens.Count == 0)
        {
            throw new InvalidOperationException(MensagemPedidoVazio);
        }

        if (pedido.Itens.Any(item => item.Quantidade <= 0))
        {
            throw new InvalidOperationException(MensagemQuantidadeInvalida);
        }

        if (pedido.Itens.Any(item => item.Desconto is < 0 or > 100))
        {
            throw new InvalidOperationException(MensagemDescontoInvalido);
        }

        if (pedido.Itens.GroupBy(item => item.IdProduto).Any(grupo => grupo.Count() > 1))
        {
            throw new InvalidOperationException(MensagemProdutoDuplicado);
        }
    }

    public static void ValidarPeriodo(DateTime dataInicio, DateTime dataFim)
    {
        if (dataInicio >= dataFim)
        {
            throw new InvalidOperationException("DataInicio deve ser menor que DataFim.");
        }
    }

    public static decimal CalcularSubTotal(decimal precoUnitario, short quantidade, decimal desconto)
    {
        return Math.Round(precoUnitario * quantidade * (1 - desconto / 100), 2);
    }

    public static decimal CalcularComissao(decimal totalPedido, decimal percentualComissao)
    {
        return Math.Round(totalPedido * percentualComissao / 100, 2);
    }
}
