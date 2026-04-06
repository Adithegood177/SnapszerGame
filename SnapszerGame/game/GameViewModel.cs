using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

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

        private int _ellensegPont;
        public int EllensegPont { get => _ellensegPont; set { _ellensegPont = value; OnPropertyChanged(nameof(EllensegPont)); } }

        private Szin _aduSzin;
        private string _aduImagePath = string.Empty;
        private bool _hasAduChosen = false;

        public Szin AduSzin
        {
            get => _aduSzin;
            set
            {
                _aduSzin = value;
                // update helper properties for the view
                AduImagePath = $"/SnapszerGame;component/szinek/{_aduSzin.ToString().ToLower()}.png";
                HasAduChosen = true;
                OnPropertyChanged(nameof(AduSzin));
            }
        }

        public string AduImagePath { get => _aduImagePath; private set { _aduImagePath = value; OnPropertyChanged(nameof(AduImagePath)); } }

        public bool HasAduChosen { get => _hasAduChosen; set { _hasAduChosen = value; OnPropertyChanged(nameof(HasAduChosen)); } }

        private bool _aduValasztasFolyamatban;
        public bool AduValasztasFolyamatban { get => _aduValasztasFolyamatban; set { _aduValasztasFolyamatban = value; OnPropertyChanged(nameof(AduValasztasFolyamatban)); } }

        private bool _enKovetkezem;
        public bool EnKovetkezem { get => _enKovetkezem; set { _enKovetkezem = value; OnPropertyChanged(nameof(EnKovetkezem)); } }

        private bool _lehet20Bemondani;
        public bool Lehet20Bemondani { get => _lehet20Bemondani; set { _lehet20Bemondani = value; OnPropertyChanged(nameof(Lehet20Bemondani)); } }

        private bool _lehet40Bemondani;
        public bool Lehet40Bemondani { get => _lehet40Bemondani; set { _lehet40Bemondani = value; OnPropertyChanged(nameof(Lehet40Bemondani)); } }

        private Visibility _pakliVisibility = Visibility.Visible;
        public Visibility PakliVisibility { get => _pakliVisibility; set { _pakliVisibility = value; OnPropertyChanged(nameof(PakliVisibility)); } }

        private bool _pakliLezarva;
        public bool PakliLezarva { get => _pakliLezarva; set { _pakliLezarva = value; OnPropertyChanged(nameof(PakliLezarva)); } }

        // New properties for tricks
        private Card _jatekosHivottLap;
        public Card JatekosHivottLap { get => _jatekosHivottLap; set { _jatekosHivottLap = value; OnPropertyChanged(nameof(JatekosHivottLap)); } }

        private Card _ellensegHivottLap;
        public Card EllensegHivottLap { get => _ellensegHivottLap; set { _ellensegHivottLap = value; OnPropertyChanged(nameof(EllensegHivottLap)); } }

        // New: status message shown in UI
        private string _statuszUzenet = string.Empty;
        public string StatuszUzenet { get => _statuszUzenet; set { _statuszUzenet = value; OnPropertyChanged(nameof(StatuszUzenet)); } }

        // New: main menu visibility binding (in MainWindow)
        private Visibility _fomenLathato = Visibility.Visible;
        public Visibility FomenLathato { get => _fomenLathato; set { _fomenLathato = value; OnPropertyChanged(nameof(FomenLathato)); } }

        private SnapszerLogic _logic = new SnapszerLogic();
        private System.Collections.Generic.List<Szin> _beMondottSzinek = new System.Collections.Generic.List<Szin>();

        private Szin? _lastBemondottSzin = null;
        public Szin? LastBemondottSzin { get => _lastBemondottSzin; set { _lastBemondottSzin = value; OnPropertyChanged(nameof(LastBemondottSzin)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Prepare game state for dealing; returns true if player will choose adu (player starts)
        public bool PrepareGameStart()
        {
            Pakli = new KartyaCsomag();
            Pakli.PakliKeveres();
            JatekosLapok.Clear();
            EllensegLapok.Clear();
            _beMondottSzinek.Clear();

            Random rnd = new Random();
            bool enKezdek = rnd.Next(2) == 0;

            // Reset state
            AduValasztasFolyamatban = false;
            EnKovetkezem = false;
            StatuszUzenet = string.Empty;
            HasAduChosen = false;
            AduImagePath = string.Empty;
            JatekosHivottLap = null;
            EllensegHivottLap = null;
            JatekosPont = 0;
            EllensegPont = 0;
            PakliVisibility = Visibility.Visible;
            FrissitBemondasLehetoseg();

            return enKezdek;
        }

        // Backwards-compatible immediate start (no animation) kept for other callers
        public void JatekInditasa()
        {
            bool enKezdek = PrepareGameStart();

            // Deal immediately 5 cards per hand
            for (int i = 0; i < 5; i++)
            {
                JatekosLapok.Add(Pakli.Huzas());
                EllensegLapok.Add(Pakli.Huzas());
            }

            if (enKezdek)
            {
                AduValasztasFolyamatban = true; // Játékos választ adut
                StatuszUzenet = "Válassz adut";
            }
            else
            {
                AduValasztasFolyamatban = false;
                AduSzin = EllensegLapok.GroupBy(l => l.szin).OrderByDescending(g => g.Count()).First().Key; // Gép választ adut
                EnKovetkezem = true; // Gép választott, játékos jön
                StatuszUzenet = "A gép választott adut";
            }
        }

        // Játékos választ adut (UI hívja)
        public void JatekosAdutValaszt(Szin valasztottSzin)
        {
            AduSzin = valasztottSzin;
            AduValasztasFolyamatban = false;
            EnKovetkezem = false; // Gép jön hívással
            StatuszUzenet = string.Empty;
            FrissitBemondasLehetoseg();
        }

        // Pakli lezárása (gomb vagy üres pakli esetén)
        public void PakliLezarasa()
        {
            PakliLezarva = true; // Szigorított szabályok élesítése
            PakliVisibility = Visibility.Collapsed;
        }

        // Bemondás lehetőségének vizsgálata (UI frissítés)
        public void FrissitBemondasLehetoseg()
        {
            // Csak saját hívásnál mondhatunk be
            if (EnKovetkezem)
            {
                var bemondasok = _logic.LehetsegesBemondasok(JatekosLapok.ToList()).Except(_beMondottSzinek).ToList();
                Lehet20Bemondani = bemondasok.Any(s => s != AduSzin);
                Lehet40Bemondani = bemondasok.Any(s => s == AduSzin);
            }
            else
            {
                Lehet20Bemondani = false;
                Lehet40Bemondani = false;
            }
        }

        // 20/40 bemondás kezelése (UI gomb)
        public void Bemond(bool is40)
        {
            var bemondasok = _logic.LehetsegesBemondasok(JatekosLapok.ToList()).Except(_beMondottSzinek).ToList();
            var cel = is40 ? bemondasok.FirstOrDefault(s => s == AduSzin) : bemondasok.FirstOrDefault(s => s != AduSzin);
            
            // Reset last
            LastBemondottSzin = null;
            // If it found a valid suit (since enum is value type, defaulting to 0 exists, so check if list actually had it)
            if (bemondasok.Contains(cel))
            {
                int szerzettPont = is40 ? 40 : 20;
                JatekosPont += szerzettPont;
                _beMondottSzinek.Add(cel);
                LastBemondottSzin = cel;
                FrissitBemondasLehetoseg();
            }
        }
    }
}