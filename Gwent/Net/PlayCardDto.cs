using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gwent.Net
{
    public class PlayCardDto
    {
        public string CardName { get; set; }
        public int PlayerIndex { get; set; }       // 0:J1, 1:J2
        public string Zone { get; set; }           // "Melee","Distance","Siege","EffetMelee","EffetDistance","EffetSiege","MeteoMelee","MeteoDistance","MeteoSiege"
        public bool IsWeather { get; set; }
        public int Power { get; set; }
        public string ImagePath { get; set; }
        public int Type { get; set; }              // (int)TypeCarte
        public int Pouvoir { get; set; }           // (int)PouvoirSpecial
    }
}
