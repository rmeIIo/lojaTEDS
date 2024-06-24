namespace loja.models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty; // Initialize with a default value
        public decimal Preco { get; set; }
        public string Fornecedor { get; set; } = string.Empty; // Initialize with a default value
        public ICollection<ProdutoDeposito> ProdutosDeposito { get; set; } 
    }
}
