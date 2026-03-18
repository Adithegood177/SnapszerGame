using System;
using System.Collections.Generic;
using System.Linq;

namespace SnapszerGame.game
{
    public class KartyaCsomag
    {
        private List<Card> lapok = new List<Card>();

        public KartyaCsomag()
        {
            // Legenerálja a 24 lapot
            foreach (Szin s in Enum.GetValues(typeof(Szin)))
            {
                foreach (Ertek e in Enum.GetValues(typeof(Ertek)))
                {
                    lapok.Add(new Card(s, e));
                }
            }
        }

        public void PakliKeveres()
        {
            Random rng = new Random();
            lapok = lapok.OrderBy(x => rng.Next()).ToList();
        }

        public Card Huzas()
        {
            if (lapok.Count == 0) return null; // Ha elfogyott a pakli
            var lap = lapok[0];
            lapok.RemoveAt(0);
            return lap;
        }
    }
}