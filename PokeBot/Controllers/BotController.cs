using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokeBot.Controllers
{
    [Route("745900249:AAGN4eqAhz6NLEn5fh8GZLywacmaH3UMgI8/new-message")]
    [ApiController]
    public class BotController : ControllerBase
    {
        [HttpPost]
        public ActionResult<string> OnNewMessage()
        {
            return "I'm alive!";
        }
    }
}
