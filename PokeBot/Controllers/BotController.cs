using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PokeBot.Controllers
{
    [Route("745900249:AAGN4eqAhz6NLEn5fh8GZLywacmaH3UMgI8/new-message")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private const string BOT_TOKEN = "745900249:AAGN4eqAhz6NLEn5fh8GZLywacmaH3UMgI8";

        private readonly ILogger<BotController> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public BotController(ILogger<BotController> logger,
            IHttpClientFactory clientFactory)
        {
            this._logger = logger;
            this._clientFactory = clientFactory;
        }

        [HttpPost]
        public async Task<ActionResult> OnNewMessage(JObject message)
        {
            _logger.LogDebug($"Received [{message}]");

            string content = (string) message["text"];
            int chatId = (int)message["chat"]["id"];

            if(!string.IsNullOrEmpty(content) && chatId > 0)
            {
                var response = JObject.FromObject(
                    new
                    {
                        chat = chatId,
                        text = $"{content}. Ok, copy!"
                    });

                var responseResult = await _clientFactory.CreateClient()
                    .PostAsync($"https://api.telegram.org/bot{BOT_TOKEN}/sendMessage",
                        new StringContent(response.ToString(), Encoding.UTF8, "application/json"));

                _logger.LogDebug($"SendMessage result [{responseResult.StatusCode}]");
            }

            return Ok();
        }
    }
}
