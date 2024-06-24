using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using loja.models;
using Loja.Data;

namespace loja.services
{
    public class VendaService
    {
        private readonly LojaDbContext _context;

        public VendaService(LojaDbContext context)
        {
            _context = context;
        }

        public async Task AddVendaAsync(Venda venda)
        {
            var cliente = await _context.Clientes.FindAsync(venda.ClienteId);
            var produto = await _context.Produtos.FindAsync(venda.ProdutoId);

            if (cliente == null || produto == null)
            {
                throw new Exception("Cliente ou produto não encontrado.");
            }

            var produtoDeposito = await _context.ProdutosDeposito
                                                .FirstOrDefaultAsync(pd => pd.ProdutoId == venda.ProdutoId && pd.DepositoId == 1); // Presume um único depósito
            if (produtoDeposito != null)
            {
                if (produtoDeposito.Quantidade < venda.QuantidadeVendida)
                {
                    throw new Exception("Estoque insuficiente.");
                }

                produtoDeposito.Quantidade -= venda.QuantidadeVendida;
            }
            else
            {
                throw new Exception("Produto não encontrado no depósito.");
            }

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Venda>> GetVendasPorProdutoDetalhadaAsync(int produtoId)
        {
            return await _context.Vendas
                                .Include(v => v.Cliente)
                                .Include(v => v.Produto)
                                .Where(v => v.ProdutoId == produtoId)
                                .ToListAsync();
        }

        public async Task<List<Venda>> GetVendasPorProdutoSumarizadaAsync(int produtoId)
        {
            return await _context.Vendas
                                .Where(v => v.ProdutoId == produtoId)
                                .GroupBy(v => new { v.ProdutoId, v.Produto.Nome })
                                .Select(g => new Venda
                                {
                                    ProdutoId = g.Key.ProdutoId,
                                    Produto = new Produto { Nome = g.Key.Nome },
                                    QuantidadeVendida = g.Sum(v => v.QuantidadeVendida),
                                    PrecoUnitario = g.Sum(v => v.QuantidadeVendida * v.PrecoUnitario)
                                })
                                .ToListAsync();
        }

        public async Task<List<Venda>> GetVendasPorClienteDetalhadaAsync(int clienteId)
        {
            return await _context.Vendas
                                .Include(v => v.Cliente)
                                .Include(v => v.Produto)
                                .Where(v => v.ClienteId == clienteId)
                                .ToListAsync();
        }

        public async Task<List<Venda>> GetVendasPorClienteSumarizadaAsync(int clienteId)
        {
            return await _context.Vendas
                                .Where(v => v.ClienteId == clienteId)
                                .GroupBy(v => new { v.ClienteId, v.Cliente.Nome })
                                .Select(g => new Venda
                                {
                                    ClienteId = g.Key.ClienteId,
                                    Cliente = new Cliente { Nome = g.Key.Nome },
                                    QuantidadeVendida = g.Sum(v => v.QuantidadeVendida),
                                    PrecoUnitario = g.Sum(v => v.QuantidadeVendida * v.PrecoUnitario)
                                })
                                .ToListAsync();
        }
    }
}
