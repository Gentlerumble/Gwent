using System.Collections.Generic;
using System.Windows.Forms;

namespace Gwent
{
    // Représente le plateau de jeu d'UN joueur. 
    // Contient toutes les zones de cartes, les contrôles UI associés,
    // et l'état actuel du joueur dans la partie (vies, effets actifs, etc.).
    // Chaque joueur a son propre PlateauJoueur qui fait le lien entre : 
    // - Les données du joueur (Joueur.cs)
    // - L'interface graphique (les FlowLayoutPanel, Labels, Buttons)
    // - L'état de la manche (météos actives, charges, etc.)
    public class PlateauJoueur
    {
        public int Index { get; }
        public Joueur Joueur { get; }
        public int Vies { get; set; } = 2;
        public bool APasse { get; set; } = false;
        public bool PouvoirUtilise { get; set; } = false;

        // Zones de combat
        public FlowLayoutPanel ZoneMelee { get; set; }
        public FlowLayoutPanel ZoneDistance { get; set; }
        public FlowLayoutPanel ZoneSiege { get; set; }

        // Zones d'effet
        public FlowLayoutPanel ZoneEffetMelee { get; set; }
        public FlowLayoutPanel ZoneEffetDistance { get; set; }
        public FlowLayoutPanel ZoneEffetSiege { get; set; }

        // Zones météo
        public FlowLayoutPanel ZoneMeteoMelee { get; set; }
        public FlowLayoutPanel ZoneMeteoDistance { get; set; }
        public FlowLayoutPanel ZoneMeteoSiege { get; set; }

        // Zone main
        public FlowLayoutPanel ZoneMain { get; set; }

        // Labels 
        public Label LabelScoreMelee { get; set; }
        public Label LabelScoreDistance { get; set; }
        public Label LabelScoreSiege { get; set; }
        public Label LabelScoreTotal { get; set; }
        public Label LabelPioche { get; set; }
        public Label LabelCimetiere { get; set; }

        // Boutons 
        public Button BoutonPasser { get; set; }
        public Button BoutonPouvoir { get; set; }
        public Button BoutonApercu { get; set; }

        // PictureBox vies 
        public PictureBox PbVie1 { get; set; }
        public PictureBox PbVie2 { get; set; }

        // États météo
        public bool MeteoMeleeActive { get; set; } = false;
        public bool MeteoDistanceActive { get; set; } = false;
        public bool MeteoSiegeActive { get; set; } = false;

        // États charge
        public bool ChargeMeleeActive { get; set; } = false;
        public bool ChargeDistanceActive { get; set; } = false;
        public bool ChargeSiegeActive { get; set; } = false;

        public PlateauJoueur(int index, Joueur joueur)
        {
            Index = index;
            Joueur = joueur;
        }

        public List<FlowLayoutPanel> ZonesCombat()
        {
            return new List<FlowLayoutPanel> { ZoneMelee, ZoneDistance, ZoneSiege };
        }

        public List<FlowLayoutPanel> ZonesEffet()
        {
            return new List<FlowLayoutPanel> { ZoneEffetMelee, ZoneEffetDistance, ZoneEffetSiege };
        }

        public List<FlowLayoutPanel> ZonesMeteo()
        {
            return new List<FlowLayoutPanel> { ZoneMeteoMelee, ZoneMeteoDistance, ZoneMeteoSiege };
        }

        public FlowLayoutPanel GetZonePourType(TypeCarte type)
        {
            switch (type)
            {
                case TypeCarte.Melee: return ZoneMelee;
                case TypeCarte.Distance: return ZoneDistance;
                case TypeCarte.Siege: return ZoneSiege;
                default: return null;
            }
        }

        public void ReinitialiserPourNouvelleManche()
        {
            APasse = false;
            MeteoMeleeActive = false;
            MeteoDistanceActive = false;
            MeteoSiegeActive = false;
            ChargeMeleeActive = false;
            ChargeDistanceActive = false;
            ChargeSiegeActive = false;
        }
    }
}