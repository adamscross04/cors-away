using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Net;

namespace CorsAway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProxyController : ControllerBase
    {
        private const string BackupLocation = "C:\\temp";

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string url)
        {
            string hash = GetSha256(url);

            string fileName = $"{BackupLocation}\\{hash}";
            
            if (System.IO.File.Exists(fileName))
            {
                
                FileInfo fileInfo = new FileInfo(fileName);
                
                if (fileInfo.CreationTime >= DateTime.Now.Subtract(TimeSpan.FromMinutes(5)))
                {
                    string contents = System.IO.File.ReadAllText($"{BackupLocation}\\{hash}");
                    return Ok(contents);
                }

                System.IO.File.Delete(fileName);
            }

            HttpResponseMessage httpResponseMessage = await GetSuccessResponseAndCache(url, fileName);
            string result = await httpResponseMessage.Content.ReadAsStringAsync();
            return StatusCode((int) httpResponseMessage.StatusCode, result);
        }

        private async Task<HttpResponseMessage> GetSuccessResponseAndCache(string url, string fileName)
        {
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(new Uri(url));
            
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                await using StreamWriter writer = new StreamWriter(fileName);
                string result = await httpResponseMessage.Content.ReadAsStringAsync();
                await writer.WriteAsync(result);
            }
            else if (httpResponseMessage.StatusCode == HttpStatusCode.Forbidden)
            {
                return await GetSuccessResponseAndCache(url, fileName);
            }
            
            return httpResponseMessage;
        }


        private string GetSha256(string randomString)
        {
            SHA256Managed crypt = new SHA256Managed();
            string hash = string.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));

            return crypto.Aggregate(hash, (current, theByte) => current + theByte.ToString("x2"));
        }
    }
}