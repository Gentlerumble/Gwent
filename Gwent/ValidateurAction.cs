namespace Gwent
{
    public static class ValidateurAction
    {
        public static ResultatValidation ValiderPouvoirDeck(PlateauJoueur plateau, PlateauJoueur adversaire)
        {
            if (plateau.PouvoirUtilise)
            {
                return new ResultatValidation
                {
                    EstValide = false,
                    MessageErreur = "Vous avez déjà utilisé votre pouvoir de deck."
                };
            }

            if (plateau.APasse)
            {
                return new ResultatValidation
                {
                    EstValide = false,
                    MessageErreur = "Vous ne pouvez pas utiliser votre pouvoir après avoir passé."
                };
            }

            // Validation spécifique pour Scoia'Tael
            if (plateau.Joueur.PouvoirPassif == Jeu.PouvoirPassifDeck.ScoiaTel)
            {
                int scoreMelee = CalculateurScore.CalculerScoreZone(adversaire.ZoneMelee, adversaire.MeteoMeleeActive, adversaire.ChargeMeleeActive);
                if (scoreMelee <= 10)
                {
                    return new ResultatValidation
                    {
                        EstValide = false,
                        MessageErreur = "Le pouvoir Scoia'Tael nécessite que la mêlée adverse ait un score > 10."
                    };
                }
            }

            return new ResultatValidation { EstValide = true };
        }
    }

    public class ResultatValidation
    {
        public bool EstValide { get; set; }
        public string MessageErreur { get; set; }
    }
}