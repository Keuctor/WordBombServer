using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Common.Packets.Response
{
    public class UpdatePlayerInfoResponse
    {
        public int Id { get; set; }
        public short AvatarId { get; set; }
    }
}
