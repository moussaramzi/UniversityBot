using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CoreBot.Services
{
    public static class ApiService<T>
    {
        private static readonly string BASE_URL = "https://lm-apiuniversityapi20241230183043.azurewebsites.net/api/";
        //private static readonly string BASE_URL = "https://localhost:7141/api/";

        private static readonly HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(60) };

        public static async Task<T> GetAsync(string endPoint)
        {
            try
            {
                string url = BASE_URL + endPoint;
                var response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var jsonData = await response.Content.ReadAsStringAsync();
                return !string.IsNullOrWhiteSpace(jsonData)
                    ? JsonConvert.DeserializeObject<T>(jsonData)
                    : throw new Exception("No content returned from the API.");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"HTTP Request Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected Error: {ex.Message}");
            }
        }

        public static async Task<TResult> PostAsync<TResult>(string endPoint, object data)
        {
            try
            {
                string url = BASE_URL + endPoint;
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);

                response.EnsureSuccessStatusCode();

                var jsonData = await response.Content.ReadAsStringAsync();
                return !string.IsNullOrWhiteSpace(jsonData)
                    ? JsonConvert.DeserializeObject<TResult>(jsonData)
                    : default; // Handle empty body responses gracefully.
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"HTTP Request Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected Error: {ex.Message}");
            }
        }

        public static async Task PutAsync(string endPoint, object data)
        {
            try
            {
                string url = BASE_URL + endPoint;
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var response = await client.PutAsync(url, content);

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"HTTP Request Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected Error: {ex.Message}");
            }
        }

        public static async Task DeleteAsync(string endPoint)
        {
            try
            {
                string url = BASE_URL + endPoint;
                var response = await client.DeleteAsync(url);

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"HTTP Request Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected Error: {ex.Message}");
            }
        }
    }
}
