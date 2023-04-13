using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Common.Packets.Response
{
    public class SubmitWordResponse
    {
        /// <summary>
        /// 0 - no fail
        /// 1 - not found word
        /// 2 - already entered
        /// 3 - target length is not valid
        /// </summary>
        public byte FailType { get; set; }
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string Word { get; set; }
    }
}
