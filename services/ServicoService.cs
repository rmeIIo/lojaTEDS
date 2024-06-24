using loja.models;
using Loja.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace loja.services
{
    public class ServicoService
    {
        private readonly LojaDbContext _context;

        public ServicoService(LojaDbContext context)
        {
            _context = context;
        }

        public async Task AddServicoAsync(Servico servico)
        {
            _context.Servicos.Add(servico);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Servico>> GetAllServicosAsync()
        {
            return await _context.Servicos.ToListAsync();
        }

        public async Task<Servico> GetServicoByIdAsync(int id)
        {
            return await _context.Servicos.FindAsync(id);
        }

        public async Task UpdateServicoAsync(Servico servico)
        {
            _context.Servicos.Update(servico);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Servico>> GetServicosContratadosPorClienteAsync(int clienteId)
        {
            // Consulte os contratos associados ao cliente
            var contratos = await _context.Contratos
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();

            // Obtenha os IDs dos serviços contratados
            var servicosIds = contratos.Select(c => c.ServicoId).ToList();

            // Consulte os serviços com base nos IDs
            var servicosContratados = await _context.Servicos
                .Where(s => servicosIds.Contains(s.Id))
                .ToListAsync();

            return servicosContratados;
        }
    }
}
