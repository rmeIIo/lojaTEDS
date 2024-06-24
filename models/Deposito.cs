namespace loja.models
{
    public class Deposito
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public ICollection<ProdutoDeposito> ProdutosDeposito { get; set; }
    }

    public class ProdutoDeposito
    {
        public int ProdutoId { get; set; }
        public Produto Produto { get; set; }
        public int DepositoId { get; set; }
        public Deposito Deposito { get; set; }
        public int Quantidade { get; set; }
    }
}


