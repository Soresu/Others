using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX.Multimedia;
using UnderratedAIO.Helpers;
using Environment = UnderratedAIO.Helpers.Environment;
using Orbwalking = UnderratedAIO.Helpers.Orbwalking;

namespace UnderratedAIO.Champions
{
    class Fiora
    {
        private static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        private static float lastQ;
        public static AutoLeveler autoLeveler;

        public Fiora()
        {
            if (player.BaseSkinName != "Fiora") return;
            InitFiora();
            InitMenu();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Fiora</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Orbwalking.AfterAttack += AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;
            Jungle.setSmiteSlot();
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
        }


        private void Game_OnGameUpdate(EventArgs args)
        {
            bool minionBlock = false;
            foreach (var minion in MinionManager.GetMinions(player.Position, player.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
            {
                if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(player, minion, false))
                    minionBlock = true;
            }
            Jungle.CastSmite(config.Item("useSmite").GetValue<KeyBind>().Active);
            if (config.Item("QSSEnabled").GetValue<bool>()) ItemHandler.UseCleanse(config);
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                default:
                    break;
            }
        }
        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            Obj_AI_Hero targ = (Obj_AI_Hero)target;
            bool rapid = player.GetAutoAttackDamage(targ) * 6 + ComboDamage(targ) > targ.Health ||(player.Health<targ.Health && player.Health<player.MaxHealth/2);
            if (unit.IsMe && E.IsReady() && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && (config.Item("usee").GetValue<bool>() || (unit.IsMe && config.Item("RapidAttack").GetValue<KeyBind>().Active || rapid)) && !Orbwalking.CanAttack())
            {
                E.Cast(config.Item("packets").GetValue<bool>());
                Orbwalking.ResetAutoAttackTimer();
            }
            if (unit.IsMe && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && (config.Item("RapidAttack").GetValue<KeyBind>().Active || rapid) && !Orbwalking.CanAttack())
            {
                if (Q.CanCast(targ))
                {
                        Q.CastOnUnit(targ, config.Item("packets").GetValue<bool>());
                        Orbwalking.ResetAutoAttackTimer();
                }
            }
        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (target == null) return;
            if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target, config, ComboDamage(target));
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            if (config.Item("useq").GetValue<bool>() && Q.CanCast(target) && lastQ.Equals(0))
            {
                Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                lastQ = System.Environment.TickCount;
            }
            if (config.Item("useq").GetValue<bool>() && Q.CanCast(target) && !lastQ.Equals(0))
            {
                var time = System.Environment.TickCount - lastQ;
                if (time > 3500f || player.Distance(target) > 350f || Q.GetDamage(target) > target.Health)
                {
                    Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                    lastQ = 0;
                }
            }
            if (config.Item("user").GetValue<bool>() && R.CanCast(target) && ComboDamage(target) > target.Health * 1.3 && NeedToUlt(target))
            {
                R.CastOnUnit(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useIgnite").GetValue<bool>() && hasIgnite && ComboDamage(target) > target.Health)
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }
        }

        private bool NeedToUlt(Obj_AI_Hero target)
        {
            if (player.Distance(target) > 300f && !Q.CanCast(target))
            {
                return true;
            }
            if (player.UnderTurret(true))
            {
                return true;
            }
            if (player.Health<target.Health && player.Health<player.MaxHealth/2)
            {
                return true;
            }
            return false;
        }

