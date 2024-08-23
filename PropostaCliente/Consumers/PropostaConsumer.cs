using PropostaCredito.Repository;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using PropostaCliente.Models;

public class PropostaConsumer
{
    private readonly ConnectionFactory _factory;
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection _connection;
    private IModel _channel;

    public PropostaConsumer(ConnectionFactory factory, IServiceScopeFactory scopeFactory)
    {
        _factory = factory;
        _scopeFactory = scopeFactory;
    }

    public void StartConsuming()
    {
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "fila_novo_cliente", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "fila_proposta_cliente", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "fila_notificacao_erro_proposta", durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var cliente = JsonSerializer.Deserialize<Cliente>(message);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<PropostasDbContext>();

                    if (ValidarSocialSecurity(cliente!.SocialSecurity))
                    {
                        // Criar e salvar a proposta
                        var proposta = new Proposta
                        {
                            Id = Guid.NewGuid(),
                            ClienteId = cliente.Id,
                            Numero = new Random().Next(1000, 9999),
                            Status = "Aprovada"
                        };
                        _context.Propostas.Add(proposta);

                        // Atualizar o status do cliente para "sent"
                        cliente.Status = "Sent";
                        _context.Clientes.Update(cliente);
                        _context.SaveChanges();

                        // Publicar a proposta na fila
                        var propostaMessage = JsonSerializer.Serialize(proposta);
                        var propostaBody = Encoding.UTF8.GetBytes(propostaMessage);
                        _channel.BasicPublish(exchange: "", routingKey: "fila_proposta_cliente", basicProperties: null, body: propostaBody);
                    }
                    else
                    {
                        var proposta = new Proposta
                        {
                            Id = Guid.NewGuid(),
                            ClienteId = cliente.Id,
                            Numero = new Random().Next(1000, 9999),
                            Status = "Reprovada"
                        };
                        _context.Propostas.Add(proposta);

                        // Atualizar o status do cliente para "erro"
                        cliente.Status = "Erro";
                        _context.Clientes.Update(cliente);
                        _context.SaveChanges();

                        // Criar e publicar a mensagem de notificação de erro
                        var notificacaoErro = new NotificacaoErro
                        {
                            ClienteId = cliente.Id,
                            TipoErro = "Proposta",
                            MensagemErro = "Erro ao criar proposta",
                            DataOcorrencia = DateTime.UtcNow
                        };
                        var notificacaoMessage = JsonSerializer.Serialize(notificacaoErro);
                        var notificacaoBody = Encoding.UTF8.GetBytes(notificacaoMessage);
                        _channel.BasicPublish(exchange: "", routingKey: "fila_notificacao_erro_proposta", basicProperties: null, body: notificacaoBody);
                    }

                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a mensagem: {ex.Message}");
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
        };

        _channel.BasicConsume(queue: "fila_novo_cliente", autoAck: false, consumer: consumer);
        Console.WriteLine("Aguardando mensagens...");
    }

    private bool ValidarSocialSecurity(string socialSecurity)
    {
        return !string.IsNullOrWhiteSpace(socialSecurity) && socialSecurity.Length == 11 && socialSecurity.All(char.IsDigit);
    }
}
