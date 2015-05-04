using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace UnderratedAIO.Helpers
{
    public class DrawHelper
    {
        public static Obj_AI_Hero player = ObjectManager.Player;

        public static void DrawCircle(Circle circle, float spellRange)
        {
            if (circle.Active) Render.Circle.DrawCircle(player.Position, spellRange, circle.Color);
            
        }

        public static void popUp(string text, int time, Color fontColor ,Color boxColor, Color borderColor)
        {
            var popUp = new Notification(text).SetTextColor(fontColor);
            popUp.SetBoxColor(boxColor);
            popUp.SetBorderColor(borderColor);
            Notifications.AddNotification(popUp);
            Utility.DelayAction.Add(time, () => popUp.Dispose());
        }
    }
}