        public static void Game_ProcessSpell(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args)
        {
            if (args==null || hero==null)
            {
                return;
            }
            var spellName = args.SData.Name;
            if (W.IsReady() && args.Target.IsMe && (Orbwalking.IsAutoAttack(spellName) || CombatHelper.IsAutoattack(spellName)) && ((config.Item("usew").GetValue<bool>() && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) || config.Item("autoW").GetValue<bool>()) && !(hero is Obj_AI_Turret || hero.Name=="OdinNeutralGuardian") && player.Distance(hero) < 700)
            {
                var perc = config.Item("minmanaP").GetValue<Slider>().Value / 100f;
                if (player.Mana > player.MaxMana * perc && hero.TotalAttackDamage>50 && ((player.UnderTurret(true) && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) || !player.UnderTurret(true)))
                {
                    W.Cast(config.Item("packets").GetValue<bool>());
                } 
                
            }
            if (!config.Item("dodgeWithR").GetValue<bool>()) return;
                if (spellName == "CurseofTheSadMummy")
                {
                    if (player.Distance(hero.Position) <= 600f)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
                if (spellName == "EnchantedCrystalArrow")
                {
                    if (player.Distance(hero.Position) <= 400f)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
                if (spellName == "EnchantedCrystalArrow" || spellName == "EzrealTrueshotBarrage" || spellName == "JinxR" || spellName == "sejuaniglacialprison")
                {
                    if (player.Distance(hero.Position) <= 400f)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
                if (spellName == "InfernalGuardian" || spellName == "UFSlash")
                {
                    if (player.Distance(args.End) <= 270f)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
                if (spellName == "BlindMonkRKick" || spellName == "SyndraR" || spellName == "VeigarPrimordialBurst" || spellName == "AlZaharNetherGrasp")
                {
                    if (args.Target.IsMe)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
                if (spellName == "TristanaR" || spellName == "ViR")
                {
                    if (args.Target.IsMe || player.Distance(args.Target.Position) <= 100f)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
                if (spellName == "GalioIdolOfDurand")
                {
                    if (player.Distance(hero.Position) <= 600f)
                    {
                        R.Cast(TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical));
                    }
                }
        }
        private void Clear()
        {
            float perc = (float)config.Item("minmana").GetValue<Slider>().Value / 100f;
            if (player.Mana < player.MaxMana * perc) return;

            var target =
                ObjectManager.Get<Obj_AI_Minion>()
                     .Where(i => i.Distance(player) < Q.Range && i.Health < Q.GetDamage(i))
                     .OrderByDescending(i => i.Distance(player))
                     .FirstOrDefault();
            if (config.Item("useqLC").GetValue<bool>() && Q.CanCast(target))
            {
                Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useeLC").GetValue<bool>() && Environment.Minion.countMinionsInrange(player.Position,Q.Range)>3)
            {
                E.Cast(config.Item("packets").GetValue<bool>());
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawaa").GetValue<Circle>(), player.AttackRange);
            DrawHelper.DrawCircle(config.Item("drawqq").GetValue<Circle>(), Q.Range);
            DrawHelper.DrawCircle(config.Item("drawrr").GetValue<Circle>(), R.Range);
            Helpers.Jungle.ShowSmiteStatus(config.Item("useSmite").GetValue<KeyBind>().Active, config.Item("smiteStatus").GetValue<bool>());
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }

        private static float ComboDamage(Obj_AI_Hero hero)
        {
            double damage = 0;
            if (Q.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.Q)*2;
            }
            if (W.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.W);
            }
            if (R.IsReady())
            {
                damage += (float) GetRDamage(hero);
            }
                damage += ItemHandler.GetItemsDamage(hero);
            var ignitedmg = player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health < damage + ignitedmg)
            {
                damage += ignitedmg;
            }
            return (float)damage;
        }

        private static float fioraRSingle(Obj_AI_Hero target)
        {
            return (float)Damage.CalcDamage(
                player, target, Damage.DamageType.Physical,
                new float[3] { 125f, 255f, 385f }[R.Level - 1] + 0.9f * player.FlatPhysicalDamageMod);
        }

        private static double GetRDamage(Obj_AI_Hero target)
        {
            float singleR = fioraRSingle(target);
              if (target.CountEnemiesInRange(400) == 1)
                {
                    return Damage.GetSpellDamage(player, target, SpellSlot.R);
                }
                if (target.CountEnemiesInRange(400) == 2)
                {
                    return singleR + (singleR*0.4)*2;
                }
                    return singleR + singleR*0.4;
        }
        private void InitFiora()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 400);
        }

        private void InitMenu()
        {
            config = new Menu("Fiora", "Fiora", true);
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
            menuD.AddItem(new MenuItem("drawaa", "Draw AA range")).SetValue(new Circle(false, Color.FromArgb(180, 58, 100, 150)));
            menuD.AddItem(new MenuItem("drawqq", "Draw Q range")).SetValue(new Circle(false, Color.FromArgb(180, 58, 100, 150)));
            menuD.AddItem(new MenuItem("drawrr", "Draw R range")).SetValue(new Circle(false, Color.FromArgb(180, 58, 100, 150)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
            menuC.AddItem(new MenuItem("user", "R if killable")).SetValue(true);
            menuC.AddItem(new MenuItem("RapidAttack", "Fast AA Combo always")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle));
            menuC.AddItem(new MenuItem("dodgeWithR", "Dodge ults with R")).SetValue(true);
            menuC.AddItem(new MenuItem("useIgnite", "Use Ignite")).SetValue(true);
            menuC = ItemHandler.addItemOptons(menuC);
            config.AddSubMenu(menuC);
            // LaneClear Settings
            Menu menuLC = new Menu("LaneClear ", "Lcsettings");
            menuLC.AddItem(new MenuItem("useqLC", "Use Q")).SetValue(true);
            menuLC.AddItem(new MenuItem("useeLC", "Use E")).SetValue(true);
            menuLC.AddItem(new MenuItem("minmana", "Keep X% mana")).SetValue(new Slider(1, 1, 100));
            config.AddSubMenu(menuLC);
            // Misc Settings
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM.AddItem(new MenuItem("autoW", "Auto W AA")).SetValue(true);
            menuM.AddItem(new MenuItem("minmanaP", "Min mana percent")).SetValue(new Slider(1, 1, 100));
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

