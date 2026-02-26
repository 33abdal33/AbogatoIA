using GenerativeAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class GeminiClient : IGeminiClient
    {
        private readonly string _apiKey;
        private readonly ILogger<GeminiClient> _logger;
        private readonly string _systemInstruction = "Eres 'Abogato IA', un asistente legal experto. Tu objetivo es ayudar a usuarios a entender términos legales, resumir contratos y dar orientación general sobre leyes.";

        public GeminiClient(IConfiguration config, ILogger<GeminiClient> logger)
        {
            _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Falta la ApiKey de Gemini");
            _logger = logger;
        }
        public async Task<string> ConsultarAsync(string prompt, string modelo = "gemini-2.5-flash")
        {
            try
            {
                _logger.LogInformation($" [PLAN B ACTIVADO] Enviando consulta directa a Gemini (Modelo: {modelo})...");

                var googleAI = new GoogleAi(_apiKey);

                var model = googleAI.CreateGenerativeModel(modelo, systemInstruction: _systemInstruction);

                var response = await model.GenerateContentAsync(prompt);
                _logger.LogInformation($"✅ [PLAN B ÉXITO] Gemini respondió correctamente.");

                return response.Text() ?? "[Gemini devolvió una respuesta vacía]";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Error Interno Google SDK]: {ex.Message}");
                return $"[Error en Gemini: {ex.Message}]";
            }
        }
    }
}