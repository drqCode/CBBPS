using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BranchPredictionSimulatorServer
{
    class PropertyFileHandler
    {
        private Dictionary<string, string> propertiesMap;
        private string configPath;

        public PropertyFileHandler(string path)
        {
            reload(path);
        }

        public string getPropertyValue(string propertyName, string defaultValue)
        {
            string propertyValue = getPropertyValue(propertyName);
            if (propertyValue != null)
            {
                return propertyValue;
            }
            else
            {
                setPropertyValue(propertyName, defaultValue);
                return defaultValue;
            }
        }

        public string getPropertyValue(string propertyName)
        {
            return propertiesMap.ContainsKey(propertyName) ? propertiesMap[propertyName] : null;
        }

        public void setPropertyValue(string propertyName, Object value)
        {
            if (!propertiesMap.ContainsKey(propertyName))
            {
                propertiesMap.Add(propertyName, value.ToString());
            }
            else
            {
                propertiesMap[propertyName] = value.ToString();
            }
        }

        public void save()
        {
            save(configPath);
        }

        public void save(string path)
        {
            this.configPath = path;

            StreamWriter streamWriter = new StreamWriter(path);
            foreach (string propertyName in propertiesMap.Keys.ToArray())
            {
                // the property value can be an empty string
                if (propertiesMap[propertyName] != null)
                {
                    streamWriter.WriteLine(propertyName + "=" + propertiesMap[propertyName]);
                }
            }
            streamWriter.Close();
        }

        public void reload()
        {
            reload(this.configPath);
        }

        public void reload(string path)
        {
            this.configPath = path;
            propertiesMap = new Dictionary<string, string>();

            if (File.Exists(path))
            {
                loadFromFile(path);
            }
        }

        private void loadFromFile(string path)
        {
            foreach (string line in File.ReadAllLines(path))
            {
                if ((!string.IsNullOrEmpty(line)) && (!line.StartsWith(";")) && (!line.StartsWith("#")) && (!line.StartsWith("'")) && (line.Contains('=')))
                {
                    int index = line.IndexOf('=');
                    string propertyName = line.Substring(0, index).Trim();
                    string propertyValue = line.Substring(index + 1).Trim();

                    if ((propertyValue.StartsWith("\"") && propertyValue.EndsWith("\"")) || (propertyValue.StartsWith("'") && propertyValue.EndsWith("'")))
                    {
                        propertyValue = propertyValue.Substring(1, propertyValue.Length - 2);
                    }
                    try
                    {
                        propertiesMap.Add(propertyName, propertyValue);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
