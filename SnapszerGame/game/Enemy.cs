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

        // Amikor a gép hív, próbálja kitalálni mit érdemes kivágni elsőnek
        public Card GepHiv(IEnumerable<Card> gepKezeben, Szin aduSzin)
        {
            var lapok = gepKezeben.ToList();
            
            // Nyomjunk be egy nem adu ászt, ha van
            var asz = lapok.FirstOrDefault(l => l.ertek == Ertek.Asz && l.szin != aduSzin);
            if (asz != null) return asz;

            // Vagy egy tízest
            var tizes = lapok.FirstOrDefault(l => l.ertek == Ertek.Tiz && l.szin != aduSzin);
            if (tizes != null) return tizes;

            // Ha nincs ász se meg tízes se, dobjuk ki a legkisebb szemetet
            var legkisebbNemAdu = lapok.Where(l => l.szin != aduSzin).OrderBy(l => l.pont).FirstOrDefault();
            if (legkisebbNemAdu != null) return legkisebbNemAdu;
            
            // Ha ez se jön össze, csak dobjuk ami a minimum
            return lapok.OrderBy(l => l.pont).First();
        }

        // Itt reagál a gép, amikor a user már lerakott valamit
        public Card GepValaszol(Card hivottLap, Szin aduSzin, bool pakliLezarva, IEnumerable<Card> gepKezeben)
        {
            var lapok = gepKezeben.ToList();

            // Melyik lapot is teheti le egyáltalán
            var szabalyosLapok = lapok.Where(lap =>
                _logic.SzabalyosKartyaRakas(lap, hivottLap, aduSzin, pakliLezarva, lapok)).ToList();

            // Ha ez valamiért üres lenne (nem baj), dobjuk az elsőt a listából
            if (!szabalyosLapok.Any())
                return lapok.First();

            var nyeroLapok = szabalyosLapok.Where(lap =>
                _logic.GetWinnerOfTrick(hivottLap, lap, null, new Player("Gep", 0, 0), aduSzin) != null 
            ).ToList();

            var valodiNyeroLapok = new List<Card>();
            foreach (var lap in szabalyosLapok)
            {
                // Ha színben megverjük
                if (lap.szin == hivottLap.szin && lap.pont > hivottLap.pont)
                    valodiNyeroLapok.Add(lap); 
                // Vagy rávágjuk az adut
                else if (lap.szin == aduSzin && hivottLap.szin != aduSzin)
                    valodiNyeroLapok.Add(lap); 
            }

            // Ha tudjuk vinni, akkor toljuk a legkisebb verő lapot
            if (valodiNyeroLapok.Any())
            {
                return valodiNyeroLapok.OrderBy(l => l.pont).First();
            }
            
            // Ha buktuk az ütést, dobjunk le valami kis nyomit
            return szabalyosLapok.OrderBy(l => l.pont).First();
        }

        // Nézzük meg van-e kedve / lapja a gépnek huszat vagy negyvenet mondani
        // Visszadobja hogy (didBemond, is40, szin)
        public (bool did, bool is40, Szin szin) TryBemond(IEnumerable<Card> gepKezeben, Szin aduSzin, IEnumerable<Szin> alreadyBemondott = null)
        {
            var lapok = gepKezeben.ToList();
            var lehet = _logic.LehetsegesBemondasok(lapok);
            
            // Kiszűrjük, amit már ellőtt
            if (alreadyBemondott != null)
            {
                var used = new HashSet<Szin>(alreadyBemondott);
                lehet = lehet.Where(s => !used.Contains(s)).ToList();
            }

            // Ha semmink sincs, kilépünk
            if (!lehet.Any()) return (false, false, default);

            // Ha van 40 (adu), azt persze hogy behirdeti a bot
            var aduSzinek = lehet.Where(s => s == aduSzin).ToList();
            if (aduSzinek.Any())
            {
                return (true, true, aduSzinek.First());
            }

            // Egyébként meg random bedob valamelyiket a 20-asok közül
            var nonAdu = lehet.Where(s => s != aduSzin).ToList();
            if (nonAdu.Any())
            {
                var pick = nonAdu[_rnd.Next(nonAdu.Count)];
                return (true, false, pick);
            }

            return (false, false, default);
        }

        // A legszemetebb gép aki egyből snapszerrel nyit, ha nagyon jó a keze
        public bool TrySnapszer(IEnumerable<Card> gepKezeben)
        {
            var lapok = gepKezeben.ToList();
            if (!lapok.Any()) return false;

            // Csak sima kis hack: ha sokat ér a keze és sok az ász/10/király, hadd szóljon
            int sum = lapok.Sum(l => l.pont);
            int highCount = lapok.Count(l => l.ertek == Ertek.Asz || l.ertek == Ertek.Tiz || l.ertek == Ertek.Kiraly);

            return sum >= 60 || highCount >= 3;
        }
    }
}

