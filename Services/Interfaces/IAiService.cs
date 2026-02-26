using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IAiService
    {
        // Este método orquestará el debate
        Task<string> ConsultarConDebateLegal(string mensajeUsuario);
        Task<string> ProcesarConsulta(string mensaje, string userId);
    }
}
