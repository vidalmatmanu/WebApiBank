using CadastroClientes.Models;
using Microsoft.EntityFrameworkCore;

public class DbContextTests
{
    [Fact]
    public void CanAddAndRetrieveClient()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CadastroClientesDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        using (var context = new CadastroClientesDbContext(options))
        {
            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Name = "Test Client",
                Status = "Active",
                Address = "123 Test St",            
                Email = "testclient@example.com",    
                SocialSecurity = "123-45-6789"       
            };

            // Act
            context.Clientes.Add(cliente);
            context.SaveChanges();

            var retrievedClient = context.Clientes.Find(cliente.Id);

            // Assert
            Assert.NotNull(retrievedClient);
            Assert.Equal("Test Client", retrievedClient.Name);
        }
    }
}
