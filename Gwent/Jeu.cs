using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gwent
{
    // Classe principale gérant la logique du jeu :  
    // - Création des joueurs
    // - Définition de tous les decks disponibles (les 4 factions)
    // - Distribution des cartes initiales
    // - Gestion des pouvoirs passifs de faction
    public class Jeu
    {
        public Joueur Joueur1 { get; set; }
        public Joueur Joueur2 { get; set; }
        private List<List<Carte>> deckDisponibles;
        private Random randomJ1;
        private Random randomJ2;

        public enum PouvoirPassifDeck
        {
            RoyaumesDuNord, // Pioche une carte en cas de victoire de manche
            Monstres,       // garde une carte aléatoire sur le plateau après une manche
            ScoiaTel,       // décide qui commence
            Nilfgaard       // gagne les égalités
        }

        public Jeu(List<Carte> deckJ1, List<Carte> deckJ2, int indexDeckJ1, int indexDeckJ2)
        {
            Joueur1 = new Joueur("Joueur 1");
            Joueur2 = new Joueur("Joueur 2");

            // Affecte le pouvoir passif selon l'index du deck
            Joueur1.PouvoirPassif = (PouvoirPassifDeck)indexDeckJ1;
            Joueur2.PouvoirPassif = (PouvoirPassifDeck)indexDeckJ2;


            randomJ1 = new Random(Guid.NewGuid().GetHashCode());
            randomJ2 = new Random(Guid.NewGuid().GetHashCode());


            Joueur1.Deck = new List<Carte>(deckJ1);
            Joueur2.Deck = new List<Carte>(deckJ2);
            DistribuerMain(Joueur1, randomJ1);
            DistribuerMain(Joueur2, randomJ2);
        }

        public Jeu()
        {
            InitialiserDeck();
        }

        // Initialise tous les decks disponibles dans le jeu.
        // Crée les 4 factions avec toutes leurs cartes. 
        // Chaque deck contient environ 40-50 cartes.
        private void InitialiserDeck()
        {
            deckDisponibles = new List<List<Carte>>();

            deckDisponibles.Add(new List<Carte> //Deck Royaumes du Nord
            {
                new Carte("Ciel dégagé", 0,Path.Combine(Application.StartupPath, "Images", "clear_sky.png"), TypeCarte.Meteo, PouvoirSpecial.Soleil),
                new Carte("Brouillard impénétrable", 0,Path.Combine(Application.StartupPath, "Images", "fog.png"), TypeCarte.Meteo, PouvoirSpecial.Brouillard),
                new Carte("Froid mordant", 0,Path.Combine(Application.StartupPath, "Images", "frost.png"), TypeCarte.Meteo, PouvoirSpecial.Gel),
                new Carte("Pluie torrentielle", 0,Path.Combine(Application.StartupPath, "Images", "rain.png"), TypeCarte.Meteo, PouvoirSpecial.Pluie),
                new Carte("Leurre", 0,Path.Combine(Application.StartupPath, "Images", "decoy.png"), TypeCarte.Aucun, PouvoirSpecial.Leurre),
                new Carte("Leurre", 0,Path.Combine(Application.StartupPath, "Images", "decoy.png"), TypeCarte.Aucun, PouvoirSpecial.Leurre),
                new Carte("Sonnerie de la charge", 0,Path.Combine(Application.StartupPath, "Images", "horn.png"), TypeCarte.Effet, PouvoirSpecial.Charge),
                new Carte("Sonnerie de la charge", 0,Path.Combine(Application.StartupPath, "Images", "horn.png"), TypeCarte.Effet, PouvoirSpecial.Charge),
                new Carte("Terre brulée", 0,Path.Combine(Application.StartupPath, "Images", "scorch.png"), TypeCarte.Effet, PouvoirSpecial.Brulure),
                new Carte("Terre brulée", 0,Path.Combine(Application.StartupPath, "Images", "scorch.png"), TypeCarte.Effet, PouvoirSpecial.Brulure),
                new Carte("Ciri", 15,Path.Combine(Application.StartupPath, "Images", "Ciri.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Siegfried", 5, Path.Combine(Application.StartupPath, "Images", "siegfried_of_denesle.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Geralt de Riv", 15, Path.Combine(Application.StartupPath, "Images", "Geralt of Rivia.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Sheldon Skaggs", 4, Path.Combine(Application.StartupPath, "Images", "sheldon_skaggs.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Triss Merigold", 7,Path.Combine(Application.StartupPath, "Images", "Triss Merigold.png"), TypeCarte.Siege, PouvoirSpecial.Aucun),
                new Carte("Fanstassin Rédanien", 1, Path.Combine(Application.StartupPath, "Images", "redanian_foot_soldier_1.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Baliste", 6, Path.Combine(Application.StartupPath, "Images", "ballista.png"), TypeCarte.Siege, PouvoirSpecial.Aucun),
                new Carte("Trebuchet", 6, Path.Combine(Application.StartupPath, "Images", "trebuchet.png"), TypeCarte.Siege, PouvoirSpecial.Aucun),
                new Carte("Dethmold", 6, Path.Combine(Application.StartupPath, "Images", "dethmold.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Esterad Thyssen", 10, Path.Combine(Application.StartupPath, "Images", "esterad.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("John Natalis", 10, Path.Combine(Application.StartupPath, "Images", "john_natalis.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Keira Metz", 5, Path.Combine(Application.StartupPath, "Images", "keira_metz.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Philippa Eilhart", 10, Path.Combine(Application.StartupPath, "Images", "philippa_eilhart.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Sabrina", 4, Path.Combine(Application.StartupPath, "Images", "sabrina.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Sheala de Tansarville", 5, Path.Combine(Application.StartupPath, "Images", "sheala_de_tansarville.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Tour de Siège", 6, Path.Combine(Application.StartupPath, "Images", "siege_tower.png"), TypeCarte.Siege, PouvoirSpecial.Aucun),
                new Carte("Vernon Roche", 10, Path.Combine(Application.StartupPath, "Images", "vernon_roche.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Ves", 5, Path.Combine(Application.StartupPath, "Images", "ves.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Yarpen Zigrin", 2, Path.Combine(Application.StartupPath, "Images", "yarpen_zigrin.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Commandant des Stries Bleues", 4, Path.Combine(Application.StartupPath, "Images", "blue_stripes_commandant.png"), TypeCarte.Melee, PouvoirSpecial.LienEtroits),
                new Carte("Commandant des Stries Bleues", 4, Path.Combine(Application.StartupPath, "Images", "blue_stripes_commandant.png"), TypeCarte.Melee, PouvoirSpecial.LienEtroits),
                new Carte("Commandant des Stries Bleues", 4, Path.Combine(Application.StartupPath, "Images", "blue_stripes_commandant.png"), TypeCarte.Melee, PouvoirSpecial.LienEtroits),
                new Carte("Catapulte", 8, Path.Combine(Application.StartupPath, "Images", "catapult.png"), TypeCarte.Siege, PouvoirSpecial.LienEtroits),
                new Carte("Catapulte", 8, Path.Combine(Application.StartupPath, "Images", "catapult.png"), TypeCarte.Siege, PouvoirSpecial.LienEtroits),
                new Carte("Chasseur de dragon de Crinfrid", 5, Path.Combine(Application.StartupPath, "Images", "crinfrid_reavers_1.png"), TypeCarte.Distance, PouvoirSpecial.LienEtroits),
                new Carte("Chasseur de dragon de Crinfrid", 5, Path.Combine(Application.StartupPath, "Images", "crinfrid_reavers_1.png"), TypeCarte.Distance, PouvoirSpecial.LienEtroits),
                new Carte("Chasseur de dragon de Crinfrid", 5, Path.Combine(Application.StartupPath, "Images", "crinfrid_reavers_1.png"), TypeCarte.Distance, PouvoirSpecial.LienEtroits),
                new Carte("Chirurgienne de la Bannière brune", 5, Path.Combine(Application.StartupPath, "Images", "dun_banner_medic.png"), TypeCarte.Siege, PouvoirSpecial.Medic),
                new Carte("Expert en siège de Kaedwen", 1, Path.Combine(Application.StartupPath, "Images", "kaedwen_siege.png"), TypeCarte.Siege, PouvoirSpecial.BoostMorale),
                new Carte("Misérable fnatassin de Mes Deux", 1, Path.Combine(Application.StartupPath, "Images", "poor_infantry.png"), TypeCarte.Melee, PouvoirSpecial.LienEtroits),
                new Carte("Misérable fnatassin de Mes Deux", 1, Path.Combine(Application.StartupPath, "Images", "poor_infantry.png"), TypeCarte.Melee, PouvoirSpecial.LienEtroits),
                new Carte("Misérable fnatassin de Mes Deux", 1, Path.Combine(Application.StartupPath, "Images", "poor_infantry.png"), TypeCarte.Melee, PouvoirSpecial.LienEtroits),
                new Carte("Prince Stennis ", 5, Path.Combine(Application.StartupPath, "Images", "prince_stennis.png"), TypeCarte.Melee, PouvoirSpecial.Espion),
                new Carte("Sigismund Dijkstra", 4, Path.Combine(Application.StartupPath, "Images", "sigismund_dijkstra.png"), TypeCarte.Melee, PouvoirSpecial.Espion),
                new Carte("Talar", 1, Path.Combine(Application.StartupPath, "Images", "thaler.png"), TypeCarte.Siege, PouvoirSpecial.Espion),
            });

            deckDisponibles.Add(new List<Carte> // Deck Monstres
            {
                new Carte("Ciel dégagé", 0,Path.Combine(Application.StartupPath, "Images", "clear_sky.png"), TypeCarte.Meteo, PouvoirSpecial.Soleil),
                new Carte("Brouillard impénétrable", 0,Path.Combine(Application.StartupPath, "Images", "fog.png"), TypeCarte.Meteo, PouvoirSpecial.Brouillard),
                new Carte("Froid mordant", 0,Path.Combine(Application.StartupPath, "Images", "frost.png"), TypeCarte.Meteo, PouvoirSpecial.Gel),
                new Carte("Pluie torrentielle", 0,Path.Combine(Application.StartupPath, "Images", "rain.png"), TypeCarte.Meteo, PouvoirSpecial.Pluie),
                new Carte("Leurre", 0,Path.Combine(Application.StartupPath, "Images", "decoy.png"), TypeCarte.Aucun, PouvoirSpecial.Leurre),
                new Carte("Leurre", 0,Path.Combine(Application.StartupPath, "Images", "decoy.png"), TypeCarte.Aucun, PouvoirSpecial.Leurre),
                new Carte("Sonnerie de la charge", 0,Path.Combine(Application.StartupPath, "Images", "horn.png"), TypeCarte.Effet, PouvoirSpecial.Charge),
                new Carte("Sonnerie de la charge", 0,Path.Combine(Application.StartupPath, "Images", "horn.png"), TypeCarte.Effet, PouvoirSpecial.Charge),
                new Carte("Terre brulée", 0,Path.Combine(Application.StartupPath, "Images", "scorch.png"), TypeCarte.Effet, PouvoirSpecial.Brulure),
                new Carte("Terre brulée", 0,Path.Combine(Application.StartupPath, "Images", "scorch.png"), TypeCarte.Effet, PouvoirSpecial.Brulure),
                new Carte("Couvin", 4, Path.Combine(Application.StartupPath, "Images", "botchling.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Cockatrix", 2, Path.Combine(Application.StartupPath, "Images", "cockatrice.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Draug", 10, Path.Combine(Application.StartupPath, "Images", "draug.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Andriague", 2, Path.Combine(Application.StartupPath, "Images", "endrega.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Fiellon", 6,Path.Combine(Application.StartupPath, "Images", "fiend.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Elementaire de feu", 6, Path.Combine(Application.StartupPath, "Images", "fire_elemental.png"), TypeCarte.Siege, PouvoirSpecial.Aucun),
                new Carte("Brumelin", 2, Path.Combine(Application.StartupPath, "Images", "foglet.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Foënard", 5, Path.Combine(Application.StartupPath, "Images", "forktail.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Epouvanteur", 5, Path.Combine(Application.StartupPath, "Images", "frightener.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Gargouille", 2, Path.Combine(Application.StartupPath, "Images", "gargoyle.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Gueunaude Sépulcrale", 5, Path.Combine(Application.StartupPath, "Images", "grave_hag.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Griffon", 5, Path.Combine(Application.StartupPath, "Images", "griffin.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Géant de glace", 5, Path.Combine(Application.StartupPath, "Images", "ice_giant.png"), TypeCarte.Siege, PouvoirSpecial.Aucun),
                new Carte("Imlerith", 10, Path.Combine(Application.StartupPath, "Images", "imlerith.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Leshen", 10, Path.Combine(Application.StartupPath, "Images", "leshen.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Vierge de la peste", 5, Path.Combine(Application.StartupPath, "Images", "plague_maiden.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Loup-garou", 5, Path.Combine(Application.StartupPath, "Images", "werewolf.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Wyvern", 2, Path.Combine(Application.StartupPath, "Images", "wyvern.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Goule", 1, Path.Combine(Application.StartupPath, "Images", "monsters_ghoul2.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Goule", 1, Path.Combine(Application.StartupPath, "Images", "monsters_ghoul2.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Goule", 1, Path.Combine(Application.StartupPath, "Images", "monsters_ghoul2.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Goule", 1, Path.Combine(Application.StartupPath, "Images", "monsters_ghoul2.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Nekker", 2, Path.Combine(Application.StartupPath, "Images", "monsters_nekker2.png"), TypeCarte.Melee,PouvoirSpecial.Rassembler),
                new Carte("Nekker", 2, Path.Combine(Application.StartupPath, "Images", "monsters_nekker2.png"), TypeCarte.Melee,PouvoirSpecial.Rassembler),
                new Carte("Nekker", 2, Path.Combine(Application.StartupPath, "Images", "monsters_nekker2.png"), TypeCarte.Melee,PouvoirSpecial.Rassembler),
                new Carte("Nekker", 2, Path.Combine(Application.StartupPath, "Images", "monsters_nekker2.png"), TypeCarte.Melee,PouvoirSpecial.Rassembler),
                new Carte("Célénos", 2, Path.Combine(Application.StartupPath, "Images", "celaeno_harpy.png"), TypeCarte.Melee, PouvoirSpecial.Agile),
                new Carte("Harpie", 2, Path.Combine(Application.StartupPath, "Images", "harpy.png"), TypeCarte.Melee, PouvoirSpecial.Agile),
                new Carte("Vampire", 4, Path.Combine(Application.StartupPath, "Images", "vampire_bruxa.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Vampire", 4, Path.Combine(Application.StartupPath, "Images", "vampire_fleder.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Vampire", 4, Path.Combine(Application.StartupPath, "Images", "vampire_ekimmara.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Vampire", 4, Path.Combine(Application.StartupPath, "Images", "vampire_garkain.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Vampire", 5, Path.Combine(Application.StartupPath, "Images", "vampire_katakan.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Arachas", 4, Path.Combine(Application.StartupPath, "Images", "arachas2.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Arachas", 4, Path.Combine(Application.StartupPath, "Images", "arachas2.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Arachas", 4, Path.Combine(Application.StartupPath, "Images", "arachas2.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Moire", 6, Path.Combine(Application.StartupPath, "Images", "crone_brewess.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Moire", 6, Path.Combine(Application.StartupPath, "Images", "crone_weavess.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Moire", 6, Path.Combine(Application.StartupPath, "Images", "crone_whispess.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Arachas", 6, Path.Combine(Application.StartupPath, "Images", "arachas_behemoth.png"), TypeCarte.Siege, PouvoirSpecial.Rassembler),
            });

            deckDisponibles.Add(new List<Carte> // Deck ScoiaTel
            {
                new Carte("Ciel dégagé", 0,Path.Combine(Application.StartupPath, "Images", "clear_sky.png"), TypeCarte.Meteo, PouvoirSpecial.Soleil),
                new Carte("Brouillard impénétrable", 0,Path.Combine(Application.StartupPath, "Images", "fog.png"), TypeCarte.Meteo, PouvoirSpecial.Brouillard),
                new Carte("Froid mordant", 0,Path.Combine(Application.StartupPath, "Images", "frost.png"), TypeCarte.Meteo, PouvoirSpecial.Gel),
                new Carte("Pluie torrentielle", 0,Path.Combine(Application.StartupPath, "Images", "rain.png"), TypeCarte.Meteo, PouvoirSpecial.Pluie),
                new Carte("Leurre", 0,Path.Combine(Application.StartupPath, "Images", "decoy.png"), TypeCarte.Aucun, PouvoirSpecial.Leurre),
                new Carte("Leurre", 0,Path.Combine(Application.StartupPath, "Images", "decoy.png"), TypeCarte.Aucun, PouvoirSpecial.Leurre),
                new Carte("Sonnerie de la charge", 0,Path.Combine(Application.StartupPath, "Images", "horn.png"), TypeCarte.Effet, PouvoirSpecial.Charge),
                new Carte("Sonnerie de la charge", 0,Path.Combine(Application.StartupPath, "Images", "horn.png"), TypeCarte.Effet, PouvoirSpecial.Charge),
                new Carte("Terre brulée", 0,Path.Combine(Application.StartupPath, "Images", "scorch.png"), TypeCarte.Effet, PouvoirSpecial.Brulure),
                new Carte("Terre brulée", 0,Path.Combine(Application.StartupPath, "Images", "scorch.png"), TypeCarte.Effet, PouvoirSpecial.Brulure),
                new Carte("Archer de Dol Blathanna", 4,Path.Combine(Application.StartupPath, "Images", "blathanna_archer.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Recrue de la brigade Vrihedd", 4, Path.Combine(Application.StartupPath, "Images", "brigade_recruit.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Dennis Cranmer", 6, Path.Combine(Application.StartupPath, "Images", "dennis_cranmer.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Eithné", 10, Path.Combine(Application.StartupPath, "Images", "eithne.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Ida Emean aep Sivney", 6,Path.Combine(Application.StartupPath, "Images", "ida.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Iorveth", 10, Path.Combine(Application.StartupPath, "Images", "iorveth.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Défenseur de Mahakam", 5, Path.Combine(Application.StartupPath, "Images", "mahakaman_defender.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Riordain", 1, Path.Combine(Application.StartupPath, "Images", "riordain.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Saesenthessis", 10, Path.Combine(Application.StartupPath, "Images", "saesenthessis_saskia.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Toruviel", 2, Path.Combine(Application.StartupPath, "Images", "toruviel.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Guérisseuse havekar", 0, Path.Combine(Application.StartupPath, "Images", "havekar_healer_1.png"), TypeCarte.Distance, PouvoirSpecial.Medic),
                new Carte("Escarmoucheur elfe", 2, Path.Combine(Application.StartupPath, "Images", "elven_skirmisher_1.png"), TypeCarte.Distance, PouvoirSpecial.Rassembler),
                new Carte("Escarmoucheur elfe", 2, Path.Combine(Application.StartupPath, "Images", "elven_skirmisher_1.png"), TypeCarte.Distance, PouvoirSpecial.Rassembler),
                new Carte("Escarmoucheur elfe", 2, Path.Combine(Application.StartupPath, "Images", "elven_skirmisher_1.png"), TypeCarte.Distance, PouvoirSpecial.Rassembler),
                new Carte("Escarmoucheur nain", 3, Path.Combine(Application.StartupPath, "Images", "dwarven_skirmisher_1.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Escarmoucheur nain", 3, Path.Combine(Application.StartupPath, "Images", "dwarven_skirmisher_1.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Escarmoucheur nain", 3, Path.Combine(Application.StartupPath, "Images", "dwarven_skirmisher_1.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Ciaran aep Easnillien", 3, Path.Combine(Application.StartupPath, "Images", "ciaran.png"), TypeCarte.Distance, PouvoirSpecial.Agile),
                new Carte("Vétéran de la brigade Vrihedd", 5, Path.Combine(Application.StartupPath, "Images", "vrihedd_brigade_veteran_2.png"), TypeCarte.Distance, PouvoirSpecial.Agile),
                new Carte("Contrebandier havekar", 5, Path.Combine(Application.StartupPath, "Images", "havekar_smuggler_2.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Contrebandier havekar", 5, Path.Combine(Application.StartupPath, "Images", "havekar_smuggler_2.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Contrebandier havekar", 5, Path.Combine(Application.StartupPath, "Images", "havekar_smuggler_2.png"), TypeCarte.Melee, PouvoirSpecial.Rassembler),
                new Carte("Eclaireur de Dol Blathanna ", 6, Path.Combine(Application.StartupPath, "Images", "dol_blathanna_scout_2.png"), TypeCarte.Distance, PouvoirSpecial.Agile),
                new Carte("Barclay Els", 6, Path.Combine(Application.StartupPath, "Images", "barclay.png"), TypeCarte.Distance, PouvoirSpecial.Agile),
                new Carte("Yaevinn", 6, Path.Combine(Application.StartupPath, "Images", "yaevinn.png"), TypeCarte.Melee, PouvoirSpecial.Agile),
                new Carte("Filavandrel aen Fidhail", 6, Path.Combine(Application.StartupPath, "Images", "filavandrel.png"), TypeCarte.Melee, PouvoirSpecial.Agile),
                new Carte("Schirru", 8, Path.Combine(Application.StartupPath, "Images", "schirru.png"), TypeCarte.Siege, PouvoirSpecial.Brulure),
                new Carte("Milva", 10, Path.Combine(Application.StartupPath, "Images", "milva.png"), TypeCarte.Distance, PouvoirSpecial.BoostMorale),
                new Carte("Isengrim Faoiliarna", 10, Path.Combine(Application.StartupPath, "Images", "isengrim.png"), TypeCarte.Melee, PouvoirSpecial.BoostMorale),

            });

            deckDisponibles.Add(new List<Carte> // Deck Nilfgaard
            {
                new Carte("Ciel dégagé", 0,Path.Combine(Application.StartupPath, "Images", "clear_sky.png"), TypeCarte.Meteo, PouvoirSpecial.Soleil),
                new Carte("Brouillard impénétrable", 0,Path.Combine(Application.StartupPath, "Images", "fog.png"), TypeCarte.Meteo, PouvoirSpecial.Brouillard),
                new Carte("Froid mordant", 0,Path.Combine(Application.StartupPath, "Images", "frost.png"), TypeCarte.Meteo, PouvoirSpecial.Gel),
                new Carte("Pluie torrentielle", 0,Path.Combine(Application.StartupPath, "Images", "rain.png"), TypeCarte.Meteo, PouvoirSpecial.Pluie),
                new Carte("Leurre", 0,Path.Combine(Application.StartupPath, "Images", "decoy.png"), TypeCarte.Aucun, PouvoirSpecial.Leurre),
                new Carte("Leurre", 0,Path.Combine(Application.StartupPath, "Images", "decoy.png"), TypeCarte.Aucun, PouvoirSpecial.Leurre),
                new Carte("Sonnerie de la charge", 0,Path.Combine(Application.StartupPath, "Images", "horn.png"), TypeCarte.Effet, PouvoirSpecial.Charge),
                new Carte("Sonnerie de la charge", 0,Path.Combine(Application.StartupPath, "Images", "horn.png"), TypeCarte.Effet, PouvoirSpecial.Charge),
                new Carte("Terre brulée", 0,Path.Combine(Application.StartupPath, "Images", "scorch.png"), TypeCarte.Effet, PouvoirSpecial.Brulure),
                new Carte("Terre brulée", 0,Path.Combine(Application.StartupPath, "Images", "scorch.png"), TypeCarte.Effet, PouvoirSpecial.Brulure),
                new Carte("Assire var Anahid", 6,Path.Combine(Application.StartupPath, "Images", "assire_var_anahid.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Archer escradrons noirs", 10, Path.Combine(Application.StartupPath, "Images", "black_infantry_archer_1.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Chahir Mawr Dyffryn aep Ceallach", 6, Path.Combine(Application.StartupPath, "Images", "cahir.png"), TypeCarte.Melee, PouvoirSpecial.Aucun),
                new Carte("Fringilla Vigo", 6, Path.Combine(Application.StartupPath, "Images", "fringilla_vigo.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Scorpion de feu lourd Zerrikanien", 10,Path.Combine(Application.StartupPath, "Images", "heavy_zerrikanian_fire_scorpion.png"), TypeCarte.Siege, PouvoirSpecial.Aucun),
                new Carte("Mangonneau", 3, Path.Combine(Application.StartupPath, "Images", "mangonel.png"), TypeCarte.Siege, PouvoirSpecial.Aucun),
                new Carte("Morvran Voorhis", 10, Path.Combine(Application.StartupPath, "Images", "morvran_voorhis.png"), TypeCarte.Siege, PouvoirSpecial.Aucun),
                new Carte("Scorpion de feu Nilfaardien", 5, Path.Combine(Application.StartupPath, "Images", "nilfgaard_fire_scorpion.png"), TypeCarte.Siege, PouvoirSpecial.Aucun),
                new Carte("Puttkammer", 3, Path.Combine(Application.StartupPath, "Images", "Puttkammer.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Renuald aep Matsen", 5, Path.Combine(Application.StartupPath, "Images", "renuald.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Ingénieure de siège", 6, Path.Combine(Application.StartupPath, "Images", "siege_engineer.png"), TypeCarte.Siege, PouvoirSpecial.Aucun),
                new Carte("Tibor Eggebracht", 10, Path.Combine(Application.StartupPath, "Images", "tibor_rggebracht.png"), TypeCarte.Distance, PouvoirSpecial.Aucun),
                new Carte("Garde impérial", 3, Path.Combine(Application.StartupPath, "Images", "impera_brigade.png"), TypeCarte.Melee, PouvoirSpecial.LienEtroits),
                new Carte("Garde impérial", 3, Path.Combine(Application.StartupPath, "Images", "impera_brigade.png"), TypeCarte.Melee, PouvoirSpecial.LienEtroits),
                new Carte("Garde impérial", 3, Path.Combine(Application.StartupPath, "Images", "impera_brigade.png"), TypeCarte.Melee, PouvoirSpecial.LienEtroits),
                new Carte("Menno Coehorn", 10, Path.Combine(Application.StartupPath, "Images", "menno_coehoorn.png"), TypeCarte.Melee, PouvoirSpecial.Medic),
                new Carte("Shilard Fitz-Oesterlen", 7, Path.Combine(Application.StartupPath, "Images", "shilard.png"), TypeCarte.Melee, PouvoirSpecial.Espion),
                new Carte("Stefan Skellen", 9, Path.Combine(Application.StartupPath, "Images", "stefan_skellen.png"), TypeCarte.Melee, PouvoirSpecial.Espion),
                new Carte("Vattier de Rideaux", 4, Path.Combine(Application.StartupPath, "Images", "vattier_de_rideaux.png"), TypeCarte.Melee, PouvoirSpecial.Espion),
                new Carte("Jeune émissaire", 5, Path.Combine(Application.StartupPath, "Images", "young_emissary1.png"), TypeCarte.Melee, PouvoirSpecial.LienEtroits),
                new Carte("Jeune émissaire", 5, Path.Combine(Application.StartupPath, "Images", "young_emissary1.png"), TypeCarte.Melee, PouvoirSpecial.LienEtroits),
                new Carte("Jeune émissaire", 5, Path.Combine(Application.StartupPath, "Images", "young_emissary1.png"), TypeCarte.Melee, PouvoirSpecial.LienEtroits),
            });

        }

        

        private void DistribuerMain(Joueur joueur, Random random)
        {
            // distribuer 10 cartes aléatoires
            for (int i = 0; i < 10 && joueur.Deck.Count > 0; i++)
            {
                int index = random.Next(joueur.Deck.Count);
                Carte carte = joueur.Deck[index];
                joueur.Main.Add(carte);
                joueur.Deck.RemoveAt(index);
            }
        }

        public static List<List<Carte>> AvoirDeckDispo()
        {
            var jeu = new Jeu();
            return jeu.deckDisponibles;
        }

        public List<List<Carte>> LesDecksDispo()
        {
            return deckDisponibles;
        }

    }
}
