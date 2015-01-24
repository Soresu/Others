using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace opgg.Helpers
{
    public class Jungle
    {
        public static Obj_AI_Hero player = ObjectManager.Player;
        private static readonly string[] jungleMonsters = { "TT_Spiderboss", "SRU_Blue", "SRU_Red", "SRU_Dragon", "SRU_Baron" };
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        public static Spell smite;
        public static Obj_AI_Minion GetNearest(Vector3 pos)
        {
            var minions =
            ObjectManager.Get<Obj_AI_Minion>()
            .Where(minion => minion.IsValid && jungleMonsters.Any(name => minion.Name.StartsWith(name)) && !jungleMonsters.Any(name => minion.Name.Contains("Mini")) && !jungleMonsters.Any(name => minion.Name.Contains("Spawn")));
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            double? nearest = null;
            foreach (Obj_AI_Minion minion in objAiMinions)
            {
                double distance = Vector3.Distance(pos, minion.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            return sMinion;
        }
        public static double smiteDamage()
        {
            int level = ObjectManager.Player.Level;
            int[] damage =
                {
                20*level + 370,
                30*level + 330,
                40*level + 240,
                50*level + 100
                };
            return damage.Max();
        }
        //Kurisu
        public static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        public static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        public static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        public static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        public static string smitetype()
        {
            if (SmiteBlue.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(id => Items.HasItem(id)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }
        public static void setSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, smitetype(), StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                smite = new Spell(smiteSlot, 700);
                return;
            }
        }
        public static void CastSmite(Obj_AI_Minion target)
        {
            smite.Slot = smiteSlot;
            ObjectManager.Player.Spellbook.CastSpell(smiteSlot, target);
        }

    }
}