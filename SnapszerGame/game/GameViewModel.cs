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

        private bool _bemondasEngedelyezett = true;
        public bool BemondasEngedelyezett { get => _bemondasEngedelyezett; set { _bemondasEngedelyezett = value; OnPropertyChanged(nameof(BemondasEngedelyezett)); } }

        public IEnumerable<Szin> BemondottSzinek => _beMondottSzinek;

        // New snapszer-related properties
        private bool _lehetSnapszer;
        public bool LehetSnapszer { get => _lehetSnapszer; set { _lehetSnapszer = value; OnPropertyChanged(nameof(LehetSnapszer)); } }

        private bool _playerDeclaredSnapszer;
        public bool PlayerDeclaredSnapszer { get => _playerDeclaredSnapszer; set { _playerDeclaredSnapszer = value; OnPropertyChanged(nameof(PlayerDeclaredSnapszer)); } }

        private bool _enemyDeclaredSnapszer;
        public bool EnemyDeclaredSnapszer { get => _enemyDeclaredSnapszer; set { _enemyDeclaredSnapszer = value; OnPropertyChanged(nameof(EnemyDeclaredSnapszer)); } }

        private int _jatekosNyertUtesek = 0;
        public int JatekosNyertUtesek { get => _jatekosNyertUtesek; set { _jatekosNyertUtesek = value; OnPropertyChanged(nameof(JatekosNyertUtesek)); } }

        private int _ellensegNyertUtesek = 0;
        public int EllensegNyertUtesek { get => _ellensegNyertUtesek; set { _ellensegNyertUtesek = value; OnPropertyChanged(nameof(EllensegNyertUtesek)); } }

        private int _kiertekeltUtesek = 0;
        public int KiertekeltUtesek { get => _kiertekeltUtesek; set { _kiertekeltUtesek = value; OnPropertyChanged(nameof(KiertekeltUtesek)); } }

        private bool _lehetTalonCsere;
        public bool LehetTalonCsere { get => _lehetTalonCsere; set { _lehetTalonCsere = value; OnPropertyChanged(nameof(LehetTalonCsere)); } }

        private int _merkozesNyertJatekos = 0;
        public int MerkozesNyertJatekos { get => _merkozesNyertJatekos; set { _merkozesNyertJatekos = value; OnPropertyChanged(nameof(MerkozesNyertJatekos)); } }

        private int _merkozesNyertEllenseg = 0;
        public int MerkozesNyertEllenseg { get => _merkozesNyertEllenseg; set { _merkozesNyertEllenseg = value; OnPropertyChanged(nameof(MerkozesNyertEllenseg)); } }

        private int _roundIndex = 1;
        public int RoundIndex { get => _roundIndex; set { _roundIndex = value; OnPropertyChanged(nameof(RoundIndex)); } }

        // Reset only round-specific data (keeps match counters)
        public void ResetRoundState()
        {
            JatekosLapok.Clear();
            EllensegLapok.Clear();
            _beMondottSzinek.Clear();
            LastBemondottSzin = null;
            BemondasEngedelyezett = true;

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
            PakliLezarva = false;

            PlayerDeclaredSnapszer = false;
            EnemyDeclaredSnapszer = false;
            JatekosNyertUtesek = 0;
            EllensegNyertUtesek = 0;
            KiertekeltUtesek = 0;

            FrissitBemondasLehetoseg();

            // Snapszer allowed only in the first round of the match
            RoundIndex++;
            LehetSnapszer = (RoundIndex == 1);
            LehetTalonCsere = false;
            FelforditottKartya = null;
        }

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
            LastBemondottSzin = null;
            BemondasEngedelyezett = true;

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

            // reset snapszer & trick counters
            PlayerDeclaredSnapszer = false;
            EnemyDeclaredSnapszer = false;
            JatekosNyertUtesek = 0;
            EllensegNyertUtesek = 0;
            KiertekeltUtesek = 0;

            FrissitBemondasLehetoseg();

            // Snapszer can be declared immediately after dealing (no tricks yet)
            // Only allow snapszer in the match's first round
            RoundIndex = 1;
            LehetSnapszer = (RoundIndex == 1);

            return enKezdek;
        }

        // Call at the start of every new trick to reset per-trick bemondas state
        public void StartNewTrick()
        {
            _beMondottSzinek.Clear();
            LastBemondottSzin = null;
            BemondasEngedelyezett = true;
            FrissitBemondasLehetoseg();

            // after the very first trick is started/completed, snapszer is no longer allowed
            if (KiertekeltUtesek > 0)
            {
                LehetSnapszer = false;
            }
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

        // Notify that a card was drawn from the deck; if it matches the currently shown felforditott kartya (talon), mark talon as taken
        public void NotifyCardDrawn(Card drawn)
        {
            if (drawn == null) return;
            if (FelforditottKartya != null && drawn.ToString() == FelforditottKartya.ToString())
            {
                TalonTaken = true;
                LehetTalonCsere = false;
                FelforditottKartya = null;
            }
        }

        // Bemondás lehetőségének vizsgálata (UI frissítés)
        public void FrissitBemondasLehetoseg()
        {
            // Csak saját hívásnál mondhatunk be
            if (!BemondasEngedelyezett)
            {
                Lehet20Bemondani = false;
                Lehet40Bemondani = false;
                // still update talon swap possibility
                LehetTalonCsere = (KiertekeltUtesek == 0) && JatekosLapok.Any(c => c.szin == AduSzin && c.ertek == Ertek.Alsokiraly);
                return;
            }

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

            // Snapszer is allowed only before any trick has been completed AND only in the match's first round
            LehetSnapszer = (RoundIndex == 1) && (KiertekeltUtesek == 0) && !PlayerDeclaredSnapszer && !EnemyDeclaredSnapszer;

            // Talon swap allowed only before any trick is evaluated, if talon not yet taken, and if player has adu 'alsokiraly'
            LehetTalonCsere = (KiertekeltUtesek == 0) && !TalonTaken && JatekosLapok.Any(c => c.szin == AduSzin && c.ertek == Ertek.Alsokiraly);
        }

        // 20/40 bemondás kezelése (UI gomb)
        public void Bemond(bool is40)
        {
            if (!BemondasEngedelyezett) return;
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
                BemondasEngedelyezett = false; // only one bemondas allowed this trick
                FrissitBemondasLehetoseg();
            }
        }

        // Called when the enemy announces a bemondas
        public void EnemyBemond(Szin szin, bool is40)
        {
            if (!BemondasEngedelyezett) return;
            if (!_beMondottSzinek.Contains(szin)) _beMondottSzinek.Add(szin);
            LastBemondottSzin = szin;
            int pont = is40 ? 40 : 20;
            EllensegPont += pont;
            BemondasEngedelyezett = false;
            FrissitBemondasLehetoseg();
        }

        // Called when player or enemy declares Snapszer
        public void DeclareSnapszer(bool player)
        {
            if (player)
            {
                PlayerDeclaredSnapszer = true;
            }
            else
            {
                EnemyDeclaredSnapszer = true;
            }
            // Once declared, no further 20/40 announcements this trick
            BemondasEngedelyezett = false;
            LehetSnapszer = false;
            FrissitBemondasLehetoseg();
        }

        private bool _talonTaken = false;
        public bool TalonTaken { get => _talonTaken; set { _talonTaken = value; OnPropertyChanged(nameof(TalonTaken)); } }
    }
}