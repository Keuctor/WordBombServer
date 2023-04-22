using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Common.Packets.Response
{
    public class PlayerLoadedResponse
    {
        public byte LoadedPlayerCount { get; set; }
        public byte TotalPlayer { get; set; }
    }
}
