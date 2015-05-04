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
    class Maokai
    {
        public static Menu config;
        public static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, Qint, W, E, R;
        public static bool turnOff = false;
        public static AutoLeveler autoLeveler;

        public Maokai()
        {
            if (player.BaseSkinName != "Maokai") return;
            InitMao();
            InitMenu();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Maokai</font>");
            Drawing.OnDraw += Game_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += OnPossibleToInterrupt;
            Helpers.Jungle.setSmiteSlot();
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
        }

        private void OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("useQint").GetValue<bool>())
            {
                if (Qint.CanCast(sender)) Q.Cast(sender, config.Item("packets").GetValue<bool>());
            }
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("useQgc").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(Qint.Range) && Q.IsReady()) Q.Cast(gapcloser.End, config.Item("packets").GetValue<bool>());
            }
        }
        private static bool maoR
        {
            get
            { return player.Buffs.Any(buff => buff.Name == "MaokaiDrain3"); }
        }
        private static int maoRStack
        {
            get
            {
                return R.Instance.Ammo;
            }
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
                        break;
                    default:
                        break;
                }
                if (!minionBlock)
                {
                    AutoE();
                }
                Jungle.CastSmite(config.Item("useSmite").GetValue<KeyBind>().Active);
                if (config.Item("QSSEnabled").GetValue<bool>()) ItemHandler.UseCleanse(config);
            
        }

        private void AutoE()
        {
            if (config.Item("autoe").GetValue<bool>() && E.IsReady())
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (E.CanCast(target) &&
                    (target.HasBuff("zhonyasringshield") || 
                     target.HasBuffOfType(BuffType.Snare) ||
                     target.HasBuffOfType(BuffType.Taunt) || 
                     target.HasBuffOfType(BuffType.Stun) ||
                     target.HasBuffOfType(BuffType.Suppression) ||
                     target.HasBuffOfType(BuffType.Fear)))
                {
                    E.Cast(target);
                }
            }
        }

        private void Clear()
        {
            float perc = config.Item("minmana").GetValue<Slider>().Value / 100f;
            if (player.Mana < player.MaxMana * perc) return;
            MinionManager.FarmLocation bestPositionE = E.GetCircularFarmLocation(MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly));
            MinionManager.FarmLocation bestPositionQ = Q.GetLineFarmLocation(MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly));
            if (config.Item("useeLC").GetValue<bool>() && E.IsReady() && bestPositionE.MinionsHit > config.Item("ehitLC").GetValue<Slider>().Value)
            {
                E.Cast(bestPositionE.Position, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useqLC").GetValue<bool>() && Q.IsReady() && bestPositionQ.MinionsHit > config.Item("qhitLC").GetValue<Slider>().Value)
            {
                Q.Cast(bestPositionQ.Position, config.Item("packets").GetValue<bool>());
            }
        }

        private void Harass()
        {
            float perc = config.Item("minmanaH").GetValue<Slider>().Value / 100f;
            if (player.Mana < player.MaxMana * perc) return;
            Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;
            if (config.Item("useqH").GetValue<bool>() && Q.CanCast(target))
            {
                Q.Cast(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useeH").GetValue<bool>() && E.CanCast(target))
            {
                    E.Cast(target, config.Item("packets").GetValue<bool>());
            }
        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                if (maoR)
                {
                    if (!turnOff)
                    {
                        turnOff = true;
                        Utility.DelayAction.Add(2600, () => turnOffUlt());
                    }
                    
                }
                return;
            }
            if (config.Item("selected").GetValue<bool>())
            {
                target = CombatHelper.SetTarget(target, TargetSelector.GetSelectedTarget());
                orbwalker.ForceTarget(target);
            }
            var manaperc = player.Mana / player.MaxMana * 100;
            if (player.HasBuff("MaokaiSapMagicMelee") && player.Distance(target)<Orbwalking.GetRealAutoAttackRange(player)+75)
            {
                return;
            }
            if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target, config, ComboDamage(target));
            if (config.Item("useq").GetValue<bool>() && Q.CanCast(target) && config.Item("usee").GetValue<bool>() &&
                player.Distance(target) <= config.Item("useqrange").GetValue<Slider>().Value &&
                ((config.Item("useqroot").GetValue<bool>() && (!target.HasBuffOfType(BuffType.Snare) && !target.HasBuffOfType(BuffType.Slow) && !target.HasBuffOfType(BuffType.Stun) && !target.HasBuffOfType(BuffType.Suppression))) || !config.Item("useqroot").GetValue<bool>()))
            {
                Q.Cast(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("usew").GetValue<bool>())
            {
                    
                    if (config.Item("blocke").GetValue<bool>() && player.Distance(target)<W.Range && W.IsReady() && E.CanCast(target))
                    {
                        E.Cast(target, config.Item("packets").GetValue<bool>());
                        CastR(target);
                        Utility.DelayAction.Add(100, () => W.Cast(target, config.Item("packets").GetValue<bool>()));
                    }
                    else if(W.CanCast(target))
                    {
                        CastR(target);
                        W.Cast(target, config.Item("packets").GetValue<bool>()); 
                    }
                    
            }
            if (config.Item("usee").GetValue<bool>() && E.CanCast(target))
            {
                if (!config.Item("blocke").GetValue<bool>() || config.Item("blocke").GetValue<bool>() && !W.IsReady())
                {
                    E.Cast(target, config.Item("packets").GetValue<bool>()); 
                }   
            }
            
            if (R.IsReady())
            {
                bool enoughEnemies = config.Item("user").GetValue<Slider>().Value <= player.CountEnemiesInRange(R.Range-50);
                Obj_AI_Hero targetR = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                if (maoR && targetR != null && ((config.Item("rks").GetValue<bool>() && (Damage.GetSpellDamage(player, targetR, SpellSlot.R) + player.CalcDamage(target, Damage.DamageType.Magical, maoRStack)) > targetR.Health) || manaperc < config.Item("rmana").GetValue<Slider>().Value || (!enoughEnemies && player.Distance(targetR) > R.Range - 50)))
                {
                    R.Cast(config.Item("packets").GetValue<bool>());
                }

                if (targetR != null && !maoR && manaperc > config.Item("rmana").GetValue<Slider>().Value && (enoughEnemies || R.IsInRange(targetR)))
                {
                    R.Cast(config.Item("packets").GetValue<bool>());
                }
            }
           var ignitedmg = (float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
           bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
           if (config.Item("useIgnite").GetValue<bool>() && ignitedmg > target.Health && hasIgnite && !E.CanCast(target))
           {
               player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
           }

        }

        private void turnOffUlt()
        {
            turnOff = false;
            if (maoR && config.Item("user").GetValue<Slider>().Value > player.CountEnemiesInRange(R.Range - 50))
            {
                R.Cast(config.Item("packets").GetValue<bool>());  
            }
            
        }

        private void CastR(Obj_AI_Hero target)
        {
            if (R.IsReady() && !maoR && player.Mana / player.MaxMana * 100 > config.Item("rmana").GetValue<Slider>().Value && config.Item("user").GetValue<Slider>().Value <= target.CountEnemiesInRange(R.Range - 50))
            {
                R.Cast(config.Item("packets").GetValue<bool>());
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawqq", true).GetValue<Circle>(), Q.Range);
            DrawHelper.DrawCircle(config.Item("drawww", true).GetValue<Circle>(), W.Range);
            DrawHelper.DrawCircle(config.Item("drawee", true).GetValue<Circle>(), E.Range);
            DrawHelper.DrawCircle(config.Item("drawrr", true).GetValue<Circle>(), R.Range);
            Helpers.Jungle.ShowSmiteStatus(config.Item("useSmite").GetValue<KeyBind>().Active, config.Item("smiteStatus").GetValue<bool>());
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }
        private static float ComboDamage(Obj_AI_Hero hero)
        {
            float damage = 0;
            if (Q.IsReady())
            {
                damage += (float)Damage.GetSpellDamage(player, hero, SpellSlot.Q);
            }
            if (W.IsReady())
            {
                damage += (float)Damage.GetSpellDamage(player, hero, SpellSlot.W);
            }
            if (E.IsReady())
            {
                damage += (float)Damage.GetSpellDamage(player, hero, SpellSlot.E);
                damage += (float)Damage.GetSpellDamage(player, hero, SpellSlot.E,1);
            }
            if (R.IsReady())
            {
                damage += (float)Damage.GetSpellDamage(player, hero, SpellSlot.R);
                damage += (float) player.CalcDamage(hero, Damage.DamageType.Magical, maoRStack);
            }
            if ((Items.HasItem(ItemHandler.Bft.Id) && Items.CanUseItem(ItemHandler.Bft.Id)) ||
                (Items.HasItem(ItemHandler.Dfg.Id) && Items.CanUseItem(ItemHandler.Dfg.Id)))
            {
                damage = (float)(damage * 1.2);
            }
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health < damage + player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite))
            {
                damage += (float)player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            damage += ItemHandler.GetItemsDamage(hero);
            return damage;
        }
        private void InitMao()
        {
 	        Q = new Spell(SpellSlot.Q, 600);
            Q.SetSkillshot(0.50f, 110f, 1200f, false, SkillshotType.SkillshotLine);
            Qint = new Spell(SpellSlot.Q, 250f);
            W = new Spell(SpellSlot.W, 500);
            E = new Spell(SpellSlot.E, 1100);
            E.SetSkillshot(1f, 250f, 1500f, false, SkillshotType.SkillshotCircle);
            R = new Spell(SpellSlot.R, 450);
        }
        private void InitMenu()
        {
            config = new Menu("Maokai", "Maokai", true);
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
            menuD.AddItem(new MenuItem("drawqq", "Draw Q range", true)).SetValue(new Circle(false, Color.FromArgb(180, 100, 146, 166)));
            menuD.AddItem(new MenuItem("drawww", "Draw W range", true)).SetValue(new Circle(false, Color.FromArgb(180, 100, 146, 166)));
            menuD.AddItem(new MenuItem("drawee", "Draw E range", true)).SetValue(new Circle(false, Color.FromArgb(180, 100, 146, 166)));
            menuD.AddItem(new MenuItem("drawrr", "Draw R range", true)).SetValue(new Circle(false, Color.FromArgb(180, 100, 146, 166)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            menuC.AddItem(new MenuItem("useqroot", "   Wait if the target stunned, slowed...")).SetValue(true);
            menuC.AddItem(new MenuItem("useqrange", "   Q max range")).SetValue(new Slider((int)Q.Range, 0, (int)Q.Range));
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
            menuC.AddItem(new MenuItem("blocke", "   EW Combo if possible")).SetValue(true);
            menuC.AddItem(new MenuItem("user", "Use R min")).SetValue(new Slider(1, 1, 5));
            menuC.AddItem(new MenuItem("rks", "   Deactivate to KS target")).SetValue(true);
            menuC.AddItem(new MenuItem("rmana", "   Deactivate min mana")).SetValue(new Slider(20, 0, 100));
            menuC.AddItem(new MenuItem("selected", "Focus Selected target")).SetValue(true);
            menuC.AddItem(new MenuItem("useIgnite", "Use Ignite")).SetValue(true);
            menuC = ItemHandler.addItemOptons(menuC);
            config.AddSubMenu(menuC);
            // Harass Settings
            Menu menuH = new Menu("Harass ", "Hsettings");
            menuH.AddItem(new MenuItem("useqH", "Use Q")).SetValue(true);
            menuH.AddItem(new MenuItem("useeH", "Use E")).SetValue(true);
            menuH.AddItem(new MenuItem("minmanaH", "Keep X% mana")).SetValue(new Slider(1, 1, 100));
            config.AddSubMenu(menuH);
            // LaneClear Settings
            Menu menuLC = new Menu("LaneClear ", "Lcsettings");
            menuLC.AddItem(new MenuItem("useqLC", "Use Q")).SetValue(true);
            menuLC.AddItem(new MenuItem("qhitLC", "   More than x minion").SetValue(new Slider(2, 1, 10)));
            menuLC.AddItem(new MenuItem("useeLC", "Use E")).SetValue(true);
            menuLC.AddItem(new MenuItem("ehitLC", "   More than x minion").SetValue(new Slider(2, 1, 10)));
            menuLC.AddItem(new MenuItem("minmana", "Keep X% mana")).SetValue(new Slider(1, 1, 100));
            config.AddSubMenu(menuLC);
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM.AddItem(new MenuItem("autoe", "Auto E target (Stun/snare...)")).SetValue(true);
            menuM.AddItem(new MenuItem("useQgc", "Use Q on gapclosers")).SetValue(false);
            menuM.AddItem(new MenuItem("useQint", "Use W to interrupt")).SetValue(true);
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
