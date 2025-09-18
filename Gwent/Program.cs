using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gwent
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Affiche d'abord le formulaire de choix de deck
            using (var formDeck = new FormDeck())
            {
                if (formDeck.ShowDialog() == DialogResult.OK)
                {
                    // Récupère les decks choisis par les joueurs
                    var deckJ1 = formDeck.DeckJ1;
                    var deckJ2 = formDeck.DeckJ2;

                    // Lance le jeu principal en passant les decks choisis
                    Application.Run(new FPrincipal(deckJ1, deckJ2, formDeck.IndexDeckJ1, formDeck.IndexDeckJ2));
                }
            }
        }
    }
}
