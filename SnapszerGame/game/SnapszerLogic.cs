using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnapszerGame.game
{
    public class SnapszerLogic
    {
        
        public Player GetWinnerOfTrick(Card hivottLap, Card valaszLap, Player hivoJatekos, Player valaszoloJatekos, Szin aduSzin)
        {
            if (valaszLap.szin == aduSzin && hivottLap.szin != aduSzin)
            {
                return valaszoloJatekos;
            }
            if (hivottLap.szin == valaszLap.szin)
            {
                return (hivottLap.pont > valaszLap.pont) ? hivoJatekos : valaszoloJatekos;
            }

            return hivoJatekos;
        }

        public bool SzabalyosKartyaRakas(Card lerakandoKartya, Card hivottLap, Szin aduSzin, bool pakliLezarva, IEnumerable<Card> jatekosKezeben)
        {
            if (hivottLap == null)
            {
                return true;
            }

            if (!pakliLezarva)
            {
                return true;
            }


            bool vanHivottSzin = jatekosKezeben.Any(lap => lap.szin == hivottLap.szin);

            if (vanHivottSzin)
            {
                
                if (lerakandoKartya.szin != hivottLap.szin) return false;

               
                bool vanNagyobbLapa = jatekosKezeben.Any(lap => lap.szin == hivottLap.szin && lap.pont > hivottLap.pont);
                if (vanNagyobbLapa && lerakandoKartya.pont < hivottLap.pont) return false;

                return true;
            }
            bool vanAduSzin = jatekosKezeben.Any(lap => lap.szin == aduSzin);
            if (vanAduSzin)
            {
                if (lerakandoKartya.szin != aduSzin) return false;
                return true;
            }

            return true;
        }

        public List<Szin> LehetsegesBemondasok(IEnumerable<Card> jatekosKezeben)
        {
            var bemondasok = new List<Szin>();
            var groupedBySzin = jatekosKezeben.GroupBy(k => k.szin);

            foreach (var group in groupedBySzin)
            {
                bool hasKiraly = group.Any(k => k.ertek == Ertek.Kiraly);
                bool hasFelso = group.Any(k => k.ertek == Ertek.Felsokiraly);

                if (hasKiraly && hasFelso)
                {
                    bemondasok.Add(group.Key);
                }
            }

            return bemondasok;
        }

        public int BemondasErteke(Szin bemondottSzin, Szin aduSzin)
        {
            return (bemondottSzin == aduSzin) ? 40 : 20;
        }
    }
}
