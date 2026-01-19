using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Gwent
{
    public static class CalculateurScore
    {
        public static int CalculerScoreTotal(PlateauJoueur plateau)
        {
            int score = 0;
            score += CalculerScoreZone(plateau.ZoneMelee, plateau.MeteoMeleeActive, plateau.ChargeMeleeActive);
            score += CalculerScoreZone(plateau.ZoneDistance, plateau.MeteoDistanceActive, plateau.ChargeDistanceActive);
            score += CalculerScoreZone(plateau.ZoneSiege, plateau.MeteoSiegeActive, plateau.ChargeSiegeActive);
            return score;
        }

        public static int CalculerScoreZone(FlowLayoutPanel zone, bool meteoActive, bool chargeActive)
        {
            if (zone == null) return 0;

            var cartes = ExtraireCartes(zone);
            if (cartes.Count == 0) return 0;

            int score = 0;

            // Compter le nombre de cartes avec Boost Morale
            int nbBoostMorale = cartes.Count(c => c.Pouvoir == PouvoirSpecial.BoostMorale);

            // Grouper par nom pour Lien Étroits
            var groupes = cartes.GroupBy(c => c.Nom);

            foreach (var groupe in groupes)
            {
                int count = groupe.Count();
                bool lienEtroits = groupe.Any(c => c.Pouvoir == PouvoirSpecial.LienEtroits);

                // Calculer la puissance de base du groupe
                int puissanceGroupe = 0;
                foreach (var carte in groupe)
                {
                    if (carte.Pouvoir == PouvoirSpecial.Leurre) continue;

                    // Puissance de base (affectée par météo)
                    int puissance = meteoActive ? 1 : carte.Puissance;

                    // Ajouter le boost moral (+1 pour chaque carte Boost Morale, sauf si c'est cette carte)
                    if (carte.Pouvoir != PouvoirSpecial.BoostMorale)
                    {
                        puissance += nbBoostMorale;
                    }

                    puissanceGroupe += puissance;
                }

                // Appliquer Lien Étroits :  multiplier le total du groupe
                if (lienEtroits && count > 1)
                {
                    puissanceGroupe *= count;
                }

                score += puissanceGroupe;
            }

            // Appliquer Charge en dernier
            if (chargeActive)
                score *= 2;

            return score;
        }

        public static List<Carte> ExtraireCartes(FlowLayoutPanel zone)
        {
            var cartes = new List<Carte>();
            if (zone == null) return cartes;

            foreach (Control ctrl in zone.Controls)
            {
                if (ctrl is PictureBox pb && pb.Tag is Carte carte)
                {
                    cartes.Add(carte);
                }
            }
            return cartes;
        }
    }
}