using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;

namespace UnderratedAIO.Helpers
{
    public class ItemHandler
    {
        public static Obj_AI_Hero player = ObjectManager.Player;
        public static Items.Item botrk = new Items.Item(3153, 450);
        public static Items.Item tiamat = new Items.Item(3077, 400);
        public static Items.Item hydra = new Items.Item(3074, 400);
        public static Items.Item randuins = new Items.Item(3143, 500);
        public static Items.Item odins = new Items.Item(3180, 520);
        public static Items.Item bilgewater = new Items.Item(3144, 450);
        public static Items.Item hexgun = new Items.Item(3146, 700);
        public static Items.Item Dfg = new Items.Item(3128, 750);
        public static Items.Item Bft = new Items.Item(3188, 750);
        public static Items.Item Ludens = new Items.Item(3188, 750);
        public static Items.Item sheen = new Items.Item(3057, player.AttackRange);
        public static Items.Item gaunlet = new Items.Item(3025, player.AttackRange);
        public static Items.Item trinity = new Items.Item(3078, player.AttackRange);
        public static Items.Item lich = new Items.Item(3100, player.AttackRange);
        public static Items.Item youmuu = new Items.Item(3142, player.AttackRange);

        public static Items.Item frost = new Items.Item(3092, 850);
        public static Items.Item mountain = new Items.Item(3401, 700);
        public static Items.Item solari = new Items.Item(3190, 600);

        public static Items.Item Qss = new Items.Item(3140, 0);
        public static Items.Item Mercurial = new Items.Item(3139, 0);
        public static Items.Item Dervish = new Items.Item(3137, 0);
        public static Items.Item Zhonya = new Items.Item(3157, 0);
        public static Items.Item Wooglet = new Items.Item(3090, 0);

        public static bool QssUsed = false;

