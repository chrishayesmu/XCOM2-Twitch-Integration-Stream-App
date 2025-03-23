using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XComStreamApp.Controllers
{
    [Route("api/auth")]
    public class AuthController(ILogger<AuthController> logger) : StreamAppControllerBase<AuthController>(logger)
    {
        [HttpGet]
        [Route("twitchApi")]
        public ActionResult<bool> IsAuthenticatedWithTwitchApi()
        {
            RecordRequest();

            return CanUseTwitchApi();
        }
    }
}
