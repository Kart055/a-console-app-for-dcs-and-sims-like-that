using System;
using System.Collections.Generic;
using System.Threading;

#pragma warning disable CA1416

namespace DcsFlightCalculator;

public static class ConsoleGraphics
{
    // COLOR PALETTE

    public static readonly ConsoleColor Background =
        ConsoleColor.Black;

    public static readonly ConsoleColor Primary =
        ConsoleColor.Cyan;

    public static readonly ConsoleColor BrightPrimary =
        ConsoleColor.White;

    public static readonly ConsoleColor Success =
        ConsoleColor.Green;

    public static readonly ConsoleColor Warning =
        ConsoleColor.Yellow;

    public static readonly ConsoleColor Error =
        ConsoleColor.Red;

    public static readonly ConsoleColor Secondary =
        ConsoleColor.DarkCyan;

    public static readonly ConsoleColor Dim =
        ConsoleColor.DarkGray;

    public static readonly ConsoleColor HighlightBackground =
        ConsoleColor.DarkCyan;


    // INITIALIZATION

    public static void Initialize()
    {
        Console.BackgroundColor = Background;
        Console.ForegroundColor = Primary;
        Console.CursorVisible = false;

        SetConsoleSize();

        try
        {
            Console.Clear();
        }
        catch
        {
            // !!!some terminals do not support every console operation!!!
        }
    }

    private static void SetConsoleSize()
    {
        try
        {
            const int targetWidth = 130;
            const int targetHeight = 40;

            Console.SetBufferSize(
                Math.Max(Console.BufferWidth, targetWidth),
                Math.Max(Console.BufferHeight, targetHeight));

            Console.SetWindowSize(
                Math.Min(targetWidth, Console.LargestWindowWidth),
                Math.Min(targetHeight, Console.LargestWindowHeight));
        }
        catch
        {
            // ignore unsupported terminal operations
        }
    }


    // BASIC

    public static void Clear()
    {
        Console.BackgroundColor = Background;
        Console.ForegroundColor = Primary;

        Console.Clear();
    }

    public static void SetColor(ConsoleColor color)
    {
        Console.ForegroundColor = color;
    }

    public static void ResetColor()
    {
        Console.ForegroundColor = Primary;
        Console.BackgroundColor = Background;
    }

    public static int GetConsoleWidth()
    {
        try
        {
            return Math.Max(
                80,
                Console.WindowWidth);
        }
        catch
        {
            return 80;
        }
    }

    public static void WriteCentered(
        string text,
        ConsoleColor color = ConsoleColor.Cyan)
    {
        int width = GetConsoleWidth();

        int left =
            Math.Max(
                0,
                (width - text.Length) / 2);

        Console.ForegroundColor = color;

        Console.WriteLine(
            new string(' ', left) + text);
    }

    public static void WriteRule(
        char character = '─',
        ConsoleColor color = ConsoleColor.DarkCyan)
    {
        Console.ForegroundColor = color;

        Console.WriteLine(
            new string(
                character,
                Math.Max(
                    20,
                    GetConsoleWidth() - 2)));
    }


    // BOXES / PANELS

    public static void DrawBox(
        int left,
        int top,
        int width,
        int height,
        ConsoleColor color = ConsoleColor.Cyan)
    {
        if (width < 4 || height < 2)
            return;

        Console.ForegroundColor = color;

        WriteAt(
            left,
            top,
            "┌" + new string('─', width - 2) + "┐");

        for (int y = 1; y < height - 1; y++)
        {
            WriteAt(
                left,
                top + y,
                "│" +
                new string(' ', width - 2) +
                "│");
        }

        WriteAt(
            left,
            top + height - 1,
            "└" + new string('─', width - 2) + "┘");
    }

    public static void DrawPanel(
        string title,
        int width = 64,
        ConsoleColor color = ConsoleColor.Cyan)
    {
        width =
            Math.Min(
                width,
                GetConsoleWidth() - 4);

        Console.ForegroundColor = color;

        Console.WriteLine(
            $"┌─ {title} " +
            new string(
                '─',
                Math.Max(
                    1,
                    width - title.Length - 4)) +
            "┐");
    }

    public static void DrawPanelBottom(
        int width = 64,
        ConsoleColor color = ConsoleColor.Cyan)
    {
        width =
            Math.Min(
                width,
                GetConsoleWidth() - 4);

        Console.ForegroundColor = color;

        Console.WriteLine(
            "└" +
            new string(
                '─',
                Math.Max(1, width - 2)) +
            "┘");
    }

