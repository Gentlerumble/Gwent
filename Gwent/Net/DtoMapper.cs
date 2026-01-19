using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Gwent.Net
{
    public static class DtoMapper
    {
        public static CardDto ToDto(Carte c) => new CardDto
        {
            Nom = c.Nom,
            Puissance = c.Puissance,
            ImagePath = c.ImagePath,
            Type = (int)c.Type,
            Pouvoir = (int)c.Pouvoir
        };

        public static Carte FromDto(CardDto d)
        {
            if (d == null) return null;

            // Reconstruire le chemin d'image localement
            string imagePath = ReconstruireCheminImage(d.ImagePath);

            return new Carte(
                d.Nom,
                d.Puissance,
                imagePath,
                (TypeCarte)d.Type,
                (PouvoirSpecial)d.Pouvoir
            );
        }

        private static string ReconstruireCheminImage(string originalPath)
        {
            if (string.IsNullOrEmpty(originalPath))
                return originalPath;

            try
            {
                // Chercher "Images" dans le chemin (insensible à la casse)
                int indexImages = originalPath.IndexOf("Images", StringComparison.OrdinalIgnoreCase);

                if (indexImages >= 0)
                {
                    // Extraire le chemin relatif à partir de "Images"
                    // Ex: "C:\Users\Hote\Desktop\Gwent\Images\geralt.png" 
                    //     → "Images\geralt.png"
                    string relativePath = originalPath.Substring(indexImages);

                    // Reconstruire avec le chemin local de l'exécutable
                    // Ex: "D:\Jeux\Gwent\" + "Images\geralt.png"
                    string localPath = Path.Combine(Application.StartupPath, relativePath);

                    // Vérifier que le fichier existe (debug)
                    if (!File.Exists(localPath))
                    {
                        System.Diagnostics.Debug.WriteLine($"[DtoMapper] Image non trouvée: {localPath}");
                    }

                    return localPath;
                }
                else
                {
                    // Pas de "Images" dans le chemin, essayer juste le nom du fichier
                    string fileName = Path.GetFileName(originalPath);
                    string localPath = Path.Combine(Application.StartupPath, "Images", fileName);
                    return localPath;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DtoMapper] Erreur reconstruction chemin: {ex.Message}");
                return originalPath; // En cas d'erreur, retourner le chemin original
            }
        }

        public static List<CardDto> ToDtoList(List<Carte> cards)
        {
            var list = new List<CardDto>();
            if (cards == null) return list;
            foreach (var c in cards) list.Add(ToDto(c));
            return list;
        }

        public static List<Carte> FromDtoList(List<CardDto> cards)
        {
            var list = new List<Carte>();
            if (cards == null) return list;
            foreach (var d in cards) list.Add(FromDto(d));
            return list;
        }
    }
}