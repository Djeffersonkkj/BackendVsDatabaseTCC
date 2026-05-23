USE SistemaDeVendas;
GO

CREATE TABLE Endereco(
	Id          INT             IDENTITY,
	UF          CHAR(2)         NOT NULL,
	CEP         CHAR(8)         NOT NULL,
	Logradouro  VARCHAR(100)    NOT NULL,
	Numero      VARCHAR(6)      NOT NULL,
	Bairro      VARCHAR(100)    NOT NULL,
	Cidade      VARCHAR(100)    NOT NULL,
	Complemento VARCHAR(100),

	CONSTRAINT PK_IdEndereco PRIMARY KEY (Id)
);
GO