    public static void WritePanelLine(
        string label,
        string value,
        int width = 64,
        ConsoleColor valueColor = ConsoleColor.White)
    {
        width =
            Math.Min(
                width,
                GetConsoleWidth() - 4);

        int contentWidth =
            width - 4;

        string left =
            label.Length >= contentWidth
                ? label[..contentWidth]
                : label;

        string line =
            $"│ {left,-20}";

        int remaining =
            contentWidth - 20;

        string safeValue =
            value.Length > remaining
                ? value[..remaining]
                : value;

        Console.ForegroundColor = Primary;

        Console.Write(line);

        Console.ForegroundColor = valueColor;

        Console.Write(
            safeValue.PadRight(
                Math.Max(0, remaining)));

        Console.ForegroundColor = Primary;

        Console.WriteLine(" │");
    }

    public static void WriteAt(
        int left,
        int top,
        string text)
    {
        try
        {
            Console.SetCursorPosition(
                Math.Max(0, left),
                Math.Max(0, top));

            Console.Write(text);
        }
        catch
        {
        }
    }

    public static void WriteAt(
        int left,
        int top,
        char character)
    {
        WriteAt(
            left,
            top,
            character.ToString());
    }


    // TITLE

    public static void DrawTitle(
        string title,
        string subtitle = "")
    {
        Clear();

        Console.WriteLine();

        WriteCentered(
            "╔══════════════════════════════════════════════════════════════╗",
            Primary);

        WriteCentered(
            "║                                                              ║",
            Primary);

        WriteCentered(
            $"║{CenterInside(title, 62)}║",
            BrightPrimary);

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            WriteCentered(
                $"║{CenterInside(subtitle, 62)}║",
                Primary);
        }
        else
        {
            WriteCentered(
                "║                                                              ║",
                Primary);
        }

        WriteCentered(
            "║                                                              ║",
            Primary);

        WriteCentered(
            "╚══════════════════════════════════════════════════════════════╝",
            Primary);

