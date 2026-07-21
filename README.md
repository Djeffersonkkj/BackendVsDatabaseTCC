# BackendVsDatabaseTCC

Estudo comparativo da implementacao de regras de negocio no backend em C# e no banco de dados utilizando SQL Server e Stored Procedures.

## Objetivo

O projeto compara duas formas de executar regras e consultas de um sistema de vendas:

- **Backend / EF Core**: regras implementadas em C#.
- **Stored Procedures**: regras implementadas diretamente no SQL Server.

A aplicacao console permite executar as duas abordagens separadamente e tambem rodar um benchmark reproduzivel.

## Requisitos

- SQL Server ou SQL Server Express.
- .NET SDK compativel com `net10.0`.
- `sqlcmd` ou SQL Server Management Studio, caso prefira executar os scripts manualmente.

## Como Preparar o Banco

### 1. Criar banco e tabelas

Execute o script:

```text
DataBase/Scripts/Tabelas/CriarBase.sql
```

Esse script cria o banco `SistemaDeVendas` e as tabelas principais.

Se estiver usando uma base que ja existia antes da coluna `RowVersion`, execute tambem:

```text
DataBase/Scripts/Tabelas/Produto_RowVersion_Alter.sql
```

Se preferir criar as tabelas separadamente, siga a ordem descrita em:

```text
DataBase/Scripts/Tabelas/OrdemCriacao.md
```

### 2. Inserir dados

Execute o script master de inserts:

```text
DataBase/Scripts/Inserts/00_MASTER.sql
```

Pelo terminal, a partir da raiz do projeto:

```powershell
sqlcmd -S "localhost\SQLEXPRESS" -d SistemaDeVendas -E -C -i DataBase\Scripts\Inserts\00_MASTER.sql
```

No SQL Server Management Studio, habilite o **SQLCMD Mode**, pois o script usa comandos `:r`.

### 3. Criar procedures e indices

Execute os scripts desta pasta:

```text
DataBase/Scripts/StoredProcedures
```

Na seguinte ordem:

```text
00_TipoItemPedido.sql
01_RegistrarPedido.sql
02_RelatorioVendasPorPeriodo.sql
03_RelatorioRankingVendedores.sql
04_RelatorioProdutosMaisVendidos.sql
05_RelatorioClientesQueMaisCompraram.sql
06_RelatorioConsolidado.sql
07_IndicesApoioComparacao.sql
```

O arquivo `00_EXECUTAR_TODAS_STORED_PROCEDURES.sql` serve como guia. Para usar os comandos `:r` dele, descomente as linhas e execute em SQLCMD Mode.

## Configurar a Conexao

A connection string fica em:

```text
Backend/SistemaDeVendas/appsettings.json
```

Valor padrao:

```json
{
  "ConnectionStrings": {
    "SistemaDeVendas": "Server=localhost\\SQLEXPRESS;Database=SistemaDeVendas;Trusted_Connection=True;"
  }
}
```

Altere o `Server` se sua instancia do SQL Server tiver outro nome.

## Como Rodar

A partir da raiz do projeto:

```powershell
cd Backend/SistemaDeVendas
dotnet run
```

A aplicacao exibira o menu:

```text
[1] Executar regra via backend
[2] Executar regra via procedure
[3] Executar benchmark reproduzivel
[4] Salvar SQL gerado pelo EF Core
[0] Sair
```

## Como Usar as Opcoes

### Opcao 1: Executar regra via backend

Executa a criacao de um pedido e o relatorio consolidado usando C# e EF Core.

Use para observar a abordagem em que a regra fica no backend.

### Opcao 2: Executar regra via procedure

Executa a criacao de um pedido e o relatorio consolidado usando Stored Procedures.

Use para observar a abordagem em que a regra fica no banco de dados.

### Opcao 3: Executar benchmark reproduzivel

Executa a comparacao principal do projeto.

O benchmark mede:

- `RegistrarPedido`
- `Relatorio consolidado`
- `Relatorio vendas por periodo`

Para cada operacao, ele compara backend e procedure usando:

- boxplot dos tempos
- media
- desvio padrao
- diferenca percentual
- quantidade de registros
- equivalencia dos resultados

Essa e a opcao mais indicada para apresentar os resultados do estudo.

### Opcao 4: Salvar SQL gerado pelo EF Core

Gera um arquivo `.sql` com o SQL produzido pelo EF Core via `ToQueryString()`.

O arquivo e salvo em:

```text
Backend/SistemaDeVendas/bin/Debug/net10.0/SqlCapturado
```

Use essa opcao para comparar visualmente o SQL gerado pelo EF Core com uma Stored Procedure equivalente.

## Fluxo Recomendado Para Demonstracao

1. Execute a opcao `[1]` para mostrar a regra via backend.
2. Execute a opcao `[2]` para mostrar a regra via procedure.
3. Execute a opcao `[3]` para gerar o benchmark comparativo.
4. Execute a opcao `[4]` se quiser analisar o SQL gerado pelo EF Core.

## Observacoes

- O benchmark usa um cenario fixo para reduzir variacoes entre execucoes.
- Apos registrar pedidos durante o benchmark, o projeto remove os pedidos criados e restaura o estoque original dos produtos usados no teste.
- O benchmark nao zera identity, cache ou plano de execucao do SQL Server.
- Os indices de `07_IndicesApoioComparacao.sql` ajudam a deixar a comparacao mais realista para consultas com filtros, joins e agregacoes.
