using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PokeAPI;
using PokeBot.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PokeBot.Controllers
{
    [Route("new-message")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private const string LANGUAGE_CODE = "en";

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

            string pokemon = null;

            var regex = new Regex(".*\\[(.*)\\].*");
            if(regex.IsMatch(content))
            {
                pokemon = regex.Match(content).Groups[1].Value;
            }

            if (!string.IsNullOrEmpty(pokemon) && chatId > 0)
            {
                try
                {
                    pokemon = pokemon.ToLower();

                    PokemonSpecies ps = await DataFetcher.GetNamedApiObject<PokemonSpecies>(pokemon);

                    string name = ps.Names
                        .Where(x => x.Language.Name == LANGUAGE_CODE)
                        .Select(x => x.Name)
                        .FirstOrDefault();

                    string description = ps.FlavorTexts
                        .Where(x => x.Language.Name == LANGUAGE_CODE)
                        .Select(x => x.FlavorText)
                        .FirstOrDefault();

                    Pokemon p = await DataFetcher.GetNamedApiObject<Pokemon>(pokemon);

                    string image = p.Sprites.FrontMale;

                    string caption = $"*{name.ToUpper()}*\n{description}";

                    await SendPhotoResponse(image, caption, chatId);
                }
                catch (Exception)
                {
                    string text = "Something went wrong with your request. Please make sure that the pokemon name is right.";
                    await SendTextResponse(text, chatId);
                }
            }

            return Ok();
        }

        private async Task SendTextResponse(string text, int chatId)
        {
            var response = JObject.FromObject(
                new
                {
                    chat_id = chatId,
                    text = text,
                    parse_mode = "markdown"
                });

            var responseString = response.ToString();

            _logger.LogDebug($"Sending [{responseString}]");

            string botToken = _configuration["Bot:Token"];

            var responseResult = await _clientFactory.CreateClient()
                .PostAsync($"https://api.telegram.org/bot{botToken}/sendMessage",
                    new StringContent(responseString, Encoding.UTF8, "application/json"));

            _logger.LogDebug($"SendMessage result [{responseResult.StatusCode}]");
        }

        private async Task SendPhotoResponse(string imageUrl, string caption, int chatId)
        {
            var response = JObject.FromObject(
                new
                {
                    chat_id = chatId,
                    photo = imageUrl,
                    caption = caption,
                    parse_mode = "markdown"
                });

            var responseString = response.ToString();

            _logger.LogDebug($"Sending [{responseString}]");

            string botToken = _configuration["Bot:Token"];

            var responseResult = await _clientFactory.CreateClient()
                .PostAsync($"https://api.telegram.org/bot{botToken}/sendPhoto",
                    new StringContent(responseString, Encoding.UTF8, "application/json"));

            _logger.LogDebug($"SendMessage result [{responseResult.StatusCode}]");
        }
    }
}
