using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Common.Packets.Response
{
    public class ChatMessageResponse
    {
        public int Id { get; set; }
        public string Message { get; set; }
    }
}
