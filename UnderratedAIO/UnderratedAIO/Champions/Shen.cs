using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using UnderratedAIO.Helpers;
using Color = System.Drawing.Color;
using Orbwalking = UnderratedAIO.Helpers.Orbwalking;

namespace UnderratedAIO.Champions
{
    class Shen
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        private static readonly Obj_AI_Hero me = ObjectManager.Player;
        public static Spell Q, W, E, EFlash, R;
        public static float currEnergy;
        public static bool haspassive = true;
        public static bool PingCasted = false;
        public static float eEnergy= 120f+5f;
        private const int XOffset = 36;
        private const int YOffset = 9;
        private const int Width = 103;
        private const int Height = 8;
        private static readonly Render.Text Text = new Render.Text(0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");
        public static AutoLeveler autoLeveler;

        public Shen()
        {
            if (me.BaseSkinName != "Shen") return;
            InitShen();
            InitMenu();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Shen</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Obj_AI_Base.OnCreate += OnCreate;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += OnPossibleToInterrupt;
            Obj_AI_Base.OnDamage += Obj_AI_Base_OnDamage;
            Jungle.setSmiteSlot();
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;

        }

        void Obj_AI_Base_OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            var t = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(h => h.NetworkId == args.SourceNetworkId);
            var s = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(h => h.NetworkId == args.TargetNetworkId);
            if (t != null && s != null && (t.IsMe && ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tw => tw.Distance(t) < 750 && tw.Distance(s) < 750) != null))
            {
                if (config.Item("autotauntattower").GetValue<bool>() && E.CanCast(s))
                {
                    E.Cast(s, config.Item("packets").GetValue<bool>());
                }
            }
        }



        private void OnPossibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!config.Item("useeint").GetValue<bool>()) return;
            if (unit.IsValidTarget(E.Range) && E.IsReady()) E.Cast(unit, config.Item("packets").GetValue<bool>());
        }

        private static void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawaa").GetValue<Circle>(), me.AttackRange);
            DrawHelper.DrawCircle(config.Item("drawqq").GetValue<Circle>(), Q.Range);
            DrawHelper.DrawCircle(config.Item("drawee").GetValue<Circle>(), E.Range);
            DrawHelper.DrawCircle(config.Item("draweeflash").GetValue<Circle>(), EFlash.Range);
            Helpers.Jungle.ShowSmiteStatus(config.Item("useSmite").GetValue<KeyBind>().Active, config.Item("smiteStatus").GetValue<bool>());
            if (config.Item("drawallyhp").GetValue<bool>()) DrawHealths();
            if (config.Item("drawincdmg").GetValue<bool>()) getIncDmg();
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();          
        }
        private static void DrawHealths()
        {
            float i = 0;
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe && !hero.IsDead))
            {
                var playername = hero.Name;
                if (playername.Length > 13) playername=playername.Remove(9) + "...";
                var champion = hero.SkinName;
                if (champion.Length > 12) champion = champion.Remove(7) + "...";
                var percent = (int)(hero.Health / hero.MaxHealth * 100);
                var color = Color.Red;
                if (percent > 25) color = Color.Orange;
                if (percent > 50) color = Color.Yellow;
                if (percent > 75) color = Color.Green;
                Drawing.DrawText(Drawing.Width * 0.8f, Drawing.Height * 0.1f + i, color, playername + "(" + champion + ")");
                Drawing.DrawText(Drawing.Width * 0.9f, Drawing.Height * 0.1f + i, color, ((int)hero.Health).ToString() + " (" + percent.ToString() + "%)");
                i += 20f;
            }
        }
        private static void getIncDmg()
        {
            var color = Color.Red;
            float result = CombatHelper.getIncDmg();
            var barPos = me.HPBarPosition;
            var damage = (float)result;
            if (damage == 0) return;
            var percentHealthAfterDamage = Math.Max(0, me.Health - damage) / me.MaxHealth;
            var xPos = barPos.X + XOffset + Width * percentHealthAfterDamage;

            if (damage > me.Health)
            {
                Text.X = (int)barPos.X + XOffset;
                Text.Y = (int)barPos.Y + YOffset - 13;
                Text.text = ((int)(me.Health - damage)).ToString();
                Text.OnEndScene();
            }

            Drawing.DrawLine(xPos, barPos.Y + YOffset, xPos, barPos.Y + YOffset + Height, 3, color);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            GetPassive();
            Ulti();
            currEnergy = me.Mana;
            bool minionBlock = false;
            foreach (var minion in MinionManager.GetMinions(me.Position, me.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
            {
                if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(me, minion, false))
                    minionBlock = true;
            }
			            if (config.Item("useeflash").GetValue<KeyBind>().Active && me.Spellbook.CanUseSpell(me.GetSpellSlot("SummonerFlash")) == SpellState.Ready)
			            {
			                FlashCombo();
			            }
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    LasthitQ();
                    if (!minionBlock) Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LasthitQ();
                    Clear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LasthitQ();
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

        private static void GetPassive()
        {
            var has=false;
            foreach (BuffInstance buff in me.Buffs)
            {
                if (buff.Name == "shenwayoftheninjaaura")
                {
                    has = true;
                }
            }
            if(has){
                  haspassive = true;
            }else{
                  haspassive = false;
            }         
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!config.Item("useeagc").GetValue<bool>()) return;
            if (gapcloser.Sender.IsValidTarget(E.Range) && E.IsReady() && me.Distance(gapcloser.Sender.Position) < 400) E.Cast(gapcloser.End, config.Item("packets").GetValue<bool>());
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
        }
        private static void Ulti()
        {
            if (!R.IsReady() || PingCasted || me.IsDead) return;

            foreach (var allyObj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsAlly && !i.IsMe && !i.IsDead && ((Checkinrange(i) && ((i.Health * 100 / i.MaxHealth) <= config.Item("atpercent").GetValue<Slider>().Value)) || (CombatHelper.CheckCriticalBuffs(i) && i.CountEnemiesInRange(600) < 1))))
                {
                    if (config.Item("user").GetValue<bool>() && orbwalker.ActiveMode!=Orbwalking.OrbwalkingMode.Combo && R.IsReady() && me.CountEnemiesInRange((int)E.Range) < 1 && !config.Item("ult" + allyObj.SkinName).GetValue<bool>())
                    {
                        
                        R.Cast(allyObj);
                        return;
                    }
                    else
                    {
                        DrawHelper.popUp("Use R to help " + allyObj.ChampionName, 3000, Color.Red, Color.White, Color.Red);
                    }
                    PingCasted = true;
                    Utility.DelayAction.Add(5000, () => PingCasted = false);
                }
            
        }
        private static bool Checkinrange(Obj_AI_Hero i)
        {
            if (i.CountEnemiesInRange(750) >= 1 && i.CountEnemiesInRange(750) < 3)
            {
                return true;
            }
            return false;
        }
        private static void AutoQ()
        {
            if (!Q.IsReady() || me.HasBuff("Recall")) return;
            if (config.Item("autoqwithe").GetValue<bool>() || config.Item("autoqwithe").GetValue<bool>() && (currEnergy - me.Spellbook.GetSpell(SpellSlot.Q).ManaCost < eEnergy)) return;
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.Distance(me.Position) < Q.Range)
            {
                if (target.IsValidTarget(Q.Range))

                    Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                    currEnergy -= me.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            }
        }

        private static void LasthitQ()
        {
            if (config.Item("autoqwithe").GetValue<bool>() && !(currEnergy - me.Spellbook.GetSpell(SpellSlot.Q).ManaCost > eEnergy)) return;
            var allMinions = MinionManager.GetMinions(me.ServerPosition, Q.Range);
            if (config.Item("autoqls").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                    HealthPrediction.GetHealthPrediction(minion,
                    (int)(me.Distance(minion.Position) * 1000 / 1400)) <
                    me.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.CastOnUnit(minion);
                        currEnergy -= me.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
                        return;
                    }
                }
            }
        }
        private static void Harass(){
            if (config.Item("harassqwithe").GetValue<bool>() && !(currEnergy - me.Spellbook.GetSpell(SpellSlot.Q).ManaCost > eEnergy)) return;
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Q.IsReady() && config.Item("harassq").GetValue<bool>())
            {
                Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                currEnergy -= me.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            }

        }
        private static void Combo()
        {
            var minHit = config.Item("useemin").GetValue<Slider>().Value;
            Obj_AI_Hero target = TargetSelector.GetTarget(E.Range+400, TargetSelector.DamageType.Magical);
            Obj_AI_Hero targetQ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;
            if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target, config, ComboDamage(target));
            if (config.Item("usee").GetValue<bool>() && E.IsReady() && me.Distance(target.Position)<E.Range)
            {
                if (minHit > 1)
                {
                    CastEmin(target, minHit);
                }
                else if (me.Distance(target.Position) > me.AttackRange && E.GetPrediction(target).Hitchance >= HitChance.Low)
                {
                    E.Cast(target, config.Item("packets").GetValue<bool>());
                    
                }
            }
            if (Q.IsReady() && config.Item("useq").GetValue<bool>() && Q.CanCast(targetQ) && currEnergy - me.Spellbook.GetSpell(SpellSlot.Q).ManaCost >= eEnergy)
            {
                Q.CastOnUnit(targetQ, config.Item("packets").GetValue<bool>());
                currEnergy -= me.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            }
            bool hasIgnite = me.Spellbook.CanUseSpell(me.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            var ignitedmg = (float)me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            if (config.Item("useIgnite").GetValue<bool>() && ignitedmg > target.Health && hasIgnite && !E.CanCast(target) && !Q.CanCast(target))
            {
                me.Spellbook.CastSpell(me.GetSpellSlot("SummonerDot"), target);
            }

        }

        public static void CastEmin(Obj_AI_Base target, int min)
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Base>().Where(i => i.Distance(me)<E.Range && i.IsEnemy && !i.IsDead && !i.IsMinion))
            {
                E.CastIfWillHit(enemy, min-1, config.Item("packets").GetValue<bool>());
                return;

            }
            if (me.Distance(target.Position) > me.AttackRange && E.IsReady())
                {
                    E.CastIfWillHit(target, 0, config.Item("packets").GetValue<bool>());    
                }
        }

        private static void FlashCombo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(EFlash.Range, TargetSelector.DamageType.Magical);
            //System.IO.File.AppendAllText(@"C:\Users\Public\TestFolder\PacketLog.txt", "Target: " + target.Position.ToString() + "\n" + "Me: " + me.Position.ToString() + "\n" + "Best: " + getPosToEflash(target.Position).ToString() + "\n");
            if (config.Item("usee").GetValue<bool>() && E.IsReady() && me.Distance(target.Position) < EFlash.Range && me.Distance(target.Position) > 480 && !((getPosToEflash(target.Position)).IsWall()))
            {
                //Game.PrintChat("ok");
                //Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(target.Position.X, target.Position.Y, me.NetworkId, 0, Packet.PingType.Fallback)).Process();
                me.Spellbook.CastSpell(me.GetSpellSlot("SummonerFlash"), getPosToEflash(target.Position));
                
                E.Cast(target.Position, config.Item("packets").GetValue<bool>());
                
            }
            if (Q.IsReady() && config.Item("useq").GetValue<bool>() && currEnergy - me.Spellbook.GetSpell(SpellSlot.E).ManaCost >= eEnergy)
            {
                Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                currEnergy -= me.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            }
            ItemHandler.UseItems(target, config);

        }
		public static Vector3 getPosToEflash(Vector3 target)
        {
            return target + (me.Position - target)/2;
        }
        private static float ComboDamage(Obj_AI_Hero hero)
        {
            float damage = 0;
            if (Q.IsReady() && me.Spellbook.GetSpell(SpellSlot.Q).ManaCost < me.Mana)
                damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.Q);
            if (E.IsReady() && me.Spellbook.GetSpell(SpellSlot.E).ManaCost < me.Mana)
                damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.E);
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Tiamat);
            }
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Hydra);
            }
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Bilgewater);
            }
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Botrk);
            }
            if (Items.HasItem(3146) && Items.CanUseItem(3146))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Hexgun);
            }
            if (Items.HasItem(3091) && Items.CanUseItem(3091))
            {
                damage += (float)me.CalcDamage(hero, Damage.DamageType.Magical, 42);
            }
            if (haspassive)
            {
                var passive = me.MaxHealth / 10 + 4+me.Level * 4;
                damage += (float)me.CalcDamage(hero, Damage.DamageType.Magical, passive);
            }
            if (me.Spellbook.CanUseSpell(me.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health - damage < (float)me.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite))
            {
                damage += (float)me.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            return damage;
        }
        public static List<Obj_AI_Base> CheckingCollision(Obj_AI_Base From, Obj_AI_Base Target, Spell Skill, bool Mid = true, bool OnlyHero = false)
        {
            var ListCol = new List<Obj_AI_Base>();
            foreach (var Obj in ObjectManager.Get<Obj_AI_Base>().Where(i => IsValid(i, Skill.Range) && Skill.GetPrediction(i).Hitchance >= HitChance.Medium && ((!OnlyHero && !(i is Obj_AI_Turret)) || (OnlyHero && i is Obj_AI_Hero)) && i != Target))
            {
                var Segment = (Mid ? Obj : Target).Position.To2D().ProjectOn(From.Position.To2D(), (Mid ? Target : Obj).Position.To2D());
                if (Segment.IsOnSegment && Obj.Position.Distance(new Vector3(Segment.SegmentPoint.X, Obj.Position.Y, Segment.SegmentPoint.Y)) <= Skill.Width + Obj.BoundingRadius) ListCol.Add(Obj);
            }
            return ListCol.Distinct().ToList();
        }
        
        public static bool IsValid(Obj_AI_Base Target, float Range = float.MaxValue, bool EnemyOnly = true, Vector3 From = default(Vector3))
        {
            if (Target == null || !Target.IsValid || Target.IsDead || !Target.IsVisible || (EnemyOnly && !Target.IsTargetable) || (EnemyOnly && Target.IsInvulnerable) || Target.IsMe) return false;
            if (EnemyOnly ? Target.IsAlly : Target.IsEnemy) return false;
            if ((From != default(Vector3) ? From : me.Position).Distance(Target.Position) > Range) return false;
            return true;
        }
        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (config.Item("autowwithe").GetValue<bool>() && !(currEnergy - me.Spellbook.GetSpell(SpellSlot.W).ManaCost > eEnergy)) return;
            if (sender is Obj_SpellMissile && sender.IsValid && config.Item("autow").GetValue<bool>() && W.IsReady())
            {
                var missle = (Obj_SpellMissile)sender;
                var caster = missle.SpellCaster;
                if (caster.IsEnemy)
                {
                    var ShieldBuff = new Int32[] { 60, 100, 140, 180, 200 }[W.Level - 1] + 0.6 * me.FlatMagicDamageMod;
                    if (missle.SData.Name.Contains("BasicAttack"))
                    {
                        if (missle.Target.IsMe && ShieldBuff / 100 * config.Item("wabove").GetValue<Slider>().Value < caster.GetAutoAttackDamage(me, true))
                        {
                            W.Cast();
                            currEnergy -= me.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                        }
                    }
                    else if (missle.Target.IsMe || missle.EndPosition.Distance(me.Position) <= 130)
                    {
                        if (missle.SData.Name == "summonerdot")
                        {
                            if (me.Health <= (caster as Obj_AI_Hero).GetSummonerSpellDamage(me, Damage.SummonerSpell.Ignite) && ShieldBuff < (caster as Obj_AI_Hero).GetSummonerSpellDamage(me, Damage.SummonerSpell.Ignite))
                            {
                                W.Cast();
                                currEnergy -= me.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                            }
                        }
                        else if (ShieldBuff / 100 * config.Item("wabove").GetValue<Slider>().Value < (caster as Obj_AI_Hero).GetSpellDamage(me, (caster as Obj_AI_Hero).GetSpellSlot(missle.SData.Name), 1))
                        {
                            W.Cast();
                            currEnergy -= me.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                        }
                    }
                }
            }
        }
        private static void InitShen()
        {
            Q = new Spell(SpellSlot.Q, 475);
            Q.SetTargetted(0.5f, 1500f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 600);
            E.SetSkillshot(0f, 50f, 1600f, false, SkillshotType.SkillshotLine);
            EFlash = new Spell(SpellSlot.E, 990);
            EFlash.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Speed, false, SkillshotType.SkillshotLine);
            R = new Spell(SpellSlot.R, float.MaxValue);
        }
        private static void InitMenu()
        {
            config = new Menu("Shen", "SRS_Shen", true);
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
            menuD.AddItem(new MenuItem("drawaa", "Draw AA range")).SetValue(new Circle(false, Color.FromArgb(150, 150, 62, 172)));
            menuD.AddItem(new MenuItem("drawqq", "Draw Q range")).SetValue(new Circle(false, Color.FromArgb(150, 150, 62, 172)));
            menuD.AddItem(new MenuItem("drawee", "Draw E range")).SetValue(new Circle(false, Color.FromArgb(150, 150, 62, 172)));
            menuD.AddItem(new MenuItem("draweeflash", "Draw E+flash range")).SetValue(new Circle(true, Color.FromArgb(50, 250, 248, 110)));
            menuD.AddItem(new MenuItem("drawallyhp", "Draw teammates' HP")).SetValue(true);
            menuD.AddItem(new MenuItem("drawincdmg", "Draw incoming damage")).SetValue(true);
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);

            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
            menuC.AddItem(new MenuItem("useemin", "Try to use E min")).SetValue(new Slider(1, 1, 5));
            menuC.AddItem(new MenuItem("useIgnite", "Use Ignite")).SetValue(true);
            menuC = ItemHandler.addItemOptons(menuC);
            config.AddSubMenu(menuC);

            // Harass Settings
            Menu menuH = new Menu("Harass ", "hsettings");
            menuH.AddItem(new MenuItem("harassq", "Harass with Q")).SetValue(true);
            menuH.AddItem(new MenuItem("harassqwithe", "Keep energy for E")).SetValue(true);
            config.AddSubMenu(menuH);

            // Lasthit Settings
            Menu menuLH = new Menu("Lasthit ", "lhsettings");
            menuLH.AddItem(new MenuItem("autoqls", "Lasthit with Q")).SetValue(true);
            menuLH.AddItem(new MenuItem("autoqwithe", "Keep energy for E")).SetValue(true);
            config.AddSubMenu(menuLH);

            // Misc Settings
            Menu menuU = new Menu("Misc ", "usettings");
            menuU.AddItem(new MenuItem("autow", "Try to block non-skillshot spells")).SetValue(true);
            menuU.AddItem(new MenuItem("wabove", "Min damage in shield %")).SetValue(new Slider(50, 0, 100));
            menuU.AddItem(new MenuItem("autowwithe", "Keep energy for E")).SetValue(true);
            menuU.AddItem(new MenuItem("autotauntattower", "Auto taunt in tower range")).SetValue(true);
            menuU.AddItem(new MenuItem("useeagc", "Use E to anti gap closer")).SetValue(false);
            menuU.AddItem(new MenuItem("useeint", "Use E to interrupt")).SetValue(true);
            menuU.AddItem(new MenuItem("useeflash", "Flash+E")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press));
            menuU.AddItem(new MenuItem("user", "Use R")).SetValue(true);
            menuU.AddItem(new MenuItem("atpercent", "Friend under")).SetValue(new Slider(20, 0, 100));
            menuU = Jungle.addJungleOptions(menuU);
            menuU = ItemHandler.addCleanseOptions(menuU);

            Menu autolvlM = new Menu("AutoLevel", "AutoLevel");
            autoLeveler = new AutoLeveler(autolvlM);
            menuU.AddSubMenu(autolvlM);

            config.AddSubMenu(menuU);
            var sulti = new Menu("Don't ult on ", "dontult");
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                if (hero.SkinName != me.SkinName)
                    sulti.AddItem(new MenuItem("ult" + hero.SkinName, hero.SkinName)).SetValue(false);
            }
            config.AddSubMenu(sulti);
            config.AddItem(new MenuItem("packets", "Use Packets")).SetValue(false);
            config.AddItem(new MenuItem("UnderratedAIO", "by Soresu v" + Program.version.ToString().Replace(",",".")));
            config.AddToMainMenu();
        }
    }
}
