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
            if (lapok.Count == 0) return null; 
            var lap = lapok[0];
            lapok.RemoveAt(0);
            return lap;
        }

        // Return the last card in the deck without removing it (the talon / turned-up card)
        public Card PeekLast()
        {
            if (lapok.Count == 0) return null;
            return lapok[lapok.Count - 1];
        }

        // Replace the last card with a provided card, returning the original last card.
        // Used for talon swap: player gives a card that becomes the new bottom, and receives the old bottom.
        public Card ReplaceLast(Card newCard)
        {
            if (lapok.Count == 0) return null;
            var last = lapok[lapok.Count - 1];
            lapok[lapok.Count - 1] = newCard ?? throw new ArgumentNullException(nameof(newCard));
            return last;
        }

        // Sum points of all remaining cards in deck and clear them out (used when awarding remaining points at round end)
        public int SumRemainingPointsAndClear()
        {
            int sum = lapok.Sum(c => c.pont);
            lapok.Clear();
            return sum;
        }
    }
}