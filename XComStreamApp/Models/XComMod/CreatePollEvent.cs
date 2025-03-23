using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XComStreamApp.Models.XComMod
{
    public class CreatePollEvent : GameEvent
    {
        public string PollId { get; set; } = "";
    }
}
