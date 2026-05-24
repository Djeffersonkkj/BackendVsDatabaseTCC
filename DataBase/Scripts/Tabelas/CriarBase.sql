CREATE DATABASE SistemaDeVendas;
GO

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

CREATE TABLE Situacao(
	Id      TINYINT     IDENTITY,
	Nome    VARCHAR(50) NOT NULL,

	CONSTRAINT PK_IdSituacao PRIMARY KEY (Id)
);
GO

CREATE TABLE Categoria(
	Id   TINYINT      IDENTITY,
	Nome VARCHAR(100) NOT NULL,

	CONSTRAINT PK_IdCategoria PRIMARY KEY (Id)
);
GO

CREATE TABLE MetodoPagamento(
	Id   TINYINT      IDENTITY,
	Nome VARCHAR(100) NOT NULL,

	CONSTRAINT PK_IdMetodoPagamento PRIMARY KEY (Id),
	CONSTRAINT Nome_MetodoPagamento UNIQUE (Nome)
);
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

CREATE TABLE EnderecoCliente(
	Id 			INT 	IDENTITY(1,1) NOT NULL,
	IdCliente  	INT 	NOT NULL,
	IdEndereco 	INT 	NOT NULL,

	CONSTRAINT PK_IdEnderecoCliente PRIMARY KEY (Id),
	CONSTRAINT FK_IdCliente_EnderecoCliente FOREIGN KEY (IdCliente) REFERENCES Cliente(Id),
	CONSTRAINT FK_IdEndereco_EnderecoCliente FOREIGN KEY (IdEndereco) REFERENCES Endereco(Id)
);
GO
