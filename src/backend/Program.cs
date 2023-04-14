using System;

namespace YTPPlusPlusPlus
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SaveData.Load();
            using (var game = new UserInterface())
                game.Run();
        }
    }
}
