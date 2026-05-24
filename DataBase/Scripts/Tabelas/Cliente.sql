USE SistemaDeVendas;
GO

CREATE TABLE Cliente(
	Id          INT             IDENTITY,
	Nome        VARCHAR(100)    NOT NULL,
	CPF         CHAR(11)        NOT NULL,
	Email       VARCHAR(255)    NOT NULL,
	Telefone    CHAR(11)        NOT NULL,

	CONSTRAINT PK_IdCliente PRIMARY KEY (Id),
	CONSTRAINT UQ_CPF_Cliente UNIQUE (CPF),
    CONSTRAINT UQ_Email_Cliente UNIQUE (Email)
);
GO