using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Gwent
{
    public static class ImageHelper
    {
        private static Dictionary<string, Image> _cache = new Dictionary<string, Image>();
        private static string _basePath;

        static ImageHelper()
        {
            _basePath = Path.Combine(Application.StartupPath, "Images");
        }

        public static string ToRelativePath(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
                return absolutePath;

            // Si c'est déjà un chemin relatif (ne contient pas de : )
            if (!absolutePath.Contains(":"))
                return absolutePath;

            // Extraire juste le nom du fichier
            return Path.GetFileName(absolutePath);
        }
        public static void AppliquerFond(Control control, string chemin)
        {
            if (control == null) return;

            var image = ChargerImage(chemin);
            if (image != null)
            {
                control.BackgroundImage = image;
                control.BackgroundImageLayout = ImageLayout.Stretch;
            }
        }


        public static string ToAbsolutePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return relativePath;

            // Si c'est déjà un chemin absolu qui existe
            if (File.Exists(relativePath))
                return relativePath;

            // Essayer de construire le chemin complet
            string fileName = Path.GetFileName(relativePath);

            // Chercher dans le dossier Images
            string fullPath = Path.Combine(_basePath, fileName);
            if (File.Exists(fullPath))
                return fullPath;

            // Chercher dans les sous-dossiers
            foreach (var dir in Directory.GetDirectories(_basePath))
            {
                fullPath = Path.Combine(dir, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }

            // Retourner le chemin original si rien n'est trouvé
            return relativePath;
        }

        public static Image ChargerImage(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            string absolutePath = ToAbsolutePath(path);

            if (_cache.TryGetValue(absolutePath, out Image cached))
                return cached;

            try
            {
                if (File.Exists(absolutePath))
                {
                    var img = Image.FromFile(absolutePath);
                    _cache[absolutePath] = img;
                    return img;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChargerImage] Erreur:  {ex.Message}");
            }

            return null;
        }

        public static void AppliquerImage(PictureBox pb, string path)
        {
            var img = ChargerImage(path);
            if (img != null)
            {
                pb.Image = img;
            }
            else
            {
                pb.BackColor = Color.Gray;
            }
        }

        public static void LibererCache()
        {
            foreach (var img in _cache.Values)
            {
                img?.Dispose();
            }
            _cache.Clear();
        }
    }
}