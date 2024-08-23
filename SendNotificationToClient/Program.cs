using CadastroClientes.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using SendNotificationToClient.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Configura��o do DbContext
builder.Services.AddDbContext<CadastroClientesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adicionar servi�os ao cont�iner
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura��o do RabbitMQ
builder.Services.AddSingleton<ConnectionFactory>(sp =>
{
    return new ConnectionFactory { HostName = "localhost" };
});

// Registrar o NotificacaoErroConsumer como um servi�o scoped
builder.Services.AddScoped<NotificacaoErroConsumer>();

var app = builder.Build();

// Configurar o pipeline de requisi��es HTTP
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
