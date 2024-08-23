using Microsoft.EntityFrameworkCore;

namespace PropostaCredito.Repository
{
    public class PropostasDbContext : DbContext
    {
        public PropostasDbContext(DbContextOptions<PropostasDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proposta> Propostas { get; set; }
    }
}