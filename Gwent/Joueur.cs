using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gwent.Jeu;

namespace Gwent
{
    // Représente un joueur dans le jeu Gwent. 
    // Gère son deck, sa main, son cimetière et son pouvoir passif de faction.
    public class Joueur
    {
        public string Nom { get; set; }
        public List<Carte> Deck { get; set; }
        public List<Carte> Main { get; set; }

        public List<Carte> Cimetiere { get; set; }

        public PouvoirPassifDeck PouvoirPassif { get; set; }


        public Joueur(string nom)
        {
            Nom = nom;
            Deck = new List<Carte>();
            Main = new List<Carte>();
            Cimetiere = new List<Carte>();
        }

        public void Piocher()
        {
            if (Deck != null && Deck.Count > 0)
            {
                Carte carte = Deck[0];
                Main.Add(carte);
                Deck.RemoveAt(0);
            }
        }

        public void envoyerAuCimetiere(Carte carte)
        {
            if (Main.Contains(carte))
            {
                Main.Remove(carte);
            }
            Cimetiere.Add(carte);
        }
    }
}
