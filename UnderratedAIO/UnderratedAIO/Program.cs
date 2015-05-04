using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using UnderratedAIO.Champions;

namespace UnderratedAIO
{
    class Program
    {
        public static Obj_AI_Hero player = ObjectManager.Player;
        public static string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
          try
          {
              switch (player.ChampionName)
              {
                  case "ChoGath"  :
                      Chogath chogath=new Chogath();
                      break;
                  case "Volibear":
                      Volibear Volibear = new Volibear();
                      break;
                  default:
                      Other other=new Other();
                      break;
              }

          }
          catch (Exception e)
          {
              Console.WriteLine(e.ToString());
          }
        }
    }
}
