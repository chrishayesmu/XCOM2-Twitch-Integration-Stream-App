using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XComStreamApp.Models.XComMod
{
    [JsonDerivedType(typeof(ChatCommandEvent), typeDiscriminator: "chatCommand")]
    [JsonDerivedType(typeof(ChatDeletionEvent), typeDiscriminator: "chatDeletion")]
    [JsonDerivedType(typeof(CreatePollEvent), typeDiscriminator: "createPoll")]
    public abstract class GameEvent
    {
    }
}
