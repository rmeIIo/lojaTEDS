namespace loja.models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty; // Initialize with a default value
        public string Cpf { get; set; } = string.Empty; // Initialize with a default value
        public string Email { get; set; } = string.Empty; // Initialize with a default value
    }
}
