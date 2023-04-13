using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Common.Packets.Response
{
    public class JoinRoomResponse
    {
        public string RoomCode { get; set; }
        public string RoomTitle { get; set; }
        public byte GameMode { get; set; }
        public byte GameLanguage { get; set; }
        public byte GameSpeed { get; set; }
        public bool IsPrivate { get; set; }
        public int HostId { get; set; }
        public Player[] Players { get; set; }
    }
}
