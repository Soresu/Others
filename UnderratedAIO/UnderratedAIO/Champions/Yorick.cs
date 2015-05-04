using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;

using UnderratedAIO.Helpers;
using Environment = UnderratedAIO.Helpers.Environment;
using Orbwalking = UnderratedAIO.Helpers.Orbwalking;

namespace UnderratedAIO.Champions
{
    class Yorick
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static bool hasGhost=false;
        public static bool GhostDelay = false;
        public static int GhostRange = 2200;
        public static AutoLeveler autoLeveler;

       public Yorick()
        {
            if (player.BaseSkinName != "Yorick") return;
            InitYorick();
            InitMenu();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Yorick</font>");
            Jungle.setSmiteSlot();
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += AfterAttack;
            Orbwalking.BeforeAttack += beforeAttack;
            Drawing.OnDraw += Game_OnDraw;
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
        }

       private void Game_OnGameUpdate(EventArgs args)
       {
           bool minionBlock = false;
           foreach (Obj_AI_Minion minion in MinionManager.GetMinions(player.Position, player.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
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

       private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
       {
           if (unit.IsMe && Q.IsReady() && ((orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && config.Item("useq").GetValue<bool>() && target is Obj_AI_Hero) || (config.Item("useqLC").GetValue<bool>() && Jungle.GetNearest(player.Position).Distance(player.Position) < player.AttackRange + 30)))
           {
               Q.Cast(config.Item("packets").GetValue<bool>());
               Orbwalking.ResetAutoAttackTimer();
               //player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
           }
       }
       private void beforeAttack(Orbwalking.BeforeAttackEventArgs args)
       {
           if (args.Unit.IsMe && Q.IsReady() && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && config.Item("useqLC").GetValue<bool>() && !(args.Target is Obj_AI_Hero) && (args.Target.Health > 700))
           {
               Q.Cast(config.Item("packets").GetValue<bool>());
               player.IssueOrder(GameObjectOrder.AutoAttack, args.Target);
           }
       }

       private void Combo()
       {
           Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
           if (target == null) return;
           if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target, config, ComboDamage(target));
           var combodmg = ComboDamage(target);
           bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
           if (config.Item("usew").GetValue<bool>() && W.CanCast(target))
           {
               W.Cast(target.Position, config.Item("packets").GetValue<bool>());
           }
           if (config.Item("usee").GetValue<bool>() && E.CanCast(target))
           {
               E.CastOnUnit(target, config.Item("packets").GetValue<bool>());
           }
           var ally = ObjectManager.Get<Obj_AI_Hero>().Where(i =>!i.IsDead && (i.Health * 100 / i.MaxHealth) <= config.Item("atpercenty").GetValue<Slider>().Value && i.IsAlly && player.Distance(i) < R.Range && !config.Item("ulty" + i.SkinName).GetValue<bool>()).OrderByDescending(i => Environment.Hero.GetAdOverFive(i)).FirstOrDefault();
           if (!Yorickghost && ally!=null && config.Item("user").GetValue<bool>() && R.IsInRange(ally) && R.IsReady() && ally != null)
           {
               R.Cast(ally, config.Item("packets").GetValue<bool>());
           }
           if (config.Item("useIgnite").GetValue<bool>() && combodmg > target.Health && hasIgnite)
           {
               player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
           }
           if (Yorickghost && !GhostDelay)
           {

               var Gtarget = TargetSelector.GetTarget(GhostRange, TargetSelector.DamageType.Physical);
               switch (config.Item("ghostTarget").GetValue<StringList>().SelectedIndex)
               {
                   case 0:
                       Gtarget = TargetSelector.GetTarget(GhostRange, TargetSelector.DamageType.Physical);
                       break;
                   case 1:
                       Gtarget = ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy && !i.IsDead && player.Distance(i) <= GhostRange).OrderBy(i => i.Health).FirstOrDefault();
                       break;
                   case 2:
                       Gtarget = ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy && !i.IsDead && player.Distance(i) <= GhostRange).OrderBy(i => player.Distance(i)).FirstOrDefault();
                       break;
                   default:
                       break;
               }
               if (Gtarget != null)
               {
                   R.CastOnUnit(Gtarget, config.Item("packets").GetValue<bool>());
                   GhostDelay = true;
                   Utility.DelayAction.Add(1000, () => GhostDelay = false);
               }
           }
       }
       private static bool Yorickghost
       {
           get { return player.Spellbook.GetSpell(SpellSlot.R).Name == "yorickreviveallyguide"; }
       }
       private void Harass()
       {
           Obj_AI_Hero target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
           if (target == null) return;
           if (config.Item("usewH").GetValue<bool>() && W.CanCast(target))
           {
               W.Cast(target, config.Item("packets").GetValue<bool>());
           }
           if (config.Item("useeH").GetValue<bool>() && E.CanCast(target))
           {
               E.CastOnUnit(target, config.Item("packets").GetValue<bool>());
           }
       }

