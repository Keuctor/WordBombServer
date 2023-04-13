using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Common.Packets.Request
{
    public class ChangeRoomSettingRequest
    {
        public byte Mode { get; set; }
        public byte Language { get; set; }
        public byte Speed { get; set; }
        public bool IsPrivate { get; set; }
    }
}