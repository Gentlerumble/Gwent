using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gwent.Net;
using Newtonsoft.Json;

namespace Gwent
{
    // Gère la sauvegarde et le chargement des parties
    public static class GameSaveManager
    {
        private static readonly string SaveDirectory;

        static GameSaveManager()
        {
            // Définir le dossier de sauvegardes
            SaveDirectory = Path.Combine(Application.StartupPath, "Saves");

            System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Dossier sauvegardes:  {SaveDirectory}");

            // Créer le dossier de sauvegardes s'il n'existe pas
            try
            {
                if (!Directory.Exists(SaveDirectory))
                {
                    Directory.CreateDirectory(SaveDirectory);
                    System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Dossier créé:  {SaveDirectory}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Erreur création dossier: {ex.Message}");
            }
        }

        // Retourne le chemin du dossier de sauvegardes
        public static string GetSaveDirectory()
        {
            return SaveDirectory;
        }

        // Construit un DTO de sauvegarde sans écrire de fichier (utile pour réseau)
        public static GameSaveDto ConstruireDtoSauvegarde(
            PartieGwent partie,
            PlateauJoueurControl controlJ1,
            PlateauJoueurControl controlJ2,
            bool estReseau,
            int localPlayerIndex,
            string hostAddress,
            int hostPort,
            string nomSauvegarde = null)
        {
            var save = new GameSaveDto
            {
                SaveId = Guid.NewGuid().ToString(),
                SaveName = nomSauvegarde ?? $"Partie_{DateTime.Now:yyyyMMdd_HHmmss}",
                SaveDate = DateTime.Now,
                IndexJoueurCourant = partie.IndexJoueurCourant,
                NumeroManche = partie.NumeroManche,
                EstPartieReseau = estReseau,
                LocalPlayerIndex = localPlayerIndex,
                HostAddress = hostAddress ?? " 172.20.10.2",
                HostPort = hostPort > 0 ? hostPort : 12345,
                Joueur1 = CreerPlayerSaveDto(partie.Plateau1, controlJ1, partie.Jeu.Joueur1),
                Joueur2 = CreerPlayerSaveDto(partie.Plateau2, controlJ2, partie.Jeu.Joueur2)
            };

            // Déterminer l'index du deck
            save.Joueur1.IndexDeck = GetIndexDeck(partie.Jeu.Joueur1.PouvoirPassif);
            save.Joueur2.IndexDeck = GetIndexDeck(partie.Jeu.Joueur2.PouvoirPassif);

            return save;
        }

        // Écrit une sauvegarde à partir d'un DTO (utilisé côté local et à la réception réseau)
        public static bool EcrireSauvegarde(GameSaveDto save)
        {
            try
            {
                if (!Directory.Exists(SaveDirectory))
                {
                    Directory.CreateDirectory(SaveDirectory);
                }

                // Nettoyer le nom de fichier
                string cleanName = CleanFileName(save.SaveName);
                string fileName = $"{cleanName}.json";
                string filePath = Path.Combine(SaveDirectory, fileName);

                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Sauvegarde vers: {filePath}");

                string json = JsonConvert.SerializeObject(save, Formatting.Indented);
                File.WriteAllText(filePath, json);

                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Partie sauvegardée avec succès!");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Erreur écriture sauvegarde: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        // Sauvegarde une partie en cours (version existante, conserve la compatibilité)
        public static bool SauvegarderPartie(
            PartieGwent partie,
            PlateauJoueurControl controlJ1,
            PlateauJoueurControl controlJ2,
            bool estReseau,
            int localPlayerIndex,
            string hostAddress,
            int hostPort,
            string nomSauvegarde = null)
        {
            try
            {
                var save = ConstruireDtoSauvegarde(
                    partie,
                    controlJ1,
                    controlJ2,
                    estReseau,
                    localPlayerIndex,
                    hostAddress,
                    hostPort,
                    nomSauvegarde);

                return EcrireSauvegarde(save);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Erreur sauvegarde: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        // Nettoie un nom de fichier en retirant les caractères invalides
        private static string CleanFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "Partie_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        // Charge une partie sauvegardée
        public static GameSaveDto ChargerPartie(string filePath)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Chargement depuis: {filePath}");

                if (!File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Fichier non trouvé: {filePath}");
                    return null;
                }

                string json = File.ReadAllText(filePath);
                var save = JsonConvert.DeserializeObject<GameSaveDto>(json);

                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Partie chargée:  {save?.SaveName}");
                return save;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Erreur chargement:  {ex.Message}");
                return null;
            }
        }

        // Liste toutes les sauvegardes disponibles
        public static List<GameSaveInfo> ListerSauvegardes()
        {
            var sauvegardes = new List<GameSaveInfo>();

            System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Listage des sauvegardes dans:  {SaveDirectory}");

            try
            {
                // Vérifier que le dossier existe
                if (!Directory.Exists(SaveDirectory))
                {
                    System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Le dossier n'existe pas, création.. .");
                    Directory.CreateDirectory(SaveDirectory);
                    return sauvegardes;
                }

                var files = Directory.GetFiles(SaveDirectory, "*.json");
                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Fichiers trouvés: {files.Length}");

                foreach (var file in files)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Lecture:  {file}");

                        string json = File.ReadAllText(file);
                        var save = JsonConvert.DeserializeObject<GameSaveDto>(json);

                        if (save != null)
                        {
                            var info = new GameSaveInfo
                            {
                                FilePath = file,
                                SaveName = save.SaveName ?? Path.GetFileNameWithoutExtension(file),
                                SaveDate = save.SaveDate,
                                Joueur1Nom = save.Joueur1?.Nom ?? "Joueur 1",
                                Joueur2Nom = save.Joueur2?.Nom ?? "Joueur 2",
                                Joueur1Vies = save.Joueur1?.Vies ?? 2,
                                Joueur2Vies = save.Joueur2?.Vies ?? 2,
                                NumeroManche = save.NumeroManche,
                                EstPartieReseau = save.EstPartieReseau
                            };

                            sauvegardes.Add(info);
                            System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Sauvegarde ajoutée: {info.SaveName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Erreur lecture {file}: {ex.Message}");
                        // Fichier corrompu, ignorer
                    }
                }

                // Trier par date décroissante
                sauvegardes = sauvegardes.OrderByDescending(s => s.SaveDate).ToList();
                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Total sauvegardes valides: {sauvegardes.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Erreur listage: {ex.Message}");
            }

            return sauvegardes;
        }

        // Supprime une sauvegarde
        public static bool SupprimerSauvegarde(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Sauvegarde supprimée: {filePath}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GameSaveManager] Erreur suppression: {ex.Message}");
            }
            return false;
        }

        #region Méthodes privées

        private static PlayerSaveDto CreerPlayerSaveDto(PlateauJoueur plateau, PlateauJoueurControl control, Joueur joueur)
        {
            var dto = new PlayerSaveDto
            {
                Nom = joueur.Nom,
                Vies = plateau.Vies,
                APasse = plateau.APasse,
                PouvoirUtilise = plateau.PouvoirUtilise,
                MeteoMeleeActive = plateau.MeteoMeleeActive,
                MeteoDistanceActive = plateau.MeteoDistanceActive,
                MeteoSiegeActive = plateau.MeteoSiegeActive,
                ChargeMeleeActive = plateau.ChargeMeleeActive,
                ChargeDistanceActive = plateau.ChargeDistanceActive,
                ChargeSiegeActive = plateau.ChargeSiegeActive,
                Main = DtoMapper.ToDtoList(joueur.Main),
                Deck = DtoMapper.ToDtoList(joueur.Deck),
                Cimetiere = DtoMapper.ToDtoList(joueur.Cimetiere)
            };

            // Extraire les cartes des zones (vérifier si control n'est pas null)
            if (control != null)
            {
                dto.ZoneMelee = ExtraireCartesDtoDeZone(control.ZoneMelee);
                dto.ZoneDistance = ExtraireCartesDtoDeZone(control.ZoneDistance);
                dto.ZoneSiege = ExtraireCartesDtoDeZone(control.ZoneSiege);

                dto.ZoneEffetMelee = ExtraireCartesDtoDeZone(control.ZoneEffetMelee);
                dto.ZoneEffetDistance = ExtraireCartesDtoDeZone(control.ZoneEffetDistance);
                dto.ZoneEffetSiege = ExtraireCartesDtoDeZone(control.ZoneEffetSiege);

                dto.ZoneMeteoMelee = ExtraireCartesDtoDeZone(control.ZoneMeteoMelee);
                dto.ZoneMeteoDistance = ExtraireCartesDtoDeZone(control.ZoneMeteoDistance);
                dto.ZoneMeteoSiege = ExtraireCartesDtoDeZone(control.ZoneMeteoSiege);
            }

            return dto;
        }

        private static List<CardDto> ExtraireCartesDtoDeZone(FlowLayoutPanel zone)
        {
            var cartes = new List<CardDto>();
            if (zone == null) return cartes;

            foreach (Control ctrl in zone.Controls)
            {
                if (ctrl is PictureBox pb && pb.Tag is Carte carte)
                {
                    cartes.Add(DtoMapper.ToDto(carte));
                }
            }

            return cartes;
        }

        private static int GetIndexDeck(Jeu.PouvoirPassifDeck pouvoir)
        {
            switch (pouvoir)
            {
                case Jeu.PouvoirPassifDeck.RoyaumesDuNord: return 0;
                case Jeu.PouvoirPassifDeck.Monstres: return 1;
                case Jeu.PouvoirPassifDeck.ScoiaTel: return 2;
                case Jeu.PouvoirPassifDeck.Nilfgaard: return 3;
                default: return 0;
            }
        }

        #endregion
    }

    // Informations résumées d'une sauvegarde (pour l'affichage dans la liste)
    public class GameSaveInfo
    {
        public string FilePath { get; set; }
        public string SaveName { get; set; }
        public DateTime SaveDate { get; set; }
        public string Joueur1Nom { get; set; }
        public string Joueur2Nom { get; set; }
        public int Joueur1Vies { get; set; }
        public int Joueur2Vies { get; set; }
        public int NumeroManche { get; set; }
        public bool EstPartieReseau { get; set; }

        public string Description => $"{Joueur1Nom} ({Joueur1Vies}♥) vs {Joueur2Nom} ({Joueur2Vies}♥) - Manche {NumeroManche}";
    }
}