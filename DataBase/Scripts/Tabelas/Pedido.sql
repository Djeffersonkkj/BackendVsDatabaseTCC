USE SistemaDeVendas;
GO

CREATE TABLE Pedido(
	Id                INT     			IDENTITY,
	IdMetodoPagamento TINYINT 			NOT NULL,
	IdCliente         INT     			NOT NULL,
	IdVendedor        INT     			NOT NULL,
	DataPedido        DATETIME    		NOT NULL,
	TotalPedido       DECIMAL(18, 2) 	NOT NULL,
	ValorComissao     DECIMAL(18, 2) 	NOT NULL,

	CONSTRAINT PK_IdPedido PRIMARY KEY (Id),
	CONSTRAINT FK_IdMetodoPagamento_Pedido FOREIGN KEY (IdMetodoPagamento) REFERENCES MetodoPagamento(Id),
	CONSTRAINT FK_IdCliente_Pedido FOREIGN KEY (IdCliente) REFERENCES Cliente(Id),
	CONSTRAINT FK_IdVendedor_Pedido FOREIGN KEY (IdVendedor) REFERENCES Vendedor(Id)
);
GO
