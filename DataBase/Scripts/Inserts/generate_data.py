"""
Script de geração de dados fictícios para o banco de dados SistemaDeVendas.
Utiliza a biblioteca Faker para gerar dados realistas em português do Brasil.
Os inserts são gerados em arquivos .sql separados por tabela, na ordem correta
para respeitar as foreign keys.
"""

import random
import os
from decimal import Decimal, ROUND_HALF_UP
from datetime import datetime, timedelta
from faker import Faker

# ──────────────────────────────────────────────
# Configurações gerais
# ──────────────────────────────────────────────
fake = Faker("pt_BR")
random.seed(42)
Faker.seed(42)

OUTPUT_DIR = "/mnt/user-data/outputs/sql_inserts"
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Tamanho dos lotes para BULK INSERT (melhora performance em servidores SQL)
BATCH_SIZE = 500

# ──────────────────────────────────────────────
# Utilitários
# ──────────────────────────────────────────────

def fmt_str(value: str) -> str:
    """Escapa aspas simples dentro de strings SQL."""
    return value.replace("'", "''")

def write_sql_file(filename: str, lines: list[str]) -> None:
    """Salva uma lista de linhas SQL em arquivo dentro do diretório de saída."""
    path = os.path.join(OUTPUT_DIR, filename)
    with open(path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))
    print(f"  ✔  {filename}  ({len(lines)} linhas)")

def batched_inserts(table: str, columns: list[str], rows: list[tuple]) -> list[str]:
    """
    Gera comandos INSERT em lotes (VALUES múltiplos), o que é muito mais
    eficiente do que um INSERT por linha, especialmente para grandes volumes.
    """
    lines = [f"-- Inserts para tabela {table}"]
    col_str = ", ".join(columns)

    for i in range(0, len(rows), BATCH_SIZE):
        batch = rows[i : i + BATCH_SIZE]
        values_parts = []
        for row in batch:
            formatted = []
            for v in row:
                if v is None:
                    formatted.append("NULL")
                elif isinstance(v, str):
                    formatted.append(f"'{fmt_str(v)}'")
                elif isinstance(v, bool):
                    formatted.append("1" if v else "0")
                elif isinstance(v, (int, float, Decimal)):
                    formatted.append(str(v))
                else:
                    formatted.append(f"'{v}'")
            values_parts.append(f"({', '.join(formatted)})")
        lines.append(f"INSERT INTO {table} ({col_str}) VALUES")
        lines.append(",\n".join(values_parts) + ";")

    return lines

def progress(msg: str) -> None:
    print(f"\n{'='*60}")
    print(f"  {msg}")
    print(f"{'='*60}")

# ──────────────────────────────────────────────
# 1. SITUACAO  (4 registros)
# ──────────────────────────────────────────────
progress("Gerando Situacao...")

situacoes = ["Ativo", "Inativo", "Férias", "Desligado"]
situacao_rows = [(nome,) for nome in situacoes]

# IDs gerados pelo IDENTITY começam em 1
situacao_ids = list(range(1, len(situacoes) + 1))

write_sql_file(
    "01_Situacao.sql",
    batched_inserts("Situacao", ["Nome"], situacao_rows),
)

# ──────────────────────────────────────────────
# 2. CATEGORIA  (12 registros)
# ──────────────────────────────────────────────
progress("Gerando Categoria...")

categorias = [
    "Eletrônicos", "Informática", "Eletrodomésticos", "Móveis",
    "Vestuário", "Calçados", "Brinquedos", "Esportes",
    "Alimentos", "Beleza e Saúde", "Automotivo", "Ferramentas",
]
categoria_rows = [(nome,) for nome in categorias]
categoria_ids = list(range(1, len(categorias) + 1))

write_sql_file(
    "02_Categoria.sql",
    batched_inserts("Categoria", ["Nome"], categoria_rows),
)

# ──────────────────────────────────────────────
# 3. METODO PAGAMENTO  (6 registros)
# ──────────────────────────────────────────────
progress("Gerando MetodoPagamento...")

