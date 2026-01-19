using System;
using System.Collections.Generic;

namespace Gwent.Net
{
    // Structure complète pour sauvegarder une partie
    public class GameSaveDto
    {
        // Métadonnées
        public string SaveId { get; set; }
        public string SaveName { get; set; }
        public DateTime SaveDate { get; set; }
        public int Version { get; set; } = 1;

        // État de la partie
        public int IndexJoueurCourant { get; set; }
        public int NumeroManche { get; set; }

        // Joueur 1
        public PlayerSaveDto Joueur1 { get; set; }

        // Joueur 2
        public PlayerSaveDto Joueur2 { get; set; }

        // Informations réseau
        public bool EstPartieReseau { get; set; }
        public int LocalPlayerIndex { get; set; }
        public string HostAddress { get; set; }
        public int HostPort { get; set; }
    }

    // État sauvegardé d'un joueur
    public class PlayerSaveDto
    {
        public string Nom { get; set; }
        public int IndexDeck { get; set; }
        public int Vies { get; set; }
        public bool APasse { get; set; }
        public bool PouvoirUtilise { get; set; }

        // Cartes
        public List<CardDto> Main { get; set; } = new List<CardDto>();
        public List<CardDto> Deck { get; set; } = new List<CardDto>();
        public List<CardDto> Cimetiere { get; set; } = new List<CardDto>();

        // Cartes sur le plateau
        public List<CardDto> ZoneMelee { get; set; } = new List<CardDto>();
        public List<CardDto> ZoneDistance { get; set; } = new List<CardDto>();
        public List<CardDto> ZoneSiege { get; set; } = new List<CardDto>();

        // Cartes effet
        public List<CardDto> ZoneEffetMelee { get; set; } = new List<CardDto>();
        public List<CardDto> ZoneEffetDistance { get; set; } = new List<CardDto>();
        public List<CardDto> ZoneEffetSiege { get; set; } = new List<CardDto>();

        // Cartes météo
        public List<CardDto> ZoneMeteoMelee { get; set; } = new List<CardDto>();
        public List<CardDto> ZoneMeteoDistance { get; set; } = new List<CardDto>();
        public List<CardDto> ZoneMeteoSiege { get; set; } = new List<CardDto>();

        // États actifs
        public bool MeteoMeleeActive { get; set; }
        public bool MeteoDistanceActive { get; set; }
        public bool MeteoSiegeActive { get; set; }
        public bool ChargeMeleeActive { get; set; }
        public bool ChargeDistanceActive { get; set; }
        public bool ChargeSiegeActive { get; set; }
    }
}