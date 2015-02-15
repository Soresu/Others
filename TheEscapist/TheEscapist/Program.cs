#region

using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using TheEscapist.Champions;

#endregion

namespace TheEscapist
{
    internal class Program
    {
        private static string _champion;
        internal static Menu _menu;

/*
         *
         * A-C Last one Corki taken by Hunter
         * Kallista taken by newchild
         * 
         * Call your initalization method Init()
         *   * Using: championLoad = new Thread(Your_Champion.Init); 
         * 
*/

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += delegate
            {
                var onGameLoad = new Thread(Game_OnGameLoad);
                onGameLoad.Start();
            };
        }

        private static void Game_OnGameLoad()
        {
            _champion = Player.ChampionName;

            Game.PrintChat("<font color=\"#7CFC00\"><b>The Escapist:</b></font> Loaded Successfully.");

            #region switch

            Thread championLoad = null;
            switch (_champion)
            {
/*
                case "Example":
                    championLoad = new Thread(Example.Init);
                    break;
*/
                case "Aatrox":
                    championLoad = new Thread(Aatrox.Init);
                    break;
                case "Ahri":
                    championLoad = new Thread(Ahri.Init);
                    break;
                case "Akali":
                    championLoad = new Thread(Akali.Init);
                    break;
                case "Alistar":
                    championLoad = new Thread(Alistar.Init);
                    break;
                case "Amumu":
                    championLoad = new Thread(Amumu.Init);
                    break;
                case "Anivia":
                    championLoad = new Thread(Anivia.Init);
                    break;
                case "Annie":
                    championLoad = new Thread(Annie.Init);
                    break;
                case "Ashe":
                    championLoad = new Thread(Ashe.Init);
                    break;
                case "Azir":
                    championLoad = new Thread(Azir.Init);
                    break;
                case "Blitzcrank":
                    championLoad = new Thread(Blitzcrank.Init);
                    break;
                case "Brand":
                    championLoad = new Thread(Brand.Init);
                    break;
                case "Braum":
                    championLoad = new Thread(Braum.Init);
                    break;
                case "Caitlyn":
                    championLoad = new Thread(Caitlyn.Init);
                    break;
                case "Cassiopeia":
                    championLoad = new Thread(Cassiopeia.Init);
                    break;
                case "ChoGath":
                    championLoad = new Thread(ChoGath.Init);
                    break;
                case "Corki":
                    championLoad = new Thread(Corki.Init);
                    break;
                case "Darius":
                    championLoad = new Thread(Darius.Init);
                    break;
                case "Diana":
                    championLoad = new Thread(Diana.Init);
                    break;
                case "DrMundo":
                    championLoad = new Thread(DrMundo.Init);
                    break;
                case "Draven":
                    championLoad = new Thread(Draven.Init);
                    break;
                case "Elise":
                    championLoad = new Thread(Elise.Init);
                    break;
                case "Evelynn":
                    championLoad = new Thread(Evelynn.Init);
                    break;
                case "Ezreal":
                    championLoad = new Thread(Ezreal.Init);
                    break;
                case "Fiddlesticks":
                    championLoad = new Thread(Fiddlesticks.Init);
                    break;
                case "Fiora":
                    championLoad = new Thread(Fiora.Init);
                    break;
                case "Fizz":
                    championLoad = new Thread(Fizz.Init);
                    break;
                case "Galio":
                    championLoad = new Thread(Galio.Init);
                    break;
                case "Gangplank":
                    championLoad = new Thread(Gangplank.Init);
                    break;
                case "Garen":
                    championLoad = new Thread(Common.Init);
                    Common.AddSpells(
                        new Skill(
                            "Garen", EscapeType.ESCAPE_MSBUFF, new Spell(SpellSlot.Q),
                            SpellType.SPELLTYPE_SELFCAST),
                        new Skill(
                            "Garen", EscapeType.ESCAPE_SUSTAIN, new Spell(SpellSlot.W),
                            SpellType.SPELLTYPE_SELFCAST)
                            );
                    break;
                case "Gnar":
                    championLoad = new Thread(Gnar.Init);
                    break;
                case "Gragas":
                    championLoad = new Thread(Gragas.Init);
                    break;
                case "Graves":
                    championLoad = new Thread(Graves.Init);
                    break;
                case "Hecarim":
                    championLoad = new Thread(Hecarim.Init);
                    break;
                case "Heimerdinger":
                    championLoad = new Thread(Heimerdinger.Init);
                    break;
                case "Irelia":
                    championLoad = new Thread(Irelia.Init);
                    break;
                case "Janna":
                    championLoad = new Thread(Janna.Init);
                    break;
                case "Jarvan IV":
                    championLoad = new Thread(Jarvan_IV.Init);
                    break;
                case "Jax":
                    championLoad = new Thread(Jax.Init);
                    break;
                case "Jayce":
                    championLoad = new Thread(Jayce.Init);
                    break;
                case "Jinx":
                    championLoad = new Thread(Jinx.Init);
                    break;
                case "Kalista":
                    championLoad = new Thread(Kalista.Init);
                    break;
                case "Karma":
                    championLoad = new Thread(Karma.Init);
                    break;
                case "Karthus":
                    championLoad = new Thread(Karthus.Init);
                    break;
                case "Kassadin":
                    championLoad = new Thread(Kassadin.Init);
                    break;
                case "Katarina":
                    championLoad = new Thread(Katarina.Init);
                    break;
                case "Kayle":
                    championLoad = new Thread(Kayle.Init);
                    break;
                case "Kennen":
                    championLoad = new Thread(Kennen.Init);
                    break;
                case "KhaZix":
                    championLoad = new Thread(KhaZix.Init);
                    break;
                case "LeBlanc":
                    championLoad = new Thread(LeBlanc.Init);
                    break;
                case "KogMaw":
                    championLoad = new Thread(KogMaw.Init);
                    break;
                case "Lee_Sin":
                    championLoad = new Thread(Lee_Sin.Init);
                    break;
                case "Leona":
                    championLoad = new Thread(Leona.Init);
                    break;
                case "Lissandra":
                    championLoad = new Thread(Lissandra.Init);
                    break;
                case "Lucian":
                    championLoad = new Thread(Lucian.Init);
                    break;
                case "Lulu":
                    championLoad = new Thread(Common.Init);
                    Common.AddSpells(
                        new Skill(
                            "Lulu", EscapeType.ESCAPE_ENEMYCCSPELL, new Spell(SpellSlot.Q, 925),
                            SpellType.SPELLTYPE_SKILLSHOT,
                            new SData(0.25f, 60, 1450, false, SkillshotType.SkillshotLine)),
                        new Skill(
                            "Lulu", EscapeType.ESCAPE_MSBUFF, new Spell(SpellSlot.W, 650),
                            SpellType.SPELLTYPE_SELFCAST),
                        new Skill(
                            "Lulu", EscapeType.ESCAPE_SUSTAIN, new Spell(SpellSlot.E, 650),
                            SpellType.SPELLTYPE_SELFCAST),
                        new Skill(
                            "Lulu", EscapeType.ESCAPE_ENEMYCCSPELL, new Spell(SpellSlot.R, 150),
                            SpellType.SPELLTYPE_SELFCAST)
                            );
                    break;
                case "Lux":
                    championLoad = new Thread(Lux.Init);
                    break;
                case "Malphite":
                    championLoad = new Thread(Malphite.Init);
                    break;
                case "Malzahar":
                    championLoad = new Thread(Malzahar.Init);
                    break;
                case "Moakai":
                    championLoad = new Thread(Maokai.Init);
                    break;
                case "Master_Yi":
                    championLoad = new Thread(Master_Yi.Init);
                    break;
            }

            if (championLoad != null)
            {
                championLoad.Start();
            }
            else
            {
                Game.PrintChat("<font color=\"#7CFC00\"><b>The Escapist:</b></font> Champion not Supported.");
            }

            #endregion switch

            _menu = new Menu("The Escapist", "The_Escapist", true);
            _menu.AddToMainMenu();
        }
    }
}