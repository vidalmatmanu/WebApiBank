using CadastroClientes.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using SendNotificationToClient.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Configuração do DbContext
builder.Services.AddDbContext<CadastroClientesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adicionar serviços ao contêiner
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do RabbitMQ
builder.Services.AddSingleton<ConnectionFactory>(sp =>
{
    return new ConnectionFactory { HostName = "localhost" };
});

// Registrar o NotificacaoErroConsumer como um serviço scoped
builder.Services.AddScoped<NotificacaoErroConsumer>();

var app = builder.Build();

// Configurar o pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Iniciar o consumidor NotificacaoErroConsumer
using (var scope = app.Services.CreateScope())
{
    var notificacaoErroConsumer = scope.ServiceProvider.GetRequiredService<NotificacaoErroConsumer>();
    notificacaoErroConsumer.StartConsuming();
}

app.Run();
