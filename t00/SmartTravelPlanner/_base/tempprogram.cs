using System;
using System.IO;
using System.Collections.Generic;
using SmartTravelPlanner;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter map filename: ");
        string mapFile = Console.ReadLine() ?? string.Empty;
        Console.Write("Enter traveler name: ");
        string travelerFile = Console.ReadLine() ?? string.Empty;
        Console.Write("Enter starting location: ");
        string startLocation = Console.ReadLine() ?? string.Empty;
        Console.Write("Enter destination: ");
        string? destination = Console.ReadLine() ?? string.Empty;

        CityGraph graph;
        try
        {
            graph = CityGraph.LoadFromFile(mapFile);
        }
        catch
        {
            Console.WriteLine("No route to find!");
            return;
        }
        Traveler traveler = new Traveler(travelerFile);
        traveler.SetLocation(startLocation);
        var originalOut = Console.Out;
        bool isSuccess = false;
        using (var x = new StringWriter())
        {
            Console.SetOut(x);
            isSuccess = traveler.PlanRouteTo(destination, graph);
            Console.SetOut(originalOut);
        }
        if (isSuccess)
        {
            List<string> currentRoute = new List<string>();
            for (int y = 0; y < traveler.GetStopCount(); y++)
            {
                currentRoute.Add(traveler[y]);
            }
            int distance = graph.GetPathDistance(currentRoute);
            Console.WriteLine(traveler.GetTravelSummary());
            Console.WriteLine($"Total distance: {distance} km");
            Logger<string> logger = new Logger<string>();
            logger.Add($"Traveler {traveler.GetName()} planned route {startLocation} -> {destination}");
            logger.Flush("log.txt");
        }
        else
        {
            Console.WriteLine("No route to find!");
        }
    }
}
