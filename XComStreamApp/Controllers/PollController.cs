#if DEBUG
#define MOCK_POLLS
#endif

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.Polls;
using TwitchLib.Api.Helix.Models.Polls.CreatePoll;
using XComStreamApp.Extensions.Twitchlib;
using XComStreamApp.Models;
using XComStreamApp.Models.Twitch.Polls;
using XComStreamApp.Models.XComMod;
using static XComStreamApp.Models.SystemEvent;

namespace XComStreamApp.Controllers
{
    [Route("api/poll")]
    public class PollController(ILogger<PollController> logger) : StreamAppControllerBase<PollController>(logger)
    {
#if MOCK_POLLS
        private static TwitchPoll? CurrentPoll;
        private static DateTime PollStartTime;
#endif

        [HttpGet]
        [Route("canCreate")]
        public ActionResult<bool> CanCreatePolls()
        {
            RecordRequest();

            return TwitchState.ConnectedUser.CanRunPolls();
        }

        [HttpGet]
        [Route("byId")]
        public async Task<ActionResult<TwitchPoll>> GetById([FromQuery] string id)
        {
            RecordRequest();

#if !MOCK_POLLS
            if (!CanUseTwitchApi())
            {
                return Unauthorized();
            }

            if (!TwitchState.ConnectedUser.CanRunPolls())
            {
                return BadRequest("Broadcaster must be affiliate or partner to run polls on their channel");
            }
#endif

#if MOCK_POLLS
            if (CurrentPoll?.Id == id)
            {
                return CurrentPoll;
            }

            return NotFound();
#else
            var response = await TwitchState.API.Helix.Polls.GetPollsAsync(TwitchState.ConnectedUser.Id, ids: [id]);

            return TwitchPoll.FromTwitchlibModel(response.Data[0]);
#endif

        }

        [HttpGet]
        [Route("current")]
        public async Task<ActionResult<TwitchPoll>> GetCurrentPoll()
        {
            RecordRequest();

#if !MOCK_POLLS
            if (!CanUseTwitchApi())
            {
                return Unauthorized();
            }

            if (!TwitchState.ConnectedUser.CanRunPolls())
            {
                return BadRequest("Broadcaster must be affiliate or partner to run polls on their channel");
            }
#endif

#if MOCK_POLLS
            if (CurrentPoll == null)
            {
                return NoContent();
            }

            int index = Random.Shared.Next(CurrentPoll.Choices.Length);
            CurrentPoll.Choices[index].NumVotes++;

            CurrentPoll.SecondsRemaining = Math.Max(0, CurrentPoll.DurationInSeconds - (int) (DateTime.Now - PollStartTime).TotalSeconds);

            if (CurrentPoll.SecondsRemaining == 0)
            {
                CurrentPoll.Status = "COMPLETED";
            }

            return CurrentPoll;
#else
            var response = await TwitchState.API.Helix.Polls.GetPollsAsync(TwitchState.ConnectedUser.Id, first: 1);

            return TwitchPoll.FromTwitchlibModel(response.Data[0]);
#endif
        }

        [HttpGet]
        [Route("create")]
        public async Task<ActionResult<TwitchPoll>> CreatePoll([FromQuery] string title, [FromQuery] string[] choices, [FromQuery] int duration, [FromQuery] int pointsPerVote)
        {
            RecordRequest();

#if !MOCK_POLLS
            if (!CanUseTwitchApi())
            {
                return Unauthorized();
            }

            if (!TwitchState.ConnectedUser.CanRunPolls())
            {
                return BadRequest("Broadcaster must be affiliate or partner to run polls on their channel");
            }
#endif

            if (string.IsNullOrEmpty(title) || title.Length > 60)
            {
                return BadRequest("Poll title must be between 1 and 60 characters in length");
            }

            if (choices.Length < 2 || choices.Length > 5)
            {
                return BadRequest("Polls must have between 2 and 5 choices");
            }

            if (choices.Any(c => string.IsNullOrWhiteSpace(c) || c.Length > 25))
            {
                return BadRequest("Poll choices must be between 1 and 25 characters in length");
            }

            if (duration < 15 || duration > 1800)
            {
                return BadRequest("Polls must have a duration between 15 and 1800 seconds");
            }

            if (pointsPerVote > 1000000)
            {
                return BadRequest("ChannelPointsPerVote must be <= 1000000");
            }

#if MOCK_POLLS
            Program.Form.AddEvent(new SystemEvent()
            {
                Description = "Created a fake Twitch poll",
                Type = EventType.GameEvent
            });

            CurrentPoll = new TwitchPoll()
            {
                Title = title,
                Id = Guid.NewGuid().ToString(),
                Status = "ACTIVE",
                DurationInSeconds = duration,
                SecondsRemaining = duration,
                Choices = choices.Select(c => new PollChoice()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = c,
                    NumVotes = 0
                }).ToArray()
            };

            PollStartTime = DateTime.Now;

            return CurrentPoll;
#else
            var choicesModel = choices.Select(c => new TwitchLib.Api.Helix.Models.Polls.CreatePoll.Choice() { Title = c }).ToArray();

            var request = new CreatePollRequest()
            {
                BroadcasterId = TwitchState.ConnectedUser!.Id,
                Title = title,
                Choices = choicesModel,
                ChannelPointsVotingEnabled = pointsPerVote > 0,
                ChannelPointsPerVote = pointsPerVote,
                DurationSeconds = duration
            };

            var response = await TwitchState.API.Helix.Polls.CreatePollAsync(request);

            // TODO: update the channel state with our new poll

            Program.Form.AddEvent(new SystemEvent()
            {
                Description = "Created a Twitch poll",
                Type = EventType.GameEvent
            });

            return TwitchPoll.FromTwitchlibModel(response.Data[0]);
#endif
        }

        [HttpGet]
        [Route("end")]
        public ActionResult EndPoll([FromQuery] string id)
        {
            RecordRequest();

#if !MOCK_POLLS
            if (!CanUseTwitchApi())
            {
                return Unauthorized();
            }

            if (!TwitchState.ConnectedUser.CanRunPolls())
            {
                return BadRequest("Broadcaster must be affiliate or partner to run polls on their channel");
            }
#endif

#if MOCK_POLLS
            if (CurrentPoll == null || CurrentPoll.Id != id || CurrentPoll.Status != "ACTIVE")
            {
                return BadRequest();
            }

            CurrentPoll = null;

            Program.Form.AddEvent(new SystemEvent()
            {
                Description = "Ended a fake Twitch poll",
                Type = EventType.GameEvent
            });
#else
            TwitchState.API.Helix.Polls.EndPollAsync(TwitchState.ConnectedUser.Id, id, TwitchLib.Api.Core.Enums.PollStatusEnum.TERMINATED);
#endif

            return Ok();
        }
    }
}
