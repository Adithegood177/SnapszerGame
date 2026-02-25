using System;
using System.Collections.Generic;
using System.Text;

namespace SnapszerGame.game
{
    public enum Szin { Piros, Zold, Makk, Tok };
    public enum Ertek { Kilenc = 0, Tiz = 10, Alsokiraly = 2, Felsokiraly = 3, Kiraly = 4, Asz = 11 };
    public class Card
    {
         public Szin szin { get; set; }
        public Ertek ertek { get; set; }
        public int pont => (int)ertek;

        // WPF-hez: az image elérési útja (pl. "kepek/Makk_Asz.png")
        public string ImagePath => $"kepek/{szin}_{ertek}.png";
        public Card(Szin szin, Ertek ertek)
        {
            this.szin = szin;
            this.ertek = ertek;
        }

        public override string ToString()
        {
            return $"{szin} {ertek}";
        }

    }
}
