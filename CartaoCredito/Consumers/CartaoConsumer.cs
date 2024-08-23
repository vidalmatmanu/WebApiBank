using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using CartaoCredito.Repository;
using CartaoCredito.Models;
using System;
using Microsoft.EntityFrameworkCore;

public class CartaoConsumer
{
    private readonly ConnectionFactory _factory;
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection _connection;
    private IModel _channel;

    public CartaoConsumer(ConnectionFactory factory, IServiceScopeFactory scopeFactory)
    {
        _factory = factory;
        _scopeFactory = scopeFactory;
    }

    public void StartConsuming()
    {
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "fila_proposta_cliente", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "fila_cartao_credito", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "fila_notificacao_erro_cartao", durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var propostaCliente = JsonSerializer.Deserialize<Proposta>(message);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<CartoesDbContext>();

                    // Primeiro, buscar a proposta pelo Id da proposta recebida na mensagem
                    var proposta = _context.Propostas.FirstOrDefault(p => p.Id == propostaCliente!.Id);

                    if (proposta != null)
                    {
                        // Agora, buscar o cliente pelo ClienteId da proposta
                        var cliente = _context.Clientes.FirstOrDefault(c => c.Id == proposta.ClienteId);

                        if (cliente != null && ValidateStatusClient(proposta.Status))
                        {
                            // Criar e salvar o cartão
                            var cartao = new Cartao
                            {
                                Id = Guid.NewGuid(),
                                ClienteId = cliente.Id,
                                Numero = new Random().Next(1000, 9999),
                                Status = "Processado"
                            };
                            _context.Cartoes.Add(cartao);

                            // Atualizar o status do cliente para "Processado"
                            cliente.Status = "Processado";
                            _context.Clientes.Update(cliente);
                            _context.SaveChanges();
                        }
                        else
                        {
                            // Criar e salvar o cartão com status "Reprovado"
                            var cartao = new Cartao
                            {
                                Id = Guid.NewGuid(),
                                ClienteId = cliente?.Id ?? Guid.NewGuid(), // No caso de cliente ser null, evitar erro
                                Numero = new Random().Next(1000, 9999),
                                Status = "Reprovado"
                            };
                            _context.Cartoes.Add(cartao);

                            if (cliente != null)
                            {
                                // Atualizar o status do cliente para "Erro"
                                cliente.Status = "Erro";
                                _context.Clientes.Update(cliente);
                                _context.SaveChanges();

                                // Criar e publicar a mensagem de notificação de erro
                                var notificacaoErro = new NotificacaoErro
                                {
                                    ClienteId = cliente.Id,
                                    TipoErro = "Cartão",
                                    MensagemErro = "Erro ao criar cartão",
                                    DataOcorrencia = DateTime.UtcNow
                                };
                                var notificacaoMessage = JsonSerializer.Serialize(notificacaoErro);
                                var notificacaoBody = Encoding.UTF8.GetBytes(notificacaoMessage);
                                _channel.BasicPublish(exchange: "", routingKey: "fila_notificacao_erro_cartao", basicProperties: null, body: notificacaoBody);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Proposta não encontrada para Id: {propostaCliente.Id}");
                    }
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                // Log error if needed
                Console.WriteLine($"Error processing message: {ex.Message}");
                // Em caso de erro, confirmar a mensagem para evitar reprocessamento
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
        };

        _channel.BasicConsume(queue: "fila_proposta_cliente", autoAck: false, consumer: consumer);
        Console.WriteLine("Aguardando mensagens...");
    }

    private bool ValidateStatusClient(string status)
    {
        // Retorna false se o status não for "Aprovada"
        return status == "Aprovada";
    }
}
