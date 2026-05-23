USE SistemaDeVendas;
GO

CREATE TABLE Vendedor(
	Id         INT           IDENTITY,
	IdSituacao TINYINT       NOT NULL,
	Nome       VARCHAR(100)  NOT NULL,
	CPF        CHAR(11)      NOT NULL,
	Comissao   DECIMAL(5, 2) NOT NULL,

	CONSTRAINT PK_IdVendedor PRIMARY KEY (Id),
	CONSTRAINT FK_IdSituacao_Vendedor FOREIGN KEY (IdSituacao) REFERENCES Situacao(Id),
	CONSTRAINT CPF_Vendedor UNIQUE (CPF)
);
GO
