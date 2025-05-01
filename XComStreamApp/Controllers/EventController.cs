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
        /// If an event lives longer than this without being requested by the game, it will be aged out.
        /// This avoids lots of events queuing up while the game is paused, or in a loading screen.
        /// </summary>
        public static readonly TimeSpan EventTtl = TimeSpan.FromSeconds(60);

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
            var now = DateTime.Now;

            while (TwitchState.PendingGameEvents.TryDequeue(out var e))
            {
                if (now - e.TimeCreated > EventTtl)
                {
                    continue;
                }

                events.Add(e);
            }

            return events;
        }
    }
}
