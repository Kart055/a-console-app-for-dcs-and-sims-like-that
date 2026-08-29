namespace DcsFlightCalculator;

internal class Program
{
    static void Main()
    {
        ConsoleGraphics.Initialize();
        ConsoleGraphics.ShowStartupAnimation();

        ConsoleUI.Run();
    }
}