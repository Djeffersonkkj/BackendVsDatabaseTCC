USE SistemaDeVendas;
GO

/*
    Adiciona controle de concorrencia otimista ao Produto.

    A Stored Procedure usa UPDLOCK, um controle pessimista: a linha e bloqueada durante a leitura
    para impedir que outra transacao a altere ate o fim da operacao. O backend usa RowVersion pelo EF Core:
    a linha e lida sem manter lock longo e o UPDATE so acontece se a versao ainda for a mesma.

    Impacto:
    - melhora concorrencia em cenarios com pouca disputa porque reduz tempo de bloqueio;
    - em alta disputa pode gerar DbUpdateConcurrencyException e exigir retry pela aplicacao;
    - o custo de escrita cresce pouco, pois o SQL Server atualiza a coluna ROWVERSION automaticamente.
*/
IF COL_LENGTH('dbo.Produto', 'RowVersion') IS NULL
BEGIN
    ALTER TABLE dbo.Produto
        ADD RowVersion ROWVERSION NOT NULL;
END;
GO
