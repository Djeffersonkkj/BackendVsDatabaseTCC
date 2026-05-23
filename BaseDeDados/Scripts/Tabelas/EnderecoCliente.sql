USE SistemaDeVendas;
GO

CREATE TABLE EnderecoCliente(
	IdCliente  INT NOT NULL,
	IdEndereco INT NOT NULL,

	CONSTRAINT PK_IdEnderecoCliente PRIMARY KEY (IdCliente, IdEndereco),
	CONSTRAINT FK_IdCliente_EnderecoCliente FOREIGN KEY (IdCliente) REFERENCES Cliente(Id),
	CONSTRAINT FK_IdEndereco_EnderecoCliente FOREIGN KEY (IdEndereco) REFERENCES Endereco(Id)
);
GO
