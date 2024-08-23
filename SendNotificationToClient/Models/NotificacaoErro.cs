namespace SendNotificationToClient.Models
{
    public class NotificacaoErro
    {
        public Guid ClienteId { get; set; }
        public string TipoErro { get; set; }
        public string MensagemErro { get; set; }
        public DateTime DataOcorrencia { get; set; }
    }
}
