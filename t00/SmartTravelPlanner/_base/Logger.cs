using System;
using System.Collections.Generic;
using System.IO;
namespace SmartTravelPlanner
{
    public class Logger<T>
    {
        private List<T> entrie = new List<T>();
        public void Add(T entry)
        {
            entrie.Add(entry);
        }
        public void Flush(string filePath)
        {
            if (entrie.Count == 0)
            {
                File.AppendAllText(filePath, "");
                return;
            }
            string contentToApped = string.Join("\n", entrie) + "\n";
            File.AppendAllText(filePath, contentToApped);
            entrie.Clear();
        }
    }
}
