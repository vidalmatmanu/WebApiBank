// Program.cs
using CartaoCredito.Repository;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Configurando o DbContext para usar o SQL Server
builder.Services.AddDbContext<CartoesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do RabbitMQ
builder.Services.AddSingleton<ConnectionFactory>(sp =>
{
    return new ConnectionFactory { HostName = "localhost" };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<CartaoConsumer>();


var app = builder.Build();

// Inicializar o PropostaConsumer aqui
using (var scope = app.Services.CreateScope())
{
    var propostaConsumer = scope.ServiceProvider.GetRequiredService<CartaoConsumer>();
    propostaConsumer.StartConsuming();
}

// Configurar middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.RoutePrefix = string.Empty; // Define o Swagger na raiz
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();