using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Collections.Specialized;

namespace SnapszerGame.game
{
    public class GameViewModel : INotifyPropertyChanged
    {
        // Pakli és kezek
        public KartyaCsomag Pakli { get; set; }
        public ObservableCollection<Card> JatekosLapok { get; set; } = new ObservableCollection<Card>();
        public ObservableCollection<Card> EllensegLapok { get; set; } = new ObservableCollection<Card>();

        private Card _kartya;
        public Card FelforditottKartya
        {
            get => _kartya;
            set { _kartya = value; OnPropertyChanged(nameof(FelforditottKartya)); }
        }

        // Körpontok
        private int _jatekosPont;
        public int JatekosPont { get => _jatekosPont; set { _jatekosPont = value; OnPropertyChanged(nameof(JatekosPont)); } }

        private int _ellensegPont;
        public int EllensegPont { get => _ellensegPont; set { _ellensegPont = value; OnPropertyChanged(nameof(EllensegPont)); } }

        // Adu
        private Szin _aduSzin;
        private string _aduImagePath = string.Empty;
        private bool _hasAduChosen = false;

        public Szin AduSzin
        {
            get => _aduSzin;
            set
            {
                _aduSzin = value;
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

        // Bemondások lehetősége
        private bool _lehet20Bemondani;
        public bool Lehet20Bemondani { get => _lehet20Bemondani; set { _lehet20Bemondani = value; OnPropertyChanged(nameof(Lehet20Bemondani)); } }

        private bool _lehet40Bemondani;
        public bool Lehet40Bemondani { get => _lehet40Bemondani; set { _lehet40Bemondani = value; OnPropertyChanged(nameof(Lehet40Bemondani)); } }

        private Visibility _pakliVisibility = Visibility.Visible;
        public Visibility PakliVisibility { get => _pakliVisibility; set { _pakliVisibility = value; OnPropertyChanged(nameof(PakliVisibility)); } }

        private bool _pakliLezarva;
        public bool PakliLezarva { get => _pakliLezarva; set { _pakliLezarva = value; OnPropertyChanged(nameof(PakliLezarva)); } }

        // Lerakott lapok
        private Card _jatekosHivottLap;
        public Card JatekosHivottLap { get => _jatekosHivottLap; set { _jatekosHivottLap = value; OnPropertyChanged(nameof(JatekosHivottLap)); } }

        private Card _ellensegHivottLap;
        public Card EllensegHivottLap { get => _ellensegHivottLap; set { _ellensegHivottLap = value; OnPropertyChanged(nameof(EllensegHivottLap)); } }

        // Üzenetek
        private string _statuszUzenet = string.Empty;
        public string StatuszUzenet { get => _statuszUzenet; set { _statuszUzenet = value; OnPropertyChanged(nameof(StatuszUzenet)); } }

        private Visibility _fomenLathato = Visibility.Visible;
        public Visibility FomenLathato { get => _fomenLathato; set { _fomenLathato = value; OnPropertyChanged(nameof(FomenLathato)); } }

        private SnapszerLogic _logic = new SnapszerLogic();
        private System.Collections.Generic.List<Szin> _beMondottSzinek = new System.Collections.Generic.List<Szin>();

        private Szin? _lastBemondottSzin = null;
        public Szin? LastBemondottSzin { get => _lastBemondottSzin; set { _lastBemondottSzin = value; OnPropertyChanged(nameof(LastBemondottSzin)); } }

        private bool _bemondasEngedelyezett = true;
        public bool BemondasEngedelyezett { get => _bemondasEngedelyezett; set { _bemondasEngedelyezett = value; OnPropertyChanged(nameof(BemondasEngedelyezett)); } }

        public IEnumerable<Szin> BemondottSzinek => _beMondottSzinek;

        // Snapszer és talon
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

        private bool _talonTaken = false;
        public bool TalonTaken { get => _talonTaken; set { _talonTaken = value; OnPropertyChanged(nameof(TalonTaken)); } }

        // Meccspontok
        private int _meccsPontJatekos = 0;
        public int MeccsPontJatekos { get => _meccsPontJatekos; set { _meccsPontJatekos = value; OnPropertyChanged(nameof(MeccsPontJatekos)); } }

        private int _meccsPontEllenseg = 0;
        public int MeccsPontEllenseg { get => _meccsPontEllenseg; set { _meccsPontEllenseg = value; OnPropertyChanged(nameof(MeccsPontEllenseg)); } }
        
        // Nyert meccsek = Merkozes
        public int MerkozesNyertJatekos { get => _meccsPontJatekos; set { _meccsPontJatekos = value; OnPropertyChanged(nameof(MerkozesNyertJatekos)); OnPropertyChanged(nameof(MeccsPontJatekos)); } }
        public int MerkozesNyertEllenseg { get => _meccsPontEllenseg; set { _meccsPontEllenseg = value; OnPropertyChanged(nameof(MerkozesNyertEllenseg)); OnPropertyChanged(nameof(MeccsPontEllenseg)); } }

        private int _roundIndex = 1;
        public int RoundIndex { get => _roundIndex; set { _roundIndex = value; OnPropertyChanged(nameof(RoundIndex)); } }

        private bool _lehetNyertem;
        public bool LehetNyertem { get => _lehetNyertem; set { _lehetNyertem = value; OnPropertyChanged(nameof(LehetNyertem)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Konstruktor: reagál a kéz változására
        public GameViewModel()
        {
            JatekosLapok.CollectionChanged += JatekosLapok_CollectionChanged;
            EllensegLapok.CollectionChanged += (s, e) => FrissitBemondasLehetoseg();
        }

        private void JatekosLapok_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            FrissitBemondasLehetoseg();
        }

        // Játék előkészítése
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

            PlayerDeclaredSnapszer = false;
            EnemyDeclaredSnapszer = false;
            JatekosNyertUtesek = 0;
            EllensegNyertUtesek = 0;
            KiertekeltUtesek = 0;

            // Merkozes nullázása új játéknál
            MerkozesNyertJatekos = 0;
            MerkozesNyertEllenseg = 0;

            FrissitBemondasLehetoseg();

            // Csak az első körben engedjük a 'snapszer' és 'nyertem' bejelentést
            RoundIndex = 1;
            LehetSnapszer = (RoundIndex == 1);
            LehetNyertem = (RoundIndex == 1);

            return enKezdek;
        }

        // Új ütés indításakor
        public void StartNewTrick()
        {
            _beMondottSzinek.Clear();
            LastBemondottSzin = null;
            BemondasEngedelyezett = true;
            FrissitBemondasLehetoseg();

            if (KiertekeltUtesek > 0)
            {
                LehetSnapszer = false; // miután értékeltünk, snapszer nem lehetséges
            }
        }

        // Gyors indítás (nincs animáció)
        public void JatekInditasa()
        {
            bool enKezdek = PrepareGameStart();

            for (int i = 0; i < 5; i++)
            {
                JatekosLapok.Add(Pakli.Huzas());
                EllensegLapok.Add(Pakli.Huzas());
            }

            if (enKezdek)
            {
                AduValasztasFolyamatban = true;
                StatuszUzenet = "Válassz adut";
            }
            else
            {
                AduValasztasFolyamatban = false;
                AduSzin = EllensegLapok.GroupBy(l => l.szin).OrderByDescending(g => g.Count()).First().Key;
                EnKovetkezem = true;
                StatuszUzenet = "A gép választott adut";
            }
        }

        public void JatekosAdutValaszt(Szin valasztottSzin)
        {
            AduSzin = valasztottSzin;
            AduValasztasFolyamatban = false;
            EnKovetkezem = false;
            StatuszUzenet = string.Empty;
            FrissitBemondasLehetoseg();
        }

        public void PakliLezarasa()
        {
            PakliLezarva = true;
            PakliVisibility = Visibility.Collapsed;
        }

        // Ha lapot huztak, jelezzük (talon kezeléshez)
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

        // UI frissítés, bemondási lehetőségek kiszámítása
        public void FrissitBemondasLehetoseg()
        {
            if (!BemondasEngedelyezett)
            {
                Lehet20Bemondani = false;
                Lehet40Bemondani = false;
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

            LehetSnapszer = (RoundIndex == 1) && (KiertekeltUtesek == 0) && !PlayerDeclaredSnapszer && !EnemyDeclaredSnapszer;
            LehetTalonCsere = (KiertekeltUtesek == 0) && !TalonTaken && JatekosLapok.Any(c => c.szin == AduSzin && c.ertek == Ertek.Alsokiraly);

            // Nyertem: öt lap, összesen 66 pont, csak első körben
            LehetNyertem = (RoundIndex == 1) && (KiertekeltUtesek == 0) && JatekosLapok.Count == 5 && JatekosLapok.Sum(c => c.pont) == 66;
        }

        public void ResetRoundState()
        {
            // Következő kör beállításai
            RoundIndex++;
            JatekosPont = 0;
            EllensegPont = 0;
            _beMondottSzinek.Clear();
            LastBemondottSzin = null;
            BemondasEngedelyezett = true;
            JatekosNyertUtesek = 0;
            EllensegNyertUtesek = 0;
            KiertekeltUtesek = 0;
            PakliLezarva = false;
            PakliVisibility = Visibility.Visible;
            PlayerDeclaredSnapszer = false;
            EnemyDeclaredSnapszer = false;
            JatekosHivottLap = null;
            EllensegHivottLap = null;
            HasAduChosen = false;
            JatekosLapok.Clear();
            EllensegLapok.Clear();
            FrissitBemondasLehetoseg();
        }

        public void Bemond(bool is40)
        {
            if (!BemondasEngedelyezett) return;
            var bemondasok = _logic.LehetsegesBemondasok(JatekosLapok.ToList()).Except(_beMondottSzinek).ToList();
            var cel = is40 ? bemondasok.FirstOrDefault(s => s == AduSzin) : bemondasok.FirstOrDefault(s => s != AduSzin);

            LastBemondottSzin = null;
            if (bemondasok.Contains(cel))
            {
                int szerzettPont = is40 ? 40 : 20;
                JatekosPont += szerzettPont;
                _beMondottSzinek.Add(cel);
                LastBemondottSzin = cel;
                BemondasEngedelyezett = false;
                FrissitBemondasLehetoseg();
            }
        }

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

        public void DeclareSnapszer(bool player)
        {
            if (player) PlayerDeclaredSnapszer = true; else EnemyDeclaredSnapszer = true;
            BemondasEngedelyezett = false;
            LehetSnapszer = false;
            FrissitBemondasLehetoseg();
        }
    }
}