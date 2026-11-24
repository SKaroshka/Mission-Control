using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SmartTravelPlanner.Models;

public class BaseModel
{
    private static Traveler? TravelerClone;

    public static ObservableCollection<string> UpdateMapFileLines(string? mapFilePath, ObservableCollection<string> mapFileLines)
    {
        if (!String.IsNullOrEmpty(mapFilePath))
        {
            mapFileLines.Clear();
            string[] lines = File.ReadAllLines(mapFilePath);
            foreach (string line in lines)
            {
                mapFileLines.Add(line);
            }
        }
        return mapFileLines;
    }

    public static ObservableCollection<string> FormatMapFileLines(ObservableCollection<string> mapFileLines)
    {
        string formattedMapFileLine = String.Empty;
        ObservableCollection<string> formattedMapFileLinesCollection = [];
        foreach (string line in mapFileLines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                string[] parts = line.Split(',');
                if (parts.Length == 2)
                {
                    string[] cities = parts[0].Split('-');
                    formattedMapFileLine = String.Join(" -> ", cities) + " : " + parts[1] + "km";
                }
                formattedMapFileLinesCollection.Add(formattedMapFileLine);
            }
        }
        formattedMapFileLinesCollection = new(formattedMapFileLinesCollection.OrderBy(i => i));
        return formattedMapFileLinesCollection;
    }

    public static string FormatSavedData(string? travelerFile, string? mapFileName,
                                    string? startLocation, string? destination,
                                    string? route, string? distance)
    {
        const string Bar = "\u258C";

        string[] lines = [
            .. !String.IsNullOrEmpty(travelerFile) ? (string[])[$"{Bar}{travelerFile}"] : [],
            .. !String.IsNullOrEmpty(mapFileName) ? (string[])[$"{Bar}{mapFileName}"] : [],
            .. !String.IsNullOrEmpty(startLocation)
                ? (string[])[String.IsNullOrEmpty(destination) ? $"{Bar}{startLocation}" : $"{Bar}{startLocation} -> {destination}"]
                : [],
            .. !String.IsNullOrEmpty(route) ? (string[])[$"{Environment.NewLine}{Bar}{route}"] : [],
            .. !String.IsNullOrEmpty(distance) ? (string[])[$"{Bar}{distance}"] : []
        ];

        string formattedSavedData = String.Join("  ", lines);

        return formattedSavedData;
    }

    public static ObservableCollection<string>? GetCitiesCollection(ObservableCollection<string>? mapFileLines)
    {
        ObservableCollection<string> citiesCollection = [];
        if (mapFileLines == null)
        {
            return null;
        }

        foreach (string line in mapFileLines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                string[] parts = line.Split(',');
                if (parts.Length == 2)
                {
                    string[] cities = parts[0].Split('-');
                    foreach (string city in cities)
                    {
                        citiesCollection.Add(city);
                    }
                }
            }
        }
        citiesCollection = new(citiesCollection.Distinct());
        citiesCollection = new(citiesCollection.OrderBy(i => i));
        return citiesCollection;
    }

    public static async Task<string[]?> PlanRoute(string? mapFilePath, string? travelerFile, string startLocation, string destination)
    {
        if (string.IsNullOrEmpty(mapFilePath) || string.IsNullOrEmpty(travelerFile))
        {
            return (["", "", "Map file or traveler name missing"]);
        }

        CityGraph graph;
        try
        {
            graph = CityGraph.LoadFromFile(mapFilePath);
        }
        catch
        {
            Console.WriteLine("No route to find!");
            return null;
        }
        Traveler traveler = new(travelerFile);
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
            List<string> currentRoute = new();
            for (int y = 0; y < traveler.GetStopCount(); y++)
            {
                currentRoute.Add(traveler[y]);
            }
            int distance = graph.GetPathDistance(currentRoute);
            traveler.SetLocation(destination);
            string[] outputTraveler = { (traveler.GetRoute()), (distance + " km"), "" };
            TravelerClone = traveler;
            return outputTraveler;
        }
        else
        {
            Console.WriteLine("No route to find!");
            string[] outputTraveler = { "", "", "Destination not reachable" };
            return outputTraveler;
        }
    }

    public static Traveler? GetTravelerClone(string? travelerFile)
    {
        if (string.IsNullOrEmpty(travelerFile))
        {
            return null;
        }

        if (TravelerClone is null)
        {
            TravelerClone = new(travelerFile);
        }

        return TravelerClone;
    }

}