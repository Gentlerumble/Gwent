using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Gwent
{
    // Gère l'exécution des pouvoirs spéciaux des cartes.
    public class GestionnairePouvoirs
    {
        private readonly PartieGwent _partie;
        private readonly Action<string> _afficherMessage;
        private readonly Func<List<Carte>, Carte> _choisirCarteCimetiere;
        private readonly Random _random = new Random();

        public GestionnairePouvoirs(
            PartieGwent partie,
            Action<string> afficherMessage,
            Func<List<Carte>, Carte> choisirCarteCimetiere)
        {
            _partie = partie ?? throw new ArgumentNullException(nameof(partie));
            _afficherMessage = afficherMessage ?? throw new ArgumentNullException(nameof(afficherMessage));
            _choisirCarteCimetiere = choisirCarteCimetiere;
        }

        public ResultatPouvoir ExecuterPouvoir(
            Carte carte,
            PlateauJoueur plateauJoueur,
            PlateauJoueur plateauAdversaire,
            FlowLayoutPanel zoneCible)
        {
            if (carte.Pouvoir == PouvoirSpecial.Aucun)
                return new ResultatPouvoir { TerminerTour = true };

            switch (carte.Pouvoir)
            {
                case PouvoirSpecial.Medic:
                    return ExecuterMedic(plateauJoueur, zoneCible);

                case PouvoirSpecial.Espion:
                    return ExecuterEspion(plateauJoueur);

                case PouvoirSpecial.Rassembler:
                    return ExecuterRassembler(carte, plateauJoueur, zoneCible);

                case PouvoirSpecial.Brulure:
                    return ExecuterBrulure(carte, plateauJoueur, plateauAdversaire);

                case PouvoirSpecial.Leurre:
                    return ExecuterLeurre(carte, zoneCible);

                case PouvoirSpecial.Charge:
                    return ExecuterCharge(plateauJoueur, zoneCible);

                case PouvoirSpecial.Gel:
                    return ExecuterMeteoGel(plateauJoueur, plateauAdversaire);

                case PouvoirSpecial.Brouillard:
                    return ExecuterMeteoBrouillard(plateauJoueur, plateauAdversaire);

                case PouvoirSpecial.Pluie:
                    return ExecuterMeteoPluie(plateauJoueur, plateauAdversaire);

                case PouvoirSpecial.Soleil:
                    return ExecuterMeteoSoleil(plateauJoueur, plateauAdversaire);

                default:
                    return new ResultatPouvoir { TerminerTour = true };
            }
        }

        private ResultatPouvoir ExecuterMedic(PlateauJoueur plateau, FlowLayoutPanel zoneCible)
        {
            if (plateau.Joueur.Cimetiere.Count == 0)
            {
                _afficherMessage("Cimetière vide.");
                return new ResultatPouvoir { TerminerTour = true };
            }

            var carteChoisie = _choisirCarteCimetiere?.Invoke(plateau.Joueur.Cimetiere);
            if (carteChoisie != null)
            {
                plateau.Joueur.Cimetiere.Remove(carteChoisie);
                // La carte sera ajoutée à la zone appropriée par l'appelant
                _afficherMessage($"{carteChoisie.Nom} a été ressuscitée !");
            }

            return new ResultatPouvoir { TerminerTour = true };
        }

        private ResultatPouvoir ExecuterEspion(PlateauJoueur plateau)
        {
            int nbAPiocher = Math.Min(2, plateau.Joueur.Deck.Count);
            if (nbAPiocher == 0)
            {
                _afficherMessage("Votre deck est vide, vous ne pouvez pas piocher.");
            }
            else
            {
                for (int i = 0; i < nbAPiocher; i++)
                {
                    plateau.Joueur.Piocher();
                }
                _afficherMessage($"Vous piochez {nbAPiocher} carte(s) grâce à l'effet Espion !");
            }

            return new ResultatPouvoir { TerminerTour = true };
        }

        private ResultatPouvoir ExecuterRassembler(Carte carte, PlateauJoueur plateau, FlowLayoutPanel zoneCible)
        {
            var cartesMain = plateau.Joueur.Main.Where(c => c.Nom == carte.Nom && c != carte).ToList();
            var cartesDeck = plateau.Joueur.Deck.Where(c => c.Nom == carte.Nom).ToList();

            foreach (var c in cartesMain)
            {
                plateau.Joueur.Main.Remove(c);
            }

            foreach (var c in cartesDeck)
            {
                plateau.Joueur.Deck.Remove(c);
            }

            int total = cartesMain.Count + cartesDeck.Count;
            if (total > 0)
                _afficherMessage($"Rassembler :  {total} carte(s) identique(s) ajoutée(s) !");
            else
                _afficherMessage("Rassembler : aucune autre carte identique trouvée.");

            return new ResultatPouvoir
            {
                TerminerTour = true,
                CartesSupplementaires = cartesMain.Concat(cartesDeck).ToList()
            };
        }

        private ResultatPouvoir ExecuterBrulure(Carte carte, PlateauJoueur plateau, PlateauJoueur adversaire)
        {
            var toutesCartes = new List<(Carte c, FlowLayoutPanel zone, Joueur proprio)>();

            foreach (var zone in plateau.ZonesCombat())
            {
                foreach (Control ctrl in zone.Controls)
                {
                    if (ctrl is PictureBox pb && pb.Tag is Carte c && c != carte)
                        toutesCartes.Add((c, zone, plateau.Joueur));
                }
            }

            foreach (var zone in adversaire.ZonesCombat())
            {
                foreach (Control ctrl in zone.Controls)
                {
                    if (ctrl is PictureBox pb && pb.Tag is Carte c)
                        toutesCartes.Add((c, zone, adversaire.Joueur));
                }
            }

            if (toutesCartes.Count == 0)
            {
                _afficherMessage("Aucune carte à brûler !");
                return new ResultatPouvoir { TerminerTour = true };
            }

            int maxPuissance = toutesCartes.Max(t => t.c.Puissance);
            var aBruler = toutesCartes.Where(t => t.c.Puissance == maxPuissance).ToList();

            _afficherMessage($"Brûlure : {aBruler.Count} carte(s) détruites !");

            return new ResultatPouvoir
            {
                TerminerTour = true,
                CartesADetruire = aBruler.Select(t => (t.c, t.zone, t.proprio)).ToList()
            };
        }

        private ResultatPouvoir ExecuterLeurre(Carte carte, FlowLayoutPanel zoneCible)
        {
            _afficherMessage("Cliquez sur une de vos cartes pour la remplacer par le Leurre.");
            return new ResultatPouvoir
            {
                AttendreLeurre = true,
                CarteLeurre = carte,
                ZoneLeurre = zoneCible
            };
        }

        private ResultatPouvoir ExecuterCharge(PlateauJoueur plateau, FlowLayoutPanel zoneCible)
        {
            if (zoneCible == plateau.ZoneMelee || zoneCible == plateau.ZoneEffetMelee)
                plateau.ChargeMeleeActive = true;
            else if (zoneCible == plateau.ZoneDistance || zoneCible == plateau.ZoneEffetDistance)
                plateau.ChargeDistanceActive = true;
            else if (zoneCible == plateau.ZoneSiege || zoneCible == plateau.ZoneEffetSiege)
                plateau.ChargeSiegeActive = true;

            _afficherMessage("Charge :  la puissance de la zone est doublée !");
            return new ResultatPouvoir { TerminerTour = true };
        }

        private ResultatPouvoir ExecuterMeteoGel(PlateauJoueur plateau, PlateauJoueur adversaire)
        {
            plateau.MeteoMeleeActive = true;
            adversaire.MeteoMeleeActive = true;
            _afficherMessage("Gel : toutes les cartes de mêlée sont réduites à 1 !");
            return new ResultatPouvoir { TerminerTour = true };
        }

        private ResultatPouvoir ExecuterMeteoBrouillard(PlateauJoueur plateau, PlateauJoueur adversaire)
        {
            plateau.MeteoDistanceActive = true;
            adversaire.MeteoDistanceActive = true;
            _afficherMessage("Brouillard : toutes les cartes de distance sont réduites à 1 !");
            return new ResultatPouvoir { TerminerTour = true };
        }

        private ResultatPouvoir ExecuterMeteoPluie(PlateauJoueur plateau, PlateauJoueur adversaire)
        {
            plateau.MeteoSiegeActive = true;
            adversaire.MeteoSiegeActive = true;
            _afficherMessage("Pluie : toutes les cartes de siège sont réduites à 1 !");
            return new ResultatPouvoir { TerminerTour = true };
        }

        private ResultatPouvoir ExecuterMeteoSoleil(PlateauJoueur plateau, PlateauJoueur adversaire)
        {
            plateau.MeteoMeleeActive = false;
            plateau.MeteoDistanceActive = false;
            plateau.MeteoSiegeActive = false;
            adversaire.MeteoMeleeActive = false;
            adversaire.MeteoDistanceActive = false;
            adversaire.MeteoSiegeActive = false;

            _afficherMessage("Soleil : tous les effets météo sont retirés !");
            return new ResultatPouvoir { TerminerTour = true, ViderZonesMeteo = true };
        }

        public void ExecuterPouvoirDeck(PlateauJoueur plateau, PlateauJoueur adversaire)
        {
            var validation = ValidateurAction.ValiderPouvoirDeck(plateau, adversaire);
            if (!validation.EstValide)
            {
                _afficherMessage(validation.MessageErreur);
                return;
            }

            plateau.PouvoirUtilise = true;

            switch (plateau.Joueur.PouvoirPassif)
            {
                case Jeu.PouvoirPassifDeck.RoyaumesDuNord:
                    ExecuterMeteoSoleil(plateau, adversaire);
                    break;

                case Jeu.PouvoirPassifDeck.Monstres:
                    ExecuterMedic(plateau, null);
                    break;

                case Jeu.PouvoirPassifDeck.ScoiaTel:
                    ExecuterBrulureMeleeAdverse(adversaire);
                    break;

                case Jeu.PouvoirPassifDeck.Nilfgaard:
                    plateau.ChargeMeleeActive = true;
                    _afficherMessage("Pouvoir Nilfgaard : la puissance de votre mêlée est doublée !");
                    break;
            }
        }

        private void ExecuterBrulureMeleeAdverse(PlateauJoueur adversaire)
        {
            var cartes = CalculateurScore.ExtraireCartes(adversaire.ZoneMelee);
            if (cartes.Count == 0)
            {
                _afficherMessage("Aucune carte à brûler !");
                return;
            }

            int maxPuissance = cartes.Max(c => c.Puissance);
            var aBruler = cartes.Where(c => c.Puissance == maxPuissance).ToList();

            _afficherMessage($"Pouvoir Scoia'Tael : {aBruler.Count} carte(s) brûlées !");
        }
    }

    public class ResultatPouvoir
    {
        public bool TerminerTour { get; set; } = false;
        public bool AttendreLeurre { get; set; } = false;
        public Carte CarteLeurre { get; set; }
        public FlowLayoutPanel ZoneLeurre { get; set; }
        public bool ViderZonesMeteo { get; set; } = false;
        public List<Carte> CartesSupplementaires { get; set; }
        public List<(Carte carte, FlowLayoutPanel zone, Joueur proprio)> CartesADetruire { get; set; }
    }
}