        Console.WriteLine();
    }


    // STARTUP

    public static void ShowStartupAnimation()
    {
        Clear();

        Console.WriteLine();
        Console.WriteLine();

        WriteCentered(
            "K A R T ' S   F L I G H T   C O M P U T E R",
            BrightPrimary);

        Console.WriteLine();

        WriteCentered(
            "F U E L   &   R A N G E",
            Primary);

        Console.WriteLine();
        Console.WriteLine();

        WriteCentered(
            "[ SYSTEM INITIALIZATION ]",
            Secondary);

        Console.WriteLine();

        DrawTrajectoryAnimation();

        Console.WriteLine();
        Console.WriteLine();

        WriteCentered(
            "[ SYSTEM READY ]",
            Success);

        Console.WriteLine();

        WriteCentered(
            "[ PRESS ANY KEY ]",
            Warning);

        Console.ReadKey(true);
    }


    // TRAJECTORY

    private static void DrawTrajectoryAnimation()
    {
        const int height = 17;

        int consoleWidth =
            GetConsoleWidth();

        int width =
            Math.Min(
                115,
                Math.Max(
                    70,
                    consoleWidth - 10));

        int left =
            Math.Max(
                2,
                (consoleWidth - width) / 2);

        int top = 9;

        DrawBox(
            left - 3,
            top - 2,
            width + 6,
            height + 5,
            Secondary);

        List<TrajectoryPoint> trajectory =
            BuildTrajectory(
                width,
                height);

        char[,] raster =
            RasterizeTrajectory(
                trajectory,
                width,
                height);

        Console.ForegroundColor =
            Secondary;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                char character =
                    raster[y, x];

                if (character != ' ')
                {
                    WriteAt(
                        left + x,
                        top + y,
                        character);
                }
            }
        }

        for (int i = 0;
             i < trajectory.Count;
             i++)
        {
            TrajectoryPoint point =
                trajectory[i];

            if (i > 0)
            {
                TrajectoryPoint previous =
                    trajectory[i - 1];

                char previousCharacter =
                    raster[
                        previous.Y,
                        previous.X];

                Console.ForegroundColor =
                    Secondary;

                WriteAt(
                    left + previous.X,
                    top + previous.Y,
                    previousCharacter == ' '
                        ? ' '
                        : previousCharacter);
            }

            Console.ForegroundColor =
                BrightPrimary;

            WriteAt(
                left + point.X,
                top + point.Y,
                '>');

            int percentage =
                (int)(
                    ((double)(i + 1) /
                     trajectory.Count) *
                    100);

            Console.ForegroundColor =
                Primary;

            WriteAt(
                left + width / 2 - 2,
                top + height + 1,
                $"{percentage,3}%");

            Thread.Sleep(5);
        }
    }

    private static char[,] RasterizeTrajectory(
        List<TrajectoryPoint> points,
        int width,
        int height)
    {
        char[,] canvas =
            new char[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                canvas[y, x] = ' ';
            }
        }

        for (int i = 0;
             i < points.Count - 1;
             i++)
        {
            RasterizeLine(
                canvas,
                points[i].X,
                points[i].Y,
                points[i + 1].X,
                points[i + 1].Y);
        }

        return canvas;
    }

    private static void RasterizeLine(
        char[,] canvas,
        int x0,
        int y0,
        int x1,
        int y1)
    {
        int width =
            canvas.GetLength(1);

        int height =
            canvas.GetLength(0);

        int dx =
            Math.Abs(x1 - x0);

        int dy =
            Math.Abs(y1 - y0);

        int sx =
            x0 < x1 ? 1 : -1;

        int sy =
            y0 < y1 ? 1 : -1;

        int error =
            dx - dy;

        int step = 0;

        while (true)
        {
            if (x0 >= 0 &&
                x0 < width &&
                y0 >= 0 &&
                y0 < height)
            {
                if (step % 3 == 0)
                {
                    canvas[y0, x0] = '·';

                    if (x0 + 1 < width)
                        canvas[y0, x0 + 1] = '·';
                }
            }

            if (x0 == x1 &&
                y0 == y1)
            {
                break;
            }

            int error2 =
                error * 2;

            if (error2 > -dy)
            {
                error -= dy;
                x0 += sx;
            }

            if (error2 < dx)
            {
                error += dx;
                y0 += sy;
            }

            step++;
        }
    }

    private static List<TrajectoryPoint> BuildTrajectory(
        int width,
        int height)
    {
        List<TrajectoryPoint> points =
            new();

        int startX = 2;
        int endX = width - 3;
        int bottomY = height - 2;
        int apexY = 7;

        double centerX =
            (startX + endX) / 2.0;

        double halfWidth =
            (endX - startX) / 2.0;

        for (int x = startX;
             x <= endX;
             x++)
        {
            double normalized =
                (x - centerX) /
                halfWidth;

            double distance =
                Math.Abs(normalized);

            double curve =
                1.0 -
                Math.Pow(
                    distance,
                    1.65);

            curve =
                Math.Clamp(
                    curve,
                    0.0,
                    1.0);

            int y =
                bottomY -
                (int)Math.Round(
                    curve *
                    (bottomY - apexY));

            y =
                Math.Clamp(
                    y,
                    apexY,
                    bottomY);

            points.Add(
                new TrajectoryPoint
                {
                    X = x,
                    Y = y
                });
        }

        return points;
    }


    // MENU

    public static void DrawMenuItem(
        int number,
        string text,
        bool selected = false)
    {
        Console.ForegroundColor =
            selected
                ? BrightPrimary
                : Primary;

        Console.Write(
            selected
                ? "  ▶ "
                : "    ");

        Console.WriteLine(
            $"[{number}] {text}");

        ResetColor();
    }


    // STATUS

    public static void SuccessMessage(
        string message)
    {
        Console.ForegroundColor =
            Success;

        Console.WriteLine(
            $"  [ OK ] {message}");

        ResetColor();
    }

    public static void WarningMessage(
        string message)
    {
        Console.ForegroundColor =
            Warning;

        Console.WriteLine(
            $"  [ !! ] {message}");

        ResetColor();
    }

    public static void ErrorMessage(
        string message)
    {
        Console.ForegroundColor =
            Error;

        Console.WriteLine(
            $"  [ERROR] {message}");

        ResetColor();
    }


    // HELPERS

    private static string CenterInside(
        string text,
        int width)
    {
        if (text.Length >= width)
            return text[..width];

        int totalPadding =
            width - text.Length;

        int left =
            totalPadding / 2;

        int right =
            totalPadding - left;

        return
            new string(' ', left) +
            text +
            new string(' ', right);
    }


    // TRAJECTORY DATA

    private readonly struct TrajectoryPoint
    {
        public int X { get; init; }
        public int Y { get; init; }
    }
}
