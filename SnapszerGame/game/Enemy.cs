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


        public Card GepHiv(IEnumerable<Card> gepKezeben, Szin aduSzin)
        {
            var lapok = gepKezeben.ToList();
            var asz = lapok.FirstOrDefault(l => l.ertek == Ertek.Asz && l.szin != aduSzin);
            if (asz != null) return asz;

            var tizes = lapok.FirstOrDefault(l => l.ertek == Ertek.Tiz && l.szin != aduSzin);
            if (tizes != null) return tizes;

            var legkisebbNemAdu = lapok.Where(l => l.szin != aduSzin).OrderBy(l => l.pont).FirstOrDefault();
            if (legkisebbNemAdu != null) return legkisebbNemAdu;
            return lapok.OrderBy(l => l.pont).First();
        }

        public Card GepValaszol(Card hivottLap, Szin aduSzin, bool pakliLezarva, IEnumerable<Card> gepKezeben)
        {
            var lapok = gepKezeben.ToList();

            
            var szabalyosLapok = lapok.Where(lap =>
                _logic.SzabalyosKartyaRakas(lap, hivottLap, aduSzin, pakliLezarva, lapok)).ToList();

            if (!szabalyosLapok.Any())
                return lapok.First();

            var nyeroLapok = szabalyosLapok.Where(lap =>
                _logic.GetWinnerOfTrick(hivottLap, lap, null, new Player("Gep", 0, 0), aduSzin) != null 
            ).ToList();

            var valodiNyeroLapok = new List<Card>();
            foreach (var lap in szabalyosLapok)
            {
                if (lap.szin == hivottLap.szin && lap.pont > hivottLap.pont)
                    valodiNyeroLapok.Add(lap); 
                else if (lap.szin == aduSzin && hivottLap.szin != aduSzin)
                    valodiNyeroLapok.Add(lap); 
            }

            if (valodiNyeroLapok.Any())
            {

                return valodiNyeroLapok.OrderBy(l => l.pont).First();
            }
            return szabalyosLapok.OrderBy(l => l.pont).First();
        }

        // New: Decide whether the enemy can and will announce 20/40.
        // Returns (didBemond, is40, szin)
        public (bool did, bool is40, Szin szin) TryBemond(IEnumerable<Card> gepKezeben, Szin aduSzin, IEnumerable<Szin> alreadyBemondott = null)
        {
            var lapok = gepKezeben.ToList();
            var lehet = _logic.LehetsegesBemondasok(lapok);
            if (alreadyBemondott != null)
            {
                var used = new HashSet<Szin>(alreadyBemondott);
                lehet = lehet.Where(s => !used.Contains(s)).ToList();
            }

            if (!lehet.Any()) return (false, false, default);

            // Prefer 40 (adu) if available
            var aduSzinek = lehet.Where(s => s == aduSzin).ToList();
            if (aduSzinek.Any())
            {
                // enemy will announce 40
                return (true, true, aduSzinek.First());
            }

            // Otherwise announce 20 on a random available non-adu suit
            var nonAdu = lehet.Where(s => s != aduSzin).ToList();
            if (nonAdu.Any())
            {
                var pick = nonAdu[_rnd.Next(nonAdu.Count)];
                return (true, false, pick);
            }

            return (false, false, default);
        }
    }
}

