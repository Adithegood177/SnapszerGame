using System;

namespace SnapszerGame.game
{
    public enum Szin { Piros, Zold, Makk, Tok }
    public enum Ertek { Kilenc = 0, Alsokiraly = 2, Felsokiraly = 3, Kiraly = 4, Tiz = 10, Asz = 11 }

    public class Card
    {
        public Szin szin { get; set; }
        public Ertek ertek { get; set; }
        public int pont => (int)ertek;

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