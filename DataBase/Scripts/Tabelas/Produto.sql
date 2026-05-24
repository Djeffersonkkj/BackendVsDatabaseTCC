USE SistemaDeVendas;
GO

CREATE TABLE Produto(
	Id            INT          IDENTITY,
	IdCategoria   TINYINT      NOT NULL,
	Nome          VARCHAR(150) NOT NULL,
	Descricao     VARCHAR(600) NOT NULL,
	Estoque       INT          NOT NULL,
	PrecoUnitario DECIMAL(18, 2) NOT NULL,

	CONSTRAINT PK_IdProduto PRIMARY KEY (Id),
	CONSTRAINT FK_IdCategoriaProduto FOREIGN KEY (IdCategoria) REFERENCES Categoria(Id)
);
GO
