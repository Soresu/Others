using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Remoting.Messaging;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using UnderratedAIO.Helpers;
using Environment = UnderratedAIO.Helpers.Environment;
using Orbwalking = UnderratedAIO.Helpers.Orbwalking;

namespace UnderratedAIO.Champions
{
    class Renekton
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        private static float lastE;
        private static Vector3 lastEpos;
        private static Bool wChancel=false;
        public static AutoLeveler autoLeveler;

        public Renekton()
        {
            if (player.BaseSkinName != "Renekton") return;
            InitRenekton();
            InitMenu();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Renekton</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += beforeAttack;
            Orbwalking.AfterAttack += afterAttack;
            Drawing.OnDraw += Game_OnDraw;
            Jungle.setSmiteSlot();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            bool minionBlock = false;
            foreach (Obj_AI_Minion minion in MinionManager.GetMinions(player.Position, player.AttackRange + 55, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
            {
                if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(player, minion, false))
                    minionBlock = true;
            }
            if (System.Environment.TickCount - lastE>4100)
            {
                lastE = 0;
            }
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (!minionBlock)
                    {
                        Harass();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                default:
                    if (!minionBlock)
                    {
                    }
                    break;
            }
            Jungle.CastSmite(config.Item("useSmite").GetValue<KeyBind>().Active);
            if (config.Item("QSSEnabled").GetValue<bool>()) ItemHandler.UseCleanse(config);
        }

        private void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && target is Obj_AI_Hero && (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed))
            {
                var time = Game.Time - W.Instance.CooldownExpires;
                if (config.Item("hyd").GetValue<bool>() && time < -9 || (!W.IsReady() && time<-1))
                {
                    ItemHandler.castHydra((Obj_AI_Hero)target);
                }
            }
        }

        private void beforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe && W.IsReady() && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && config.Item("usew").GetValue<bool>() && args.Target is Obj_AI_Hero)
            {
                if ((player.Mana > 40 && !fury ) || (Q.IsReady() && canBeOpWIthQ(player.Position))) return;
                
                W.Cast(config.Item("packets").GetValue<bool>());
                return;

            }
            if (args.Unit.IsMe && W.IsReady() && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed &&
                config.Item("usewH").GetValue<bool>() && args.Target is Obj_AI_Hero)
            {
                W.Cast(config.Item("packets").GetValue<bool>());
            }
        }
        private static bool rene
        {
            get
            { return player.Buffs.Any(buff => buff.Name == "renektonsliceanddicedelay"); }
        }
        private static bool fury
        {
            get
            { return player.Buffs.Any(buff => buff.Name == "renektonrageready"); }
        }
        private static bool renw
        {
            get
            { return player.Buffs.Any(buff => buff.Name == "renektonpreexecute"); }
        }
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(E.Range * 2, TargetSelector.DamageType.Physical);
            if (target == null) return;
            if (config.Item("selected").GetValue<bool>())
            {
                target = CombatHelper.SetTarget(target, TargetSelector.GetSelectedTarget());
                orbwalker.ForceTarget(target);
            }
            if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target, config, ComboDamage(target));
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            var FuryQ = Damage.GetSpellDamage(player, target, SpellSlot.Q) * 0.5;
            var FuryW = Damage.GetSpellDamage(player, target, SpellSlot.W) * 0.5;
            var eDmg = Damage.GetSpellDamage(player, target, SpellSlot.E);
            var combodamage = ComboDamage(target);
            if (config.Item("useIgnite").GetValue<bool>() && hasIgnite && player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }
            if (player.Distance(target) > E.Range && E.IsReady()&& (W.IsReady() || Q.IsReady()) && lastE.Equals(0) && config.Item("usee").GetValue<bool>())
            {
                var closeGapTarget = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(i => i.IsEnemy && player.Distance(i)<E.Range && !i.IsDead && i.Distance(target.ServerPosition) < E.Range);
                if (closeGapTarget != null)
                {
                    if ((canBeOpWIthQ(closeGapTarget.Position) || fury) && !rene)
                    {
                        if (E.CanCast(closeGapTarget))
                        {
                            E.Cast(closeGapTarget.Position, config.Item("packets").GetValue<bool>());
                            lastE = System.Environment.TickCount;
                            return;
                        }
                    }
                }
            }
            if (config.Item("useq").GetValue<bool>() && Q.CanCast(target) && !renw && !player.IsDashing() && (!W.IsReady() || ((W.IsReady() && !fury) || (player.Health<target.Health) || Environment.Minion.countMinionsInrange(player.Position, Q.Range)+player.CountEnemiesInRange(Q.Range)>3 && fury) ))
            {
                Q.Cast(config.Item("packets").GetValue<bool>());
            }
            var distance = player.Distance(target.Position);
            if (config.Item("usee").GetValue<bool>() && lastE.Equals(0) && E.CanCast(target) && (eDmg > target.Health || (((W.IsReady() && canBeOpWIthQ(target.Position) && !rene) || (distance > target.Distance(player.Position.Extend(target.Position, E.Range)) - distance)))))
            {
                E.Cast(target.Position, config.Item("packets").GetValue<bool>());
                lastE = System.Environment.TickCount;
                return;
            }
            if (config.Item("usee").GetValue<bool>() && !lastE.Equals(0) && (eDmg + player.GetAutoAttackDamage(target) > target.Health || (((W.IsReady() && canBeOpWIthQ(target.Position) && !rene) || (distance < target.Distance(player.Position.Extend(target.Position, E.Range)) - distance) || player.Distance(target) > E.Range-100))))
            {
                var time = System.Environment.TickCount - lastE;
                if (time > 3600f || combodamage > target.Health || (player.Distance(target) > E.Range - 100))
                {
                   E.Cast(target.Position, config.Item("packets").GetValue<bool>());
                    lastE = 0;
                }
                
            }
            if ((player.Health * 100 / player.MaxHealth) <= config.Item("user").GetValue<Slider>().Value || config.Item("userindanger").GetValue<Slider>().Value < player.CountEnemiesInRange(R.Range))
            {
                R.Cast(config.Item("packets").GetValue<bool>());
            }
        }

        private bool canBeOpWIthQ(Vector3 vector3)
        {
            if(fury)return false;
            if((player.Mana>45 && !fury) || (Q.IsReady() && player.Mana + Environment.Minion.countMinionsInrange(vector3, Q.Range)*2.5 + player.CountEnemiesInRange(Q.Range)*10>50))return true;
            return false;
        }
        private bool canBeOpwithW()
        {
            if (player.Mana + 20 > 50) return true;
            return false;
        }
        private void Harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (target == null) return;
            if (config.Item("eqweb").GetValue<bool>() && Q.IsReady() && E.IsReady() && lastE.Equals(0) && fury && !rene)
            {
                if (config.Item("donteqwebtower").GetValue<bool>() &&  player.Position.Extend(target.Position,E.Range).UnderTurret(true))
                {
                    return;                
                }
                var closeGapTarget = ObjectManager.Get<Obj_AI_Minion>().Where(i => i.IsEnemy && player.Distance(i) < E.Range && !i.IsDead && i.Distance(target.ServerPosition) < Q.Range-40).OrderByDescending(i=> Environment.Minion.countMinionsInrange(i.Position,Q.Range)).FirstOrDefault();
                if (closeGapTarget!=null)
                {
                        lastEpos = player.ServerPosition;
                        Utility.DelayAction.Add(4100, () => lastEpos=new Vector3());
                        E.Cast(closeGapTarget.Position, config.Item("packets").GetValue<bool>());
                        lastE = System.Environment.TickCount;
                        return;
                }
                else
                {
                        lastEpos = player.ServerPosition;
                        Utility.DelayAction.Add(4100, () => lastEpos = new Vector3());
                        E.Cast(target.Position, config.Item("packets").GetValue<bool>());
                        lastE = System.Environment.TickCount;
                        return;
                }
            }
            if (config.Item("useqH").GetValue<bool>() && Q.CanCast(target))
            {
                Q.Cast(config.Item("packets").GetValue<bool>());
            }
            if (config.Item("eqweb").GetValue<bool>() && !lastE.Equals(0) && rene && !Q.IsReady() && !renw)
            {
                    if (lastEpos.IsValid())
                    {
                        E.Cast(player.Position.Extend(lastEpos, 350f), config.Item("packets").GetValue<bool>());
                    }
            }
        }

        private void Clear()
        {
            if (config.Item("useqLC").GetValue<bool>() && Q.IsReady() && !player.IsDashing())
            {
                if (Environment.Minion.countMinionsInrange(player.Position, Q.Range) >= config.Item("minimumMini").GetValue<Slider>().Value)
                {
                    Q.Cast(config.Item("packets").GetValue<bool>());
                    return;
                }
            }
            if (config.Item("useeLC").GetValue<bool>() && E.IsReady())
            {
                var minionsForE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);
                MinionManager.FarmLocation bestPosition = E.GetLineFarmLocation(minionsForE);
                if (bestPosition.Position.IsValid() && !player.Position.Extend(bestPosition.Position.To3D(), E.Range).UnderTurret(true) && !bestPosition.Position.IsWall())
                    if (bestPosition.MinionsHit >= 2)
                        E.Cast(bestPosition.Position, config.Item("packets").GetValue<bool>());
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawaa").GetValue<Circle>(), player.AttackRange + 55);
            DrawHelper.DrawCircle(config.Item("drawqq").GetValue<Circle>(), Q.Range);
            DrawHelper.DrawCircle(config.Item("drawee").GetValue<Circle>(), E.Range);
            DrawHelper.DrawCircle(config.Item("drawrr").GetValue<Circle>(), R.Range);
            Helpers.Jungle.ShowSmiteStatus(config.Item("useSmite").GetValue<KeyBind>().Active, config.Item("smiteStatus").GetValue<bool>());
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }

        private float ComboDamage(Obj_AI_Hero hero)
        {
            double damage = 0;
            if (Q.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.Q);
            }
            if (W.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.W);
            }
            if (E.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.E);
            }
            if (R.IsReady())
            {
                if (config.Item("rDamage").GetValue<bool>())
                {
                    damage += Damage.GetSpellDamage(player, hero, SpellSlot.R)*15;
                }
            }

            damage += ItemHandler.GetItemsDamage(hero);

            if ((Items.HasItem(ItemHandler.Bft.Id) && Items.CanUseItem(ItemHandler.Bft.Id)) ||
                 (Items.HasItem(ItemHandler.Dfg.Id) && Items.CanUseItem(ItemHandler.Dfg.Id)))
            {
                damage = (float)(damage * 1.2);
            }

            var ignitedmg = player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health < damage + ignitedmg)
            {
                damage += ignitedmg;
            }
            return (float)damage;
        }

        private void InitRenekton()
        {
            Q = new Spell(SpellSlot.Q, 300);
            W = new Spell(SpellSlot.W, player.AttackRange + 55);
            E = new Spell(SpellSlot.E, 450);
            E.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Speed, false, SkillshotType.SkillshotCone);
            R = new Spell(SpellSlot.R, 300);
        }

        private void InitMenu()
        {
            config = new Menu("Renekton", "Renekton", true);
            // Target Selector
            Menu menuTS = new Menu("Selector", "tselect");
            TargetSelector.AddToMenu(menuTS);
            config.AddSubMenu(menuTS);

            // Orbwalker
            Menu menuOrb = new Menu("Orbwalker", "orbwalker");
            orbwalker = new Orbwalking.Orbwalker(menuOrb);
            config.AddSubMenu(menuOrb);

            // Draw settings
            Menu menuD = new Menu("Drawings ", "dsettings");
            menuD.AddItem(new MenuItem("drawaa", "Draw AA range")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawqq", "Draw Q range")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawee", "Draw E range")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawrr", "Draw R range")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            menuD.AddItem(new MenuItem("rDamage", "Calc R damge too")).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
            menuC.AddItem(new MenuItem("user", "Use R under")).SetValue(new Slider(20, 0, 100));
            menuC.AddItem(new MenuItem("userindanger", "Use R above X enemy")).SetValue(new Slider(2, 1, 5));
            menuC.AddItem(new MenuItem("selected", "Focus Selected target")).SetValue(true);
            menuC.AddItem(new MenuItem("useIgnite", "Use Ignite")).SetValue(true);
            menuC = ItemHandler.addItemOptons(menuC);
            config.AddSubMenu(menuC);
            // Harass Settings
            Menu menuH = new Menu("Harass ", "Hsettings");
            menuH.AddItem(new MenuItem("useqH", "Use Q")).SetValue(true);
            menuH.AddItem(new MenuItem("usewH", "Use W")).SetValue(true);
            menuH.AddItem(new MenuItem("eqweb", "E-furyQ-Eback if possible")).SetValue(true);
            menuH.AddItem(new MenuItem("donteqwebtower", "Don't dash under tower")).SetValue(true);
            config.AddSubMenu(menuH);
            // LaneClear Settings
            Menu menuLC = new Menu("LaneClear ", "Lcsettings");
            menuLC.AddItem(new MenuItem("useqLC", "Use Q")).SetValue(true);
            menuLC.AddItem(new MenuItem("minimumMini", "Use Q min minion")).SetValue(new Slider(2, 1, 6));
            menuLC.AddItem(new MenuItem("usewLC", "Use W")).SetValue(true);
            menuLC.AddItem(new MenuItem("useeLC", "Use E")).SetValue(true);
            config.AddSubMenu(menuLC);
            // Misc Settings
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM = Jungle.addJungleOptions(menuM);
            menuM = ItemHandler.addCleanseOptions(menuM);

            Menu autolvlM = new Menu("AutoLevel", "AutoLevel");
            autoLeveler = new AutoLeveler(autolvlM);
            menuM.AddSubMenu(autolvlM);

            config.AddSubMenu(menuM);
            config.AddItem(new MenuItem("packets", "Use Packets")).SetValue(false);
            config.AddItem(new MenuItem("UnderratedAIO", "by Soresu v" + Program.version.ToString().Replace(",", ".")));
            config.AddToMainMenu();
        }
    }
}
