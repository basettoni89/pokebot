using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PokeBot.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PokeBot.Controllers
{
    [Route("new-message")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly ILogger<BotController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IPokeRepository _pokeRepository;
        private readonly IConfiguration _configuration;

        public BotController(ILogger<BotController> logger,
            IHttpClientFactory clientFactory,
            IPokeRepository pokeRepository,
            IConfiguration configuration)
        {
            this._logger = logger;
            this._clientFactory = clientFactory;
            this._pokeRepository = pokeRepository;
            this._configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult> OnNewMessage(JObject update)
        {
            _logger.LogDebug($"Received [{update}]");

            var message = update["message"];

            string content = (string) message["text"];
            int chatId = (int)message["chat"]["id"];

            if(!string.IsNullOrEmpty(content) && chatId > 0)
            {
                var response = JObject.FromObject(
                    new
                    {
                        chat_id = chatId,
                        text = $"{content}. Ok, copy!"
                    });

                var responseString = response.ToString();

                _logger.LogDebug($"Sending [{responseString}]");

                string botToken = _configuration["Bot:Token"];

                var responseResult = await _clientFactory.CreateClient()
                    .PostAsync($"https://api.telegram.org/bot{botToken}/sendMessage",
                        new StringContent(responseString, Encoding.UTF8, "application/json"));

                _logger.LogDebug($"SendMessage result [{responseResult.StatusCode}]");
            }

            return Ok();
        }
    }
}
