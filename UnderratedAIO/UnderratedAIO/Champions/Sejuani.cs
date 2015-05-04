using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using UnderratedAIO.Helpers;
using Color = System.Drawing.Color;
using Environment = UnderratedAIO.Helpers.Environment;
using Orbwalking = UnderratedAIO.Helpers.Orbwalking;

namespace UnderratedAIO.Champions
{
    class Sejuani
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        private static readonly Obj_AI_Hero me = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static AutoLeveler autoLeveler;

        public Sejuani()
        {
            if (me.BaseSkinName != "Sejuani") return;
            InitSejuani();
            InitMenu();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Sejuani</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += OnPossibleToInterrupt;
            Orbwalking.AfterAttack += AfterAttack;
            Jungle.setSmiteSlot();
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
        }

        private void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {

            if (unit.IsMe && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && W.IsReady() && config.Item("usew").GetValue<bool>())
            {
                W.Cast(config.Item("packets").GetValue<bool>());
               Orbwalking.ResetAutoAttackTimer();
            }
        }

        private void OnPossibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("useqint").GetValue<bool>())
            {
                if (unit.IsValidTarget(Q.Range) && Q.IsReady() && me.Distance(unit) < Q.Range) Q.Cast(unit.Position, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("userint").GetValue<bool>())
            {
                if (unit.IsValidTarget(R.Range) && R.IsReady() && me.Distance(unit) < R.Range) R.Cast(unit.Position, config.Item("packets").GetValue<bool>());
            }
        }

        private static void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawaa").GetValue<Circle>(), me.AttackRange);
            DrawHelper.DrawCircle(config.Item("drawqq").GetValue<Circle>(), Q.Range);
            DrawHelper.DrawCircle(config.Item("drawww").GetValue<Circle>(), W.Range);
            DrawHelper.DrawCircle(config.Item("drawee").GetValue<Circle>(), E.Range);
            DrawHelper.DrawCircle(config.Item("drawrr").GetValue<Circle>(), R.Range);
            Helpers.Jungle.ShowSmiteStatus(config.Item("useSmite").GetValue<KeyBind>().Active, config.Item("smiteStatus").GetValue<bool>());
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            bool minionBlock = false;
            foreach (Obj_AI_Minion minion in MinionManager.GetMinions(me.Position, me.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
            {
                if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(me, minion, false))
                    minionBlock = true;
            }
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    //if (!minionBlock) Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (!minionBlock) Clear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                default:
                    break;
            }

            Jungle.CastSmite(config.Item("useSmite").GetValue<KeyBind>().Active);
            if (config.Item("manualR").GetValue<KeyBind>().Active && R.IsReady()) CastR();
            if (config.Item("QSSEnabled").GetValue<bool>()) ItemHandler.UseCleanse(config);
            
        }

        private static void CastR()
        {
            me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy && !i.IsDead && me.Distance(i) < R.Range).OrderByDescending(l =>l.CountEnemiesInRange(350f)))
            {
                R.Cast(enemy, config.Item("packets").GetValue<bool>());
                break;
            }
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("useqgc").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(Q.Range) && Q.IsReady() && me.Distance(gapcloser.End) < Q.Range) Q.Cast(gapcloser.End, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("usergc").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(R.Range) && R.IsReady() && me.Distance(gapcloser.End) < R.Range) R.Cast(gapcloser.End, config.Item("packets").GetValue<bool>());
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
            if (me.Mana < me.MaxMana * perc) return;

            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);
            var minionsSpells = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(W.Range)).ToList();
            if (W.IsReady() && minionsSpells.Count() > 1 && config.Item("usewC").GetValue<bool>() && me.Spellbook.GetSpell(SpellSlot.W).ManaCost <= me.Mana) W.Cast();
            var minHit = config.Item("useeCmin").GetValue<Slider>().Value;
            if (E.IsReady() && me.Spellbook.GetSpell(SpellSlot.Q).ManaCost <= me.Mana && CombatHelper.SejuaniCountFrostMinion(E.Range) >= minHit && (!(!Q.IsReady() && me.Mana - me.Spellbook.GetSpell(SpellSlot.Q).ManaCost < me.MaxMana * perc) || !(!W.IsReady() && me.Mana - me.Spellbook.GetSpell(SpellSlot.W).ManaCost < me.MaxMana * perc)))
            {
                E.Cast();
            }
            if (config.Item("useqC").GetValue<bool>() && Q.IsReady() && me.Spellbook.GetSpell(SpellSlot.Q).ManaCost <= me.Mana)
            {
                var minionsForQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                MinionManager.FarmLocation bestPosition = Q.GetLineFarmLocation(minionsForQ);
                if (bestPosition.Position.IsValid())
                    if (bestPosition.MinionsHit >= 2)
                        Q.Cast(bestPosition.Position, config.Item("packets").GetValue<bool>());
                //Q.Cast(enemy.Position, config.Item("packets").GetValue<bool>());
            }

            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, true, SkillshotType.SkillshotLine);
        }

        private static void Ulti()
        {

            if (!R.IsReady() || config.Item("useRmin").GetValue<Slider>().Value == 0) return;
            if (config.Item("useRmin").GetValue<Slider>().Value == 1)
            {
                var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                if (!config.Item("ult" + target.SkinName).GetValue<bool>()) R.Cast(target, config.Item("packets").GetValue<bool>());
                }
            else {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy && !i.IsDead && me.Distance(i) < R.Range && me.Distance(i) > config.Item("useRminr").GetValue<Slider>().Value && !config.Item("ult" + i.SkinName).GetValue<bool>() && Environment.Hero.countChampsAtrange(i.Position, 350f) >= config.Item("useRmin").GetValue<Slider>().Value).OrderByDescending(l => Environment.Hero.countChampsAtrange(l.Position, 350f)))
                {
                    R.Cast(enemy, config.Item("packets").GetValue<bool>());
                    return;
                }
            }
        }

        private static void Combo()
        {
            
            Ulti();
            float perc = (float)config.Item("minmana").GetValue<Slider>().Value / 100f;
            var minHit = config.Item("useemin").GetValue<Slider>().Value;
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target, config, ComboDamage(target));

            var buffs = CombatHelper.SejuaniCountFrostHero(E.Range);
            if (E.IsReady() && me.Distance(target.Position) < E.Range && buffs > 0 && (
                (buffs > minHit)
                || (Damage.GetSpellDamage(me, target, SpellSlot.E) >= target.Health)
                || (me.Distance(target) > config.Item("useEminr").GetValue<Slider>().Value && me.Distance(target) < E.Range && buffs == 1)))
            {
                if (!(Q.IsReady() && me.Mana - me.Spellbook.GetSpell(SpellSlot.Q).ManaCost < me.MaxMana * perc) || !(W.IsReady() && me.Mana - me.Spellbook.GetSpell(SpellSlot.W).ManaCost < me.MaxMana * perc)) E.Cast();
            }
            if (Q.IsReady() && config.Item("useq").GetValue<bool>())
            {
                Q.Cast(target, config.Item("packets").GetValue<bool>());
            }
            bool hasIgnite = me.Spellbook.CanUseSpell(me.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            var ignitedmg = (float)me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            if (ignitedmg > target.Health && hasIgnite && !E.CanCast(target) && !W.CanCast(target) && !Q.CanCast(target))
            {
                me.Spellbook.CastSpell(me.GetSpellSlot("SummonerDot"), target);
            }
        }

        private static float ComboDamage(Obj_AI_Hero hero)
        {
            float damage = 0;
            if (Q.IsReady())
            {
                damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.Q);
            }
            if (E.IsReady())
            {
                damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.E);
            }
            if (W.IsReady()) {
                double wdot = new double[] { 40, 70, 100, 130, 160 }[W.Level] + (new double[] { 4, 6, 8, 10, 12 }[W.Level] / 100) * me.MaxHealth;
                damage += (float)me.CalcDamage(hero, Damage.DamageType.Magical, wdot);
                damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.W);
            }
            if (R.IsReady())
            {
                damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.R);
            }

            if ((Items.HasItem(ItemHandler.Bft.Id) && Items.CanUseItem(ItemHandler.Bft.Id)) ||
                (Items.HasItem(ItemHandler.Dfg.Id) && Items.CanUseItem(ItemHandler.Dfg.Id)))
            {
                damage = (float)(damage * 1.2);
            }
            if (me.Spellbook.CanUseSpell(me.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health < damage + me.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite))
            {
                damage += (float)me.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            damage += ItemHandler.GetItemsDamage(hero);
            return damage;
        }

        private static void InitSejuani()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 1175);
            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(R.Instance.SData.SpellCastTime, R.Instance.SData.LineWidth, R.Instance.SData.MissileSpeed, true, SkillshotType.SkillshotLine);
        }

        private static void InitMenu()
        {
            config = new Menu("Sejuani", "Sejuani", true);
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
            menuD.AddItem(new MenuItem("drawaa", "Draw AA range")).SetValue(new Circle(true, Color.FromArgb(150, 150, 177, 208)));
            menuD.AddItem(new MenuItem("drawqq", "Draw Q range")).SetValue(new Circle(true, Color.FromArgb(150, 150, 177, 208)));
            menuD.AddItem(new MenuItem("drawww", "Draw W range")).SetValue(new Circle(true, Color.FromArgb(150, 150, 177, 208)));
            menuD.AddItem(new MenuItem("drawee", "Draw E range")).SetValue(new Circle(true, Color.FromArgb(150, 150, 177, 208)));
            menuD.AddItem(new MenuItem("drawrr", "Draw R range")).SetValue(new Circle(true, Color.FromArgb(150, 150, 177, 208)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);

            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            menuC.AddItem(new MenuItem("useemin", "Use E min")).SetValue(new Slider(1, 1, 5));
            menuC.AddItem(new MenuItem("useEminr", "E minimum range")).SetValue(new Slider(250, 0, 900));
            menuC.AddItem(new MenuItem("useRmin", "R minimum target")).SetValue(new Slider(1, 0, 5));
            menuC.AddItem(new MenuItem("useRminr", "Ulti minimum range")).SetValue(new Slider(0, 0, 350));
            menuC.AddItem(new MenuItem("manualR", "Cast R asap")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press));
            menuC.AddItem(new MenuItem("useIgnite", "Use Ignite")).SetValue(true);
            menuC = ItemHandler.addItemOptons(menuC);
            
            config.AddSubMenu(menuC);
            // Clear/Jungle
            Menu menuJ = new Menu("Clear ", "jsettings");
            menuJ.AddItem(new MenuItem("useqC", "Use Q")).SetValue(true);
            menuJ.AddItem(new MenuItem("usewC", "Use W")).SetValue(true);
            menuJ.AddItem(new MenuItem("useeCmin", "Use E min")).SetValue(new Slider(1, 1, 5));
            menuJ.AddItem(new MenuItem("useiC", "Use Items")).SetValue(true);
            menuJ.AddItem(new MenuItem("minmana", "Keep X% mana")).SetValue(new Slider(1, 1, 100));
            config.AddSubMenu(menuJ);
            // Misc Settings
            Menu menuU = new Menu("Misc ", "usettings");
            menuU.AddItem(new MenuItem("useqgc", "Use Q to anti gap closer")).SetValue(false);
            menuU.AddItem(new MenuItem("useqint", "Use Q to interrupt")).SetValue(true);
            menuU.AddItem(new MenuItem("usergc", "Use R to anti gap closer")).SetValue(false);
            menuU.AddItem(new MenuItem("userint", "Use R to interrupt")).SetValue(false);
            menuU = Jungle.addJungleOptions(menuU);
            menuU = ItemHandler.addCleanseOptions(menuU);

            Menu autolvlM = new Menu("AutoLevel", "AutoLevel");
            autoLeveler = new AutoLeveler(autolvlM);
            menuU.AddSubMenu(autolvlM);

            config.AddSubMenu(menuU);
            var sulti = new Menu("Don't ult on ", "dontult");
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
