using GenerativeAI;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class GeminiService : IGeminiService
    {
        private readonly string _apiKey;
        private readonly string _systemInstruction = "Eres 'Abogato IA', un asistente legal experto. " +
            "Tu objetivo es ayudar a usuarios a entender términos legales, resumir contratos y dar orientación general " +
            "sobre leyes. Siempre aclara que no reemplazas a un abogado humano.";

        public GeminiService(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"];
        }

        public async Task<string> ConsultarAbogadoIA(string consultaUsuario)
        {
            var googleAi = new GoogleAi(_apiKey);

            var model = googleAi.CreateGenerativeModel(
                "gemini-1.5-flash",
                systemInstruction: _systemInstruction
            );

            try
            {
                var response = await model.GenerateContentAsync(consultaUsuario);

                return response.Text();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en Gemini: {ex.Message}");
            }
        }
    }
}