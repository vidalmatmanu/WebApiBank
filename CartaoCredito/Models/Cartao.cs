namespace CartaoCredito.Models
{
    public class Cartao
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public int Numero { get; set; }
        public string Status { get; set; }
    }
}