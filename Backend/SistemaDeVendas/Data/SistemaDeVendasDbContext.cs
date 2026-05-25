using Microsoft.EntityFrameworkCore;
using SistemaDeVendas.Models;

namespace SistemaDeVendas.Data;

// Camada Data: concentra o acesso ao SQL Server e o mapeamento entre tabelas e entidades.
public sealed class SistemaDeVendasDbContext(DbContextOptions<SistemaDeVendasDbContext> options) : DbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<EnderecoCliente> EnderecosClientes => Set<EnderecoCliente>();
    public DbSet<MetodoPagamento> MetodosPagamento => Set<MetodoPagamento>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoProduto> PedidosProdutos => Set<PedidoProduto>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Situacao> Situacoes => Set<Situacao>();
    public DbSet<Vendedor> Vendedores => Set<Vendedor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Endereco>(entity =>
        {
            entity.ToTable("Endereco");
            entity.HasKey(e => e.Id).HasName("PK_IdEndereco");
            entity.Property(e => e.UF).HasColumnType("char(2)").IsRequired();
            entity.Property(e => e.CEP).HasColumnType("char(8)").IsRequired();
            entity.Property(e => e.Logradouro).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.Numero).HasMaxLength(6).IsUnicode(false).IsRequired();
            entity.Property(e => e.Bairro).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.Cidade).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.Complemento).HasMaxLength(100).IsUnicode(false);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Cliente");
            entity.HasKey(e => e.Id).HasName("PK_IdCliente");
            entity.Property(e => e.Nome).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.CPF).HasColumnType("char(11)").IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsUnicode(false).IsRequired();
            entity.Property(e => e.Telefone).HasColumnType("char(11)").IsRequired();
            entity.HasIndex(e => e.CPF).IsUnique().HasDatabaseName("UQ_CPF_Cliente");
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_Email_Cliente");
        });

        modelBuilder.Entity<Situacao>(entity =>
        {
            entity.ToTable("Situacao");
            entity.HasKey(e => e.Id).HasName("PK_IdSituacao");
            entity.Property(e => e.Nome).HasMaxLength(50).IsUnicode(false).IsRequired();
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categoria");
            entity.HasKey(e => e.Id).HasName("PK_IdCategoria");
            entity.Property(e => e.Nome).HasMaxLength(100).IsUnicode(false).IsRequired();
        });

        modelBuilder.Entity<MetodoPagamento>(entity =>
        {
            entity.ToTable("MetodoPagamento");
            entity.HasKey(e => e.Id).HasName("PK_IdMetodoPagamento");
            entity.Property(e => e.Nome).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.HasIndex(e => e.Nome).IsUnique().HasDatabaseName("Nome_MetodoPagamento");
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("Produto");
            entity.HasKey(e => e.Id).HasName("PK_IdProduto");
            entity.Property(e => e.Nome).HasMaxLength(150).IsUnicode(false).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(600).IsUnicode(false).IsRequired();
            entity.Property(e => e.PrecoUnitario).HasColumnType("decimal(18, 2)");
            entity.HasOne(e => e.Categoria)
                .WithMany(e => e.Produtos)
                .HasForeignKey(e => e.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_IdCategoriaProduto");
        });

        modelBuilder.Entity<Vendedor>(entity =>
        {
            entity.ToTable("Vendedor");
            entity.HasKey(e => e.Id).HasName("PK_IdVendedor");
            entity.Property(e => e.Nome).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.CPF).HasColumnType("char(11)").IsRequired();
            entity.Property(e => e.Comissao).HasColumnType("decimal(5, 2)");
            entity.HasIndex(e => e.CPF).IsUnique().HasDatabaseName("CPF_Vendedor");
            entity.HasOne(e => e.Situacao)
                .WithMany(e => e.Vendedores)
                .HasForeignKey(e => e.IdSituacao)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_IdSituacao_Vendedor");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.ToTable("Pedido");
            entity.HasKey(e => e.Id).HasName("PK_IdPedido");
            entity.Property(e => e.DataPedido).HasColumnType("datetime");
            entity.Property(e => e.TotalPedido).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ValorComissao).HasColumnType("decimal(18, 2)");
            entity.HasOne(e => e.MetodoPagamento)
                .WithMany(e => e.Pedidos)
                .HasForeignKey(e => e.IdMetodoPagamento)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_IdMetodoPagamento_Pedido");
            entity.HasOne(e => e.Cliente)
                .WithMany(e => e.Pedidos)
                .HasForeignKey(e => e.IdCliente)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_IdCliente_Pedido");
            entity.HasOne(e => e.Vendedor)
                .WithMany(e => e.Pedidos)
                .HasForeignKey(e => e.IdVendedor)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_IdVendedor_Pedido");
        });

        modelBuilder.Entity<PedidoProduto>(entity =>
        {
            entity.ToTable("PedidoProduto");
            entity.HasKey(e => e.Id).HasName("PK_IdVendaProduto");
            entity.Property(e => e.Desconto).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PrecoUnitario).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.HasOne(e => e.Produto)
                .WithMany(e => e.PedidoProdutos)
                .HasForeignKey(e => e.IdProduto)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_IdProduto_VendaProduto");
            entity.HasOne(e => e.Pedido)
                .WithMany(e => e.PedidoProdutos)
                .HasForeignKey(e => e.IdPedido)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_IdPedido_VendaProduto");
        });

        modelBuilder.Entity<EnderecoCliente>(entity =>
        {
            entity.ToTable("EnderecoCliente");
            entity.HasKey(e => e.Id).HasName("PK_IdEnderecoCliente");
            entity.HasOne(e => e.Cliente)
                .WithMany(e => e.EnderecosClientes)
                .HasForeignKey(e => e.IdCliente)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_IdCliente_EnderecoCliente");
            entity.HasOne(e => e.Endereco)
                .WithMany(e => e.EnderecosClientes)
                .HasForeignKey(e => e.IdEndereco)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_IdEndereco_EnderecoCliente");
        });
    }
}
