using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomaruBot.ChatBot;
using KomaruBot.Common;
using KomaruBot.WebAPI.Helpers;
using KomaruBot.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KomaruBot.WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class KeepAliveController : Controller
    {

        public KeepAliveController()
        {

        }

        [HttpGet]
        public ActionResult Get()
        {
            return StatusCode((int)System.Net.HttpStatusCode.NoContent);
        }
    }
}
