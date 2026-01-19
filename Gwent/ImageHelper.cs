using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Gwent
{
    // Classe utilitaire pour la gestion des images avec cache. 
    public static class ImageHelper
    {
        private static readonly Dictionary<string, Image> _cache = new Dictionary<string, Image>();

        public static Image ChargerImage(string chemin)
        {
            if (string.IsNullOrEmpty(chemin) || !File.Exists(chemin))
                return null;

            if (_cache.TryGetValue(chemin, out var cachedImage))
                return cachedImage;

            try
            {
                var image = Image.FromFile(chemin);
                _cache[chemin] = image;
                return image;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur chargement image {chemin}: {ex.Message}");
                return null;
            }
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

        public static void AppliquerImage(PictureBox pb, string chemin)
        {
            if (pb == null) return;

            var image = ChargerImage(chemin);
            if (image != null)
            {
                pb.Image = image;
            }
        }

        public static void LibererCache()
        {
            foreach (var image in _cache.Values)
            {
                try { image?.Dispose(); } catch { }
            }
            _cache.Clear();
        }
    }
}