metodos = [
    "Cartão de Crédito", "Cartão de Débito", "Pix",
    "Boleto Bancário", "Dinheiro", "Transferência Bancária",
]
metodo_rows = [(nome,) for nome in metodos]
metodo_ids = list(range(1, len(metodos) + 1))

write_sql_file(
    "03_MetodoPagamento.sql",
    batched_inserts("MetodoPagamento", ["Nome"], metodo_rows),
)

# ──────────────────────────────────────────────
# 4. ENDERECO  (1500 registros)
# ──────────────────────────────────────────────
progress("Gerando Endereco (1500 registros)...")

# UFs brasileiras reais
ufs = [
    "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA",
    "MT","MS","MG","PA","PB","PR","PE","PI","RJ","RN",
    "RS","RO","RR","SC","SP","SE","TO",
]

endereco_rows = []
for _ in range(1500):
    uf = random.choice(ufs)
    cep = fake.numerify(text="########")          # 8 dígitos numéricos
    logradouro = fake.street_name()[:100]
    numero = str(random.randint(1, 9999))
    bairro = fake.bairro()[:100]
    cidade = fake.city()[:100]
    complemento = random.choice([
        None, None, None,                          # 3/6 de chance de NULL
        f"Apto {random.randint(1,200)}",
        f"Casa {random.randint(1,10)}",
        "Fundos",
        f"Bloco {random.choice('ABCDE')}",
    ])
    endereco_rows.append((uf, cep, logradouro, numero, bairro, cidade, complemento))

# IDs de 1..1500
endereco_ids = list(range(1, 1501))

write_sql_file(
    "04_Endereco.sql",
    batched_inserts(
        "Endereco",
        ["UF", "CEP", "Logradouro", "Numero", "Bairro", "Cidade", "Complemento"],
        endereco_rows,
    ),
)

# ──────────────────────────────────────────────
# 5. CLIENTE  (1000 registros)
# ──────────────────────────────────────────────
progress("Gerando Cliente (1000 registros)...")

# Conjuntos para garantir unicidade de CPF e Email
cpfs_usados: set[str] = set()
emails_usados: set[str] = set()

cliente_rows = []
for i in range(1000):
    # CPF único (apenas dígitos, 11 chars)
    while True:
        cpf = fake.cpf().replace(".", "").replace("-", "")
        if cpf not in cpfs_usados:
            cpfs_usados.add(cpf)
            break

    # E-mail único
    while True:
        email = fake.email()[:255]
        if email not in emails_usados:
            emails_usados.add(email)
            break

    nome = fake.name()[:100]
    telefone = fake.msisdn()[:11]          # 11 dígitos numéricos

    cliente_rows.append((nome, cpf, email, telefone))

cliente_ids = list(range(1, 1001))

write_sql_file(
    "05_Cliente.sql",
    batched_inserts("Cliente", ["Nome", "CPF", "Email", "Telefone"], cliente_rows),
)

# ──────────────────────────────────────────────
# 6. ENDERECO_CLIENTE  (associação)
# Distribuição: entre 1 e 3 endereços por cliente
# Total: ~1500 registros; utilizamos todos os endereços gerados.
# ──────────────────────────────────────────────
progress("Gerando EnderecoCliente...")

# Embaralhamos os endereços e distribuímos entre os clientes
endereco_pool = list(endereco_ids)
random.shuffle(endereco_pool)

endereco_cliente_rows = []
pool_idx = 0

for cliente_id in cliente_ids:
    qtd = random.randint(1, 3)
    for _ in range(qtd):
        if pool_idx >= len(endereco_pool):
            break
        endereco_cliente_rows.append((cliente_id, endereco_pool[pool_idx]))
        pool_idx += 1
    if pool_idx >= len(endereco_pool):
        break

# Se sobraram endereços, associamos a clientes aleatórios
while pool_idx < len(endereco_pool):
    cliente_id = random.choice(cliente_ids)
    endereco_cliente_rows.append((cliente_id, endereco_pool[pool_idx]))
    pool_idx += 1

