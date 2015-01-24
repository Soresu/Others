using LeagueSharp;
using LeagueSharp.Common;

namespace opgg.Helpers
{
    public class DrawHelper
    {
        public static Obj_AI_Hero player = ObjectManager.Player;

        public static void DrawCircle(Circle circle, float spellRange)
        {
            if (circle.Active) Render.Circle.DrawCircle(player.Position, spellRange, circle.Color);
            
        }
    }
}