USE SistemaDeVendas;
GO

CREATE TABLE EnderecoCliente(
	Id 			INT 	IDENTITY(1,1) NOT NULL,
	IdCliente  	INT 	NOT NULL,
	IdEndereco 	INT 	NOT NULL,

	CONSTRAINT PK_IdEnderecoCliente PRIMARY KEY (Id),
	CONSTRAINT FK_IdCliente_EnderecoCliente FOREIGN KEY (IdCliente) REFERENCES Cliente(Id),
	CONSTRAINT FK_IdEndereco_EnderecoCliente FOREIGN KEY (IdEndereco) REFERENCES Endereco(Id)
);
GO
