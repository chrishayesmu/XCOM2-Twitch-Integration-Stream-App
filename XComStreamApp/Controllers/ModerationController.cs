using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.Moderation.BanUser;

namespace XComStreamApp.Controllers
{
    [Route("api/moderation")]
    public class ModerationController(ILogger<ModerationController> logger) : StreamAppControllerBase<ModerationController>(logger)
    {
        [HttpGet]
        [Route("timeout")]
        public async Task<ActionResult> TimeOutUser([FromQuery] string viewerLogin, [FromQuery] int durationInSeconds)
        {
            RecordRequest();

            if (!CanUseTwitchApi())
            {
                return Unauthorized();
            }

            // Need to map from the viewer's login to their user ID. Try using the channel state first
            var user = TwitchState.Channel.Chatters.FirstOrDefault(user => string.Equals(user.UserLogin, viewerLogin, StringComparison.OrdinalIgnoreCase));
            string? userId = user?.UserId;

            // If that fails, make a separate request for it
            if (userId == null)
            {
                _logger.LogInformation("Didn't find viewer {viewerLogin} in channel state; making a separate request to get user ID", viewerLogin);

                var getUsersResponse = await TwitchState.API.Helix.Users.GetUsersAsync(logins: [viewerLogin]);

                if (getUsersResponse.Users.Length > 0) {
                    userId = getUsersResponse.Users[0].Id;
                    _logger.LogInformation("Mapped user to ID {userId}", userId);
                }
            }

            if (userId == null)
            {
                _logger.LogError("Could not map viewer {viewerLogin} to a user ID! Unable to timeout user.", viewerLogin);
                return NotFound();
            }

            var request = new BanUserRequest()
            {
                Duration = durationInSeconds,
                UserId = userId,
                Reason = "XCOM 2 Twitch Integration"
            };

            var banUserResponse = await TwitchState.API.Helix.Moderation.BanUserAsync(TwitchState.ConnectedUser.Id, TwitchState.ConnectedUser.Id, request);

            if (banUserResponse.Data.Length == 0)
            {
                return StatusCode(500);
            }

            return Ok();
        }
    }
}