write_sql_file(
    "06_EnderecoCliente.sql",
    batched_inserts("EnderecoCliente", ["IdCliente", "IdEndereco"], endereco_cliente_rows),
)

# ──────────────────────────────────────────────
# 7. VENDEDOR  (20 registros)
# ──────────────────────────────────────────────
progress("Gerando Vendedor (20 registros)...")

vendedor_cpfs: set[str] = cpfs_usados.copy()   # evita CPF igual ao de cliente

vendedor_rows = []
for _ in range(20):
    while True:
        cpf = fake.cpf().replace(".", "").replace("-", "")
        if cpf not in vendedor_cpfs:
            vendedor_cpfs.add(cpf)
            break

    id_situacao = random.choices(
        situacao_ids,
        weights=[60, 15, 15, 10],   # Maioria ativa
        k=1,
    )[0]

    nome = fake.name()[:100]
    # Comissão entre 2% e 15%
    comissao = Decimal(str(round(random.uniform(2.0, 15.0), 2)))

    vendedor_rows.append((id_situacao, nome, cpf, comissao))

vendedor_ids = list(range(1, 21))

write_sql_file(
    "07_Vendedor.sql",
    batched_inserts("Vendedor", ["IdSituacao", "Nome", "CPF", "Comissao"], vendedor_rows),
)

# Mapa de comissão por vendedor (indexado pelo id 1-based)
vendedor_comissao = {
    i + 1: vendedor_rows[i][3] for i in range(len(vendedor_rows))
}

# ──────────────────────────────────────────────
# 8. PRODUTO  (500 registros)
# ──────────────────────────────────────────────
progress("Gerando Produto (500 registros)...")

# Nomes de produto por categoria para parecer realista
nomes_por_categoria = {
    1: ["Smartphone", "Tablet", "Smartwatch", "Fone Bluetooth", "Câmera Digital"],
    2: ["Notebook", "Monitor", "Teclado Mecânico", "Mouse Gamer", "SSD Externo"],
    3: ["Geladeira", "Micro-ondas", "Ar Condicionado", "Lavadora", "Fogão"],
    4: ["Sofá", "Guarda-roupa", "Cama Box", "Mesa de Jantar", "Estante"],
    5: ["Camiseta", "Calça Jeans", "Vestido", "Moletom", "Jaqueta"],
    6: ["Tênis Running", "Sandália", "Bota Social", "Chinelo", "Sapatênis"],
    7: ["Lego", "Boneca", "Carrinho RC", "Jogo de Tabuleiro", "Pelúcia"],
    8: ["Bicicleta", "Tênis Esportivo", "Raquete", "Halteres", "Colchonete"],
    9: ["Café Gourmet", "Azeite Extra Virgem", "Whey Protein", "Granola", "Chocolate"],
    10: ["Perfume", "Sérum Facial", "Protetor Solar", "Shampoo", "Creme Hidratante"],
    11: ["Pneu", "Óleo Lubrificante", "Capa de Chuva", "Tapete", "Câmara de Ar"],
    12: ["Furadeira", "Serra Circular", "Parafusadeira", "Nível Digital", "Alicate"],
}

produto_rows = []
# Estoque mantido em memória para controle durante geração dos pedidos
estoque: dict[int, int] = {}   # produto_id -> quantidade em estoque

for i in range(500):
    prod_id = i + 1
    id_categoria = random.choice(categoria_ids)
    base_nome = random.choice(nomes_por_categoria[id_categoria])
    nome = f"{base_nome} {fake.word().capitalize()} {random.randint(100,9999)}"[:150]
    descricao = fake.text(max_nb_chars=300)[:600]
    qtd_estoque = random.randint(1500, 4000)  # estoque alto para suportar 30k pedidos
    preco = Decimal(str(round(random.uniform(15.0, 8000.0), 2)))

    estoque[prod_id] = qtd_estoque
    produto_rows.append((id_categoria, nome, descricao, qtd_estoque, preco))

produto_ids = list(range(1, 501))

