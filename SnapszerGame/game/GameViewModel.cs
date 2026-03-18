using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SnapszerGame.game
{
    public class GameViewModel : INotifyPropertyChanged
    {
        public KartyaCsomag Pakli { get; set; }

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

        private Szin _aduSzin;
        public Szin AduSzin { get => _aduSzin; set { _aduSzin = value; OnPropertyChanged(nameof(AduSzin)); } }

        private bool _aduValasztasFolyamatban;
        public bool AduValasztasFolyamatban { get => _aduValasztasFolyamatban; set { _aduValasztasFolyamatban = value; OnPropertyChanged(nameof(AduValasztasFolyamatban)); } }

        private bool _enKovetkezem;
        public bool EnKovetkezem { get => _enKovetkezem; set { _enKovetkezem = value; OnPropertyChanged(nameof(EnKovetkezem)); } }

        private bool _lehetBemondani;
        public bool LehetBemondani { get => _lehetBemondani; set { _lehetBemondani = value; OnPropertyChanged(nameof(LehetBemondani)); } }

        private bool _pakliLezarva;
        public bool PakliLezarva { get => _pakliLezarva; set { _pakliLezarva = value; OnPropertyChanged(nameof(PakliLezarva)); } }

        private SnapszerLogic _logic = new SnapszerLogic();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void JatekInditasa()
        {
            // Alaphelyzet
            Pakli = new KartyaCsomag();
            Pakli.PakliKeveres();
            JatekosLapok.Clear();
            EllensegLapok.Clear();

            // 5 lap fejenként
            for (int i = 0; i < 5; i++)
            {
                JatekosLapok.Add(Pakli.Huzas());
                EllensegLapok.Add(Pakli.Huzas());
            }

            // Kezdő játékos sorsolása, adu választás
            Random rnd = new Random();
            bool enKezdek = rnd.Next(2) == 0;

            if (enKezdek)
            {
                AduValasztasFolyamatban = true; // Játékos választ adut
            }
            else
            {
                AduValasztasFolyamatban = false;
                AduSzin = EllensegLapok.GroupBy(l => l.szin).OrderByDescending(g => g.Count()).First().Key; // Gép választ adut
                EnKovetkezem = true; // Gép választott, játékos jön
            }
        }

        // Játékos választ adut (UI hívja)
        public void JatekosAdutValaszt(Szin valasztottSzin)
        {
            AduSzin = valasztottSzin;
            AduValasztasFolyamatban = false;
            EnKovetkezem = false; // Gép jön hívással
        }

        // Pakli lezárása (gomb vagy üres pakli esetén)
        public void PakliLezarasa()
        {
            PakliLezarva = true; // Szigorított szabályok élesítése
        }

        // Bemondás lehetőségének vizsgálata (UI frissítés)
        public void FrissitBemondasLehetoseg()
        {
            // Csak saját hívásnál mondhatunk be
            if (EnKovetkezem)
            {
                var bemondasok = _logic.LehetsegesBemondasok(JatekosLapok.ToList());
                LehetBemondani = bemondasok.Any();
            }
            else
            {
                LehetBemondani = false;
            }
        }

        // 20/40 bemondás kezelése (UI gomb)
        public void JatekosBemond(Szin bemondottSzin)
        {
            // Validáció: csak saját körben és ha van mit bemondani
            if (EnKovetkezem && _logic.LehetsegesBemondasok(JatekosLapok).Contains(bemondottSzin))
            {
                int szerzettPont = _logic.BemondasErteke(bemondottSzin, AduSzin);
                // TODO: Pontokat csak az első ütés után lehet jóváírni ténylegesen
                JatekosPont += szerzettPont; 

                // Gomb letiltása bemondás után
                LehetBemondani = false;
            }
        }
    }
}