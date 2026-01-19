using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gwent.Net
{
    public class CardDto
    {
        public string Nom { get; set; }
        public int Puissance { get; set; }
        public string ImagePath { get; set; }
        public int Type { get; set; }      // mappe TypeCarte à int
        public int Pouvoir { get; set; }   // mappe PouvoirSpecial à int
    }
}