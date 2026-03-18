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
            Pakli = new KartyaCsomag();
            Pakli.PakliKeveres();
            JatekosLapok.Clear();
            EllensegLapok.Clear();

            // 5-5 lap osztása
            for (int i = 0; i < 5; i++)
            {
                JatekosLapok.Add(Pakli.Huzas());
                EllensegLapok.Add(Pakli.Huzas());
            }

            // Sorsolás és adu választás indítása
            Random rnd = new Random();
            bool enKezdek = rnd.Next(2) == 0;

            if (enKezdek)
            {
                AduValasztasFolyamatban = true; // Én jövök, várjuk a gombnyomást
            }
            else
            {
                AduValasztasFolyamatban = false;
                AduSzin = EllensegLapok.GroupBy(l => l.szin).OrderByDescending(g => g.Count()).First().Key; // Gép választ
                EnKovetkezem = true; // Gép választott, én jövök lerakni
            }
        }

        // Ezt hívják majd a gombok
        public void JatekosAdutValaszt(Szin valasztottSzin)
        {
            AduSzin = valasztottSzin;
            AduValasztasFolyamatban = false;
            EnKovetkezem = false; // Én választottam, a gép jön lerakni
        }

        // Pakli lezárása (gomb hívja, vagy automatikusan, ha elfogynak a lapok)
        public void PakliLezarasa()
        {
            PakliLezarva = true;
            // Ha lezárjuk, onnantól szigorúbb szabályok élnek, amit a SnapszerLogic már lekezel
        }

        // Megvizsgáljuk, hogy jelen helyzetben lehet-e bemondani húszat / negyvenet
        public void FrissitBemondasLehetoseg()
        {
            // Csak akkor mondhatunk be, ha nálunk van a hívás joga (és még van lapunk!)
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

        // A játékos rákattintott a Bemondás (20/40) gombra
        public void JatekosBemond(Szin bemondottSzin)
        {
            // Csak akkor ér, ha mi jövünk hívással
            if (EnKovetkezem && _logic.LehetsegesBemondasok(JatekosLapok).Contains(bemondottSzin))
            {
                int szerzettPont = _logic.BemondasErteke(bemondottSzin, AduSzin);
                // Pontot csak akkor kaphat, ha volt már ütése a játszmában,
                // esetleg "ideiglenes" pontként felírható, és mikor beüt, akkor adódik hozzá ténylegesen.
                JatekosPont += szerzettPont; 

                // FRISSÍTÉS: ha bemondta, gomb inaktív
                LehetBemondani = false;
            }
        }
    }
}