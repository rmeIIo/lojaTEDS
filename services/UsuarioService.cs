using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using loja.models;
using Loja.Data;
using System.ComponentModel.DataAnnotations;

namespace loja.services
{
    public class UsuarioService
    {
        public readonly LojaDbContext _context;

        public UsuarioService(LojaDbContext context)
        {
            _context = context;
        }

        public async Task AddUsuarioAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

        }

        public async Task<List<Usuario>> GetAllUsuariosAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string senha)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email && u.Senha == senha);
            return usuario != null;
        }
    }
}