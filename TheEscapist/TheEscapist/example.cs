#region

using System;
using System.Threading;
using LeagueSharp.Common;

#endregion

namespace TheEscapist
{
    internal class Example
    {
        internal static void Init()
        {
            CustomEvents.Game.OnGameLoad += delegate
            {
                var onGameLoad = new Thread(Game_OnGameLoad);
                onGameLoad.Start();
            };
        }

        private static void Game_OnGameLoad()
        {
            // Do things, be cool!
            throw new NotImplementedException();
        }
    }
}