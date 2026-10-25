using System;
using System.IO;
using System.Collections.Generic;

using TNode = System.String;
using System.Reflection.Metadata.Ecma335;

namespace Travelling
{
    public class CityGraph
    {
        public class TEdge
        {
            public TNode City {get; set;}
            public int Distance {get; set;}
            public TEdge(string city, int distance)
            {
                City = city;
                Distance = distance;
            }
        }
        private Dictionary<TNode, List<TEdge>> adjacencyList;
        private List<string> orderedGraph;
        private CityGraph()
        {
            adjacencyList = new Dictionary<TNode, List<TEdge>>();
            orderedGraph = new List<string>();
        }
        public static CityGraph LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }
            CityGraph graph = new CityGraph();
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split(',');
                if (parts.Length == 2)
                {
                    string[] cities = parts[0].Split('-');
                    if (cities.Length == 2)
                    {
                        TNode cityA = cities[0].Trim();
                        TNode cityB = cities[1].Trim();
                        if (int.TryParse(parts[1].Trim(), out int distance))
                        {
                            if (!graph.adjacencyList.TryGetValue(cityA, out List<TEdge>? edgesA))
                            {
                                edgesA = new List<TEdge>();
                                graph.adjacencyList[cityA] = edgesA;
                            }
                            edgesA.Add(new TEdge(cityB, distance));
                            if (!graph.adjacencyList.TryGetValue(cityB, out List<TEdge>? edgesB))
                            {
                                edgesB = new List<TEdge>();
                                graph.adjacencyList[cityB] = edgesB;
                            }
                            edgesB.Add(new TEdge(cityA, distance));
                            graph.orderedGraph.Add($"{cityA}-{cityB},{distance}");
                            graph.orderedGraph.Add($"{cityB}-{cityA},{distance}"); 
                        }
                    }
                }
            }
            return graph;
        }
        public override string ToString()
        {
            return string.Join("\n", orderedGraph);
        }
        public List<string> FindShortestPath(string from, string to)
        {
            List<string> path = new List<TNode>();
            if (!adjacencyList.ContainsKey(from) || !adjacencyList.ContainsKey(to))
            {
                return path;
            }
            Dictionary<string, int> distance = new Dictionary<TNode, int>();
            Dictionary<string, string> previousCity = new Dictionary<TNode, TNode>();
            HashSet<string> unvisitedCity = new HashSet<TNode>();
            foreach (var node in adjacencyList.Keys)
            {
                distance[node] = int.MaxValue;
                unvisitedCity.Add(node);
            }
            distance[from] = 0;
            while (unvisitedCity.Count > 0) //цикл Дейкстри
            {
                string? currentNode = null;
                int minDistance = int.MaxValue;
                foreach (var node in unvisitedCity)
                {
                    if (distance[node] < minDistance)
                    {
                        minDistance = distance[node];
                        currentNode = node;
                    }
                }
                if (currentNode == null || currentNode == to) break;
                unvisitedCity.Remove(currentNode);
                if (adjacencyList.TryGetValue(currentNode, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        if (unvisitedCity.Contains(neighbor.City))
                        {
                            int altDistance = distance[currentNode] + neighbor.Distance;
                            if (altDistance < distance[neighbor.City])
                            {
                                distance[neighbor.City] = altDistance;
                                previousCity[neighbor.City] = currentNode;
                            }
                        }
                    }
                }
            }
            string? currentPathNode = to;
            if (previousCity.ContainsKey(currentPathNode) || currentPathNode == from)
            {
                while (currentPathNode != null)
                {
                    path.Add(currentPathNode);
                    if (previousCity.ContainsKey(currentPathNode))
                        currentPathNode = previousCity[currentPathNode];
                    else
                        currentPathNode = null;
                }
                path.Reverse();
            }
            return path;
        }
        public int GetPathDistance(List<string> path)
        {
            if (path == null || path.Count < 2) return 0;
            int totalDistance = 0;
            for (int x = 0; x < path.Count - 1; x++)
            {
                string currentCity = path[x];
                string nextCity = path[x + 1];
                if (!adjacencyList.TryGetValue(currentCity, out List<TEdge>? edges)) return -1;
                bool connection = false;
                foreach (var edge in edges)
                {
                    if (edge.City == nextCity)
                    {
                        totalDistance += edge.Distance;
                        connection = true;
                        break;
                    }
                }
                if (!connection) return -1;
            }
            return totalDistance;
        }
    }
}