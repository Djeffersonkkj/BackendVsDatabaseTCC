USE SistemaDeVendas;
GO

/*
    Tipo de tabela usado como parametro da procedure dbo.usp_RegistrarPedido.

    Por que usar Table-Valued Parameter (TVP):
    - evita concatenar SQL no C#;
    - permite enviar varios itens em uma chamada parametrizada;
    - reduz risco de SQL Injection;
    - deixa o contrato entre aplicacao e banco explicito.
*/
IF TYPE_ID(N'dbo.TipoItemPedido') IS NULL
BEGIN
    EXEC(N'
        CREATE TYPE dbo.TipoItemPedido AS TABLE
        (
            IdProduto  INT           NOT NULL,
            Quantidade SMALLINT      NOT NULL,
            Desconto   DECIMAL(5, 2) NOT NULL
        );
    ');
END;
GO
