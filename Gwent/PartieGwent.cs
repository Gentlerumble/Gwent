using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Gwent
{
    // Gère l'état et la logique d'une partie de Gwent.
    // Sépare la logique métier de l'interface utilisateur.
    public class PartieGwent
    {
        #region Propriétés

        public Jeu Jeu { get; }
        public PlateauJoueur Plateau1 { get; private set; }
        public PlateauJoueur Plateau2 { get; private set; }

        public int IndexJoueurCourant { get;  set; } = 0;
        public PlateauJoueur PlateauCourant => IndexJoueurCourant == 0 ? Plateau1 : Plateau2;
        public PlateauJoueur PlateauAdversaire => IndexJoueurCourant == 0 ? Plateau2 : Plateau1;

        public Joueur JoueurCourant => PlateauCourant.Joueur;
        public Joueur JoueurAdversaire => PlateauAdversaire.Joueur;

        public int NumeroManche { get; private set; } = 1;
        public bool PartieTerminee { get; private set; } = false;
        public Joueur Gagnant { get; private set; }

        public Joueur PerdantDerniereManche { get; private set; }

        private readonly Random _random = new Random();

        #endregion

        #region Événements

        public event Action<string> Message;
        public event Action<PlateauJoueur> MancheGagnee;
        public event Action<PlateauJoueur> PartieGagnee;
        public event Action TourChange;
        public event Action MancheTerminee;
        public event Action EtatChange;

        #endregion

        #region Constructeur

        public PartieGwent(List<Carte> deckJ1, List<Carte> deckJ2, int indexDeckJ1, int indexDeckJ2)
        {
            Jeu = new Jeu(deckJ1, deckJ2, indexDeckJ1, indexDeckJ2);
            Plateau1 = new PlateauJoueur(0, Jeu.Joueur1);
            Plateau2 = new PlateauJoueur(1, Jeu.Joueur2);

            DeterminerPremierJoueur();
        }

        #endregion

        #region Gestion des tours

        private void DeterminerPremierJoueur()
        {
            // Pouvoir passif Scoia'Tael :  choisit qui commence
            bool j1ScoiaTel = Jeu.Joueur1.PouvoirPassif == Jeu.PouvoirPassifDeck.ScoiaTel;
            bool j2ScoiaTel = Jeu.Joueur2.PouvoirPassif == Jeu.PouvoirPassifDeck.ScoiaTel;

            if (j1ScoiaTel && !j2ScoiaTel)
            {
                // J1 a Scoia'Tael, il choisira via l'UI
                IndexJoueurCourant = 0; // Par défaut, sera modifié par l'UI
            }
            else if (j2ScoiaTel && !j1ScoiaTel)
            {
                // J2 a Scoia'Tael, il choisira via l'UI
                IndexJoueurCourant = 1; // Par défaut, sera modifié par l'UI
            }
            else
            {
                // Tirage au sort
                IndexJoueurCourant = _random.Next(2);
            }
        }

        public void DefinirPremierJoueur(int index)
        {
            if (index < 0 || index > 1)
                throw new ArgumentOutOfRangeException(nameof(index));
            IndexJoueurCourant = index;
        }

        public void PasserTour()
        {
            PlateauCourant.APasse = true;
            OnMessage($"{JoueurCourant.Nom} passe son tour.");

            if (Plateau1.APasse && Plateau2.APasse)
            {
                TerminerManche();
            }
            else
            {
                PasserAuJoueurSuivant();
            }
        }

        private void PasserAuJoueurSuivant()
        {
            // Si J1 a passé, c'est toujours au tour de J2
            if (Plateau1.APasse && !Plateau2.APasse)
            {
                IndexJoueurCourant = 1;
            }
            // Si J2 a passé, c'est toujours au tour de J1
            else if (Plateau2.APasse && !Plateau1.APasse)
            {
                IndexJoueurCourant = 0;
            }
            // Si personne n'a passé, on alterne normalement
            else if (!Plateau1.APasse && !Plateau2.APasse)
            {
                IndexJoueurCourant = (IndexJoueurCourant == 0) ? 1 : 0;
            }
            // Si les deux ont passé, on ne devrait pas être ici (géré par TerminerManche)

            TourChange?.Invoke();
            EtatChange?.Invoke();
        }

        public void TerminerTour()
        {
            if (Plateau1.APasse && Plateau2.APasse)
            {
                TerminerManche();
            }
            else
            {
                PasserAuJoueurSuivant();
            }
        }

        #endregion

        #region Gestion des manches

        private void TerminerManche()
        {
            int scoreJ1 = CalculateurScore.CalculerScoreTotal(Plateau1);
            int scoreJ2 = CalculateurScore.CalculerScoreTotal(Plateau2);

            PlateauJoueur gagnant = null;
            PlateauJoueur perdant = null;

            if (scoreJ1 > scoreJ2)
            {
                gagnant = Plateau1;
                perdant = Plateau2;
            }
            else if (scoreJ2 > scoreJ1)
            {
                gagnant = Plateau2;
                perdant = Plateau1;
            }
            else
            {
                // Égalité - Pouvoir passif Nilfgaard
                bool j1Nilfgaard = Jeu.Joueur1.PouvoirPassif == Jeu.PouvoirPassifDeck.Nilfgaard;
                bool j2Nilfgaard = Jeu.Joueur2.PouvoirPassif == Jeu.PouvoirPassifDeck.Nilfgaard;

                if (j1Nilfgaard && !j2Nilfgaard)
                {
                    gagnant = Plateau1;
                    perdant = Plateau2;
                    OnMessage("Égalité !  Pouvoir Nilfgaard :  Joueur 1 gagne la manche !");
                }
                else if (j2Nilfgaard && !j1Nilfgaard)
                {
                    gagnant = Plateau2;
                    perdant = Plateau1;
                    OnMessage("Égalité ! Pouvoir Nilfgaard : Joueur 2 gagne la manche !");
                }
                else
                {
                    // Les deux perdent une vie
                    Plateau1.Vies--;
                    Plateau2.Vies--;
                    perdant = _random.Next(2) == 0 ? Plateau1 : Plateau2;
                    OnMessage("Égalité ! Les deux joueurs perdent une vie.");
                }
            }

            if (gagnant != null && perdant != null)
            {
                perdant.Vies--;
                OnMessage($"{gagnant.Joueur.Nom} gagne la manche ! (Score:  {(gagnant == Plateau1 ? scoreJ1 : scoreJ2)} vs {(perdant == Plateau1 ? scoreJ1 : scoreJ2)})");
                MancheGagnee?.Invoke(gagnant);

                // Pouvoir passif Royaumes du Nord :  pioche une carte
                if (gagnant.Joueur.PouvoirPassif == Jeu.PouvoirPassifDeck.RoyaumesDuNord)
                {
                    if (gagnant.Joueur.Deck.Count > 0)
                    {
                        gagnant.Joueur.Piocher();
                        OnMessage("Pouvoir Royaumes du Nord :  Vous piochez une carte !");
                    }
                }
            }

            PerdantDerniereManche = perdant?.Joueur;

            // Vérifier fin de partie
            if (Plateau1.Vies <= 0 || Plateau2.Vies <= 0)
            {
                TerminerPartie();
                return;
            }

            // Préparer la manche suivante
            PreparerNouvelleManche();
        }

        private void PreparerNouvelleManche()
        {
            NumeroManche++;

            // Envoyer les cartes au cimetière
            EnvoyerPlateauAuCimetiere(Plateau1);
            EnvoyerPlateauAuCimetiere(Plateau2);

            // Réinitialiser les états
            Plateau1.ReinitialiserPourNouvelleManche();
            Plateau2.ReinitialiserPourNouvelleManche();

            // Le perdant commence la nouvelle manche
            if (PerdantDerniereManche == Jeu.Joueur1)
                IndexJoueurCourant = 0;
            else if (PerdantDerniereManche == Jeu.Joueur2)
                IndexJoueurCourant = 1;

            MancheTerminee?.Invoke();
            EtatChange?.Invoke();
        }

        private void EnvoyerPlateauAuCimetiere(PlateauJoueur plateau)
        {
            // Pouvoir passif Monstres : garder une carte aléatoire
            PictureBox carteAGarder = null;
            FlowLayoutPanel zoneCarteGardee = null;

            if (plateau.Joueur.PouvoirPassif == Jeu.PouvoirPassifDeck.Monstres)
            {
                var toutesCartes = new List<(System.Windows.Forms.PictureBox pb, System.Windows.Forms.FlowLayoutPanel zone)>();
                foreach (var zone in plateau.ZonesCombat())
                {
                    foreach (System.Windows.Forms.Control ctrl in zone.Controls)
                    {
                        if (ctrl is System.Windows.Forms.PictureBox pb && pb.Tag is Carte)
                        {
                            toutesCartes.Add((pb, zone));
                        }
                    }
                }

                if (toutesCartes.Count > 0)
                {
                    var selection = toutesCartes[_random.Next(toutesCartes.Count)];
                    carteAGarder = selection.pb;
                    zoneCarteGardee = selection.zone;
                    zoneCarteGardee.Controls.Remove(carteAGarder);
                }
            }

            // Vider toutes les zones vers le cimetière
            foreach (var zone in plateau.ZonesCombat())
            {
                ViderZoneVersCimetiere(zone, plateau.Joueur);
            }

            foreach (var zone in plateau.ZonesEffet())
            {
                zone.Controls.Clear();
            }

            foreach (var zone in plateau.ZonesMeteo())
            {
                ViderZoneVersCimetiere(zone, plateau.Joueur);
            }

            // Remettre la carte gardée (Monstres)
            if (carteAGarder != null && zoneCarteGardee != null)
            {
                zoneCarteGardee.Controls.Add(carteAGarder);
                OnMessage("Pouvoir Monstres :  Une carte reste sur le plateau !");
            }
        }

        private void ViderZoneVersCimetiere(System.Windows.Forms.FlowLayoutPanel zone, Joueur joueur)
        {
            var cartes = CalculateurScore.ExtraireCartes(zone);
            foreach (var carte in cartes)
            {
                joueur.Cimetiere.Add(carte);
            }
            zone.Controls.Clear();
        }

        #endregion

        #region Fin de partie

        private void TerminerPartie()
        {
            PartieTerminee = true;

            if (Plateau1.Vies > Plateau2.Vies)
            {
                Gagnant = Jeu.Joueur1;
            }
            else if (Plateau2.Vies > Plateau1.Vies)
            {
                Gagnant = Jeu.Joueur2;
            }
            else
            {
                // Égalité de vies (ne devrait pas arriver normalement)
                Gagnant = null;
            }

            OnMessage($"Partie terminée ! {Gagnant?.Nom ?? "Personne"} remporte la partie !");
            PartieGagnee?.Invoke(Gagnant == Jeu.Joueur1 ? Plateau1 : Plateau2);
        }

        #endregion

        #region Utilitaires

        private void OnMessage(string msg)
        {
            Message?.Invoke(msg);
        }

        public PlateauJoueur GetPlateau(int index)
        {
            return index == 0 ? Plateau1 : Plateau2;
        }

        public PlateauJoueur GetPlateauAdverse(int index)
        {
            return index == 0 ? Plateau2 : Plateau1;
        }

        // Définit directement le joueur courant (utilisé pour la synchronisation réseau)
        public void DefinirJoueurCourant(int index)
        {
            if (index < 0 || index > 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            IndexJoueurCourant = index;
            TourChange?.Invoke();
            EtatChange?.Invoke();
        }

        #endregion
    }
}