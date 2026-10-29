using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
namespace SmartTravelPlanner
{
    public class Traveler : ICloneable
    {
        private string name;
        private string currentLocation = string.Empty;
        private List<string> route = new List<string>(); 

        public Traveler(string name) => this.name = name = name?.Trim() ?? string.Empty;
        public string GetName() => name;
        private string FormatName(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return city;
            
            string[] word = city.Split(' ');
            for (int x = 0; x < word.Length; x++)
            {
                if (word[x].Length > 0)
                {
                    word[x] = char.ToUpper(word[x][0]) + word[x].Substring(1).ToLower();
                }
            }
            return string.Join(" ", word);
        }
        public void SetLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
                currentLocation = location;
            else 
                currentLocation = FormatName(location);
        }
        public string GetLocation() => currentLocation;

        public void AddCity(string city)
        {
            if (string.IsNullOrEmpty(city))
                throw new Exception("Invalid city!");
            else
                route.Add(FormatName(city));
        }
        public string GetRoute() => string.Join(" -> ", route);

        public void ClearRoute() => route.Clear();

        public int GetStopCount() => route.Count;

        public void SortRoute()
        {
            route.Sort();
        }

        public bool HasCity(string city)
        {
            if (string.IsNullOrEmpty(city))
                return route.Contains(city);
            else
                return route.Contains(FormatName(city));
        }

        public bool RemoveCity(string city)
        {
            if (string.IsNullOrEmpty(city))
                return route.Remove(city);
            else
                return route.Remove(FormatName(city));
        }

        public string? GetNextStop()
        {
            if (route.Count == 0)
                return null;
            return route[0];
        }
        public string this[int index]
        {
            get {return route[index];}
        }
        //t00 virtual method
        public virtual string GetTravelSummary()
        {
            return $"Traveler: {name} | Location: {currentLocation} | Route: {GetRoute()}";
        }

        public override string ToString()
        {
            return $"Traveler: {name} | Location: {currentLocation} | Route: {GetRoute()}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is Traveler other)
                return (name, currentLocation) == (other.name, other.currentLocation);
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, currentLocation);
        }

        public static bool operator ==(Traveler a, Traveler b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(Traveler a, Traveler b)
        {
            return !(a == b);
        }

        public object Clone()
        {
            Traveler cloneTraveler = new Traveler (this.name);
            cloneTraveler.currentLocation = this.currentLocation;
            cloneTraveler.route = new List<string>(this.route);

            return cloneTraveler;
        }
        // Збереження у JSON
        public void SaveToFile(string filePath)
        {
            List<string> formattedRoute = new List<string>();
            foreach (var city in route)
            {
                formattedRoute.Add($"\"{city}\"");
            }
            string routeArray = string.Join(", ", formattedRoute);
            string jsonString = "{\n" +
                                $"  \"name\": \"{name}\",\n" +
                                $"  \"currentLocation\": \"{currentLocation}\",\n" +
                                $"  \"route\": [{routeArray}]\n" +
                                "}";
            File.WriteAllText(filePath, jsonString);
        }
        public static Traveler LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) 
            {
                throw new FileNotFoundException("File doesn't exist");
            }
            string jsonString = File.ReadAllText(filePath);
            try
            {
                using (JsonDocument docx = JsonDocument.Parse(jsonString))
                {
                    JsonElement root = docx.RootElement;
                    string name = root.GetProperty("name").GetString() ?? string.Empty;
                    Traveler loadedTraveler = new Traveler(name);
                    if (root.TryGetProperty("currentLocation", out JsonElement locationElement))
                    {
                        loadedTraveler.SetLocation(locationElement.GetString() ?? string.Empty);
                    }
                    if (root.TryGetProperty("route", out JsonElement routeArray))
                    {
                        foreach (JsonElement cityElement in routeArray.EnumerateArray())
                        {
                            loadedTraveler.AddCity(cityElement.GetString() ?? string.Empty);
                        }
                    }
                    return loadedTraveler;
                }
            }
            catch (Exception)
            {
                throw new FileLoadException("Invalid travel data");
            }
        }
        public bool PlanRouteTo(string destination, CityGraph map)
        {
            string start = currentLocation;
            if (string.IsNullOrEmpty(start))
            {
                if (route.Count > 0)
                    start = route[0];
                else
                {
                    Console.WriteLine("No route!");
                    return false;
                }
            }
            List<string> shortestPath = map.FindShortestPath(start, destination);
            if (shortestPath == null || shortestPath.Count == 0)
            {
                Console.WriteLine("No route!");
                return false;
            }
            ClearRoute();
            foreach (string city in shortestPath)
            {
                AddCity(city);
            }
            return true;
        }
    }
}
