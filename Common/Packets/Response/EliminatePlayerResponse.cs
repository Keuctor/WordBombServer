using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Common.Packets.Response
{
    public class EliminatePlayerResponse
    {
        public int Id { get; set; }
        public byte Reason { get; set; }
    }
}
