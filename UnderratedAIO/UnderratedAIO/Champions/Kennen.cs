using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Remoting.Messaging;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Win32;
using UnderratedAIO.Helpers;
using Environment = UnderratedAIO.Helpers.Environment;
using Orbwalking = LeagueSharp.Common.Orbwalking;

namespace UnderratedAIO.Champions
{
    class Kennen
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static AutoLeveler autoLeveler;

        public Kennen()
        {
            if (player.BaseSkinName != "Kennen") return;
            InitKennen();
            InitMenu();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Kennen</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            bool minionBlock = false;
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            foreach (var minion in MinionManager.GetMinions(player.Position, player.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
            {
                if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(player, minion, false))
                    minionBlock = true;
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
                    if (!minionBlock)
                    {
                        Clear();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    if (!minionBlock)
                    {
                        LastHit();
                    }
                    break;
                default:
                    if (!minionBlock)
                    {
                    }
                    break;
            }
            if (config.Item("QSSEnabled").GetValue<bool>()) ItemHandler.UseCleanse(config);
            if (target == null)return;
            if (player.HasBuff("KennenShurikenStorm") && config.Item("Minhelath").GetValue<Slider>().Value > player.Health / player.MaxHealth * 100)
            {
                if (Items.HasItem(ItemHandler.Wooglet.Id) && Items.CanUseItem(ItemHandler.Wooglet.Id))
                {
                    ItemHandler.Wooglet.Cast();
                }
                if (Items.HasItem(ItemHandler.Zhonya.Id) && Items.CanUseItem(ItemHandler.Zhonya.Id))
                {
                    ItemHandler.Zhonya.Cast();
                }
            }
            if (config.Item("autoq").GetValue<bool>())
            {
                if (Q.CanCast(target) && !target.IsDashing() && (MarkOfStorm(target) > 1 || (MarkOfStorm(target) > 0 && player.Distance(target) < W.Range)))
                {
                    Q.Cast(target, config.Item("packets").GetValue<bool>());
                }
            }
            if (config.Item("autow").GetValue<bool>() && W.IsReady() && MarkOfStorm(target) > 1)
            {
                if (player.Distance(target) < W.Range)
                {
                    W.Cast(config.Item("packets").GetValue<bool>());
                }
            }
        }

        private void Clear()
        {
            var targetQ = ObjectManager.Get<Obj_AI_Base>().Where(m => m.IsEnemy && Q.CanCast(m) && !(m is Obj_AI_Turret)).OrderByDescending(m => m.Health).FirstOrDefault();
            var targetW = ObjectManager.Get<Obj_AI_Base>().Where(m => m.IsEnemy && player.Distance(m) < W.Range && m.HasBuff("KennenMarkOfStorm"));
            var targetE = ObjectManager.Get<Obj_AI_Base>().Where(m => m.IsEnemy && player.Distance(m) < W.Range && Environment.Hero.countChampsAtrange(m.Position, 1000f) < 1 && !m.IsDead && !(m is Obj_AI_Turret) && !m.HasBuff("KennenMarkOfStorm") && !m.UnderTurret(true)).OrderBy(m => player.Distance(m));
            if (config.Item("useeClear").GetValue<bool>() && E.IsReady() && ((targetE.FirstOrDefault() != null && Environment.Hero.countChampsAtrange(player.Position, 1200f)<1 && !player.HasBuff("KennenLightningRush") && targetE.Count() > 1) || (player.HasBuff("KennenLightningRush") && targetE.FirstOrDefault() == null)))
            {
                E.Cast(config.Item("packets").GetValue<bool>());
                return;
            }
            if (config.Item("useqClear").GetValue<bool>() && Q.CanCast(targetQ) && !targetQ.IsDashing())
            {
                Q.Cast(targetQ, config.Item("packets").GetValue<bool>());
            }
            if (W.IsReady() && targetW.Count() >= config.Item("minw").GetValue<Slider>().Value && !player.HasBuff("KennenLightningRush"))
            {
                W.Cast(config.Item("packets").GetValue<bool>());
            }
            if (player.HasBuff("KennenLightningRush"))
            {
                player.IssueOrder(GameObjectOrder.MoveTo, targetE.FirstOrDefault());
            }
        }

        private void LastHit()
        {
            var targetQ = MinionManager.GetMinions(Q.Range).FirstOrDefault(m => m.IsEnemy && m.Health < Q.GetDamage(m) && Q.CanCast(m));
            var targetW = MinionManager.GetMinions(W.Range).FirstOrDefault(m => m.IsEnemy && m.HasBuff("KennenMarkOfStorm") && m.Health < W.GetDamage(m, 1) && player.Distance(m) < W.Range);
            if (config.Item("useqLH").GetValue<bool>() && Q.CanCast(targetQ))
            {
                Q.Cast(targetQ, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("usewLH").GetValue<bool>() && W.IsReady() && targetW!=null)
            {
                W.Cast(config.Item("packets").GetValue<bool>());
            }
        }

        private void Harass()
        {
            LastHit();
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;
            if (config.Item("useqLC").GetValue<bool>() && Q.CanCast(target) && !target.IsDashing())
            {
                Q.Cast(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("usewLC").GetValue<bool>() && W.IsReady() && W.Range < player.Distance(target) && target.HasBuff("KennenMarkOfStorm"))
            {
                W.Cast(config.Item("packets").GetValue<bool>());
            }
            
        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;
            if (config.Item("selected").GetValue<bool>())
            {
                target = CombatHelper.SetTarget(target, TargetSelector.GetSelectedTarget());
                orbwalker.ForceTarget(target);
            }
            if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target, config, ComboDamage(target));
            if (player.HasBuff("KennenLightningRush") && player.Health>target.Health && target.UnderTurret(true))
            {
                player.IssueOrder(GameObjectOrder.MoveTo, target);
            }
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            var combodamage = ComboDamage(target);
            if (config.Item("useIgnite").GetValue<bool>() && combodamage > target.Health && hasIgnite && !Q.CanCast(target) && !W.IsReady())
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }

            if (config.Item("useq").GetValue<bool>() && Q.CanCast(target) && !target.IsDashing())
            {
                Q.Cast(target, config.Item("packets").GetValue<bool>());
            } 
            if (config.Item("usew").GetValue<bool>() && W.IsReady() && W.Range>player.Distance(target) && MarkOfStorm(target)>0)
            {
                W.Cast(config.Item("packets").GetValue<bool>());
            }
            if (config.Item("usee").GetValue<bool>() && !target.UnderTurret(true)
                && E.IsReady() && (player.Distance(target) < 80
                || (!player.HasBuff("KennenLightningRush") && !Q.CanCast(target) && config.Item("useemin").GetValue<Slider>().Value < player.Health / player.MaxHealth * 100 && MarkOfStorm(target) > 0 && CombatHelper.IsPossibleToReachHim(target, 1f, new float[5] { 2f, 2f, 2f, 2f, 2f }[Q.Level - 1]))))
            {
                E.Cast(config.Item("packets").GetValue<bool>());
            }

            if (R.IsReady() && target.Distance(player) < config.Item("userrange").GetValue<Slider>().Value && (config.Item("user").GetValue<Slider>().Value < player.CountEnemiesInRange(R.Range) || (config.Item("user").GetValue<Slider>().Value == 1 && combodamage > target.Health && !Q.CanCast(target))))
            {
                R.Cast(config.Item("packets").GetValue<bool>());
            }

        }

        private void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawaa").GetValue<Circle>(), player.AttackRange);
            DrawHelper.DrawCircle(config.Item("drawee").GetValue<Circle>(), Q.Range);
            DrawHelper.DrawCircle(config.Item("drawee").GetValue<Circle>(), W.Range);
            DrawHelper.DrawCircle(config.Item("drawrr").GetValue<Circle>(), R.Range);
            DrawHelper.DrawCircle(config.Item("drawrrr").GetValue<Circle>(), config.Item("userrange").GetValue<Slider>().Value);
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }

        private float ComboDamage(Obj_AI_Hero hero)
        {
            double damage = 0;
            if (R.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.R)*2;
            }
            damage += ItemHandler.GetItemsDamage(hero);
            if (Q.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.Q);
            }
            if (W.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.W, 1);
            }
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
        private int MarkOfStorm(Obj_AI_Base target)
        {
            return target.Buffs.First(a => a.DisplayName == "KennenMarkOfStorm").Count;
        }
        private void InitKennen()
        {
            Q = new Spell(SpellSlot.Q, 950);
            Q.SetSkillshot(0.125f, 50, 1700, true, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 500);
        }

        private void InitMenu()
        {
            config = new Menu("Kennen", "Kennen", true);
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
            menuD.AddItem(new MenuItem("drawww", "Draw W range")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawee", "Draw E range")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawrr", "Draw R range")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawrrr", "Draw R activate range")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
            menuC.AddItem(new MenuItem("useemin", "Min healt to E")).SetValue(new Slider(50, 0, 100));
            menuC.AddItem(new MenuItem("user", "Use R min")).SetValue(new Slider(1, 1, 5));
            menuC.AddItem(new MenuItem("userrange", "R activate range")).SetValue(new Slider(350, 0, 550));
            menuC.AddItem(new MenuItem("selected", "Focus Selected target")).SetValue(true);
            menuC.AddItem(new MenuItem("useIgnite", "Use Ignite")).SetValue(true);
            menuC = ItemHandler.addItemOptons(menuC);
            config.AddSubMenu(menuC);
            // Harass Settings
            Menu menuLC = new Menu("Harass ", "Hcsettings");
            menuLC.AddItem(new MenuItem("useqLC", "Use Q")).SetValue(true);
            menuLC.AddItem(new MenuItem("usewLC", "Use W")).SetValue(true);
            config.AddSubMenu(menuLC);
            // Clear Settings
            Menu menuClear = new Menu("Clear ", "Clearsettings");
            menuClear.AddItem(new MenuItem("useqClear", "Use Q")).SetValue(true);
            menuClear.AddItem(new MenuItem("minw", "Min to W")).SetValue(new Slider(3, 1, 8));
            menuClear.AddItem(new MenuItem("useeClear", "Use E")).SetValue(true);
            config.AddSubMenu(menuClear);
            // LastHit Settings
            Menu menuLH = new Menu("LastHit ", "Lcsettings");
            menuLH.AddItem(new MenuItem("useqLH", "Use Q")).SetValue(true);
            menuLH.AddItem(new MenuItem("usewLH", "Use W")).SetValue(true);
            config.AddSubMenu(menuLH);
            // Misc Settings
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM.AddItem(new MenuItem("Minhelath", "Use Zhonya under x health")).SetValue(new Slider(35, 0, 100));
            menuM.AddItem(new MenuItem("autoq", "Auto Q to prepare stun")).SetValue(true);
            menuM.AddItem(new MenuItem("autow", "Auto W to stun")).SetValue(true);
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
