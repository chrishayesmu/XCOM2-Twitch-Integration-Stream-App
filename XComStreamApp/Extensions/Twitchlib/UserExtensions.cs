using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace XComStreamApp.Extensions.Twitchlib
{
    public static class UserExtensions
    {
        public static bool CanRunPolls(this User user)
        {
            return user.IsAffiliate() || user.IsPartner();
        }

        public static bool IsMonetizable(this User user)
        {
            return user.IsAffiliate() || user.IsPartner();
        }

        public static bool IsAffiliate(this User user)
        {
            return user.BroadcasterType == "affiliate";
        }

        public static bool IsPartner(this User user)
        {
            return user.BroadcasterType == "partner";
        }
    }
}
