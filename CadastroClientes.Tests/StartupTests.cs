using CadastroClientes.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CadastroClientes.Tests
{
    public class StartupTests
    {
        [Fact]
        public void ConfigureServices_RegistersDbContext()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<CadastroClientesDbContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
                });

            // Act
            using var host = hostBuilder.Build();
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var dbContext = services.GetService<CadastroClientesDbContext>();

            // Assert
            Assert.NotNull(dbContext);

            // Verificar se o dbContext pode acessar o banco de dados
            Assert.True(dbContext!.Database.CanConnect());
        }
    }
}
