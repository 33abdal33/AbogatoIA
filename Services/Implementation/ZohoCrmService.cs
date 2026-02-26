using Microsoft.Extensions.Configuration;
using Services.Interfaces;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class ZohoCrmService : IZohoCrmService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        public ZohoCrmService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> ObtenerTokenFrescoAsync()
        {
            // Leemos las credenciales
            string clientId = _config["Zoho:ClientId"];
            string clientSecret = _config["Zoho:ClientSecret"];
            string refreshToken = _config["Zoho:RefreshToken"];
            string url = $"https://accounts.zoho.com/oauth/v2/token?refresh_token={refreshToken}&client_id={clientId}&client_secret={clientSecret}&grant_type=refresh_token";

            var respuesta = await _httpClient.PostAsync(url, null);

            if (respuesta.IsSuccessStatusCode)
            {
                string jsonRespuesta = await respuesta.Content.ReadAsStringAsync();

                // Extraemos solo el texto del nuevo token del JSON
                using JsonDocument doc = JsonDocument.Parse(jsonRespuesta);
                return doc.RootElement.GetProperty("access_token").GetString();
            }

            throw new Exception("Error crítico: No se pudo refrescar el token de Zoho. Verifica tus credenciales.");
        }

        // --- 1. LEER POSIBLES CLIENTES (LEADS) ---
        public async Task<string> ObtenerPosiblesClientesAsync()
        {
            string tokenFresco = await ObtenerTokenFrescoAsync();
            string url = "https://www.zohoapis.com/crm/v8/Leads?fields=First_Name,Last_Name,Email,Phone";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", tokenFresco);

            var respuesta = await _httpClient.GetAsync(url);

            if (respuesta.IsSuccessStatusCode)
            {
                return await respuesta.Content.ReadAsStringAsync();
            }
            else
            {
                string errorDeZoho = await respuesta.Content.ReadAsStringAsync();
                throw new Exception($"ZOHO RECHAZÓ LA PETICIÓN. Código: {respuesta.StatusCode} | Detalle: {errorDeZoho}");
            }
        }

        // --- 2. CREAR POSIBLE CLIENTE (LEAD) ---
        public async Task<bool> CrearPosibleClienteAsync(string nombre, string apellido, string correo, string telefono)
        {
            string tokenFresco = await ObtenerTokenFrescoAsync();

            string url = "https://www.zohoapis.com/crm/v8/Leads";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", tokenFresco);

            var nuevoCliente = new
            {
                data = new[]
                {
                    new
                    {
                        First_Name = nombre,
                        Last_Name = apellido,
                        Email = correo,
                        Phone = telefono
                    }
                }
            };

            string json = JsonSerializer.Serialize(nuevoCliente);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");

            var respuesta = await _httpClient.PostAsync(url, contenido);

            return respuesta.IsSuccessStatusCode;
        }
        public async Task<string> ObtenerRegistrosPorModuloAsync(string nombreModulo, string campos)
        {
            string tokenFresco = await ObtenerTokenFrescoAsync();

            // Armamos la URL dinámicamente según lo que nos pida el controlador
            string url = $"https://www.zohoapis.com/crm/v8/{nombreModulo}?fields={campos}";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", tokenFresco);

            var respuesta = await _httpClient.GetAsync(url);

            if (respuesta.IsSuccessStatusCode)
            {
                return await respuesta.Content.ReadAsStringAsync();
            }
            else
            {
                string errorDeZoho = await respuesta.Content.ReadAsStringAsync();
                throw new Exception($"ZOHO RECHAZÓ LA PETICIÓN AL MÓDULO '{nombreModulo}'. Código: {respuesta.StatusCode} | Detalle: {errorDeZoho}");
            }
        }
    }
    
}