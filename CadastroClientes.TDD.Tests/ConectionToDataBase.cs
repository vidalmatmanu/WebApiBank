using CadastroClientes.Models;
using Microsoft.EntityFrameworkCore;

namespace CadastroClientes.Tests
{
    public class ConectionToDataBase
    {
        [Fact]
        public void CanConnectToDatabase()
        {
            var options = new DbContextOptionsBuilder<CadastroClientesDbContext>()
                .UseSqlServer("Server=DESKTOP-EORJ0SB\\MSSQLSERVER01;Database=CadastroClientesDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True")
                .Options;

            using (var context = new CadastroClientesDbContext(options))
            {
                var canConnect = context.Database.CanConnect();

                Assert.True(canConnect, "Não foi possível conectar ao banco de dados.");
            }
        }

    }
}
