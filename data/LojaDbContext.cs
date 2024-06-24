using Microsoft.EntityFrameworkCore;
using loja.models;

namespace Loja.Data
{
    public class LojaDbContext : DbContext
    {
        public LojaDbContext(DbContextOptions<LojaDbContext> options) : base(options) { }

        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<Deposito> Depositos { get; set; }
        public DbSet<ProdutoDeposito> ProdutosDeposito { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>().HasKey(c => c.Id);
            modelBuilder.Entity<Fornecedor>().HasKey(f => f.Id);
            modelBuilder.Entity<Produto>().HasKey(p => p.Id);
            modelBuilder.Entity<Usuario>().HasKey(u => u.Id);
            modelBuilder.Entity<Venda>().HasKey(v => v.Id);
            modelBuilder.Entity<Deposito>().HasKey(d => d.Id);
            modelBuilder.Entity<ProdutoDeposito>().HasKey(pd => new { pd.ProdutoId, pd.DepositoId });

            modelBuilder.Entity<ProdutoDeposito>()
                        .HasOne(pd => pd.Produto)
                        .WithMany(p => p.ProdutosDeposito)
                        .HasForeignKey(pd => pd.ProdutoId);

            modelBuilder.Entity<ProdutoDeposito>()
                        .HasOne(pd => pd.Deposito)
                        .WithMany(d => d.ProdutosDeposito)
                        .HasForeignKey(pd => pd.DepositoId);
        }
    }
}
