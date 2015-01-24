using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace opgg.Helpers
{
    public class Environment
    {
        public static Obj_AI_Hero player = ObjectManager.Player;

        public class Minion
        {
            public static int countMinionsInrange(Obj_AI_Minion l, float p)
            {
                return ObjectManager.Get<Obj_AI_Minion>().Count(i => !i.IsDead && i.IsEnemy && l.Distance(i) < p);
            }
            public static int countMinionsInrange(Vector3 l, float p)
            {
                return ObjectManager.Get<Obj_AI_Minion>().Count(i => !i.IsDead && i.IsEnemy && i.Distance(l) < p);
            }
            public static Vector3 bestVectorToAoeFarm(Vector3 center, float spellrange, float spellWidth)
            {
                var minions = MinionManager.GetMinions(center, spellrange, MinionTypes.All, MinionTeam.NotAlly);
                Vector3 bestPos = new Vector3();
                int hits = 0;
                foreach (var minion in minions)
                {

                    if (countMinionsInrange(minion.Position, spellWidth) > hits)
                    {
                        bestPos = minion.Position;
                        hits = countMinionsInrange(minion.Position, spellWidth);
                    }
                    Vector3 newPos = new Vector3(minion.Position.X + 80, minion.Position.Y + 80, minion.Position.Z);
                    for (int i = 1; i < 4; i++)
                    {
                        var rotated = newPos.To2D().RotateAroundPoint(newPos.To2D(), 90 * i).To3D();
                        if (countMinionsInrange(rotated, spellWidth) > hits && player.Distance(rotated) <= spellrange)
                        {
                            bestPos = newPos;
                            hits = countMinionsInrange(rotated, spellWidth);
                        }
                    }
                }

                return bestPos;
            }
        }

        public class Hero
        {
            public static int countChampsAtrange(Vector3 l, float p)
            {
                return ObjectManager.Get<Obj_AI_Hero>().Count(i => !i.IsDead && i.IsEnemy && i.Distance(l) < p);
            }
            public static int countChampsAtrangeA(Vector3 l, float p)
            {
                return ObjectManager.Get<Obj_AI_Hero>().Count(i => !i.IsDead && i.IsAlly && i.Distance(l) < p);
            }
            public static Obj_AI_Hero mostEnemyAtFriend(Obj_AI_Hero player, float spellRange, float spellWidth, int min=0)
            {
                return
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(i => !i.IsDead && i.IsAlly && i.CountEnemysInRange(spellWidth)>min && i.Distance(player) < spellRange)
                        .OrderByDescending(i => i.CountEnemysInRange(spellWidth))
                        .FirstOrDefault();
            }

            public static Vector3 bestVectorToAoeSpell(IEnumerable<Obj_AI_Hero> heroes,
                float spellrange,
                float spellwidth)
            {
                Vector3 bestPos = new Vector3();
                int hits = 0;
                foreach (var hero in heroes)
                {

                    if (countChampsAtrange(hero.Position, spellwidth) > hits)
                    {
                        bestPos = hero.Position;
                        hits = countChampsAtrange(hero.Position, spellwidth);
                    }
                    Vector3 newPos = new Vector3(hero.Position.X + 80, hero.Position.Y + 80, hero.Position.Z);
                    for (int i = 1; i < 4; i++)
                    {
                        var rotated = newPos.To2D().RotateAroundPoint(newPos.To2D(), 90 * i).To3D();
                        if (countChampsAtrange(rotated, spellwidth) > hits && player.Distance(rotated) <= spellrange)
                        {
                            bestPos = newPos;
                            hits = countChampsAtrange(rotated, spellwidth);
                        }
                    }
                }

                return bestPos;
            }

            public static float GetAdOverFive(Obj_AI_Hero hero)
            {
                    double basicDmg = 0;
                    int attacks = (int)Math.Floor(hero.AttackSpeedMod * 5);
                    for (int i = 0; i < attacks; i++)
                    {

                        if (hero.Crit > 0)
                        {

                            basicDmg += hero.GetAutoAttackDamage(player) * (1 + hero.Crit / attacks);
                        }
                        else
                        {

                            basicDmg += hero.GetAutoAttackDamage(player);
                        }
                    } 
                return (float)basicDmg;
                }

             
        }

        public class Turret
        {
            public static int countTurretsInRange(Obj_AI_Hero l)
            {
                return ObjectManager.Get<Obj_AI_Turret>().Count(i => !i.IsDead && i.IsEnemy && l.Distance(i) < 750f);

            }
        }

        public class Map
        {
            public static bool CheckWalls(Vector3 player, Vector3 enemy)
            {
                var distance = player.Distance(enemy);
                for (int i = 1; i < 6; i++)
                {
                    if (player.Extend(enemy, distance + 60 * i).IsWall())
                        return true;
                }
                return false;
            }
        }
    }
}