﻿using System.IO;
using System.Windows.Forms;

namespace Launcher
{
    internal static class Program
    {
        private static void Main()
        {
            //ProcessInteraction.Run(Application.StartupPath + Path.DirectorySeparatorChar + "Gw.exe", "-oldauth", Application.StartupPath + Path.DirectorySeparatorChar + "entice" + Path.DirectorySeparatorChar + "Entice.dll", "Main");
            ProcessInteraction.RunFastInjection("D:\\Program Files (x86)\\GUILD WARS\\Gw.exe", "-oldauth", "D:\\Program Files (x86)\\GUILD WARS\\Entice.dll", "Main");
            //ProcessInteraction.RunFastInjection("D:\\Program Files (x86)\\GUILD WARS\\Gw.exe", "-oldauth", "D:\\Program Files (x86)\\GUILD WARS\\PacketExplorer\\Debug\\StreamDumper.dll", "Main");
            //ProcessInteraction.StartInject("D:\\Program Files (x86)\\GUILD WARS\\Gw.exe", "-oldauth", "D:\\Program Files (x86)\\GUILD WARS\\TemplateUpdater.dll");
        }
    }
}