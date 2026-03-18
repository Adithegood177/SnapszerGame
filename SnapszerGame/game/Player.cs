using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace SnapszerGame.game
{
    public class Player : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public ObservableCollection<Card> Hand { get; set; }
        private int _score;
        public int Score
        {
            get => _score;
            set { _score = value; OnPropertyChanged(nameof(Score)); }
        }

        private int _gamePoints;
        public int GamePoints
        {
            get => _gamePoints;
            set { _gamePoints = value; OnPropertyChanged(nameof(GamePoints)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Player(string name, int score, int gamepoints)
        {
            Name = name;
            Score = score;
            GamePoints = gamepoints;
            this.Hand = new ObservableCollection<Card>();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
