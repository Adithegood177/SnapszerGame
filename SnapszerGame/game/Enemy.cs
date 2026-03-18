using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapszerGame.game
{
    public class Enemy
    {
        private SnapszerLogic _logic;
        private Random _rnd;

        public Enemy(SnapszerLogic logic)
        {
            _logic = logic;
            _rnd = new Random();
        }

        // A gép hív
        public Card GepHiv(IEnumerable<Card> gepKezeben, Szin aduSzin)
        {
            var lapok = gepKezeben.ToList();

            // TODO: 20/40 bemondás megnézése

            // Sima Ász
            var asz = lapok.FirstOrDefault(l => l.ertek == Ertek.Asz && l.szin != aduSzin);
            if (asz != null) return asz;

            // Sima Tízes
            var tizes = lapok.FirstOrDefault(l => l.ertek == Ertek.Tiz && l.szin != aduSzin);
            if (tizes != null) return tizes;

            // Legkisebb nem adu
            var legkisebbNemAdu = lapok.Where(l => l.szin != aduSzin).OrderBy(l => l.pont).FirstOrDefault();
            if (legkisebbNemAdu != null) return legkisebbNemAdu;

            // Ha már csak adu maradt, a legkisebbet dobjuk
            return lapok.OrderBy(l => l.pont).First();
        }

        // A gép válaszol
        public Card GepValaszol(Card hivottLap, Szin aduSzin, bool pakliLezarva, IEnumerable<Card> gepKezeben)
        {
            var lapok = gepKezeben.ToList();

            // Szabályosan lerakható lapok
            var szabalyosLapok = lapok.Where(lap =>
                _logic.SzabalyosKartyaRakas(lap, hivottLap, aduSzin, pakliLezarva, lapok)).ToList();

            // Biztonsági csekkülés
            if (!szabalyosLapok.Any())
                return lapok.First();

            var nyeroLapok = szabalyosLapok.Where(lap =>
                _logic.GetWinnerOfTrick(hivottLap, lap, null, new Player("Gep", 0, 0), aduSzin) != null 
            ).ToList();

            var valodiNyeroLapok = new List<Card>();
            foreach (var lap in szabalyosLapok)
            {
                if (lap.szin == hivottLap.szin && lap.pont > hivottLap.pont)
                    valodiNyeroLapok.Add(lap); // Színre szín és nagyobb
                else if (lap.szin == aduSzin && hivottLap.szin != aduSzin)
                    valodiNyeroLapok.Add(lap); // Aduzás
            }

            if (valodiNyeroLapok.Any())
            {
                // Legkisebb nyerő lappal viszünk
                return valodiNyeroLapok.OrderBy(l => l.pont).First();
            }

            // Ha esélytelen, eldobjuk a legkisebb lapot
            return szabalyosLapok.OrderBy(l => l.pont).First();
        }
    }
}

