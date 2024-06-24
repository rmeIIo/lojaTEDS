namespace loja.models
{
    public class Fornecedor
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty; // Inicializa com valor padrão
        public string Cnpj { get; set; } = string.Empty; // Inicializa com valor padrão
        public string Email { get; set; } = string.Empty; // Inicializa com valor padrão
    }
}
