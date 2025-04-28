using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.XComMod
{
    [JsonDerivedType(typeof(ChannelPointRedeemEvent), typeDiscriminator: "channelPointRedeem")]
    [JsonDerivedType(typeof(ChatCommandEvent), typeDiscriminator: "chatCommand")]
    [JsonDerivedType(typeof(ChatDeletionEvent), typeDiscriminator: "chatDeletion")]
    [JsonDerivedType(typeof(CreatePollEvent), typeDiscriminator: "createPoll")]
    public abstract class GameEvent
    {
        // Replaces certain characters that are known to cause UE3's JSON parser to crash.
        protected void SafeStringSet(ref string field, string value)
        {
            field = value.Replace("]", "%5D").Replace("}", "%7D");
        }
    }
}
