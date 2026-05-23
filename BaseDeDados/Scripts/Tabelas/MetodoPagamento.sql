USE SistemaDeVendas;
GO

CREATE TABLE MetodoPagamento(
	Id   TINYINT      IDENTITY,
	Nome VARCHAR(100) NOT NULL,

	CONSTRAINT PK_IdMetodoPagamento PRIMARY KEY (Id),
	CONSTRAINT Nome_MetodoPagamento UNIQUE (Nome)
);
GO
