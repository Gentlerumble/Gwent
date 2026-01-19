using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gwent
{
    // Énumération définissant les types de cartes possibles. 
    // Le type détermine où la carte peut être placée sur le plateau.
    public enum TypeCarte
    {
        Melee,
        Distance,
        Siege,
        Effet,
        Meteo,
        Aucun
    }

    public enum PouvoirSpecial
    {
        Espion,         //Copie une carte de la main de l'adversaire
        BoostMorale,    //+1 à toutes les cartes sur la même zone
        LienEtroits,    //*2 pour toutes cartes identiques sur la même zone
        Medic,          //Ramnène une carte du cimetière dans la main
        Rassembler,     //Mets les cartes similaires du deck sur le plateau
        Agile,          //Peut être placer sur 2 zones différentes
        Brulure,        //Détruit la/les cartes les+ fortes du plateau
        Charge,         //*2 sur la zone
        Leurre,         //remplace une carte du plateau par cette carte
        Pluie,          //ramène la puissance des cartes siege à 1
        Soleil,         //Enlève toute cartes météos
        Brouillard,     //ramène la puissance des cartes distance à 1
        Gel,            //ramène la puissance des cartes melee à 1
        Aucun,          
    }

    // Représente une carte du jeu Gwent. 
    // C'est la structure de données fondamentale utilisée partout dans le jeu.
    // Cette classe est sérialisable en JSON grâce à Newtonsoft.Json.
    // Elle peut être sauvegardée/chargée et envoyée via le réseau.
    public class Carte
    {
        public string Nom { get; set; }
        public int Puissance { get; set; }
        public string ImagePath { get; set; }

        public TypeCarte Type { get; set; }
        public PouvoirSpecial Pouvoir { get; set; } 


        public Carte(string nom, int puissance, string imagePath, TypeCarte type)
        {
            Nom = nom;
            Puissance = puissance;
            ImagePath = imagePath;
            Type = type;
            Pouvoir = PouvoirSpecial.Aucun; 
        }

        [JsonConstructor]
        public Carte(string nom, int puissance, string imagePath, TypeCarte type, PouvoirSpecial pouvoir)
        {
            Nom = nom;
            Puissance = puissance;
            ImagePath = imagePath;
            Type = type;
            Pouvoir = pouvoir;
        }

        public Carte() { } 
    }
}
