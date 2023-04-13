

namespace WordBombServer.Common.Packets.Response
{
    public class TurnChangedResponse
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string NewWordPart { get; set; }
        public byte TargetLength { get; set; }
        public int Timer { get; set; }
        public short Round { get; set; }
    }
}
