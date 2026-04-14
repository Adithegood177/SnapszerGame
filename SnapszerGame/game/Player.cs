using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace SnapszerGame.game
{
    public class Player : INotifyPropertyChanged
    {
        // Játékos neve (pl. "Te" vagy "Gép")
        public string Name { get; set; }

        // Kézben lévő lapok
        public ObservableCollection<Card> Hand { get; set; }

        private int _score;

        // Aktuális pont
        public int Score
        {
            get => _score;
            set { _score = value; OnPropertyChanged(nameof(Score)); }
        }

        private int _gamePoints;

        // Meccspont
        public int GamePoints
        {
            get => _gamePoints;
            set { _gamePoints = value; OnPropertyChanged(nameof(GamePoints)); }
        }

        // UI értesítés → ha változik valami, szólunk
        public event PropertyChangedEventHandler PropertyChanged;

        // Konstruktor → új játékos létrehozása alap adatokkal
        public Player(string name, int score, int gamepoints)
        {
            Name = name;
            Score = score;
            GamePoints = gamepoints;

            // Üres kéz induláskor
            this.Hand = new ObservableCollection<Card>();
        }

        // Triggereli az UI frissítést → "hé, ez a property változott!"
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}