using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace AbogatoIA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrmController : ControllerBase
    {
        private readonly IZohoCrmService _zohoService;

        public CrmController(IZohoCrmService zohoService)
        {
            _zohoService = zohoService;
        }

        [HttpGet("clientes")]
        public async Task<IActionResult> ObtenerClientesDeZoho()
        {
            try
            {
                var jsonClientes = await _zohoService.ObtenerPosiblesClientesAsync();

                if (string.IsNullOrEmpty(jsonClientes))
                {
                    return BadRequest(new { error = "No se pudo obtener la lista de clientes de Zoho." });
                }

                return Content(jsonClientes, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpPost("cliente")]
        public async Task<IActionResult> CrearClienteEnZoho(string nombre, string apellido, string correo, string telefono)
        {
            try
            {
                bool exito = await _zohoService.CrearPosibleClienteAsync(nombre, apellido, correo, telefono);

                if (exito)
                {
                    return Ok(new { mensaje = "¡Cliente creado exitosamente en Zoho CRM!" });
                }

                return BadRequest(new { error = "Hubo un problema al intentar crear el cliente en Zoho." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet("contactos")]
        public async Task<IActionResult> ObtenerContactos()
        {
            try
            {
                var json = await _zohoService.ObtenerRegistrosPorModuloAsync("Contacts", "First_Name,Last_Name,Email,Phone");
                return Content(json, "application/json");
            }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }
        [HttpGet("tratos")]
        public async Task<IActionResult> ObtenerTratos()
        {
            try
            {
                var json = await _zohoService.ObtenerRegistrosPorModuloAsync("Deals", "Deal_Name,Amount,Stage,Closing_Date");
                return Content(json, "application/json");
            }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }
        [HttpGet("tareas")]
        public async Task<IActionResult> ObtenerTareas()
        {
            try
            {
                var json = await _zohoService.ObtenerRegistrosPorModuloAsync("Tasks", "Subject,Status,Priority");
                return Content(json, "application/json");
            }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }
    }
}