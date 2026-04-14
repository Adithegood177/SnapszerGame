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
        // Pakli + kezek → itt él az összes játék adat
        public KartyaCsomag Pakli { get; set; }
        public ObservableCollection<Card> JatekosLapok { get; set; } = new ObservableCollection<Card>();
        public ObservableCollection<Card> EllensegLapok { get; set; } = new ObservableCollection<Card>();

        private Card _kartya;

        // Felfordított lap 
        public Card FelforditottKartya
        {
            get => _kartya;
            set { _kartya = value; OnPropertyChanged(nameof(FelforditottKartya)); }
        }

        // Pontok → ki hol tart épp
        private int _jatekosPont;
        public int JatekosPont { get => _jatekosPont; set { _jatekosPont = value; OnPropertyChanged(nameof(JatekosPont)); } }

        private int _ellensegPont;
        public int EllensegPont { get => _ellensegPont; set { _ellensegPont = value; OnPropertyChanged(nameof(EllensegPont)); } }

        // Adu cuccok (szín + kép + flag hogy már van-e)
        private Szin _aduSzin;
        private string _aduImagePath = string.Empty;
        private bool _hasAduChosen = false;

        public Szin AduSzin
        {
            get => _aduSzin;
            set
            {
                _aduSzin = value;
                // Adu kép frissítés → UI rögtön vált
                AduImagePath = $"/SnapszerGame;component/szinek/{_aduSzin.ToString().ToLower()}.png";
                HasAduChosen = true;
                OnPropertyChanged(nameof(AduSzin));
            }
        }

        public string AduImagePath { get => _aduImagePath; private set { _aduImagePath = value; OnPropertyChanged(nameof(AduImagePath)); } }

        // Volt-e már adu választás
        public bool HasAduChosen { get => _hasAduChosen; set { _hasAduChosen = value; OnPropertyChanged(nameof(HasAduChosen)); } }

        // Épp adut választunk-e
        private bool _aduValasztasFolyamatban;
        public bool AduValasztasFolyamatban { get => _aduValasztasFolyamatban; set { _aduValasztasFolyamatban = value; OnPropertyChanged(nameof(AduValasztasFolyamatban)); } }

        // Ki jön most
        private bool _enKovetkezem;
        public bool EnKovetkezem { get => _enKovetkezem; set { _enKovetkezem = value; OnPropertyChanged(nameof(EnKovetkezem)); } }

        // Gomb enable-ek (20 / 40)
        private bool _lehet20Bemondani;
        public bool Lehet20Bemondani { get => _lehet20Bemondani; set { _lehet20Bemondani = value; OnPropertyChanged(nameof(Lehet20Bemondani)); } }

        private bool _lehet40Bemondani;
        public bool Lehet40Bemondani { get => _lehet40Bemondani; set { _lehet40Bemondani = value; OnPropertyChanged(nameof(Lehet40Bemondani)); } }

        // Pakli látszik-e (ha lezárod → eltűnik)
        private Visibility _pakliVisibility = Visibility.Visible;
        public Visibility PakliVisibility { get => _pakliVisibility; set { _pakliVisibility = value; OnPropertyChanged(nameof(PakliVisibility)); } }

        private bool _pakliLezarva;
        public bool PakliLezarva { get => _pakliLezarva; set { _pakliLezarva = value; OnPropertyChanged(nameof(PakliLezarva)); } }

        // Aktuális kör lapjai az asztalon
        private Card _jatekosHivottLap;
        public Card JatekosHivottLap { get => _jatekosHivottLap; set { _jatekosHivottLap = value; OnPropertyChanged(nameof(JatekosHivottLap)); } }

        private Card _ellensegHivottLap;
        public Card EllensegHivottLap { get => _ellensegHivottLap; set { _ellensegHivottLap = value; OnPropertyChanged(nameof(EllensegHivottLap)); } }

        // UI status text → mit írjunk ki a usernek
        private string _statuszUzenet = string.Empty;
        public string StatuszUzenet { get => _statuszUzenet; set { _statuszUzenet = value; OnPropertyChanged(nameof(StatuszUzenet)); } }

        private Visibility _fomenLathato = Visibility.Visible;
        public Visibility FomenLathato { get => _fomenLathato; set { _fomenLathato = value; OnPropertyChanged(nameof(FomenLathato)); } }

        private SnapszerLogic _logic = new SnapszerLogic();

        // Már bemondott színek (ne lehessen duplázni)
        private System.Collections.Generic.List<Szin> _beMondottSzinek = new System.Collections.Generic.List<Szin>();

        private Szin? _lastBemondottSzin = null;

        // Utolsó bemondás → UI highlightolhatja
        public Szin? LastBemondottSzin { get => _lastBemondottSzin; set { _lastBemondottSzin = value; OnPropertyChanged(nameof(LastBemondottSzin)); } }

        // Szabad-e még bemondani
        private bool _bemondasEngedelyezett = true;
        public bool BemondasEngedelyezett { get => _bemondasEngedelyezett; set { _bemondasEngedelyezett = value; OnPropertyChanged(nameof(BemondasEngedelyezett)); } }

        public IEnumerable<Szin> BemondottSzinek => _beMondottSzinek;

        // Snapszer + talon csere flag-ek
        private bool _lehetSnapszer;
        public bool LehetSnapszer { get => _lehetSnapszer; set { _lehetSnapszer = value; OnPropertyChanged(nameof(LehetSnapszer)); } }

        private bool _playerDeclaredSnapszer;
        public bool PlayerDeclaredSnapszer { get => _playerDeclaredSnapszer; set { _playerDeclaredSnapszer = value; OnPropertyChanged(nameof(PlayerDeclaredSnapszer)); } }

        private bool _enemyDeclaredSnapszer;
        public bool EnemyDeclaredSnapszer { get => _enemyDeclaredSnapszer; set { _enemyDeclaredSnapszer = value; OnPropertyChanged(nameof(EnemyDeclaredSnapszer)); } }

        // Ki hány ütést vitt
        private int _jatekosNyertUtesek = 0;
        public int JatekosNyertUtesek { get => _jatekosNyertUtesek; set { _jatekosNyertUtesek = value; OnPropertyChanged(nameof(JatekosNyertUtesek)); } }

        private int _ellensegNyertUtesek = 0;
        public int EllensegNyertUtesek { get => _ellensegNyertUtesek; set { _ellensegNyertUtesek = value; OnPropertyChanged(nameof(EllensegNyertUtesek)); } }

        // Hány ütést számoltunk már ki
        private int _kiertekeltUtesek = 0;
        public int KiertekeltUtesek { get => _kiertekeltUtesek; set { _kiertekeltUtesek = value; OnPropertyChanged(nameof(KiertekeltUtesek)); } }

        private bool _lehetTalonCsere;

        // Lehet-e cserélni a talonból
        public bool LehetTalonCsere { get => _lehetTalonCsere; set { _lehetTalonCsere = value; OnPropertyChanged(nameof(LehetTalonCsere)); } }

        private bool _talonTaken = false;

        // Elvittük-e már a talont
        public bool TalonTaken { get => _talonTaken; set { _talonTaken = value; OnPropertyChanged(nameof(TalonTaken)); } }

        // Meccspontok (round win-ek)
        private int _meccsPontJatekos = 0;
        public int MeccsPontJatekos { get => _meccsPontJatekos; set { _meccsPontJatekos = value; OnPropertyChanged(nameof(MeccsPontJatekos)); } }

        private int _meccsPontEllenseg = 0;
        public int MeccsPontEllenseg { get => _meccsPontEllenseg; set { _meccsPontEllenseg = value; OnPropertyChanged(nameof(MeccsPontEllenseg)); } }

        // XAML kompat alias (ugyanaz csak más néven)
        public int MerkozesNyertJatekos { get => _meccsPontJatekos; set { _meccsPontJatekos = value; OnPropertyChanged(nameof(MerkozesNyertJatekos)); OnPropertyChanged(nameof(MeccsPontJatekos)); } }
        public int MerkozesNyertEllenseg { get => _meccsPontEllenseg; set { _meccsPontEllenseg = value; OnPropertyChanged(nameof(MerkozesNyertEllenseg)); OnPropertyChanged(nameof(MeccsPontEllenseg)); } }

        // Hányadik körben vagyunk
        private int _roundIndex = 1;
        public int RoundIndex { get => _roundIndex; set { _roundIndex = value; OnPropertyChanged(nameof(RoundIndex)); } }

        // "Nyertem" gomb enable
        private bool _lehetNyertem;
        public bool LehetNyertem { get => _lehetNyertem; set { _lehetNyertem = value; OnPropertyChanged(nameof(LehetNyertem)); } }

        public event PropertyChangedEventHandler PropertyChanged;

        // UI frissítés trigger → "hé, változott valami!"
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Konstruktor → figyeljük ha változik a kéz
        public GameViewModel()
        {
            JatekosLapok.CollectionChanged += JatekosLapok_CollectionChanged;
            EllensegLapok.CollectionChanged += (s, e) => FrissitBemondasLehetoseg();
        }

        private void JatekosLapok_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Ha változik a kezed → újraszámoljuk mit mondhatsz be
            FrissitBemondasLehetoseg();
        }

        // Új játék setup → full reset + keverés
        public bool PrepareGameStart()
        {
            Pakli = new KartyaCsomag();
            Pakli.PakliKeveres();

            // Minden töröl → tiszta lap
            JatekosLapok.Clear();
            EllensegLapok.Clear();
            _beMondottSzinek.Clear();

            // Random ki kezd
            Random rnd = new Random();
            bool enKezdek = rnd.Next(2) == 0;

            // Reset spam
            StatuszUzenet = string.Empty;
            JatekosPont = 0;
            EllensegPont = 0;

            // Snapszer csak első körben megy
            RoundIndex = 1;
            LehetSnapszer = true;
            LehetNyertem = true;

            return enKezdek;
        }

        // Új ütés → reset mini state
        public void StartNewTrick()
        {
            _beMondottSzinek.Clear();
            LastBemondottSzin = null;
            BemondasEngedelyezett = true;

            // Ha már volt kör → snapszer off
            if (KiertekeltUtesek > 0)
            {
                LehetSnapszer = false;
            }
        }

        // Gyors game start (osztás + adu)
        public void JatekInditasa()
        {
            bool enKezdek = PrepareGameStart();

            // 5-5 lap kiosztás
            for (int i = 0; i < 5; i++)
            {
                JatekosLapok.Add(Pakli.Huzas());
                EllensegLapok.Add(Pakli.Huzas());
            }

            if (enKezdek)
            {
                // Te választasz adut
                AduValasztasFolyamatban = true;
                StatuszUzenet = "Válassz adut";
            }
            else
            {
                // Gép választ → legtöbb szín alapján
                AduSzin = EllensegLapok.GroupBy(l => l.szin).OrderByDescending(g => g.Count()).First().Key;
                EnKovetkezem = true;
                StatuszUzenet = "A gép választott adut";
            }
        }

        // Játékos kiválasztja az adut
        public void JatekosAdutValaszt(Szin valasztottSzin)
        {
            AduSzin = valasztottSzin;
            AduValasztasFolyamatban = false;
            EnKovetkezem = false;
            StatuszUzenet = string.Empty;
        }

        // Pakli lezárása → nincs több húzás
        public void PakliLezarasa()
        {
            PakliLezarva = true;
            PakliVisibility = Visibility.Collapsed;
        }

        // Ha valaki felhúzta a talont → csere off
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

        // Újraszámolja mit nyomhat a player (20/40/snapszer/stb)
        public void FrissitBemondasLehetoseg()
        {
            if (!BemondasEngedelyezett)
            {
                Lehet20Bemondani = false;
                Lehet40Bemondani = false;
                return;
            }

            if (EnKovetkezem)
            {
                var bemondasok = _logic.LehetsegesBemondasok(JatekosLapok.ToList()).Except(_beMondottSzinek).ToList();

                // Van-e 20 / 40
                Lehet20Bemondani = bemondasok.Any(s => s != AduSzin);
                Lehet40Bemondani = bemondasok.Any(s => s == AduSzin);
            }

            // Snapszer only early game
            LehetSnapszer = (RoundIndex == 1) && (KiertekeltUtesek == 0);

            // "Nyertem" only ha pont 66 van kézben (edge case)
            LehetNyertem = (RoundIndex == 1) && JatekosLapok.Sum(c => c.pont) == 66;
        }

        // Kövi round → full reset megint
        public void ResetRoundState()
        {
            RoundIndex++;

            // Minden nullázva
            JatekosPont = 0;
            EllensegPont = 0;
            JatekosLapok.Clear();
            EllensegLapok.Clear();
        }

        // Player bemond 20 vagy 40
        public void Bemond(bool is40)
        {
            if (!BemondasEngedelyezett) return;

            int pont = is40 ? 40 : 20;
            JatekosPont += pont;

            // Letiltjuk további bemondást
            BemondasEngedelyezett = false;
        }

        // Bot bemondása
        public void EnemyBemond(Szin szin, bool is40)
        {
            int pont = is40 ? 40 : 20;
            EllensegPont += pont;

            BemondasEngedelyezett = false;
        }

        // Snapszer call → all-in mode 💀
        public void DeclareSnapszer(bool player)
        {
            if (player) PlayerDeclaredSnapszer = true;
            else EnemyDeclaredSnapszer = true;

            BemondasEngedelyezett = false;
            LehetSnapszer = false;
        }
    }
}