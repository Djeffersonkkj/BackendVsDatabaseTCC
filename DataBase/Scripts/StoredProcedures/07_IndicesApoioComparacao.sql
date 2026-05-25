USE SistemaDeVendas;
GO

/*
    Indices de apoio para comparacao Backend x Stored Procedure.

    Principio da revisao:
    - manter poucos indices, ligados diretamente aos filtros/joins dos relatorios;
    - evitar indices redundantes que cobrem a mesma chave inicial;
    - registrar o custo: cada INSERT em Pedido/PedidoProduto e cada UPDATE relacionado passa a manter
      tambem estes indices, aumentando escrita, log e possivel fragmentacao.

    Indices naturais ja existentes:
    - PKs clustered/nao clustered criadas nas tabelas;
    - uniques de Cliente(CPF), Cliente(Email), Vendedor(CPF), MetodoPagamento(Nome).
*/

/*
    Pedido(DataPedido)
    Motivo: todos os relatorios filtram periodo com intervalo semiaberto.
    INCLUDE cobre agregacoes e joins mais comuns sem buscar a linha base para TotalPedido/Comissao/FKs.
    Custo: RegistrarPedido insere mais uma entrada de indice por pedido.
*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pedido_DataPedido' AND object_id = OBJECT_ID('dbo.Pedido'))
BEGIN
    CREATE INDEX IX_Pedido_DataPedido
        ON dbo.Pedido (DataPedido)
        INCLUDE (TotalPedido, ValorComissao, IdCliente, IdVendedor, IdMetodoPagamento);
END;
GO

/*
    Pedido(IdVendedor, DataPedido)
    Motivo: ranking por vendedor combina FK e periodo. Nao substitui IX_Pedido_DataPedido,
    porque filtros puramente por periodo se beneficiam da chave iniciando em DataPedido.
    Custo: escrita adicional em Pedido; mantido porque o ranking e consulta central da comparacao.
*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pedido_IdVendedor_DataPedido' AND object_id = OBJECT_ID('dbo.Pedido'))
BEGIN
    CREATE INDEX IX_Pedido_IdVendedor_DataPedido
        ON dbo.Pedido (IdVendedor, DataPedido)
        INCLUDE (TotalPedido, ValorComissao);
END;
GO

/*
    Pedido(IdCliente, DataPedido)
    Motivo: relatorio de clientes que mais compraram agrupa por cliente apos filtro de periodo.
    Custo: aumenta INSERT em Pedido. Mantido porque evita depender apenas de IX_Pedido_DataPedido
    quando o otimizador preferir navegar por cliente em bases maiores.
*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pedido_IdCliente_DataPedido' AND object_id = OBJECT_ID('dbo.Pedido'))
BEGIN
    CREATE INDEX IX_Pedido_IdCliente_DataPedido
        ON dbo.Pedido (IdCliente, DataPedido)
        INCLUDE (TotalPedido);
END;
GO

/*
    PedidoProduto(IdPedido)
    Motivo: join principal dos relatorios que partem de Pedido filtrado por DataPedido para seus itens.
    INCLUDE cobre quantidade/desconto/subtotal/produto usados nas agregacoes.
    Custo: cada item vendido insere uma entrada neste indice.
*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PedidoProduto_IdPedido' AND object_id = OBJECT_ID('dbo.PedidoProduto'))
BEGIN
    CREATE INDEX IX_PedidoProduto_IdPedido
        ON dbo.PedidoProduto (IdPedido)
        INCLUDE (IdProduto, Quantidade, Desconto, SubTotal);
END;
GO

/*
    PedidoProduto(IdProduto)
    Motivo: produtos mais vendidos agrupa por produto; tambem ajuda joins Produto -> PedidoProduto.
    Custo: escrita adicional por item. Mantido por ser tabela de maior crescimento e consulta recorrente.
*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PedidoProduto_IdProduto' AND object_id = OBJECT_ID('dbo.PedidoProduto'))
BEGIN
    CREATE INDEX IX_PedidoProduto_IdProduto
        ON dbo.PedidoProduto (IdProduto)
        INCLUDE (Quantidade, Desconto, SubTotal, IdPedido);
END;
GO

/*
    Produto(IdCategoria)
    Motivo: relatorios consolidados e produtos mais vendidos juntam Produto com Categoria.
    Custo: pequeno INSERT/UPDATE adicional em Produto; Produto muda menos que PedidoProduto.
*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Produto_IdCategoria' AND object_id = OBJECT_ID('dbo.Produto'))
BEGIN
    CREATE INDEX IX_Produto_IdCategoria
        ON dbo.Produto (IdCategoria)
        INCLUDE (Nome);
END;
GO
