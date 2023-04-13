using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Common.Packets.Response
{
    public class RoomSettingsChangedResponse
    {
        public byte NewMode { get; set; }
        public byte NewLanguage { get; set; }
        public byte NewSpeed { get; set; }
        public bool NewLobbyIsPrivate { get; set; }
    }
}
