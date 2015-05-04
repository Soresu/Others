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
    class Chogath
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R, RFlash;
        public static List<int> silence = new List<int>(new int[] { 1500, 1750, 2000, 2250, 2500});
        public static int knockUp = 1000;
        public static bool flashRblock = false;
        public static AutoLeveler autoLeveler;

        public Chogath()
        {
            if (player.BaseSkinName != "Chogath") return;
            InitChoGath();
            InitMenu();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Cho'Gath</font>");
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
                if (Q.CanCast(sender)) Q.Cast(sender, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useWint").GetValue<bool>())
            {
                if (W.CanCast(sender)) W.Cast(sender, config.Item("packets").GetValue<bool>());
            }
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            /*
            vSpikes = VorpalSpikes;
            if (Environment.Turret.countTurretsInRange(player) > 0 && vSpikes && E.GetHitCount() > 0)
            {
                E.Cast();
            }*/
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
            if (config.Item("useRJ").GetValue<bool>() || config.Item("useSmite").GetValue<KeyBind>().Active)
            {
                Jungle();
            }
            if (config.Item("QSSEnabled").GetValue<bool>()) ItemHandler.UseCleanse(config);
        }
        private static bool VorpalSpikes
        {
            get
            { return player.Buffs.Any(buff => buff.Name == "VorpalSpikes"); }
        }
        private static void Jungle()
        {
            var target = Helpers.Jungle.GetNearest(player.Position);
            bool smiteReady = Helpers.Jungle.SmiteReady(config.Item("useSmite").GetValue<KeyBind>().Active);
            if (target != null)
            {
                if (target.CountEnemiesInRange(760f) > 0)
                {
                    bool hasFlash = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerFlash")) == SpellState.Ready;
                    if (config.Item("useRJ").GetValue<bool>() && config.Item("useFlashJ").GetValue<bool>() && R.IsReady() && hasFlash && 1000+player.FlatMagicDamageMod*0.7f >= target.Health &&  player.GetSpell(SpellSlot.R).ManaCost <= player.Mana &&
                        player.Distance(target.Position) > 400 && player.Distance(target.Position) <= RFlash.Range &&
                        !player.Position.Extend(target.Position, 400).IsWall())
                    {
                        player.Spellbook.CastSpell(player.GetSpellSlot("SummonerFlash"), player.Position.Extend(target.Position, 400));
                        //Utility.DelayAction.Add(50, () => R.Cast(target, config.Item("packets").GetValue<bool>()));
                    }
                }
                if (config.Item("useRJ").GetValue<bool>() && R.CanCast(target) && !(config.Item("priorizeSmite").GetValue<bool>() && smiteReady) && player.GetSpell(SpellSlot.R).ManaCost <= player.Mana && 1000f + player.FlatMagicDamageMod*0.7f >= target.Health)
                {
                    R.Cast(target, config.Item("packets").GetValue<bool>());
                }
                if (Helpers.Jungle.smiteSlot == SpellSlot.Unknown)
                {
                return;    
                }
                if (R.CanCast(target) && config.Item("useSmite").GetValue<KeyBind>().Active && target.CountEnemiesInRange(750f)>0 &&  config.Item("useRSJ").GetValue<bool>() && smiteReady && 1000f + player.FlatMagicDamageMod * 0.7f + Helpers.Jungle.smiteDamage(target) >= target.Health)
                {
                    R.Cast(target, config.Item("packets").GetValue<bool>());
                }
                if (config.Item("useSmite").GetValue<KeyBind>().Active && Helpers.Jungle.smite.CanCast(target) && smiteReady && Helpers.Jungle.smiteSlot != SpellSlot.Unknown && player.Distance(target) <= Helpers.Jungle.smite.Range && Helpers.Jungle.smiteDamage(target) >= target.Health)
                {
                    Helpers.Jungle.setSmiteSlot();
                    Helpers.Jungle.CastSmite(target);
                }
            }
        }

        private static void Clear()
        {
            
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(400)).ToList();
            if (minions.Count() > 2)
            {
                if (Items.HasItem(3077) && Items.CanUseItem(3077))
                    Items.UseItem(3077);
                if (Items.HasItem(3074) && Items.CanUseItem(3074))
                    Items.UseItem(3074);
            }
            
            float perc = (float)config.Item("minmana").GetValue<Slider>().Value / 100f;
            if (player.Mana < player.MaxMana * perc) return;
            if (config.Item("usewLC").GetValue<bool>() && W.IsReady() && player.Spellbook.GetSpell(SpellSlot.W).ManaCost <= player.Mana)
            {
                var minionsForW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.NotAlly);
                MinionManager.FarmLocation bestPositionW = W.GetLineFarmLocation(minionsForW);
                if (bestPositionW.Position.IsValid())
                    if (bestPositionW.MinionsHit > config.Item("whitLC").GetValue<Slider>().Value)
                        W.Cast(bestPositionW.Position, config.Item("packets").GetValue<bool>());
            }

            if (config.Item("useqLC").GetValue<bool>() && Q.IsReady() && player.Spellbook.GetSpell(SpellSlot.Q).ManaCost <= player.Mana)
            {
                var minionsForQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                MinionManager.FarmLocation bestPositionQ = Q.GetCircularFarmLocation(minionsForQ);
                if (Q.IsReady() && bestPositionQ.MinionsHit > config.Item("qhitLC").GetValue<Slider>().Value)
                {
                    Q.Cast(bestPositionQ.Position, config.Item("packets").GetValue<bool>());
                }
            }
        }

        private static void Harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (config.Item("useqH").GetValue<bool>())
            {
                if (target.IsValidTarget(Q.Range) && Q.IsReady()) Q.Cast(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useeH").GetValue<bool>())
            {
                if (target.IsValidTarget(W.Range) && W.IsReady()) W.Cast(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useeH").GetValue<bool>() && !VorpalSpikes && E.GetHitCount() > 0)
            {
                E.Cast();
            }
        }

        private static void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);
            if (config.Item("usee").GetValue<bool>() && !VorpalSpikes && E.GetHitCount() > 0 && (Environment.Turret.countTurretsInRange(player) < 1 || target.Health < 150))
            {
                E.Cast();
            }
            if (target == null) return;
            if (config.Item("selected").GetValue<bool>())
            {
                target = CombatHelper.SetTarget(target, TargetSelector.GetSelectedTarget());
                orbwalker.ForceTarget(target);
            }
            var combodmg = ComboDamage(target);
            if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target, config, combodmg);
            bool hasFlash = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerFlash")) == SpellState.Ready;
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            var ignitedmg=(float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            if (hasIgnite && ignitedmg > target.Health && !R.CanCast(target) && !W.CanCast(target) && !Q.CanCast(target))
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }
            if (hasIgnite && combodmg > target.Health &&  R.CanCast(target) && (float)Damage.GetSpellDamage(player, target, SpellSlot.R) < target.Health)
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }
            if (hasIgnite)
            {
                flashRblock = true;
            }
            else
            {
                flashRblock = false;
            }
            if (config.Item("useq").GetValue<bool>() && Q.IsReady())
            {
                if (config.Item("useqfaster").GetValue<bool>())
                {
                    
                    if (target.IsValidTarget(Q.Range) && Q.CanCast(target))
                    {
                        var nextpos = target.Position.Extend(target.ServerPosition, target.MoveSpeed * 0.7f);
                        if (target.HasBuff("OdinCaptureChanner"))
                        {
                            nextpos = target.Position;
                        }
                        Q.Cast(nextpos, config.Item("packets").GetValue<bool>());
                    }   
                }
                else
                {
                    int qHit = config.Item("qHit", true).GetValue<Slider>().Value;
                    var hitC = HitChance.High;
                    switch (qHit)
                    {
                        case 1:
                            hitC = HitChance.Low;
                            break;
                        case 2:
                            hitC = HitChance.Medium;
                            break;
                        case 3:
                            hitC = HitChance.High;
                            break;
                        case 4:
                            hitC = HitChance.VeryHigh;
                            break;
                    }
                    Q.CastIfHitchanceEquals(target, hitC, config.Item("packets").GetValue<bool>());
                }
            }
            if (config.Item("usew").GetValue<bool>() && W.CanCast(target))
            {
                    W.Cast(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("UseFlashC").GetValue<bool>() && !flashRblock && R.IsReady() && hasFlash && !CombatHelper.CheckCriticalBuffs(target) && player.GetSpell(SpellSlot.R).ManaCost <= player.Mana && player.Distance(target.Position) >= 400 && player.GetSpellDamage(target, SpellSlot.R) > target.Health && !Q.IsReady() && !W.IsReady() && player.Distance(target.Position) <= RFlash.Range && !player.Position.Extend(target.Position, 400).IsWall())
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerFlash"), player.Position.Extend(target.Position, 400));
                Utility.DelayAction.Add(50, () => R.Cast(target, config.Item("packets").GetValue<bool>()));
            }
            if (config.Item("user").GetValue<bool>() && R.CanCast(target) && player.GetSpellDamage(target, SpellSlot.R) > target.Health)
            {
                R.Cast(target, config.Item("packets").GetValue<bool>());
            }
            
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            
            if (config.Item("useQgc").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(Q.Range) && Q.IsReady()) Q.Cast(gapcloser.End, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useWgc").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(W.Range) && W.IsReady()) W.Cast(gapcloser.End, config.Item("packets").GetValue<bool>());
            }
        }

        private static void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawaa", true).GetValue<Circle>(), player.AttackRange);
            DrawHelper.DrawCircle(config.Item("drawqq", true).GetValue<Circle>(), Q.Range);
            DrawHelper.DrawCircle(config.Item("drawww", true).GetValue<Circle>(), W.Range);
            DrawHelper.DrawCircle(config.Item("drawee", true).GetValue<Circle>(), E.Range);
            DrawHelper.DrawCircle(config.Item("drawrrflash").GetValue<Circle>(), RFlash.Range);
            Helpers.Jungle.ShowSmiteStatus(config.Item("useSmite").GetValue<KeyBind>().Active, config.Item("smiteStatus").GetValue<bool>());
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }
        public static float ComboDamage(Obj_AI_Hero hero)
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
            if ((Items.HasItem(ItemHandler.Bft.Id) && Items.CanUseItem(ItemHandler.Bft.Id)) ||
                (Items.HasItem(ItemHandler.Dfg.Id) && Items.CanUseItem(ItemHandler.Dfg.Id)))
            {
                damage = (float) (damage * 1.2);
            }
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready)
            {
                damage += player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            if (R.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.R);
            }
            damage += ItemHandler.GetItemsDamage(hero);
            return (float)damage;
        }
        private static void InitChoGath()
        {
            Q = new Spell(SpellSlot.Q, 950);
            Q.SetSkillshot(500f, 175f, 750f, false, SkillshotType.SkillshotCircle);
            W = new Spell(SpellSlot.W, 700);
            W.SetSkillshot(W.Instance.SData.SpellCastTime, W.Instance.SData.LineWidth, W.Speed, false, SkillshotType.SkillshotCone);
            E = new Spell(SpellSlot.E, 500);
            E.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Speed, false, SkillshotType.SkillshotLine);
            R = new Spell(SpellSlot.R, 175);
            RFlash = new Spell(SpellSlot.R, 555);
        }

        private static void InitMenu()
        {
            config = new Menu("Cho'Gath", "ChoGath", true);
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
            menuD.AddItem(new MenuItem("drawaa", "Draw AA range", true)).SetValue(new Circle(false, Color.FromArgb(180, 200, 46, 66)));
            menuD.AddItem(new MenuItem("drawqq", "Draw Q range", true)).SetValue(new Circle(false, Color.FromArgb(180, 200, 46, 66)));
            menuD.AddItem(new MenuItem("drawww", "Draw W range", true)).SetValue(new Circle(false, Color.FromArgb(180, 200, 46, 66)));
            menuD.AddItem(new MenuItem("drawee", "Draw E range", true)).SetValue(new Circle(false, Color.FromArgb(180, 200, 46, 66)));
            menuD.AddItem(new MenuItem("drawrrflash", "Draw R+flash range")).SetValue(new Circle(true, Color.FromArgb(150, 250, 248, 110)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            menuC.AddItem(new MenuItem("qHit", "Q hitChance", true).SetValue(new Slider(3, 1, 4)));
            menuC.AddItem(new MenuItem("useqfaster", "Use faster Q prediction")).SetValue(false);
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
            menuC.AddItem(new MenuItem("user", "Use R")).SetValue(true);
            menuC.AddItem(new MenuItem("UseFlashC", "Use flash")).SetValue(false);
            menuC.AddItem(new MenuItem("selected", "Focus Selected target")).SetValue(true);
            menuC.AddItem(new MenuItem("useIgnite", "Use Ignite")).SetValue(true);
            menuC = ItemHandler.addItemOptons(menuC);
            config.AddSubMenu(menuC);
            // Harass Settings
            Menu menuH = new Menu("Harass ", "Hsettings");
            menuH.AddItem(new MenuItem("useqH", "Use Q")).SetValue(true);
            menuH.AddItem(new MenuItem("usewH", "Use W")).SetValue(true);
            menuH.AddItem(new MenuItem("useeH", "Use E")).SetValue(true);
            config.AddSubMenu(menuH);
            // LaneClear Settings
            Menu menuLC = new Menu("LaneClear ", "Lcsettings");
            menuLC.AddItem(new MenuItem("useqLC", "Use Q")).SetValue(true);
            menuLC.AddItem(new MenuItem("qhitLC", "More than x minion").SetValue(new Slider(2, 1, 10)));
            menuLC.AddItem(new MenuItem("usewLC", "Use W")).SetValue(true);
            menuLC.AddItem(new MenuItem("whitLC", "More than x minion").SetValue(new Slider(2, 1, 10)));
            menuLC.AddItem(new MenuItem("minmana", "Keep X% mana")).SetValue(new Slider(1, 1, 100));
            config.AddSubMenu(menuLC);
            // Misc Settings
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM.AddItem(new MenuItem("useQint", "Use Q to interrupt")).SetValue(true);
            menuM.AddItem(new MenuItem("useQgc", "Use Q on gapclosers")).SetValue(false);
            menuM.AddItem(new MenuItem("useWint", "Use W to interrupt")).SetValue(true);
            menuM.AddItem(new MenuItem("useWgc", "Use W on gapclosers")).SetValue(false);
            menuM = Helpers.Jungle.addJungleOptions(menuM);
            menuM = ItemHandler.addCleanseOptions(menuM);
            menuM.AddItem(new MenuItem("useRJ", "Use R in jungle")).SetValue(false);
            menuM.AddItem(new MenuItem("useRSJ", "Use R+Smite")).SetValue(false);
            menuM.AddItem(new MenuItem("priorizeSmite", "Use smite if possible")).SetValue(false);
            menuM.AddItem(new MenuItem("useFlashJ", "Use Flash+R to steal buffs")).SetValue(true);

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