       private void Clear()
       {
           float perc = (float)config.Item("minmana").GetValue<Slider>().Value / 100f;
           if (player.Mana < player.MaxMana * perc) return;
           var bestpos = W.GetCircularFarmLocation(MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth),100);
           if (config.Item("usewLC").GetValue<bool>() && W.IsReady() && config.Item("usewLChit").GetValue<Slider>().Value <= bestpos.MinionsHit)
           {
               W.Cast(bestpos.Position, config.Item("packets").GetValue<bool>());
           }
           var target =
               ObjectManager.Get<Obj_AI_Minion>()
                   .Where(i => i.Distance(player) < E.Range && !i.IsAlly && i.Health < E.GetDamage(i))
                   .OrderByDescending(i => i.Distance(player))
                   .FirstOrDefault();
           var targetJ =
            ObjectManager.Get<Obj_AI_Minion>()
                .Where(i => i.Distance(player) < E.Range && !i.IsAlly && i.Health > 500f)
                .OrderByDescending(i => i.Health)
                .FirstOrDefault();
           if (target==null)
           {
               target = targetJ;
           }
           if (config.Item("useeLC").GetValue<bool>() && E.CanCast(target))
           {
               E.CastOnUnit(target, config.Item("packets").GetValue<bool>());
           }
           if (config.Item("useqLC").GetValue<bool>() && Q.IsReady())
           {
               var targetQ = ObjectManager.Get<Obj_AI_Minion>()
                .Where(i => i.Distance(player) < Q.Range && (i.Health < Damage.GetSpellDamage(player, i, SpellSlot.Q) && !(i.Health < player.GetAutoAttackDamage(i))))
                .OrderByDescending(i => i.Health)
                .FirstOrDefault();
               if(targetQ==null)return;
               Q.Cast(config.Item("packets").GetValue<bool>());
               player.IssueOrder(GameObjectOrder.AutoAttack, targetQ);
           }


       }

       private void Game_OnDraw(EventArgs args)
       {
           DrawHelper.DrawCircle(config.Item("drawaa").GetValue<Circle>(), player.AttackRange);
           DrawHelper.DrawCircle(config.Item("drawww").GetValue<Circle>(), W.Range);
           DrawHelper.DrawCircle(config.Item("drawee").GetValue<Circle>(), E.Range);
           DrawHelper.DrawCircle(config.Item("drawrr").GetValue<Circle>(), R.Range);
           Helpers.Jungle.ShowSmiteStatus(config.Item("useSmite").GetValue<KeyBind>().Active, config.Item("smiteStatus").GetValue<bool>());
           Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
       }

       private float ComboDamage(Obj_AI_Hero hero)
       {
           double damage = 0;
           if (W.IsReady())
           {
               damage += Damage.GetSpellDamage(player, hero, SpellSlot.W);
           }
           if ((Items.HasItem(ItemHandler.Bft.Id) && Items.CanUseItem(ItemHandler.Bft.Id)) ||
                (Items.HasItem(ItemHandler.Dfg.Id) && Items.CanUseItem(ItemHandler.Dfg.Id)))
           {
               damage = (float)(damage * 1.2);
           }
           if (Q.IsReady())
           {
               damage += Damage.GetSpellDamage(player, hero, SpellSlot.Q);
           }
           if (E.IsReady())
           {
               damage += Damage.GetSpellDamage(player, hero, SpellSlot.E);
           }
           if (R.IsReady())
           {
               damage += Damage.GetSpellDamage(player, hero, SpellSlot.R);
           }
           damage += ItemHandler.GetItemsDamage(hero);
           var ignitedmg = player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
           if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health < damage + ignitedmg)
           {
               damage += ignitedmg;
           }
           return (float)damage;
       }

        private void InitYorick()
        {
            Q = new Spell(SpellSlot.Q, player.AttackRange);
            W = new Spell(SpellSlot.W, 600);
            W.SetSkillshot(W.Instance.SData.SpellCastTime, W.Instance.SData.LineWidth, W.Speed, false, SkillshotType.SkillshotCircle);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 850);
        }

        private void InitMenu()
       {
           config = new Menu("Yorick", "Yorick", true);
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
           menuD.AddItem(new MenuItem("drawaa", "Draw AA range")).SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));
           menuD.AddItem(new MenuItem("drawqq", "Draw Q range")).SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));
           menuD.AddItem(new MenuItem("drawww", "Draw W range")).SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));
           menuD.AddItem(new MenuItem("drawee", "Draw E range")).SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));
           menuD.AddItem(new MenuItem("drawrr", "Draw R range")).SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));
           menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
           config.AddSubMenu(menuD);
           // Combo Settings
           Menu menuC = new Menu("Combo ", "csettings");
           menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
           menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
           menuC.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
           menuC.AddItem(new MenuItem("user", "Use R")).SetValue(true);
           menuC.AddItem(new MenuItem("atpercenty", "Ult friend under")).SetValue(new Slider(30, 0, 100));
           menuC.AddItem(new MenuItem("useIgnite", "Use Ignite")).SetValue(true);
           menuC = ItemHandler.addItemOptons(menuC);
           config.AddSubMenu(menuC);
           // Harass Settings
           Menu menuH = new Menu("Harass ", "Hsettings");
           menuH.AddItem(new MenuItem("usewH", "Use W")).SetValue(true);
           menuH.AddItem(new MenuItem("useeH", "Use E")).SetValue(true);
           config.AddSubMenu(menuH);
           // LaneClear Settings
           Menu menuLC = new Menu("LaneClear ", "Lcsettings");
           menuLC.AddItem(new MenuItem("useqLC", "Use Q")).SetValue(true);
           menuLC.AddItem(new MenuItem("usewLC", "Use W")).SetValue(true);
           menuLC.AddItem(new MenuItem("usewLChit", "Min hit")).SetValue(new Slider(3, 1, 8));
           menuLC.AddItem(new MenuItem("useeLC", "Use E")).SetValue(true);
           menuLC.AddItem(new MenuItem("minmana", "Keep X% mana")).SetValue(new Slider(1, 1, 100));
           config.AddSubMenu(menuLC);
           // Misc Settings
           Menu menuM = new Menu("Misc ", "Msettings");
           menuM = Jungle.addJungleOptions(menuM);
           menuM = ItemHandler.addCleanseOptions(menuM);

           Menu autolvlM = new Menu("AutoLevel", "AutoLevel");
           autoLeveler = new AutoLeveler(autolvlM);
           menuM.AddSubMenu(autolvlM);

           menuM.AddItem(new MenuItem("ghostTarget", "Ghost target priority")).SetValue(new StringList(new[] { "Targetselector", "Lowest health", "Closest to you" }, 0));
           config.AddSubMenu(menuM);
           var sulti = new Menu("Don't ult on ", "dontult");
           foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
           {
                   sulti.AddItem(new MenuItem("ulty" + hero.SkinName, hero.SkinName)).SetValue(false);
           }
           config.AddSubMenu(sulti);
           config.AddItem(new MenuItem("packets", "Use Packets")).SetValue(false);
           config.AddItem(new MenuItem("UnderratedAIO", "by Soresu v" + Program.version.ToString().Replace(",", ".")));
           config.AddToMainMenu();
       }
    }
}
