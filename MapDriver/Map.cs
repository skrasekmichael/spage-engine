using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;

namespace MapDriver
{
    [Serializable]
    public class Map
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public string Description { get; set; } = "Podle zpráv, které dostáváme z území, se zde nachází silná armáda, měli bychom zaútočit, než se zformují.";
        public string Scout { get; set; } = "Nepřítel nás překvapil velmi silnou obranou. Většina své amrády drží na severo-západě. Musíme si dát pozor na útok ze zálohy. Nepřítel je velmi dobře připraven, má oddíl nejlepších paladínů a kopce střeží lukostřelci. ";
        public string Win { get; set; } = "Boj to byl těžký, ale zv ....";
        public Quest[] QuestList { get; set; } = new Quest[] { Quest.KillAllUnits() };

        private Terrain[,] terrain;
        private MapObject[,] objs;
        public Dictionary<Point, Unit> Units = new Dictionary<Point, Unit>();
        private int[,] elevation;

        public Map(int width, int height)
        {
            Width = width;
            Height = height;

            terrain = new Terrain[Width, Height];
            objs = new MapObject[Width, Height];
            elevation = new int[Width + 1, Height + 1];
        }

        public bool IsTroughput(Engine engine, Unit unit, int x, int y, int player = 1)
        {
            bool t = GetTerrain(x, y).Throughput;
            MapObject o = GetMapObject(x, y);
            if (o != null)
            {
                t &= o.Throughput;
                if (unit.Type == UnitType.Cavalery)
                    return false;
            }
            Unit u = GetUnit(x, y);
            if (u != null)
            {
                if (u.Player == player)
                    return false;
                else
                {
                    Visibility v = engine.GetVisibility(x, y, player - 1);
                    if (v == Visibility.Visible || v == Visibility.Sighted)
                        return false;
                }
            }
            return t;
        }

        public void SetUnit(int x, int y, Unit unit)
        {
                Point key = new Point(x, y);
                if (Units.ContainsKey(key))
                {
                    Units[key] = unit;
                    if (unit == null)
                        Units.Remove(key);
                }
                else
                {
                    if (unit != null)
                        Units.Add(key, unit);
                }
        }

        public Unit GetUnit(int x, int y)
        {
                Point key = new Point(x, y);
                if (Units.ContainsKey(key))
                    return Units[key];
                else
                    return null;
        }

        public Terrain GetTerrain(int x, int y) => terrain[x, y];

        public MapObject GetMapObject(int x, int y) => objs[x, y];

        public int GetElevation(int x, int y)
        {
            return elevation[x, y];
        }

        public void SetElevation(int x, int y, int val)
        {
            elevation[x, y] = val;
            SetTerrain(x, y, GetTerrain(x, y));
            SetTerrain(x - 1, y, GetTerrain(x - 1, y));
            SetTerrain(x, y - 1, GetTerrain(x, y - 1));
            SetTerrain(x - 1, y - 1, GetTerrain(x - 1, y - 1));
        }

        public void SetTerrain(int x, int y, Terrain t)
        {
            if (TryCoordinates(x, y))
            {
                Terrain terrain = (Terrain)Activator.CreateInstance(t.GetType());
                int[] elev = { elevation[x, y], elevation[x + 1, y], elevation[x, y + 1], elevation[x + 1, y + 1] };
                terrain.Elevation = (double)(elev.Max() + elev.Min()) / 2;
                this.terrain[x, y] = terrain;
            }
        }

        public void Rotate(int x, int y)
        {
            MapObject obj = GetMapObject(x, y);
            if (obj != null)
            {
                obj.Rotate();
            }
        }

        public void SetMapObject(int x, int y, MapObject obj)
        {
            if (TryCoordinates(x, y))
            {
                objs[x, y] = obj;
            }
            else
                throw new Exception($"Location [{x}, {y}] is not in map area.");
        }

        public void GenTerrain(int x, int y, int w, int h, Terrain t)
        {
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    SetTerrain(x + i, y + j, t);
                }
            }
        }

        public void Save(string path)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Map saving failed! Detail message: {0}", ex.Message);
            } 
        }

        public static Map Load(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        Map map = (Map)formatter.Deserialize(stream);
                        stream.Close();
                        return map;
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

        public bool TryCoordinates(int x, int y) => (x >= 0 && y >= 0 && x < Width && y < Height);
    }
}