# Mapa de preço unitário por produto (1-based)
produto_preco: dict[int, Decimal] = {
    i + 1: produto_rows[i][4] for i in range(len(produto_rows))
}

write_sql_file(
    "08_Produto.sql",
    batched_inserts(
        "Produto",
        ["IdCategoria", "Nome", "Descricao", "Estoque", "PrecoUnitario"],
        produto_rows,
    ),
)

# ──────────────────────────────────────────────
# 9. PEDIDO + PEDIDOPRODUTO  (30.000 pedidos)
# ──────────────────────────────────────────────
progress("Gerando Pedido e PedidoProduto (30.000 pedidos)...")

N_PEDIDOS = 30_000
hoje = datetime.now()
dois_anos_atras = hoje - timedelta(days=730)

pedido_rows = []
pedido_produto_rows = []

# Atualizações de estoque acumuladas — serão geradas como UPDATE ao final
estoque_decrementos: dict[int, int] = {}   # produto_id -> total decrementado

pp_id = 0   # contador auxiliar (não usado no INSERT; IDENTITY cuida)

for pedido_num in range(N_PEDIDOS):
    pedido_id = pedido_num + 1   # IDENTITY começa em 1

    # Data aleatória nos últimos 2 anos
    delta_segundos = random.randint(0, 730 * 24 * 3600)
    data_pedido = dois_anos_atras + timedelta(seconds=delta_segundos)
    data_str = data_pedido.strftime("%Y%m%d %H:%M:%S")

    id_metodo = random.choice(metodo_ids)
    id_cliente = random.choice(cliente_ids)
    id_vendedor = random.choice(vendedor_ids)
    comissao_pct = vendedor_comissao[id_vendedor]

    # Quantidade de produtos por pedido: 2 a 5
    n_itens = random.randint(2, 5)

    # Seleção de produtos com estoque disponível
    # Para performance, filtramos apenas produtos com estoque > 0
    produtos_disponiveis = [pid for pid in produto_ids if estoque[pid] > 0]

    if not produtos_disponiveis:
        # Sem estoque disponível — pula o pedido
        continue

    # Ajusta n_itens para não ultrapassar o número de produtos disponíveis
    n_itens = min(n_itens, len(produtos_disponiveis))

    itens_selecionados = random.sample(produtos_disponiveis, n_itens)

    total_pedido = Decimal("0.00")

    for prod_id in itens_selecionados:
        max_qty = min(estoque[prod_id], 10)   # no máximo 10 un. por item
        if max_qty < 1:
            continue

        quantidade = random.randint(1, max_qty)
        desconto_pct = Decimal(str(round(random.choice([0, 0, 0, 5, 10, 15, 20]), 2)))
        preco_unit = produto_preco[prod_id]

        # SubTotal = PrecoUnitario * Quantidade * (1 - Desconto/100)
        subtotal = (preco_unit * quantidade * (1 - desconto_pct / 100)).quantize(
            Decimal("0.01"), rounding=ROUND_HALF_UP
        )

        pedido_produto_rows.append(
            (prod_id, pedido_id, quantidade, desconto_pct, preco_unit, subtotal)
        )

        total_pedido += subtotal

        # Decrementa estoque na memória
        estoque[prod_id] -= quantidade
        estoque_decrementos[prod_id] = (
            estoque_decrementos.get(prod_id, 0) + quantidade
        )

    if total_pedido == 0:
        # Pedido sem itens válidos — pula
        continue

    # ValorComissao = TotalPedido * Comissao% / 100
    valor_comissao = (total_pedido * comissao_pct / 100).quantize(
        Decimal("0.01"), rounding=ROUND_HALF_UP
    )

    pedido_rows.append(
        (id_metodo, id_cliente, id_vendedor, data_str, total_pedido, valor_comissao)
    )

    # Progresso a cada 5000 pedidos
    if (pedido_num + 1) % 5000 == 0:
        print(f"    → {pedido_num + 1:,} pedidos processados...")

print(f"  Total de pedidos gerados: {len(pedido_rows):,}")
print(f"  Total de itens de pedido: {len(pedido_produto_rows):,}")

