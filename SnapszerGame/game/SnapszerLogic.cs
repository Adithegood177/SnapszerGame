using System;
using System.Collections.Generic;
using System.Text;

namespace SnapszerGame.game
{
    public class SnapszerLogic
    {
        public  Player GetWinnerOfTrick(Card hivottLap, Card felforditottLap, Player jatekos, Player ellenseg)
        {
            if (hivottLap.szin == felforditottLap.szin)
            {
                return (hivottLap.pont > felforditottLap.pont) ? jatekos : ellenseg;
            }
            else
            {
                return (hivottLap.szin == felforditottLap.szin) ? jatekos : ellenseg;
            }
           
        }
        /public bool SzabalyosKartyaRakas(Card lerakandoKartya, Player jatekos, Card hivottLap, Card felforditottLap, bool pakliLezarva)
        {
            if (lerakandoKartya.szin != hivottLap.szin)
            {
                if (lerakandoKartya.szin != felforditottLap.szin)
                {
                   
                }
            }
        }
    }
}
