using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

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
                var type = Type.GetType("UnderratedAIO.Champions." + player.ChampionName);
                if (type != null)
                {
                    Helpers.DynamicInitializer.NewInstance(type);
                }
                else
                {
                    var common = Type.GetType("UnderratedAIO.Champions." + "Other");
                    if (common != null)
                    {
                        Helpers.DynamicInitializer.NewInstance(common);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
