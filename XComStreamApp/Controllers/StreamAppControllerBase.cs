using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XComStreamApp.Controllers
{
    [ApiController]
    public abstract class StreamAppControllerBase<T>(ILogger<T> logger) : ControllerBase where T : StreamAppControllerBase<T>
    {
        protected ILogger<T> _logger = logger;

        protected bool CanUseTwitchApi()
        {
            bool canUse = TwitchState.API != null || TwitchState.ConnectedUser != null;

            if (!canUse)
            {
                _logger.LogWarning("Twitch API cannot be used currently. API == null? {IsTwitchApiNull} | ConnectedUser == null? {IsConnectedUserNull}", TwitchState.API == null, TwitchState.ConnectedUser == null);
            }

            return canUse;
        }

        protected void RecordRequest()
        {
            Program.Form.RequestReceivedFromGame();
        }
    }
}
