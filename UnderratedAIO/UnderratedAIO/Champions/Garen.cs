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
    class Garen
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        private static float lastE;
        public static AutoLeveler autoLeveler;

        public Garen()
        {
            if (player.BaseSkinName != "Garen") return;
            InitGaren();
            InitMenu();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Garen</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += AfterAttack;
            Drawing.OnDraw += Game_OnDraw;
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
            
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (!minionBlock)
                    {
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

        private void Clear()
        {
            if (config.Item("useeLC").GetValue<bool>() && E.IsReady() && Environment.Minion.countMinionsInrange(player.Position,E.Range)>2)
            {
                        E.Cast(config.Item("packets").GetValue<bool>());
            }
        }

        private void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && Q.IsReady() && config.Item("useqAAA").GetValue<bool>() && !GarenE && target.IsEnemy && target is Obj_AI_Hero)
            {
                Q.Cast(config.Item("packets").GetValue<bool>());
                player.IssueOrder(GameObjectOrder.AutoAttack, target);
            }
        }

        private static bool GarenE
        {
            get
            { return player.Buffs.Any(buff => buff.Name == "GarenE"); }
        }

        private static bool GarenQ
        {
            get
            { return player.Buffs.Any(buff => buff.Name == "GarenQ"); }
        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
            if (target == null) return;
            var combodamage = ComboDamage(target);
            if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target, config, combodamage);
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            if (config.Item("useIgnite").GetValue<bool>() && combodamage > target.Health && hasIgnite)
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }
            if (config.Item("useq").GetValue<bool>() && Q.IsReady() && player.Distance(target)>player.AttackRange && !GarenE && !GarenQ && CombatHelper.IsPossibleToReachHim(target, 0.35f, new float[5]{ 1.5f, 2.25f, 3f, 3.75f, 4.5f }[Q.Level - 1]))
            {
                Q.Cast(config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useq").GetValue<bool>() && Q.IsReady() && !GarenQ && (!GarenE || (Q.IsReady() && Damage.GetSpellDamage(player, target, SpellSlot.Q) > target.Health)))
            {
                if (GarenE)
                {
                    E.Cast(config.Item("packets").GetValue<bool>());
                }
                Q.Cast(config.Item("packets").GetValue<bool>());
                player.IssueOrder(GameObjectOrder.AutoAttack, target);
            }
            if (config.Item("usee").GetValue<bool>() && E.IsReady() && !Q.IsReady() && !GarenQ && !GarenE && player.CountEnemiesInRange(E.Range)>0)
            {
                E.Cast(config.Item("packets").GetValue<bool>());
            }
            if (config.Item("user").GetValue<bool>() && R.IsReady() && (!config.Item("ult" + target.SkinName).GetValue<bool>() || player.CountEnemiesInRange(1500)==1 ) && Damage.GetSpellDamage(player, target, SpellSlot.R) > target.Health + 20)
            {
                if (GarenE)
                {
                    E.Cast(config.Item("packets").GetValue<bool>());
                }
                Utility.DelayAction.Add(100, () => R.Cast(target, config.Item("packets").GetValue<bool>()));
                
            }
            if (config.Item("usew").GetValue<bool>() && W.IsReady() && player.CountEnemiesInRange(E.Range) > 0 && target.IsFacing(player))
            {
                W.Cast(config.Item("packets").GetValue<bool>());
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawaa").GetValue<Circle>(), player.AttackRange);
            DrawHelper.DrawCircle(config.Item("drawee").GetValue<Circle>(), E.Range);
            DrawHelper.DrawCircle(config.Item("drawrr").GetValue<Circle>(), R.Range);
            Jungle.ShowSmiteStatus(config.Item("useSmite").GetValue<KeyBind>().Active, config.Item("smiteStatus").GetValue<bool>());
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }

        private float ComboDamage(Obj_AI_Hero hero)
        {
            double damage = 0;
            if (R.IsReady())
            {
                    damage += Damage.GetSpellDamage(player, hero, SpellSlot.R);
            }
            damage += ItemHandler.GetItemsDamage(hero);

            if ((Items.HasItem(ItemHandler.Bft.Id) && Items.CanUseItem(ItemHandler.Bft.Id)) ||
                 (Items.HasItem(ItemHandler.Dfg.Id) && Items.CanUseItem(ItemHandler.Dfg.Id)))
            {
                damage = (float)(damage * 1.2);
            }
            if (Q.IsReady() && !GarenQ)
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.Q);
            }
            if (E.IsReady() && !GarenE)
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.E)*3;
            }
            var ignitedmg = player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health < damage + ignitedmg)
            {
                damage += ignitedmg;
            }
            return (float)damage;
        }

        private void InitGaren()
        {
            Q = new Spell(SpellSlot.Q, player.AttackRange);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 400);
        }

        private void InitMenu()
        {
            config = new Menu("Garen", "Garen", true);
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
            menuD.AddItem(new MenuItem("drawee", "Draw E range")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawrr", "Draw R range")).SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
            menuC.AddItem(new MenuItem("user", "Use R")).SetValue(true);
            menuC.AddItem(new MenuItem("useIgnite", "Use Ignite")).SetValue(true);
            menuC = ItemHandler.addItemOptons(menuC);
            config.AddSubMenu(menuC);
            // LaneClear Settings
            Menu menuLC = new Menu("LaneClear ", "Lcsettings");
            menuLC.AddItem(new MenuItem("useeLC", "Use E")).SetValue(true);
            config.AddSubMenu(menuLC);
            // Misc Settings
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM.AddItem(new MenuItem("useqAAA", "Use Q after AA")).SetValue(true);
            menuM = Jungle.addJungleOptions(menuM);
            menuM = ItemHandler.addCleanseOptions(menuM);

            Menu autolvlM = new Menu("AutoLevel", "AutoLevel");
            autoLeveler = new AutoLeveler(autolvlM);
            menuM.AddSubMenu(autolvlM);

            config.AddSubMenu(menuM);
            var sulti = new Menu("TeamFight Ult block", "dontult");
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                sulti.AddItem(new MenuItem("ult" + hero.SkinName, hero.SkinName)).SetValue(false);
            }
            config.AddSubMenu(sulti);
            config.AddItem(new MenuItem("packets", "Use Packets")).SetValue(false);
            config.AddItem(new MenuItem("UnderratedAIO", "by Soresu v" + Program.version.ToString().Replace(",", ".")));
            config.AddToMainMenu();
        }
    }
}