        public static void UseItems(Obj_AI_Hero target, Menu config, float comboDmg = 0f)
        {
            if (config.Item("hyd").GetValue<bool>() && player.BaseSkinName != "Renekton")
            {
                castHydra(target);
            }
            if (config.Item("ran").GetValue<bool>() && Items.HasItem(randuins.Id) && Items.CanUseItem(randuins.Id))
            {
                if (target != null && player.Distance(target) < randuins.Range &&
                    player.CountEnemiesInRange(randuins.Range) >= config.Item("ranmin").GetValue<Slider>().Value)
                {
                    Items.UseItem(randuins.Id);
                }
            }
            if (config.Item("odin").GetValue<bool>() && target != null && Items.HasItem(odins.Id) &&
                Items.CanUseItem(odins.Id))
            {
                if (config.Item("odinonlyks").GetValue<bool>())
                {
                    if (Damage.GetItemDamage(player, target, Damage.DamageItems.OdingVeils) > target.Health)
                    {
                        odins.Cast(target);
                    }
                }
                else if (player.CountEnemiesInRange(odins.Range) >= config.Item("odinmin").GetValue<Slider>().Value ||
                         comboDmg > target.Health && player.Distance(target) < odins.Range)
                {
                    odins.Cast();
                }
            }
            if (target != null && config.Item("bil").GetValue<bool>() && Items.HasItem(bilgewater.Id) &&
                Items.CanUseItem(bilgewater.Id))
            {
                if (config.Item("bilonlyks").GetValue<bool>())
                {
                    if (Damage.GetItemDamage(player, target, Damage.DamageItems.Bilgewater) > target.Health)
                    {
                        bilgewater.Cast(target);
                    }
                }
                else if ((player.Distance(target) > config.Item("bilminr").GetValue<Slider>().Value &&
                          IsHeRunAway(target) && player.Distance(target) > Orbwalking.GetRealAutoAttackRange(player) + 50 &&
                          (target.Health / target.MaxHealth * 100f) < 40) ||
                         (comboDmg > target.Health && (player.Health / player.MaxHealth * 100f) < 50))
                {
                    bilgewater.Cast(target);
                }
            }
            if (target != null && config.Item("botr").GetValue<bool>() && Items.HasItem(botrk.Id) &&
                Items.CanUseItem(botrk.Id))
            {
                if (config.Item("botronlyks").GetValue<bool>())
                {
                    if (Damage.GetItemDamage(player, target, Damage.DamageItems.Botrk) > target.Health)
                    {
                        botrk.Cast(target);
                    }
                }
                else if ((player.Distance(target) > config.Item("botrminr").GetValue<Slider>().Value &&
                          (player.Health / player.MaxHealth * 100f) <
                          config.Item("botrmyhealth").GetValue<Slider>().Value &&
                          (target.Health / target.MaxHealth * 100f) <
                          config.Item("botrenemyhealth").GetValue<Slider>().Value) ||
                         (IsHeRunAway(target) && player.Distance(target) > Orbwalking.GetRealAutoAttackRange(player) + 50 &&
                          (target.Health / target.MaxHealth * 100f) < 40) ||
                         (comboDmg > target.Health && (player.Health / player.MaxHealth * 100f) < 50))
                {
                    botrk.Cast(target);
                }
            }
            if (config.Item("hex").GetValue<bool>() && Items.HasItem(hexgun.Id) && Items.CanUseItem(hexgun.Id))
            {
                if (config.Item("hexonlyks").GetValue<bool>())
                {
                    if (target != null &&
                        Damage.GetItemDamage(player, target, Damage.DamageItems.Hexgun) > target.Health)
                    {
                        hexgun.Cast(target);
                    }
                }
                else if ((player.Distance(target) > config.Item("hexminr").GetValue<Slider>().Value &&
                          IsHeRunAway(target) && player.Distance(target) > Orbwalking.GetRealAutoAttackRange(player) + 50 &&
                          (target.Health / target.MaxHealth * 100f) < 40) ||
                         (comboDmg > target.Health && (player.Health / player.MaxHealth * 100f) < 50))
                {
                    hexgun.Cast(target);
                }
            }
            if (Items.HasItem(Dfg.Id) && Items.CanUseItem(Dfg.Id))
            {
                Dfg.Cast(target);
            }
            if (Items.HasItem(Bft.Id) && Items.CanUseItem(Bft.Id))
            {
                Bft.Cast(target);
            }
            if (config.Item("you").GetValue<bool>() && Items.HasItem(youmuu.Id) && Items.CanUseItem(youmuu.Id) &&
                target != null && player.Distance(target) < player.AttackRange + 50)
            {
                youmuu.Cast();
            }

            if (Items.HasItem(frost.Id) && Items.CanUseItem(frost.Id) && target != null &&
                config.Item("frost").GetValue<bool>())
            {
                if (player.Distance(target) < frost.Range &&
                    (config.Item("frostmin").GetValue<Slider>().Value <= target.CountEnemiesInRange(225f) &&
                     ((target.Health / target.MaxHealth * 100f) < 40 && config.Item("frostlow").GetValue<bool>() ||
                      !config.Item("frostlow").GetValue<bool>())))
                {
                    frost.Cast(target);
                }
            }
            if (Items.HasItem(solari.Id) && Items.CanUseItem(solari.Id) && config.Item("solari").GetValue<bool>())
            {
                if ((config.Item("solariminally").GetValue<Slider>().Value <= player.CountAlliesInRange(solari.Range) &&
                     config.Item("solariminenemy").GetValue<Slider>().Value <= player.CountEnemiesInRange(solari.Range)) ||
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            h => h.IsAlly && !h.IsDead && solari.IsInRange(h) && CombatHelper.CheckCriticalBuffs(h)) !=
                    null)
                {
                    solari.Cast();
                }
            }
            if (Items.HasItem(mountain.Id) && Items.CanUseItem(mountain.Id) && config.Item("mountain").GetValue<bool>())
            {
                if (config.Item("castonme").GetValue<bool>() &&
                    (player.Health / player.MaxHealth * 100f) < config.Item("mountainmin").GetValue<Slider>().Value &&
                    (player.CountEnemiesInRange(700f) > 0 || CombatHelper.CheckCriticalBuffs(player)))
                {
                    mountain.Cast(player);
                    return;
                }
                var targ =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            h =>
                                h.IsAlly && !h.IsMe && !h.IsDead && player.Distance(h) < mountain.Range && config.Item("mountainpriority" + h.ChampionName).GetValue<Slider>().Value>0 &&
                                (h.Health / h.MaxHealth * 100f) < config.Item("mountainmin").GetValue<Slider>().Value);
                if (targ != null)
                {
                    var finaltarg =
                        targ.OrderByDescending(
                            t => config.Item("mountainpriority" + t.ChampionName).GetValue<Slider>().Value)
                            .ThenBy(t => t.Health)
                            .FirstOrDefault();
                    if (finaltarg != null &&
                        (finaltarg.CountEnemiesInRange(700f) > 0 || finaltarg.UnderTurret(true) ||
                         CombatHelper.CheckCriticalBuffs(finaltarg)))
                    {
                        mountain.Cast(finaltarg);
                    }
                }
            }
        }

        public static bool IsHeRunAway(Obj_AI_Hero target)
        {
            return (!target.IsFacing(player) &&
                    Prediction.GetPrediction(target, 600, 100f).CastPosition.Distance(player.Position) >
                    target.Position.Distance(player.Position));
        }

        public static void castHydra(Obj_AI_Hero target)
        {
            if (target != null && player.Distance(target) < hydra.Range && !LeagueSharp.Common.Orbwalking.CanAttack())
            {
                if (Items.HasItem(tiamat.Id) && Items.CanUseItem(tiamat.Id))
                {
                    Items.UseItem(tiamat.Id);
                }
                if (Items.HasItem(hydra.Id) && Items.CanUseItem(hydra.Id))
                {
                    Items.UseItem(hydra.Id);
                }
            }
        }

        public static Menu addItemOptons(Menu config)
        {
            var mConfig = config;
            Menu menuI = new Menu("Items ", "Itemsettings");
            menuI.AddItem(new MenuItem("hyd", "Hydra/Tiamat")).SetValue(true);
            Menu menuRan = new Menu("Randuin's Omen", "Rands ");
            menuRan.AddItem(new MenuItem("ran", "Enabled")).SetValue(true);
            menuRan.AddItem(new MenuItem("ranmin", "Min enemy")).SetValue(new Slider(2, 1, 6));
            menuI.AddSubMenu(menuRan);

            Menu menuOdin = new Menu("Odyn's Veil ", "Odyns");
            menuOdin.AddItem(new MenuItem("odin", "Enabled")).SetValue(true);
            menuOdin.AddItem(new MenuItem("odinonlyks", "KS only")).SetValue(false);
            menuOdin.AddItem(new MenuItem("odinmin", "Min enemy")).SetValue(new Slider(2, 1, 6));
            menuI.AddSubMenu(menuOdin);

            Menu menuBilgewater = new Menu("Bilgewater Cutlass ", "Bilgewaters");
            menuBilgewater.AddItem(new MenuItem("bil", "Enabled")).SetValue(true);
            menuBilgewater.AddItem(new MenuItem("bilonlyks", "KS only")).SetValue(false);
            menuBilgewater.AddItem(new MenuItem("bilminr", "Min range"))
                .SetValue(
                    new Slider(
                        (int)
                            (Orbwalking.GetRealAutoAttackRange(player) < bilgewater.Range
                                ? (int) Orbwalking.GetRealAutoAttackRange(player)
                                : bilgewater.Range - 20), 0, (int) bilgewater.Range));
            menuI.AddSubMenu(menuBilgewater);

            Menu menuBlade = new Menu("Blade of the Ruined King", "Blades");
            menuBlade.AddItem(new MenuItem("botr", "Enabled")).SetValue(true);
            menuBlade.AddItem(new MenuItem("botronlyks", "KS only")).SetValue(false);
            menuBlade.AddItem(new MenuItem("botrminr", "Min range"))
                .SetValue(
                    new Slider(
                        (int)
                            (Orbwalking.GetRealAutoAttackRange(player) < botrk.Range
                                ? (int) Orbwalking.GetRealAutoAttackRange(player)
                                : botrk.Range - 20), 0, (int) botrk.Range));
            menuBlade.AddItem(new MenuItem("botrmyhealth", "Use if player healt lower"))
                .SetValue(new Slider(40, 0, 100));
            menuBlade.AddItem(new MenuItem("botrenemyhealth", "Use if enemy healt lower"))
                .SetValue(new Slider(50, 0, 100));
            menuI.AddSubMenu(menuBlade);

            Menu menuHextech = new Menu("Hextech Gunblade", "Hextechs");
            menuHextech.AddItem(new MenuItem("hex", "Enabled")).SetValue(true);
            menuHextech.AddItem(new MenuItem("hexonlyks", "KS only")).SetValue(false);
            menuHextech.AddItem(new MenuItem("hexminr", "Min range"))
                .SetValue(
                    new Slider(
                        (int)
                            (Orbwalking.GetRealAutoAttackRange(player) < hexgun.Range
                                ? (int) Orbwalking.GetRealAutoAttackRange(player)
                                : hexgun.Range - 20), 0, (int) hexgun.Range));
            menuI.AddSubMenu(menuHextech);

            Menu menuFrost = new Menu("Frost Queen's Claim ", "Frost");
            menuFrost.AddItem(new MenuItem("frost", "Enabled")).SetValue(true);
            menuFrost.AddItem(new MenuItem("frostlow", "Use on low HP")).SetValue(true);
            menuFrost.AddItem(new MenuItem("frostmin", "Min enemy")).SetValue(new Slider(2, 1, 6));
            menuI.AddSubMenu(menuFrost);

            Menu menuMountain = new Menu("Face of the Mountain ", "Mountain");
            menuMountain.AddItem(new MenuItem("mountain", "Enabled")).SetValue(true);
            menuMountain.AddItem(new MenuItem("castonme", "SelfCast")).SetValue(true);
            menuMountain.AddItem(new MenuItem("mountainmin", "Under x % health")).SetValue(new Slider(20, 0, 100));
            Menu menuMountainprior = new Menu("Target priority", "MountainPriorityMenu");
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                menuMountainprior.AddItem(new MenuItem("mountainpriority" + ally.ChampionName, ally.ChampionName))
                    .SetValue(new Slider(5, 0, 5));
            }
            menuMountainprior.AddItem(new MenuItem("off", "0 is off"));
            menuMountain.AddSubMenu(menuMountainprior);
            menuI.AddSubMenu(menuMountain);

            Menu menuSolari = new Menu("Locket of the Iron Solari ", "Solari");
            menuSolari.AddItem(new MenuItem("solari", "Enabled")).SetValue(true);
            menuSolari.AddItem(new MenuItem("solariminally", "Min ally")).SetValue(new Slider(2, 1, 6));
            menuSolari.AddItem(new MenuItem("solariminenemy", "Min enemy")).SetValue(new Slider(2, 1, 6));
            menuI.AddSubMenu(menuSolari);

            menuI.AddItem(new MenuItem("you", "Youmuu's Ghostblade")).SetValue(true);
            menuI.AddItem(new MenuItem("useItems", "Use Items")).SetValue(true);
            mConfig.AddSubMenu(menuI);
            return mConfig;
        }

        public static float GetItemsDamage(Obj_AI_Hero target)
        {
            double damage = 0;
            if (Items.HasItem(odins.Id) && Items.CanUseItem(odins.Id))
            {
                damage += Damage.GetItemDamage(player, target, Damage.DamageItems.OdingVeils);
            }
            if (Items.HasItem(hexgun.Id) && Items.CanUseItem(hexgun.Id))
            {
                damage += Damage.GetItemDamage(player, target, Damage.DamageItems.Hexgun);
            }
            var ludenStacks = player.Buffs.FirstOrDefault(buff => buff.Name == "itemmagicshankcharge");
            if (ludenStacks != null && (Items.HasItem(Ludens.Id) && ludenStacks.Count == 100))
            {
                damage += player.CalcDamage(
                    target, Damage.DamageType.Magical,
                    Damage.CalcDamage(player, target, Damage.DamageType.Magical, 100 + player.FlatMagicDamageMod * 0.15));
            }
            if (Items.HasItem(lich.Id) && Items.CanUseItem(lich.Id))
            {
                damage += player.CalcDamage(
                    target, Damage.DamageType.Magical, player.BaseAttackDamage * 0.75 + player.FlatMagicDamageMod * 0.5);
            }
            if (Items.HasItem(Dfg.Id) && Items.CanUseItem(Dfg.Id))
            {
                damage = damage * 1.2;
                damage += Damage.GetItemDamage(player, target, Damage.DamageItems.Dfg);
            }
            if (Items.HasItem(Bft.Id) && Items.CanUseItem(Bft.Id))
            {
                damage = damage * 1.2;
                damage += Damage.GetItemDamage(player, target, Damage.DamageItems.BlackFireTorch);
            }
            if (Items.HasItem(tiamat.Id) && Items.CanUseItem(tiamat.Id))
            {
                damage += Damage.GetItemDamage(player, target, Damage.DamageItems.Tiamat);
            }
            if (Items.HasItem(hydra.Id) && Items.CanUseItem(hydra.Id))
            {
                damage += Damage.GetItemDamage(player, target, Damage.DamageItems.Hydra);
            }
            if (Items.HasItem(bilgewater.Id) && Items.CanUseItem(bilgewater.Id))
            {
                damage += Damage.GetItemDamage(player, target, Damage.DamageItems.Bilgewater);
            }
            if (Items.HasItem(botrk.Id) && Items.CanUseItem(botrk.Id))
            {
                damage += Damage.GetItemDamage(player, target, Damage.DamageItems.Botrk);
            }
            if (Items.HasItem(sheen.Id) && (Items.CanUseItem(sheen.Id) || player.HasBuff("sheen", true)))
            {
                damage += player.CalcDamage(target, Damage.DamageType.Physical, player.BaseAttackDamage);
            }
            if (Items.HasItem(gaunlet.Id) && Items.CanUseItem(gaunlet.Id))
            {
                damage += player.CalcDamage(target, Damage.DamageType.Physical, player.BaseAttackDamage * 1.25);
            }
            if (Items.HasItem(trinity.Id) && Items.CanUseItem(trinity.Id))
            {
                damage += player.CalcDamage(target, Damage.DamageType.Physical, player.BaseAttackDamage * 2);
            }
            return (float) damage;
        }


        public static Menu addCleanseOptions(Menu config)
        {
            var mConfig = config;
            Menu menuQ = new Menu("QSS", "QSSsettings");
            menuQ.AddItem(new MenuItem("slow", "Slow")).SetValue(false);
            menuQ.AddItem(new MenuItem("blind", "Blind")).SetValue(false);
            menuQ.AddItem(new MenuItem("silence", "Silence")).SetValue(false);
            menuQ.AddItem(new MenuItem("snare", "Snare")).SetValue(false);
            menuQ.AddItem(new MenuItem("stun", "Stun")).SetValue(false);
            menuQ.AddItem(new MenuItem("charm", "Charm")).SetValue(true);
            menuQ.AddItem(new MenuItem("taunt", "Taunt")).SetValue(true);
            menuQ.AddItem(new MenuItem("fear", "Fear")).SetValue(true);
            menuQ.AddItem(new MenuItem("suppression", "Suppression")).SetValue(true);
            menuQ.AddItem(new MenuItem("polymorph", "Polymorph")).SetValue(true);
            menuQ.AddItem(new MenuItem("damager", "Vlad/Zed ult")).SetValue(true);
            menuQ.AddItem(new MenuItem("QSSdelay", "Delay in ms")).SetValue(new Slider(600, 0, 1500));
            menuQ.AddItem(new MenuItem("QSSEnabled", "Enabled")).SetValue(true);
            mConfig.AddSubMenu(menuQ);
            return mConfig;
        }

        public static void UseCleanse(Menu config)
        {
            if (QssUsed)
            {
                return;
            }
            if (Items.CanUseItem(Qss.Id) && Items.HasItem(Qss.Id))
            {
                Cleanse(Qss, config);
            }
            if (Items.CanUseItem(Mercurial.Id) && Items.HasItem(Mercurial.Id))
            {
                Cleanse(Mercurial, config);
            }
            if (Items.CanUseItem(Dervish.Id) && Items.HasItem(Dervish.Id))
            {
                Cleanse(Dervish, config);
            }
        }

        private static void Cleanse(Items.Item Item, Menu config)
        {
            var delay = config.Item("QSSdelay").GetValue<Slider>().Value;
            foreach (var buff in player.Buffs)
            {
                if (config.Item("slow").GetValue<bool>() && buff.Type == BuffType.Slow)
                {
                    QssUsed = true;
                    Utility.DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(Item.Id, player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config.Item("blind").GetValue<bool>() && buff.Type == BuffType.Blind)
                {
                    QssUsed = true;
                    Utility.DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(Item.Id, player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config.Item("silence").GetValue<bool>() && buff.Type == BuffType.Silence)
                {
                    QssUsed = true;
                    Utility.DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(Item.Id, player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config.Item("snare").GetValue<bool>() && buff.Type == BuffType.Snare)
                {
                    QssUsed = true;
                    Utility.DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(Item.Id, player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config.Item("stun").GetValue<bool>() && buff.Type == BuffType.Stun)
                {
                    QssUsed = true;
                    Utility.DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(Item.Id, player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config.Item("charm").GetValue<bool>() && buff.Type == BuffType.Charm)
                {
                    QssUsed = true;
                    Utility.DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(Item.Id, player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config.Item("taunt").GetValue<bool>() && buff.Type == BuffType.Taunt)
                {
                    QssUsed = true;
                    Utility.DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(Item.Id, player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config.Item("fear").GetValue<bool>() && (buff.Type == BuffType.Fear || buff.Type == BuffType.Flee))
                {
                    QssUsed = true;
                    Utility.DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(Item.Id, player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config.Item("suppression").GetValue<bool>() && buff.Type == BuffType.Suppression)
                {
                    QssUsed = true;
                    Utility.DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(Item.Id, player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config.Item("polymorph").GetValue<bool>() && buff.Type == BuffType.Polymorph)
                {
                    QssUsed = true;
                    Utility.DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(Item.Id, player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config.Item("damager").GetValue<bool>())
                {
                    switch (buff.Name)
                    {
                        case "zedulttargetmark":
                            QssUsed = true;
                            Utility.DelayAction.Add(
                                2900, () =>
                                {
                                    Items.UseItem(Item.Id, player);
                                    QssUsed = false;
                                });
                            break;
                        case "VladimirHemoplague":
                            QssUsed = true;
                            Utility.DelayAction.Add(
                                4900, () =>
                                {
                                    Items.UseItem(Item.Id, player);
                                    QssUsed = false;
                                });
                            break;
                        case "MordekaiserChildrenOfTheGrave":
                            QssUsed = true;
                            Utility.DelayAction.Add(
                                delay, () =>
                                {
                                    Items.UseItem(Item.Id, player);
                                    QssUsed = false;
                                });
                            break;
                        case "urgotswap2":
                            QssUsed = true;
                            Utility.DelayAction.Add(
                                900, () =>
                                {
                                    Items.UseItem(Item.Id, player);
                                    QssUsed = false;
                                });
                            break;
                        case "skarnerimpale":
                            QssUsed = true;
                            Utility.DelayAction.Add(
                                delay, () =>
                                {
                                    Items.UseItem(Item.Id, player);
                                    QssUsed = false;
                                });
                            break;
                        case "poppydiplomaticimmunity":
                            QssUsed = true;
                            Utility.DelayAction.Add(
                                delay, () =>
                                {
                                    Items.UseItem(Item.Id, player);
                                    QssUsed = false;
                                });
                            break;
                    }
                }
            }
        }
    }
}