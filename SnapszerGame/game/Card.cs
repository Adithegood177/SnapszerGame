using System;

namespace SnapszerGame.game
{
    public enum Szin { Piros, Zold, Makk, Tok }
    public enum Ertek { Kilenc = 0, Alsokiraly = 2, Felsokiraly = 3, Kiraly = 4, Tiz = 10, Asz = 11 }

    public class Card : System.ComponentModel.INotifyPropertyChanged
    {
        private Szin _szin;
        private Ertek _ertek;
        private bool _isFaceUp = false;

        public Szin szin { get => _szin; set { _szin = value; OnPropertyChanged(nameof(szin)); OnPropertyChanged(nameof(ImagePath)); } }
        public Ertek ertek { get => _ertek; set { _ertek = value; OnPropertyChanged(nameof(ertek)); OnPropertyChanged(nameof(ImagePath)); } }
        public int pont => (int)ertek;

        // Whether the card is currently face-up (visible) or face-down
        public bool IsFaceUp { get => _isFaceUp; set { _isFaceUp = value; OnPropertyChanged(nameof(IsFaceUp)); } }

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

                // Összerakja a kép elérési útját → innen húzza be a kártyaképet
                return $"/SnapszerGame;component/kartyak/{s}_{e}.png";
            }
        }

        //konstruktor létrehozza a kártyát a megadott színnel és értékkel
        public Card(Szin szin, Ertek ertek)
        {
            this._szin = szin;
            this._ertek = ertek;
        }

        //kiiras pl piros asz
        public override string ToString()
        {
            return $"{szin} {ertek}";
        }
        //rendszer update, ha a kártya tulajdonságai megváltoznak, értesíti a UI-t, hogy frissítse a megjelenést
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        // Triggereli a frissítést bratyesz olyat szól pl: "hé UI, ez megváltozott!"
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}