using System;
using System.Collections.Generic;
using System.Reflection;

namespace CgeGames;

public class Program
{
    static List<Type> GetGames()
    {
        var games = new List<Type>();
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            if (type.BaseType != typeof(Cge)) continue;
            games.Add(type);
        }
        return games;
    }

    static void Main()
    {
        var games = GetGames();
        if (games.Count == 0)
        {
            Console.WriteLine("No games found. Press any key to exit.");
            Console.ReadKey();
            return;
        }

        var id = -1;
        while (id == -1)
        {
            Console.Clear();
            Console.WriteLine("Select your game: ");
            for (var i = 0; i < games.Count; i++)
                Console.WriteLine($"{i}. {games[i].Name}");

            Console.Write("Select the game ID: ");
            if (!int.TryParse(Console.ReadLine(), out id) || id >= games.Count)
            {
                id = -1;
                Console.WriteLine("Couldn't parse ID. Try again.");
                continue;
            }
        }

        var game = Activator.CreateInstance(games[id]) as Cge;
        game.Run();
    }
}