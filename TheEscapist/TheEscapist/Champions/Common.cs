#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace TheEscapist.Champions
{
    
    internal class Common
    {
        public static List<Skill> skills =new List<Skill>();
        public static readonly Obj_AI_Hero player = ObjectManager.Player;

        internal static void Init()
        {
            Game.PrintChat("<font color='#7CFC00'>The Escapist Loaded </font><font color='#FFFFFF'>- " + player.ChampionName + "</font>");
            LoadMenu();
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        internal static void AddSpells(Skill qSkill = null,
            Skill wSkill = null,
            Skill eSkill = null,
            Skill rSkill = null,
            Skill q2Skill = null,
            Skill w2Skill = null,
            Skill e2Skill = null,
            Skill r2Skill = null)
        {
            skills.Add(qSkill);
            skills.Add(wSkill);
            skills.Add(eSkill);
            skills.Add(rSkill);
            skills.Add(q2Skill);
            skills.Add(w2Skill);
            skills.Add(e2Skill);
            skills.Add(r2Skill);
        }

        private static void LoadMenu()
        {
            // Flee settings
            Menu menuF = new Menu("Escape ", "Esettings");
            foreach (var skill in skills)
            {
                if (skill!=null)
                {
                    menuF.AddItem(new MenuItem(skill.Spell.Instance.Name, skill.Spell.Instance.Name)).SetValue(true);
                }
            }
            menuF.AddItem(new MenuItem("flee", "Escape")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press));
            Program._menu.AddSubMenu(menuF);
            // Misc Settings
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM.AddItem(new MenuItem("packets", "Use Packets")).SetValue(false);
            Program._menu.AddSubMenu(menuM);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Program._menu.Item("flee").GetValue<KeyBind>().Active)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
              var enemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsEnemy && !x.IsDead && player.Distance(x)<1000)
                    .OrderBy(x => player.Distance(x.Position)).FirstOrDefault();

                if(enemy==null)return;

                foreach (var skill in skills.Where(x=> x!=null))
                {
                        CastSkill(skill, enemy);
                }
                }
            }

        private static void CastSkill(Skill spell, Obj_AI_Hero enemy)
        {
            //cant cast
            if (!spell.Spell.IsReady() || player.Mana < spell.Spell.Instance.ManaCost)
            {
                return;
            }
            //Isn't enabled
            if (!Program._menu.Item(spell.Spell.Instance.SData.Name).GetValue<bool>())
            {
                return;
            }
            //you running to him, so you don't want to escape
            if (player.Position.Distance(enemy.Position) >= player.Position.Extend(player.ServerPosition, 100).Distance(enemy.Position))
            {
                return;
            }
            var escapepoint = player.Position.Extend(Game.CursorPos, spell.Spell.Range);

            if (spell.SpellType == SpellType.SPELLTYPE_SELFCAST)
            {
                if (spell.EscapeType == EscapeType.ESCAPE_ENEMYCCSPELL && spell.Spell.Range > player.Distance(enemy.Position))
                {
                    spell.Spell.Cast(packets);
                    return;
                }
                    spell.Spell.CastOnUnit(player, packets);
                    return;
            }

            if (spell.SpellType == SpellType.SPELLTYPE_TARGETED)
            {
                if (spell.EscapeType == EscapeType.ESCAPE_ENEMYCCSPELL && spell.Spell.CanCast(enemy))
                {
                    spell.Spell.CastOnUnit(enemy, packets);
                    return;
                }
                if (spell.EscapeType == EscapeType.ESCAPE_WARDJUMP)
                {
                    //katarinaE
                }
                if (spell.EscapeType == EscapeType.ESCAPE_JUMPTOMINIONS)
                {
                    //katarinaE
                }
                if (spell.EscapeType == EscapeType.ESCAPE_SUSTAIN)
                {
                    //VictorQ
                    spell.Spell.CastOnUnit(enemy);
                    return;
                }
            }

            if (spell.SpellType == SpellType.SPELLTYPE_SKILLSHOT)
            {
                if (spell.EscapeType == EscapeType.ESCAPE_ENEMYCCSPELL && spell.Spell.CanCast(enemy))
                {
                    spell.Spell.Cast(enemy.Position, packets);
                    return;
                }
            }
            //EzrealE?
            if (spell.SpellType == SpellType.SPELLTYPE_TARGETLESS)
            {
                spell.Spell.Cast(escapepoint, packets);
                return;
            }
        }
        private static bool packets
        {
            get { return Program._menu.Item("packets").GetValue<bool>(); }
        }
    }
}