write_sql_file(
    "09_Pedido.sql",
    batched_inserts(
        "Pedido",
        ["IdMetodoPagamento", "IdCliente", "IdVendedor", "DataPedido", "TotalPedido", "ValorComissao"],
        pedido_rows,
    ),
)

write_sql_file(
    "10_PedidoProduto.sql",
    batched_inserts(
        "PedidoProduto",
        ["IdProduto", "IdPedido", "Quantidade", "Desconto", "PrecoUnitario", "SubTotal"],
        pedido_produto_rows,
    ),
)

# ──────────────────────────────────────────────
# 10. ATUALIZAÇÃO DE ESTOQUE
# Gera UPDATEs para sincronizar o estoque da tabela Produto
# com os decrementos causados pelos pedidos gerados.
# ──────────────────────────────────────────────
progress("Gerando script de atualização de estoque...")

estoque_lines = ["-- Atualização de estoque após inserção dos pedidos"]
estoque_lines.append("-- Cada UPDATE reflete o total vendido por produto\n")

for prod_id, total_dec in sorted(estoque_decrementos.items()):
    estoque_lines.append(
        f"UPDATE Produto SET Estoque = Estoque - {total_dec} WHERE Id = {prod_id};"
    )

write_sql_file("11_AtualizaEstoque.sql", estoque_lines)

# ──────────────────────────────────────────────
# 11. MASTER SCRIPT — executa tudo na ordem correta
# ──────────────────────────────────────────────
progress("Gerando script master...")

master_lines = [
    "-- ============================================================",
    "-- Script master: executa todos os inserts na ordem correta",
    "-- para respeitar as foreign keys do banco SistemaDeVendas.",
    "-- ============================================================",
    "",
    "USE SistemaDeVendas;",
    "GO",
    "",
]

scripts_order = [
    ("01_Situacao.sql",        "Situacao"),
    ("02_Categoria.sql",       "Categoria"),
    ("03_MetodoPagamento.sql", "MetodoPagamento"),
    ("04_Endereco.sql",        "Endereco"),
    ("05_Cliente.sql",         "Cliente"),
    ("06_EnderecoCliente.sql", "EnderecoCliente"),
    ("07_Vendedor.sql",        "Vendedor"),
    ("08_Produto.sql",         "Produto"),
    ("09_Pedido.sql",          "Pedido"),
    ("10_PedidoProduto.sql",   "PedidoProduto"),
    ("11_AtualizaEstoque.sql", "Atualização de Estoque"),
]

for filename, label in scripts_order:
    master_lines += [
        f"-- ── {label} ──",
        f":r {filename}",
        "GO",
        "",
    ]

write_sql_file("00_MASTER.sql", master_lines)

# ──────────────────────────────────────────────
# Resumo final
# ──────────────────────────────────────────────
progress("✅ Geração concluída!")
print(f"""
Resumo dos arquivos gerados em: {OUTPUT_DIR}

  00_MASTER.sql          → Script master (executa todos em ordem)
  01_Situacao.sql        → {len(situacao_rows):>8,} registros
  02_Categoria.sql       → {len(categoria_rows):>8,} registros
  03_MetodoPagamento.sql → {len(metodo_rows):>8,} registros
  04_Endereco.sql        → {len(endereco_rows):>8,} registros
  05_Cliente.sql         → {len(cliente_rows):>8,} registros
  06_EnderecoCliente.sql → {len(endereco_cliente_rows):>8,} registros
  07_Vendedor.sql        → {len(vendedor_rows):>8,} registros
  08_Produto.sql         → {len(produto_rows):>8,} registros
  09_Pedido.sql          → {len(pedido_rows):>8,} registros
  10_PedidoProduto.sql   → {len(pedido_produto_rows):>8,} registros
  11_AtualizaEstoque.sql → {len(estoque_decrementos):>8,} UPDATEs

Dica: no SSMS, abra 00_MASTER.sql e execute com :r (sqlcmd mode)
ou rode cada arquivo individualmente na ordem numérica.
""")
