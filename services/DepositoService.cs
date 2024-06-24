using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using loja.models;
using Loja.Data;

namespace loja.services
{
    public class DepositoService
    {
        private readonly LojaDbContext _context;

        public DepositoService(LojaDbContext context)
        {
            _context = context;
        }

        public async Task AddDepositoAsync(Deposito deposito)
        {
            _context.Depositos.Add(deposito);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Deposito>> GetAllDepositosAsync()
        {
            return await _context.Depositos.ToListAsync();
        }

        public async Task<Deposito> GetDepositoByIdAsync(int id)
        {
            return await _context.Depositos.FindAsync(id);
        }

        public async Task<List<ProdutoDeposito>> GetProdutosNoDepositoAsync(int depositoId)
        {
            return await _context.ProdutosDeposito
                                .Where(pd => pd.DepositoId == depositoId)
                                .Include(pd => pd.Produto)
                                .ToListAsync();
        }

        public async Task<int> GetQuantidadeProdutoNoDepositoAsync(int produtoId)
        {
            var quantidade = await _context.ProdutosDeposito
                                        .Where(pd => pd.ProdutoId == produtoId)
                                        .SumAsync(pd => pd.Quantidade);
            return quantidade;
        }

        public async Task DepositProdutoAsync(int depositoId, ProdutoDeposito produtoDeposito)
        {
            // Verifica se o depósito existe
            var deposito = await _context.Depositos.FindAsync(depositoId);
            if (deposito == null)
            {
                throw new ArgumentException($"Depósito com ID {depositoId} não encontrado.");
            }

            // Verifica se o produto já está no depósito
            var existingProduto = await _context.ProdutosDeposito
                                                .Where(pd => pd.DepositoId == depositoId && pd.ProdutoId == produtoDeposito.ProdutoId)
                                                .FirstOrDefaultAsync();

            if (existingProduto != null)
            {
                // Se o produto já existe, apenas incrementa a quantidade
                existingProduto.Quantidade += produtoDeposito.Quantidade;
            }
            else
            {
                // Caso contrário, adiciona um novo registro de ProdutoDeposito
                _context.ProdutosDeposito.Add(produtoDeposito);
                deposito.ProdutosDeposito.Add(produtoDeposito);
            }

            await _context.SaveChangesAsync();
        }
    }
}
