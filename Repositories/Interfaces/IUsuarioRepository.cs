using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<ApplicationUser> ObtenerPorIdAsync(string userId);
        Task ActualizarConsumoTokensAsync(string userId, int tokensConsumidos);
    }
}
