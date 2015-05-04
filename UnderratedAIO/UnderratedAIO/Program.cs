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
    internal class Program
    {
        public static Obj_AI_Hero player = ObjectManager.Player;
        public static string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            try
            {
                switch (player.ChampionName)
                {
                    case "ChoGath":
                        Chogath Chogath = new Chogath();
                        break;
                    case "Evelynn":
                        Evelynn Evelynn = new Evelynn();
                        break;
                    case "Fiora":
                        Fiora Fiora = new Fiora();
                        break;
                    case "Garen":
                        Garen Garen = new Garen();
                        break;
                    case "Kennen":
                        Kennen Kennen = new Kennen();
                        break;
                    case "Maokai":
                        Maokai Maokai = new Maokai();
                        break;
                    case "Mordekaiser":
                        Mordekaiser Mordekaiser = new Mordekaiser();
                        break;
                    case "Poppy":
                        Poppy Poppy = new Poppy();
                        break;
                    case "Renekton":
                        Renekton Renekton = new Renekton();
                        break;
                    case "Sejuani":
                        Sejuani Sejuani = new Sejuani();
                        break;
                    case "Shen":
                        Shen Shen = new Shen();
                        break;
                    case "Volibear":
                        Volibear Volibear = new Volibear();
                        break;
                    case "Yorick":
                        Yorick Yorick = new Yorick();
                        break;
                    default:
                        Other Other = new Other();
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