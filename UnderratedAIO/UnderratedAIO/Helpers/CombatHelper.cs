using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace UnderratedAIO.Helpers
{
    public class CombatHelper
    {
        public static Obj_AI_Hero player = ObjectManager.Player;

        private static List<string> dotsHighDmg =
            new List<string>(
                new string[]
                {
                    "karthusfallenonecastsound", "CaitlynAceintheHole", "zedulttargetmark", "timebombenemybuff",
                    "VladimirHemoplague"
                });

        private static List<string> dotsMedDmg =
            new List<string>(
                new string[]
                {
                    "summonerdot", "cassiopeiamiasmapoison", "cassiopeianoxiousblastpoison", "bantamtraptarget",
                    "explosiveshotdebuff", "swainbeamdamage", "SwainTorment", "AlZaharMaleficVisions",
                    "fizzmarinerdoombomb"
                });

        private static List<string> dotsSmallDmg =
            new List<string>(
                new string[]
                { "deadlyvenom", "toxicshotparticle", "MordekaiserChildrenOfTheGrave", "DariusNoxianTacticsONH" });

        private static List<string> defSpells = new List<string>(new string[] { "summonerheal", "summonerbarrier" });

        private static List<string> autoAttacks =
            new List<string>(
                new string[]
                {
                    "frostarrow", "CaitlynHeadshotMissile", "KennenMegaProc", "QuinnWEnhanced", "TrundleQ",
                    "XenZhaoThrust", "XenZhaoThrust2", "XenZhaoThrust3",  "RenektonExecute", "RenektonSuperExecute",
                    "MasterYiDoubleStrike", "Parley"
                });

        private static List<int> defItems =
            new List<int>(new int[] { ItemHandler.Qss.Id, ItemHandler.Qss.Id, ItemHandler.Dervish.Id });

        public static Obj_AI_Hero SetTarget(Obj_AI_Hero target, Obj_AI_Hero targetSelected)
        {
            if (targetSelected != null && player.Distance(targetSelected) < 1500f)
            {
                return targetSelected;
            }
            return target;
        }

        #region Poppy

        public static Vector3 bestVectorToPoppyFlash(Obj_AI_Base target)
        {
            if (target == null)
            {
                return new Vector3();
            }
            Vector3 newPos = new Vector3();
            for (int i = 1; i < 7; i++)
            {
                for (int j = 1; j < 6; j++)
                {
                    newPos = new Vector3(target.Position.X + 65 * j, target.Position.Y + 65 * j, target.Position.Z);
                    var rotated = newPos.To2D().RotateAroundPoint(target.Position.To2D(), 45 * i).To3D();
                    if (rotated.IsValid() && Environment.Map.CheckWalls(rotated, target.Position) &&
                        player.Distance(rotated) < 400)
                    {
                        return rotated;
                    }
                }
            }

            return new Vector3();
        }

        #endregion

        #region Riven

        private static float RivenDamageQ(SpellDataInst spell, Obj_AI_Hero src, Obj_AI_Hero dsc)
        {
            double dmg = 0;
            if (spell.IsReady())
            {
                dmg += src.CalcDamage(
                    dsc, Damage.DamageType.Physical,
                    (-10 + (spell.Level * 20) +
                     (0.35 + (spell.Level * 0.05)) * (src.FlatPhysicalDamageMod + src.BaseAttackDamage)) * 3);
            }
            return (float) dmg;
        }

        #endregion

        #region Sejuani

        public static int SejuaniCountFrostHero(float p)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(i => i.IsEnemy && !i.IsDead && player.Distance(i) < p)
                    .SelectMany(enemy => enemy.Buffs)
                    .Count(buff => buff.Name == "sejuanifrost");
        }

        public static int KennenCountMarkHero(float p)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(i => i.IsEnemy && !i.IsDead && player.Distance(i) < p)
                    .SelectMany(enemy => enemy.Buffs)
                    .Count(buff => buff.Name == "KennenMarkOfStorm");
        }

        public static int SejuaniCountFrostMinion(float p)
        {
            var num = 0;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Minion>().Where(i => !i.IsDead && player.Distance(i) < p))
            {
                foreach (BuffInstance buff in enemy.Buffs)
                {
                    if (buff.Name == "sejuanifrost")
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        #endregion

        #region Common

        public static bool CheckCriticalBuffs(Obj_AI_Hero i)
        {
            foreach (BuffInstance buff in i.Buffs)
            {
                if (i.Health <= 6 * player.Level && dotsSmallDmg.Contains(buff.Name))
                {
                    return true;
                }
                if (i.Health <= 12*player.Level && dotsMedDmg.Contains(buff.Name))
                {
                    return true;
                }
                if (i.Health <= 25*player.Level && dotsHighDmg.Contains(buff.Name))
                {
                    return true;
                }
            }
            return false;
        }

        public static float getIncDmg()
        {
            double result = 0;
            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        i =>
                            i.Distance(player.Position) < 950 && i.IsEnemy && !i.IsAlly && !i.IsDead && !i.IsMinion &&
                            !i.IsMe)) {}


            return (float) result;
        }

        public static float GetChampDmgToMe(Obj_AI_Hero enemy)
        {
            double result = 0;
            double basicDmg = 0;
            int attacks = (int) Math.Floor(enemy.AttackSpeedMod * 5);
            for (int i = 0; i < attacks; i++)
            {
                if (enemy.Crit > 0)
                {
                    basicDmg += enemy.GetAutoAttackDamage(player) * (1 + enemy.Crit / attacks);
                }
                else
                {
                    basicDmg += enemy.GetAutoAttackDamage(player);
                }
            }
            result += basicDmg;
            var spells = enemy.Spellbook.Spells;
            foreach (var spell in spells)
            {
                var t = spell.CooldownExpires - Game.Time;
                if (t < 0.5)
                {
                    switch (enemy.SkinName)
                    {
                        case "Ahri":
                            if (spell.Slot == SpellSlot.Q)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot));
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot, 1));
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Akali":
                            if (spell.Slot == SpellSlot.R)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * spell.Ammo);
                            }
                            else if (spell.Slot == SpellSlot.Q)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot));
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot, 1));
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Amumu":
                            if (spell.Slot == SpellSlot.W)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * 5);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Cassiopeia":
                            if (spell.Slot == SpellSlot.Q || spell.Slot == SpellSlot.E || spell.Slot == SpellSlot.W)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * 2);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Fiddlesticks":
                            if (spell.Slot == SpellSlot.W || spell.Slot == SpellSlot.E)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * 5);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Garen":
                            if (spell.Slot == SpellSlot.E)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * 3);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Irelia":
                            if (spell.Slot == SpellSlot.W)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * attacks);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Karthus":
                            if (spell.Slot == SpellSlot.Q)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * 4);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "KogMaw":
                            if (spell.Slot == SpellSlot.W)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * attacks);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "LeeSin":
                            if (spell.Slot == SpellSlot.Q)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot, 1);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Lucian":
                            if (spell.Slot == SpellSlot.R)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot) * 4;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Nunu":
                            if (spell.Slot != SpellSlot.R && spell.Slot != SpellSlot.Q)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "MasterYi":
                            if (spell.Slot != SpellSlot.E)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot) * attacks;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "MonkeyKing":
                            if (spell.Slot != SpellSlot.R)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot) * 4;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Pantheon":
                            if (spell.Slot == SpellSlot.E)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot) * 3;
                            }
                            else if (spell.Slot == SpellSlot.R)
                            {
                                result += 0;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }

                            break;
                        case "Rammus":
                            if (spell.Slot == SpellSlot.R)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot) * 6;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Riven":
                            if (spell.Slot == SpellSlot.Q)
                            {
                                result += RivenDamageQ(spell, enemy, player);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Viktor":
                            if (spell.Slot == SpellSlot.R)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot, 1) * 5;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Vladimir":
                            if (spell.Slot == SpellSlot.E)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot) * 2;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        default:
                            result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            break;
                    }
                }
            }
            if (enemy.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready)
            {
                result += enemy.GetSummonerSpellDamage(player, Damage.SummonerSpell.Ignite);
            }
            foreach (var minions in
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(i => i.Distance(player.Position) < 750 && i.IsMinion && !i.IsAlly && !i.IsDead))
            {
                result += minions.GetAutoAttackDamage(player, false);
            }
            return (float) result;
        }

        public static bool HasDef(Obj_AI_Hero target)
        {
            foreach (SpellDataInst spell in target.Spellbook.Spells)
            {
                if (defSpells.Contains(spell.Name) && (spell.CooldownExpires - Game.Time) < 0)
                {
                    return true;
                }
            }
            foreach (var item in target.InventoryItems)
            {
                if (defItems.Contains((int) item.Id))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsPossibleToReachHim(Obj_AI_Hero target, float moveSpeedBuff, float duration)
        {
            var distance = player.Distance(target);
            var diff = Math.Abs((player.MoveSpeed * (1 + moveSpeedBuff)) - target.MoveSpeed);
            if (diff * duration > distance)
            {
                return true;
            }
            return false;
        }

        public static bool IsAutoattack(string spellName)
        {
            if (autoAttacks.Contains(spellName))
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}