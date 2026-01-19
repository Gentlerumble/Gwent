using System;
using System.Collections.Generic;

namespace Gwent.Net
{
    public class PlayerDto
    {
        public string Name { get; set; }
        public int DeckCount { get; set; }
        public int HandCount { get; set; }
        public int Score { get; set; }
        public List<string> HandCardNames { get; set; } = new List<string>();
        public List<string> BoardCardNames { get; set; } = new List<string>();
    }

    public class GameStateDto
    {
        public PlayerDto Player1 { get; set; }
        public PlayerDto Player2 { get; set; }
        public int CurrentTurnPlayerIndex { get; set; }
        public bool RoundOver { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;


        public bool EstChargementSauvegarde { get; set; } = false;

        public GameSaveDto SaveData { get; set; }
        public int AssignedPlayerIndex { get; set; }
    }
}