using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XComStreamApp.Models.Twitch;

namespace XComStreamApp.Controllers
{
    [Route("api/channelState")]
    public class ChannelStateController(ILogger<ChannelStateController> logger) : StreamAppControllerBase<ChannelStateController>(logger)
    {
        [HttpGet]
        public ActionResult<ChannelState?> GetChannelState()
        {
            RecordRequest();

            if (!CanUseTwitchApi())
            {
                return Unauthorized();
            }

            return TwitchState.Channel;
        }
    }
}
