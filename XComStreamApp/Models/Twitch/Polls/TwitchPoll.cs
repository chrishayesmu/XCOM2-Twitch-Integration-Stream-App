using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.Polls;

namespace XComStreamApp.Models.Twitch.Polls
{
    public class TwitchPoll
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("choices")]
        public PollChoice[] Choices { get; set; } = [];

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("duration")]
        public int DurationInSeconds { get; set; } // initial duration, doesn't change during the poll

        [JsonPropertyName("seconds_remaining")]
        public int SecondsRemaining { get; set; }

        public static TwitchPoll FromTwitchlibModel(Poll other)
        {
            return new TwitchPoll()
            {
                Id = other.Id,
                Title = other.Title,
                Choices = other.Choices.Select(c => new PollChoice()
                {
                    Id = c.Id,
                    Title = c.Title,
                    NumVotes = c.Votes
                }).ToArray(),
                Status = other.Status,
                DurationInSeconds = other.DurationSeconds,
                SecondsRemaining = GetSecondsRemaining(other)
            };
        }

        private static int GetSecondsRemaining(Poll other)
        {
            if (other.Status != "ACTIVE")
            {
                return 0;
            }

            int secondsSinceStart = (int) (DateTime.Now - other.StartedAt).TotalSeconds;
            return Math.Max(0, other.DurationSeconds - secondsSinceStart);
        }
    }
}
