using System;
using System.Collections.Generic;
using System.Text;

namespace SnapszerGame.game
{
    class KartyaCsomag //deck
    {
        private List<Card> _cards = new List<Card>();
        private Random _random = new Random();
        public KartyaCsomag()
        { //24  KÁRTYA: 4 szín x 6 érték
            foreach (Szin szin in Enum.GetValues(typeof(Szin)))
            {
                foreach (Ertek ertek in Enum.GetValues(typeof(Ertek)))
                {
                    _cards.Add(new Card(szin, ertek));
                }
            }
        }

        public void PakliKeveres() => _cards = _cards.OrderBy(c => _random.Next()).ToList();

        public Card Huzas()
        {
            if (_cards.Count == 0) return null;
            var card = _cards[0];
            _cards.RemoveAt(0);
            return card;
        }
    }
}
