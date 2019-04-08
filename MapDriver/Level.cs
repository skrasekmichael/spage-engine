using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MapDriver
{
    [Serializable]
    public class Level
    {
        public Bitmap Map { get; set; }
        public string PilotMap { get; set; } = null;

        public List<LevelMap> Maps => maps.Values.ToList();

        private Dictionary<string, LevelMap> maps = new Dictionary<string, LevelMap>();
        private string[] indexes;
        private bool[] borders;

        public void Build(Bitmap map, int width)
        {
            indexes = new string[Map.Width * Map.Height];
            borders = new bool[Map.Width * Map.Height];
            maps.ToList().ForEach(kvp => kvp.Value.borders = new bool[Map.Width * Map.Height]);

            List<List<Point>> center = new List<List<Point>>();
            for (int i = 0; i < Maps.Count; i++)
                center.Add(new List<Point>());

            for (int x = 0; x < Map.Width; x++)
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    string key = null;
                    int index = 0;
                    foreach (KeyValuePair<string, LevelMap> kvp in maps)
                    {
                        if (map.GetPixel(x, y) == kvp.Value.KeyColor)
                        {
                            center[index].Add(new Point(x, y));
                            key = kvp.Key;
                        }
                        index++;
                    }
                    indexes[y * Map.Width + x] = key;
                }
            }

            for (int i = 0; i < Maps.Count; i++)
            {
                Point c = new Point(0, 0);
                center[i].ForEach(p => { c.X += p.X; c.Y += p.Y; });
                Maps[i].Center = new Point(c.X / center[i].Count, c.Y / center[i].Count);
            }

            set_borders(width);
        }

        private void set_borders(int width)
        {
            for (int x = 0; x < Map.Width; x++)
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    int index = y * Map.Width + x;
                    if (indexes[index] == null)
                    {
                        bool is_border = false;
                        HashSet<string> nei = new HashSet<string>();

                        for (int d = 0; d < width; d++)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                int nx = x, ny = y;

                                #region direction
                                switch (i)
                                {
                                    case 0:
                                        ny = y - d;
                                        break;
                                    case 4:
                                        ny = y - d;
                                        nx = x + d;
                                        break;
                                    case 1:
                                        nx = x + d;
                                        break;
                                    case 5:
                                        nx = x + d;
                                        ny = y + d;
                                        break;
                                    case 2:
                                        ny = y + d;
                                        break;
                                    case 6:
                                        ny = y + d;
                                        nx = x - d;
                                        break;
                                    case 3:
                                        nx = x - d;
                                        break;
                                    case 7:
                                        nx = x - d;
                                        ny = y - d;
                                        break;
                                }
                                #endregion

                                if (nx >= 0 && ny >= 0 && nx < Map.Width && ny < Map.Height)
                                {
                                    int pi = ny * Map.Width + nx;
                                    if (indexes[pi] != null)
                                    {
                                        is_border = true;
                                        nei.Add(indexes[pi]);
                                    };
                                }
                            }
                        }

                        if (is_border)
                        {
                            borders[index] = true;
                            foreach (KeyValuePair<string, LevelMap> kvp in maps.ToList())
                            {
                                if (nei.Contains(kvp.Key))
                                {
                                    kvp.Value.borders[index] = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        public Bitmap DeBuildMaps()
        {
            Bitmap bmp = new Bitmap(Map.Width, Map.Height);
            for (int x = 0; x < Map.Width; x++)
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    int index = y * Map.Width + x;
                    if (indexes[index] == null)
                        bmp.SetPixel(x, y, Color.Transparent);
                    else
                        bmp.SetPixel(x, y, maps[indexes[index]].KeyColor);
                }
            }
            return bmp;
        }

        public Bitmap DeBuildBorders()
        {
            Bitmap bmp = new Bitmap(Map.Width, Map.Height);
            for (int x = 0; x < Map.Width; x++)
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    int index = y * Map.Width + x;
                    if (borders[index])
                        bmp.SetPixel(x, y, Color.Black);
                    else
                        bmp.SetPixel(x, y, Color.Transparent);
                }
            }
            return bmp;
        }

        public bool AddMap(string name, LevelMap level)
        {
            if (maps.ContainsKey(name))
                return false;

            level.Name = name;
            maps.Add(name, level);
            return true;
        }

        public void Remove(string index)
        {
            maps.Remove(index);
        }

        public bool Save(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, this);
                    stream.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Map saving failed! Detail message: {0}", ex.Message);
                return false;
            }
        }

        public static Level Load(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        Level level = (Level)formatter.Deserialize(stream);
                        stream.Close();
                        return level;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Loading file failed. Detail Message: " + ex.Message);
                }
            }
            else
            {
                throw new Exception("File not exist.");
            }
        }

        public string GetMap(int x, int y) => indexes[y * Map.Width + x];
        public bool GetBorders(int x, int y) => borders[y * Map.Width + x];

        public int Count => maps.Count;

        public bool SetName(LevelMap map, string name)
        {
            if (maps.ContainsKey(name))
                return false;

            LevelMap temp = map;
            maps.Remove(map.Name);
            AddMap(name, temp);
            return true;
        }

        public LevelMap this[string index]
        {
            get
            {
                if (index == null || !maps.ContainsKey(index))
                    return null;
                else
                    return maps[index];
            }
            set
            {
                if (index != null)
                {
                    if (maps.ContainsKey(index))
                        maps[index] = value;
                    else
                        AddMap(index, value);
                }
            }
        }

        public void SetNeighbors(LevelMap map, List<string> neighbors)
        {
            for (int i = 0; i < map.Neighbors.Count; i++)
            {
                if (this[map.Neighbors[i]].Neighbors.Contains(map.Name) && !neighbors.Contains(map.Name))
                    this[neighbors[i]].Neighbors.Remove(map.Name);
            }

            for (int i = 0; i < neighbors.Count; i++)
            {
                if (!this[neighbors[i]].Neighbors.Contains(map.Name))
                    this[neighbors[i]].Neighbors.Add(map.Name);
            }

            map.Neighbors = neighbors;
        }

        public LevelMap GetByIndex(int index)
        {
            return maps.Values.ToArray()[index];
        }
    }

    [Serializable]
    public class LevelMap
    {
        public string Name { get; internal set; }
        public Color KeyColor { get; set; }
        public bool IsLast { get; set; } = false;
        public List<string> Neighbors { get; internal set; } = new List<string>();
        public int Player { get; set; } = 2;
        public int Rounds { get; set; } = 0;
        public int Sources { get; set; } = 0;
        public bool IsVisibled { get; set; } = false;
        internal bool[] borders;
        public bool GetBorders(int index) => borders[index];
        public Point Center { get; set; } = new Point(0, 0);
    }
}
