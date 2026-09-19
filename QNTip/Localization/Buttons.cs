namespace Qntip.Localization;

internal static class Buttons
{
    public static void Draw()
    {
        try
        {
            var w = Console.WindowWidth;
            if (w < 30) return;

            var saveX = Console.CursorLeft;
            var saveY = Console.CursorTop;

            // Ust sag - Durdur
            var stopLabel = $" [S] {Loc.T("btn_stop")} ";
            DrawButton(stopLabel, 0, ConsoleColor.DarkRed, w);

            // Alt sag - Dil
            var langKey = Loc.Lang == "TR" ? "E" : "T";
            var langLabel = Loc.Lang == "TR" ? "ENG" : "TR";
            var langText = $" [{langKey}] {langLabel} ";
            DrawButton(langText, 1, ConsoleColor.DarkBlue, w);

            Console.SetCursorPosition(saveX, saveY);
        }
        catch { }
    }

    private static void DrawButton(string label, int row, ConsoleColor bg, int winWidth)
    {
        var col = Math.Max(0, winWidth - label.Length - 1);
        Console.SetCursorPosition(col, row);
        Console.BackgroundColor = bg;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(label);
        Console.ResetColor();
    }
}