using Models;
using Persistence;
using Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser> ObtenerPorIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task ActualizarConsumoTokensAsync(string userId, int tokensConsumidos)
        {
            var usuario = await _context.Users.FindAsync(userId);
            if (usuario != null)
            {
                usuario.TokenUsados += tokensConsumidos;
                await _context.SaveChangesAsync();
            }
        }
    }
}