using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Admin_Component.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;


namespace Admin_Component.Connector
{
    // Client class responsible for interacting with the Ollama API
    public class OllamaAdminConnector : IOllamaAdminConnector
    {
        // HttpClient instance to manage HTTP calls
        private readonly HttpClient _httpClient;

        // Constructor to initialize HttpClient via Dependency Injection
        public OllamaAdminConnector(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        // Calling /api/tags 
        public async Task<InstalledModelsResponse> GetInstalledModelsAsync()
        {
            // GET request to the /api/tags 
            var JsonResponse = await _httpClient.GetAsync(new Uri(_httpClient.BaseAddress, "api/tags"));
            JsonResponse.EnsureSuccessStatusCode();

            var responseString = await JsonResponse.Content.ReadAsStringAsync();

            // Deserialize the response into a list of Model 
            InstalledModelsResponse modelsList = JsonSerializer.Deserialize<InstalledModelsResponse>(responseString);

            return modelsList;
        }
    }

















    // Extension method to register OllamaClient as a service
    public static class ServiceExtensions
    {
        public static void AddOllamaClient(this IServiceCollection services)
        {
            // Configure HttpClient for the OllamaClient
            services.AddHttpClient<OllamaAdminConnector>(client =>
            {
                client.BaseAddress = new Uri("http://127.0.0.1:11434"); // Set base URL for API
                client.DefaultRequestHeaders.Accept.Clear();            // Clear default headers
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // Add JSON content type header
            });
        }
    }
}
