USE SistemaDeVendas;
GO

/*
    Stored Procedure: dbo.usp_RegistrarPedido

    Regra implementada no banco:
    - valida cliente, vendedor, metodo de pagamento e itens;
    - calcula subtotal por produto com desconto;
    - calcula total do pedido;
    - calcula valor da comissao a partir do percentual do vendedor;
    - grava Pedido e PedidoProduto;
    - baixa estoque;
    - executa tudo em uma transacao SQL Server.

    Comentarios tecnicos:
    - Acoplamento: a regra fica mais proxima do SQL Server e depende do contrato do tipo dbo.TipoItemPedido.
    - Manutencao: mudancas de regra exigem versionar script SQL e codigo C# que chama a procedure.
    - Legibilidade: regras em lote e validacoes de integridade ficam concentradas, mas T-SQL pode ser menos familiar para parte do time.
    - Desempenho: reduz idas e voltas entre aplicacao e banco, especialmente quando o pedido tem muitos itens.
    - Custo de processamento: desloca CPU e locks para o banco; isso pode ser bom para consistencia, mas aumenta pressao no SQL Server.
*/
CREATE OR ALTER PROCEDURE dbo.usp_RegistrarPedido
    @IdMetodoPagamento TINYINT,
    @IdCliente         INT,
    @IdVendedor        INT,
    @DataPedido        DATETIME,
    @Itens             dbo.TipoItemPedido READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE
        @IdPedido INT,
        @TotalPedido DECIMAL(18, 2),
        @ValorComissao DECIMAL(18, 2),
        @PercentualComissao DECIMAL(5, 2),
        @RegistrosAfetados INT = 0;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM @Itens)
            THROW 51001, 'O pedido precisa ter pelo menos um item.', 1;

        IF EXISTS (SELECT 1 FROM @Itens WHERE Quantidade <= 0)
            THROW 51002, 'A quantidade do item deve ser maior que zero.', 1;

        IF EXISTS (SELECT 1 FROM @Itens WHERE Desconto < 0 OR Desconto > 100)
            THROW 51003, 'O desconto deve ficar entre 0 e 100 porcento.', 1;

        IF EXISTS (
            SELECT IdProduto
            FROM @Itens
            GROUP BY IdProduto
            HAVING COUNT(*) > 1
        )
            THROW 51004, 'O mesmo produto nao deve ser informado mais de uma vez no pedido.', 1;

        IF NOT EXISTS (SELECT 1 FROM Cliente WHERE Id = @IdCliente)
            THROW 51005, 'Cliente informado nao existe.', 1;

        IF NOT EXISTS (SELECT 1 FROM MetodoPagamento WHERE Id = @IdMetodoPagamento)
            THROW 51006, 'Metodo de pagamento informado nao existe.', 1;

        SELECT @PercentualComissao = Comissao
        FROM Vendedor
        WHERE Id = @IdVendedor;

        IF @PercentualComissao IS NULL
            THROW 51007, 'Vendedor informado nao existe.', 1;

        DECLARE @ItensCalculados TABLE
        (
            IdProduto     INT           NOT NULL PRIMARY KEY,
            Quantidade    SMALLINT      NOT NULL,
            Desconto      DECIMAL(5, 2) NOT NULL,
            PrecoUnitario DECIMAL(18, 2) NOT NULL,
            SubTotal      DECIMAL(18, 2) NOT NULL,
            EstoqueAtual  INT           NOT NULL
        );

        INSERT INTO @ItensCalculados
        (
            IdProduto,
            Quantidade,
            Desconto,
            PrecoUnitario,
            SubTotal,
            EstoqueAtual
        )
        SELECT
            produto.Id,
            item.Quantidade,
            item.Desconto,
            produto.PrecoUnitario,
            ROUND(produto.PrecoUnitario * item.Quantidade * (1 - item.Desconto / 100.0), 2),
            produto.Estoque
        FROM @Itens AS item
        INNER JOIN Produto AS produto WITH (UPDLOCK, ROWLOCK)
            ON produto.Id = item.IdProduto;

        IF (SELECT COUNT(*) FROM @ItensCalculados) <> (SELECT COUNT(*) FROM @Itens)
            THROW 51008, 'Um ou mais produtos informados nao existem.', 1;

        IF EXISTS (SELECT 1 FROM @ItensCalculados WHERE EstoqueAtual < Quantidade)
            THROW 51009, 'Estoque insuficiente para um ou mais produtos.', 1;

        SELECT @TotalPedido = SUM(SubTotal)
        FROM @ItensCalculados;

        SET @ValorComissao = ROUND(@TotalPedido * @PercentualComissao / 100, 2);

        BEGIN TRANSACTION;

            INSERT INTO Pedido
            (
                IdMetodoPagamento,
                IdCliente,
                IdVendedor,
                DataPedido,
                TotalPedido,
                ValorComissao
            )
            VALUES
            (
                @IdMetodoPagamento,
                @IdCliente,
                @IdVendedor,
                @DataPedido,
                @TotalPedido,
                @ValorComissao
            );

            SET @RegistrosAfetados += @@ROWCOUNT;
            SET @IdPedido = CONVERT(INT, SCOPE_IDENTITY());

            INSERT INTO PedidoProduto
            (
                IdProduto,
                IdPedido,
                Quantidade,
                Desconto,
                PrecoUnitario,
                SubTotal
            )
            SELECT
                IdProduto,
                @IdPedido,
                Quantidade,
                Desconto,
                PrecoUnitario,
                SubTotal
            FROM @ItensCalculados;

            SET @RegistrosAfetados += @@ROWCOUNT;

            UPDATE produto
                SET Estoque = produto.Estoque - item.Quantidade
            FROM Produto AS produto
            INNER JOIN @ItensCalculados AS item
                ON item.IdProduto = produto.Id;

            SET @RegistrosAfetados += @@ROWCOUNT;

        COMMIT TRANSACTION;

        SELECT
            @IdPedido AS IdPedido,
            @RegistrosAfetados AS RegistrosAfetados,
            @TotalPedido AS TotalPedido,
            @ValorComissao AS ValorComissao,
            CAST('Pedido registrado via Stored Procedure com sucesso.' AS VARCHAR(200)) AS Mensagem;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        DECLARE @MensagemErro NVARCHAR(2048) = ERROR_MESSAGE();
        THROW 51099, @MensagemErro, 1;
    END CATCH;
END;
GO
