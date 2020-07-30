using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CorsAway.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class ProxyController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string url)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(new Uri(url));
            string result = await httpResponseMessage.Content.ReadAsStringAsync();
            return StatusCode((int) httpResponseMessage.StatusCode, result);
        }
    }
}