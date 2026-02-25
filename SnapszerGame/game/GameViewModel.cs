using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SnapszerGame.game
{
    class GameViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Card> JatekosLapok { get; set; } = new ObservableCollection<Card>();
        public ObservableCollection<Card> EllensegLapok { get; set; } = new ObservableCollection<Card>();

        private Card _kartya;
        public Card FelforditottKartya
        {
            get => _kartya;
            set { _kartya = value; OnPropertyChanged(nameof(FelforditottKartya)); }
        }
        private int _jatekosPont;
        public int JatekosPont { get => _jatekosPont; set { _jatekosPont = value; OnPropertyChanged(nameof(JatekosPont)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void UjPakliUjLapokkal()
        {
            var pakli = new KartyaCsomag();
            pakli.PakliKeveres();
            JatekosLapok.Clear();
            EllensegLapok.Clear();
            for (int i = 0; i < 5; i++)
            {
                JatekosLapok.Add(pakli.Huzas());
                EllensegLapok.Add(pakli.Huzas());
            }
            FelforditottKartya = pakli.Huzas();
        }
        public bool Enkezdek()
        {
            Random rnd = new Random();
            bool kezdek = rnd.Next(2) == 0; //50% esély
            return kezdek;
        }
        public void JatekInditasa()
        {
            JatekosLapok.Clear();
            EllensegLapok.Clear();
            UjPakliUjLapokkal();
           
        }
    }
}
