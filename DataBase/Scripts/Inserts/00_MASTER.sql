-- ============================================================
-- Script master: executa todos os inserts na ordem correta
-- para respeitar as foreign keys do banco SistemaDeVendas.

-- sqlcmd -S "localhost\SQLEXPRESS" -d SistemaDeVendas -E -C -i 00_MASTER.sql
-- ============================================================

USE SistemaDeVendas;
GO

-- ── Situacao ──
:r 01_Situacao.sql
GO

-- ── Categoria ──
:r 02_Categoria.sql
GO

-- ── MetodoPagamento ──
:r 03_MetodoPagamento.sql
GO

-- ── Endereco ──
:r 04_Endereco.sql
GO

-- ── Cliente ──
:r 05_Cliente.sql
GO

-- ── EnderecoCliente ──
:r 06_EnderecoCliente.sql
GO

-- ── Vendedor ──
:r 07_Vendedor.sql
GO

-- ── Produto ──
:r 08_Produto.sql
GO

-- ── Pedido ──
:r 09_Pedido.sql
GO

-- ── PedidoProduto ──
:r 10_PedidoProduto.sql
GO

-- ── Atualização de Estoque ──
:r 11_AtualizaEstoque.sql
GO
