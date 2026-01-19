using System;

namespace Gwent
{
    // Gère les aspects UI communs. 
    public class GestionnaireUI : IDisposable
    {
        private readonly PartieGwent _partie;

        public GestionnaireUI(PartieGwent partie)
        {
            _partie = partie ?? throw new ArgumentNullException(nameof(partie));
        }

        public void Dispose()
        {
            // Nettoyage si nécessaire
        }
    }
}