using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnapszerGame.game
{
    public class SnapszerLogic
    {
        // Kiszámolja, ki nyeri az adott ütést
        public Player GetWinnerOfTrick(Card hivottLap, Card valaszLap, Player hivoJatekos, Player valaszoloJatekos, Szin aduSzin)
        {
            // Ha a válaszoló adut (tromfot) rakott, de a hívás nem adu volt, akkor a válaszoló nyer
            if (valaszLap.szin == aduSzin && hivottLap.szin != aduSzin)
            {
                return valaszoloJatekos;
            }

            // Ha azonos színűeket raktak, a nagyobb értékű lap viszi az ütést
            if (hivottLap.szin == valaszLap.szin)
            {
                return (hivottLap.pont > valaszLap.pont) ? hivoJatekos : valaszoloJatekos;
            }

            // Minden más esetben, ha valaszlap nem azonos színű és nem is adu, akkor a hívó játékos viszi
            return hivoJatekos;
        }

        // Eldönti, hogy egy lapot szabályosan le lehet-e rakni
        public bool SzabalyosKartyaRakas(Card lerakandoKartya, Card hivottLap, Szin aduSzin, bool pakliLezarva, IEnumerable<Card> jatekosKezeben)
        {
            // Ha mi kezdjük az ütést , bármit rakhatunk
            if (hivottLap == null)
            {
                return true;
            }

            // Ha nincs lezárva a talon (van még húzópakli), nincs semmilyen kényszer
            if (!pakliLezarva)
            {
                return true;
            }

            // --- INNENTŐL SZÍNKÉNYSZER, ÜTÉSKÉNYSZER ÉS ADUKÉNYSZER VAN (mert lezárták a talont vagy elfogyott) ---

            bool vanHivottSzin = jatekosKezeben.Any(lap => lap.szin == hivottLap.szin);

            if (vanHivottSzin)
            {
                // Színkényszer: színre színt kell adni
                if (lerakandoKartya.szin != hivottLap.szin) return false;

                // Ütéskényszer (Felülütési kényszer): ha van nagyobb lap a hívott színből, akkor azt kell rakni
                bool vanNagyobbLapa = jatekosKezeben.Any(lap => lap.szin == hivottLap.szin && lap.pont > hivottLap.pont);
                if (vanNagyobbLapa && lerakandoKartya.pont < hivottLap.pont) return false;

                return true;
            }

            // Adukényszer: ha nincs a hívott színből lapod, de van adud, kötelező azt tenni
            bool vanAduSzin = jatekosKezeben.Any(lap => lap.szin == aduSzin);
            if (vanAduSzin)
            {
                if (lerakandoKartya.szin != aduSzin) return false;
                return true;
            }

            // Ha se hívott színünk, se adunk nincsen, akkor bármit ledobhatunk
            return true;
        }

        // Visszaadja, hogy milyen színekből tudunk bemondani (Húsz / Negyven)
        // 20 pont: Sima színű Király + Felső pár.
        // 40 pont: Adu színű Király + Felső pár.
        // Ezt a metódust a ViewModelből érdemes hívni, amikor a játékos következik hívással.
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

        // Kiszámolja egy bemondás értékét (húsz vagy negyven)
        public int BemondasErteke(Szin bemondottSzin, Szin aduSzin)
        {
            return (bemondottSzin == aduSzin) ? 40 : 20;
        }
    }
}
