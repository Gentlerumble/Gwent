using System.Collections.Generic;

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

        public static Carte FromDto(CardDto d) =>
            new Carte(d.Nom, d.Puissance, d.ImagePath, (TypeCarte)d.Type, (PouvoirSpecial)d.Pouvoir);

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
