using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheEscapist
{
    public enum EscapeType
    {
        // Wardjumps || Jumping to minions with a skill || a spam spell to get away (to mouse pos) || spell to cc an enemy || Movement speed buff || Shields, Heal etc
        ESCAPE_WARDJUMP, ESCAPE_JUMPTOMINIONS, ESCAPE_SPAMSPELL, ESCAPE_ENEMYCCSPELL, ESCAPE_MSBUFF, ESCAPE_SUSTAIN
    }

    public enum SpellType
    {
        SPELLTYPE_SKILLSHOT, SPELLTYPE_TARGETED, SPELLTYPE_SELFCAST, SPELLTYPE_TARGETLESS
    }
    public class Skill
    {
        public Skill(String championName, EscapeType escapeType, Spell spell, SpellType spellType, SData spellData = null)
        {
            Spell = spell;
            ChampionName = championName;
            EscapeType = escapeType;
            SpellType = spellType;
            if (spellData != null && spellType == SpellType.SPELLTYPE_SKILLSHOT)
            {
                Spell.SetSkillshot(spellData.Delay, spellData.Width, spellData.Speed, spellData.Collision, spellData.SkillshotType);
            }
        }

        public String ChampionName;
        public EscapeType EscapeType;
        public Spell Spell;
        public SpellType SpellType;

    }

    public class SData
    {
        public SData(float delay, float width, float speed, bool collison, SkillshotType skillshotType)
        {
            Delay = delay;
            Width = width;
            Speed = speed;
            Collision = collison;
            SkillshotType = skillshotType;
        }
        public float Delay;
        public float Width;
        public float Speed;
        public bool Collision;
        public SkillshotType SkillshotType;
    }
}
