using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Common.Packets.Request
{
    public class UpdateDisplayNameResponse
    {
        public string DisplayName { get; set; }
        public int EmeraldCount { get; set; }
    }
}
