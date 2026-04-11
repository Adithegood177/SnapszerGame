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
            // Összedobunk minden egy színhez paazig minden értéket
            foreach (Szin s in Enum.GetValues(typeof(Szin)))
            {
                foreach (Ertek e in Enum.GetValues(typeof(Ertek)))
                {
                    lapok.Add(new Card(s, e));
                }
            }
        }

        // Összekuszáljuk a srácokat izlésesen, ne mindig ugyanúgy jöjjenek ki
        public void PakliKeveres()
        {
            Random rng = new Random();
            lapok = lapok.OrderBy(x => rng.Next()).ToList();
        }

        // Kapj ki egy lapot a pakli tetejéről, mehet játszani
        public Card Huzas()
        {
            if (lapok.Count == 0) return null; 
            var lap = lapok[0];
            lapok.RemoveAt(0);
            return lap;
        }

        // Kicsűrjük mi az utolsó lap gubizás nélkül (ez az a pici gyerek ott alul)
        public Card PeekLast()
        {
            if (lapok.Count == 0) return null;
            return lapok[lapok.Count - 1];
        }

        // Amikor talon csere van, kicseréljük a legalsó pacákot arra, amit a játékos idead.
        // A játékosé megy be alulra, övé jön ki felénk.
        public Card ReplaceLast(Card newCard)
        {
            if (lapok.Count == 0) return null;
            var last = lapok[lapok.Count - 1];
            lapok[lapok.Count - 1] = newCard ?? throw new ArgumentNullException(nameof(newCard));
            return last;
        }

        // Kiszámoltatjuk hány pont rothad még a pakliban és le is nullázzuk takarítás gyanánt (általában év végén)
        public int SumRemainingPointsAndClear()
        {
            int sum = lapok.Sum(c => c.pont);
            lapok.Clear();
            return sum;
        }
    }
}