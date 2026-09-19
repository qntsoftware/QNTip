namespace Qntip.UI;

internal static class Banner
{
    public static void Show()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
   ___  _   _ _____ _     
  / _ \| \ | |_   _(_)_ __
 | | | |  \| | | | | | '_ \
 | |_| | |\  | | | | | |_) | Best IP changer and packet encryptor
  \__\_\_| \_| |_| |_| .__/
                     |_|
");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  ============================================");
        Console.WriteLine("   QNTip  -  Tor IP Rotator & Encrypted Tunnel");
        Console.WriteLine("   Developer: QNT SOFTWARE");
        Console.WriteLine("  ============================================");
        Console.ResetColor();
        Console.WriteLine();
    }
}