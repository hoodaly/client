using Entice.Debugging;
using GuildWarsInterface;
using GuildWarsInterface.Declarations;
using GuildWarsInterface.Modification.Hooks;
using RGiesecke.DllExport;
using System;
using System.Threading;

namespace Entice
{
    internal class EntryPoint
    {
        [DllExport("Main")]
        internal static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Debug.Error(args.ExceptionObject.ToString());
            GuildWarsInterface.Debugging.Debug.ThrowException += exception => Debug.Error(exception.ToString());

            HookHelper.Initialize();
            Game.Initialize();
            Linking.Initialize();

            new Thread(() =>
                    {
                        while (true)
                        {
                            if (Game.State == GameState.Playing) Game.TimePassed(1000);
                            Thread.Sleep(1000);
                        }
                    }).Start();

            //ToDo: Fix Method in Movement.cs L67-96
            new Thread(Movement.Task).Start();
        }
    }
}
