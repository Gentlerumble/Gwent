using System;

namespace Gwent.Net
{
    public enum MessageType
    {
        Connect,
        DeckChoice,
        StartGame,
        GameState,
        PlayCard,
        Pass,
        Ack,
        Error,
        TurnSwitched,
    }

    public class NetMessage
    {
        public MessageType Type { get; set; }
        public string Payload { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
