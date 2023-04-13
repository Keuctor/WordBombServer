

namespace WordBombServer.Common.Packets.Request
{
    public class SuggestWordRequest
    {
        public byte Language { get; set; }
        public string Word { get; set; }
    }
}
