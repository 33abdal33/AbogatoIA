using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IZohoCrmService
    {
        Task<string> ObtenerTokenFrescoAsync();
        Task<string> ObtenerPosiblesClientesAsync();
        Task<bool> CrearPosibleClienteAsync(string nombre, string apellido, string correo, string telefono);
        Task<string> ObtenerRegistrosPorModuloAsync(string nombreModulo, string campos);
    }
}