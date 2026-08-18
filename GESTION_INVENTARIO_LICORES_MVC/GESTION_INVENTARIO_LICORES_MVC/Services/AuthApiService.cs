using GESTION_INVENTARIO_LICORES_MVC.DTOs.Request;
using GESTION_INVENTARIO_LICORES_MVC.DTOs.Response;
using Newtonsoft.Json;
using System.Text;

namespace GESTION_INVENTARIO_LICORES_MVC.Services
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _httpClient;

        public AuthApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginRespDto?> LoginAsync(LoginReqDto request)
        {
            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("Auth/login", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<LoginRespDto>(jsonString);
        }
    }
}