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

        public string ImagePath
        {
            get
            {
                // Szin leképezése kisbetűsre
                string s = szin.ToString().ToLower();

                // Ertek leképezése a specifikáció szerint ("9", "10", "also", "felso", "kiraly", "asz")
                string e = ertek switch
                {
                    Ertek.Kilenc => "9",
                    Ertek.Tiz => "10",
                    Ertek.Alsokiraly => "also",
                    Ertek.Felsokiraly => "felso",
                    Ertek.Kiraly => "kiraly",
                    Ertek.Asz => "asz",
                    _ => ertek.ToString().ToLower()
                };

                // Használjunk abszolút pack URI-t a megbízható betöltéshez a build-ből (Build Action: Resource esetén) vagy legalább pontos gyökérrel
                return $"pack://application:,,,/kartyak/{s}_{e}.png";
            }
        }

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