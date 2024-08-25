using CadastroClientes.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SendNotificationToClient.Models;
using System.Text;
using System.Text.Json;

namespace SendNotificationToClient.Consumers
{
    public class NotificacaoErroConsumer
    {
        private readonly CadastroClientesDbContext _context;
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;

        public NotificacaoErroConsumer(CadastroClientesDbContext context, ConnectionFactory factory)
        {
            _context = context;
            _factory = factory;
        }

        public void StartConsuming()
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "fila_notificacao_erro_proposta", durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "fila_notificacao_erro_cartao", durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var erroInfo = JsonSerializer.Deserialize<NotificacaoErro>(message);

                // Notificar o microserviço de Cadastro de Clientes
                await NotificationErroToClient(erroInfo.ClienteId, erroInfo.MensagemErro, erroInfo.TipoErro);

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: "fila_notificacao_erro_proposta", autoAck: false, consumer: consumer);
            _channel.BasicConsume(queue: "fila_notificacao_erro_cartao", autoAck: false, consumer: consumer);
        }

        private async Task NotificationErroToClient(Guid clienteId, string errorMessage, string tipoErro)
        {
            // Enviar uma solicitação HTTP POST para o microserviço de Cadastro de Clientes
            using var httpClient = new HttpClient();
            var notification = new
            {
                ClienteId = clienteId,
                MensagemErro = errorMessage,
                TipoErro = tipoErro
            };
            var content = new StringContent(JsonSerializer.Serialize(notification), Encoding.UTF8, "application/json");

            await httpClient.PostAsync("https://localhost:7056/api/clientes/notificar-erro", content);
        }
    }
}
