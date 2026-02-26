using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class OpenRouterService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly ILogger<OpenRouterService> _logger;
        private readonly IGeminiClient _geminiClient;

        public OpenRouterService(IConfiguration config, IUsuarioRepository usuarioRepo, ILogger<OpenRouterService> logger, IGeminiClient geminiClient)
        {
            _httpClient = new HttpClient();

            _httpClient.Timeout = TimeSpan.FromMinutes(5);
            _apiKey = config["OpenRouter:ApiKey"];
            _usuarioRepo = usuarioRepo;
            _logger = logger;
            _geminiClient = geminiClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:8080");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "Abogato IA");
        }

        private async Task<string> LlamarModelo(string prompt, string modeloPrincipal)
        {
            var requestBody = new
            {
                model = modeloPrincipal,
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 3000 
            };

            var response = await _httpClient.PostAsJsonAsync("https://openrouter.ai/api/v1/chat/completions", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                return $"[Error en {modeloPrincipal}: {response.StatusCode}]";
            }

            var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>();
            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "[El modelo devolvió una respuesta vacía]";
        }
        public async Task<string> ConsultarConDebateLegal(string mensajeUsuario)
        {
            try
            {
                //var t1 = _geminiClient.ConsultarAsync(mensajeUsuario); // ✅ CORRECTO: Limpio y delegando la decisión
                var t1 = LlamarModelo(mensajeUsuario, "google/gemma-3-27b-it:free");
                var t2 = LlamarModelo(mensajeUsuario, "arcee-ai/trinity-mini:free");
                var t3 = LlamarModelo(mensajeUsuario, "stepfun/step-3.5-flash:free");

                await Task.WhenAll(t1, t2, t3);


                _logger.LogInformation($"[IA 1]: {LimitarString(t1.Result, 1000)}");
                _logger.LogInformation($"[AI 2]: {LimitarString(t2.Result, 1000)}");
                _logger.LogInformation($"[AI 3]: {LimitarString(t3.Result, 1000)}");
                // ------------------------------------------------

                // Fase 2: El Juez
                string promptJuez = $@"
                Eres un Abogado Senior en Perú. Revisa estas 3 propuestas legales:
                ANALISTA 1: {t1.Result}
                ANALISTA 2: {t2.Result}
                ANALISTA 3: {t3.Result}

                Genera la respuesta legal definitiva para el usuario: {mensajeUsuario}";
                _logger.LogInformation($"[Prompt para el Juez]: {LimitarString(promptJuez, 1000)}");
                return await LlamarModelo(promptJuez, "openrouter/aurora-alpha");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en el orquestador de AbogatoIA: {ex.Message}");
            }
        }

        private string LimitarString(string texto, int maxCaracteres)
        {
            if (string.IsNullOrEmpty(texto)) return "[Vacío]";
            return texto.Length <= maxCaracteres ? texto : texto.Substring(0, maxCaracteres) + "...";
        }

        public async Task<string> ProcesarConsulta(string mensaje, string userId)
        {
            // 1. Usamos el repositorio para obtener datos
            var usuario = await _usuarioRepo.ObtenerPorIdAsync(userId);

            if (usuario == null)
            {
                _logger.LogError($"❌ Error: No se encontró al usuario con ID {userId}");
                return "Error: Usuario no encontrado.";
            }

            // Unificamos el límite a 50 (igual que en tu AuthController)
            int limiteGratis = 50;
            bool esPro = usuario.Plan == "PRO";
            bool tieneSaldo = usuario.TokenUsados < limiteGratis;

            if (esPro || tieneSaldo)
            {
                // ✨ LOG: Avisamos que hay saldo y arranca el debate en OpenRouter
                _logger.LogInformation($"⚖️ [MODO DEBATE] Usuario {usuario.Email} tiene saldo (Usados: {usuario.TokenUsados}/{limiteGratis}). Arrancando orquestación de 3 IAs...");

                var respuesta = await ConsultarConDebateLegal(mensaje);

                if (!esPro)
                {
                    // Restamos el token
                    await _usuarioRepo.ActualizarConsumoTokensAsync(userId, 1);
                    _logger.LogInformation($"🪙 Se cobró 1 token a {usuario.Email}.");
                }

                return respuesta;
            }
            else
            {
                // ✨ LOG: Avisamos que NO hay saldo y nos vamos por el Plan B
                _logger.LogWarning($"⚠️ [MODO FALLBACK] Usuario {usuario.Email} superó el límite gratuito. Redirigiendo a Gemini directo...");

                return await _geminiClient.ConsultarAsync(mensaje);
            }
        }
    }
}

