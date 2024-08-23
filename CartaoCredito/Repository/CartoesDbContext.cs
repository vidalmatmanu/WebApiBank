using CartaoCredito.Models;
using Microsoft.EntityFrameworkCore;

namespace CartaoCredito.Repository
{
    public class CartoesDbContext : DbContext
    {
        public CartoesDbContext(DbContextOptions<CartoesDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cartao> Cartoes { get; set; }

        public DbSet<Proposta> Propostas { get; set; }
    }
}
