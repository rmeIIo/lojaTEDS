using loja.models;
using Loja.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Loja.Services
{
    public class ContratoService
    {
        private readonly LojaDbContext _dbContext;

        public ContratoService(LojaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddContratoAsync(Contrato contrato)
        {
            _dbContext.Contratos.Add(contrato);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Contrato>> GetAllContratosAsync()
        {
            return await _dbContext.Contratos.ToListAsync();
        }

        public async Task<Contrato> GetContratoByIdAsync(int id)
        {
            return await _dbContext.Contratos.FindAsync(id);
        }
    }
}
