USE SistemaDeVendas;
GO

CREATE TABLE PedidoProduto(
	Id         		INT          	IDENTITY,
	IdProduto  		INT          	NOT NULL,
	IdPedido   		INT          	NOT NULL,
	Quantidade 		SMALLINT      	NOT NULL,
	Desconto   		DECIMAL(5, 2) 	NOT NULL,
	PrecoUnitario 	DECIMAL(18, 2)  NOT NULL,
	SubTotal   		DECIMAL(18, 2)  NOT NULL,

	CONSTRAINT PK_IdVendaProduto PRIMARY KEY (Id),
	CONSTRAINT FK_IdProduto_VendaProduto FOREIGN KEY (IdProduto) REFERENCES Produto(Id),
	CONSTRAINT FK_IdPedido_VendaProduto FOREIGN KEY (IdPedido) REFERENCES Pedido(Id)
);
GO
