using System;
using System.Collections.Generic;

namespace Gwent.Net
{
    public class StartGameDto
    {
        public int HostDeckIndex { get; set; }
        public int ClientDeckIndex { get; set; }
        public int Seed { get; set; }
        public int StartingPlayerIndex { get; set; }
        public System.Collections.Generic.List<CardDto> HostMain { get; set; }
        public System.Collections.Generic.List<CardDto> HostDeck { get; set; }
        public System.Collections.Generic.List<CardDto> ClientMain { get; set; }
        public System.Collections.Generic.List<CardDto> ClientDeck { get; set; }
    }
}
