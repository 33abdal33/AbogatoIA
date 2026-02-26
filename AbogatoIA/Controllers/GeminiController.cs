using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;
using System.Threading.Tasks;

namespace AbogatoIA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly IPdfReaderService _pdfReaderService;
        private readonly IGeminiClient _geminiClient;

        public GeminiController(IAiService aiService, IPdfReaderService pdfReaderService, IGeminiClient geminiClient)
        {
            _aiService = aiService;
            _pdfReaderService = pdfReaderService;
            _geminiClient = geminiClient;
        }

        [HttpPost("consultar")]
        [Authorize]
        public async Task<IActionResult> Consultar([FromBody] ConsultaRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Mensaje))
            {
                return BadRequest(new { error = "El mensaje no puede estar vacío." });
            }

            var userId = User.FindFirst("uid")?.Value;

            if(string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "No se pudo identificar al usuario." });
            }

            try
            {
                var respuesta = await _aiService.ProcesarConsulta(request.Mensaje, userId);

                return Ok(new { respuesta });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = $"Error al procesar el debate legal: {ex.Message}" });
            }
        }
        [HttpPost("analizar-documentos")]
        [Authorize]
        public async Task<IActionResult> AnalizarDocumentos([FromForm] DocumentAnalysisRequest request)
        {
            // 1. Validaciones básicas
            if (request.Documentos == null || request.Documentos.Count == 0)
            {
                return BadRequest(new { error = "Debes subir al menos un documento PDF válido." });
            }

            try
            {
                string textoGiganteDeLosPdfs = await _pdfReaderService.ExtraerTextoDeMultiplesPdfsAsync(request.Documentos);

                if (string.IsNullOrWhiteSpace(textoGiganteDeLosPdfs))
                {
                    return BadRequest(new { error = "No se pudo extraer texto de los documentos. Asegúrate de que no sean imágenes escaneadas." });
                }

                string instruccion = request.InstruccionPersonalizada ?? "Por favor, actúa como un auditor legal senior. Lee detenidamente los siguientes documentos y genera un resumen ejecutivo completo, destacando las obligaciones clave, riesgos legales, fechas importantes y cualquier cláusula inusual.";

                string megaPrompt = $@"
                INSTRUCCIÓN PRINCIPAL: {instruccion}

                --- INICIO DEL PAQUETE DE DOCUMENTOS ---
                {textoGiganteDeLosPdfs}
                --- FIN DEL PAQUETE DE DOCUMENTOS ---

                RESPUESTA FINAL (Usa formato Markdown y sé muy detallado):
                ";

                var respuestaAnalisis = await _geminiClient.ConsultarAsync(megaPrompt, "gemini-2.5-flash");

                return Ok(new { analisis = respuestaAnalisis });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Ocurrió un error al procesar los documentos: {ex.Message}" });
            }
        }
    }

    public class ConsultaRequest
    {
        public string Mensaje { get; set; }
    }
}