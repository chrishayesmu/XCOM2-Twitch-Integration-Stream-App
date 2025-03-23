using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XComStreamApp.Models.Twitch.Chat;
using XComStreamApp.Services;

namespace XComStreamApp.Controllers
{
    [Route("api/chat")]
    public class ChatController(ILogger<ChatController> logger) : StreamAppControllerBase<ChatController>(logger)
    {
        [HttpGet]
        [Route("chatters")]
        public ActionResult<IList<TwitchUser>> GetChatters()
        {
            RecordRequest();

            if (TwitchState.Channel == null)
            {
                return BadRequest("Twitch connection not initialized yet");
            }

            return Ok(TwitchState.Channel.Chatters);
        }
    }
}
