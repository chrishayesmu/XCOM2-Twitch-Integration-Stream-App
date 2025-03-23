using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XComStreamApp.Models.XComMod;

namespace XComStreamApp.Controllers
{
    [Route("api/events")]
    public class EventController(ILogger<EventController> logger) : StreamAppControllerBase<EventController>(logger)
    {
        /// <summary>
        /// Gets all of the game events which are pending processing by the game.
        /// This clears the event queue in the process.
        /// </summary>
        /// <returns>All pending game events, if any.</returns>
        [HttpGet]
        [Route("pending")]
        public ActionResult<IList<GameEvent>> GetPendingEvents()
        {
            RecordRequest();

            var events = new List<GameEvent>();

            while (TwitchState.PendingGameEvents.TryDequeue(out var e))
            {
                events.Add(e);
            }

            return events;
        }
    }
}
