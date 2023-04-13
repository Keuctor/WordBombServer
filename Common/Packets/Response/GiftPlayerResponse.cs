using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Common.Packets.Response
{
    public class GiftPlayerResponse
    {
        public int Sender { get; set; }
        public int Target { get; set; }
    }
}
