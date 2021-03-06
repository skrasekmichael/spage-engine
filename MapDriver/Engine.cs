﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MapDriver
{
    [Flags]
    public enum Visibility : int
    {
        Sighted = 3, Visible = 2, Hidden = 1, Gone = 0
    }

    public class Engine
    {
        private int w, h, phase = 1;
        private PointF parse(PointF val) => new PointF(val.X - w / 2, h / 2 - val.Y);

        private Map map;
        private PointF[,] points, minimap;
        private int size = 20;
        private double u, v;
        private PointF center = new PointF(0, 0);

        public PointF Center { get; set; } = new PointF(0, 0);

        private PointF _view = new PointF(0, 0);
        public PointF View
        {
            get => _view;
            set
            {
                if (value != _view)
                {
                    if (value == new PointF(0, 0))
                        _view = value;
                    else
                    {
                        int cx = (int)Math.Floor((double)map.Width / 2);
                        int cy = (int)Math.Floor((double)map.Height / 2);

                        PointF center = parse(GetPoint(cx, cy));

                        PointF p1 = parse(GetPoint(0, 0));
                        PointF p2 = parse(GetPoint(map.Width, 0));
                        PointF p3 = parse(GetPoint(0, map.Height));
                        PointF p4 = parse(GetPoint(map.Width, map.Height));
                        PointF pos = (new PointF(center.X + value.X, center.Y - value.Y));

                        PointF[] area = new PointF[] { p1, p2, p3, p4 };

                        if (is_in_area(area, pos))
                            _view = value;
                        else
                        {
                            double angle = Vector.FromPoints(center, pos).Atan();
                            double a1 = Vector.FromPoints(center, p1).Atan();
                            double a2 = Vector.FromPoints(center, p2).Atan();
                            double a3 = Vector.FromPoints(center, p3).Atan();
                            double a4 = Vector.FromPoints(center, p4).Atan();

                            double dx = _view.X - value.X, dy = _view.Y - value.Y;

                            if (angle > a4 && angle < a2)
                                View = new PointF(_view.X - (float)Math.Abs(dy), _view.Y - (float)Math.Abs(dx));
                            else if (angle > a2 && angle < a1)
                                View = new PointF(_view.X - (float)Math.Abs(dy), _view.Y + (float)Math.Abs(dx));
                            else if (angle > a1 && angle < a3)
                                View = new PointF(_view.X + (float)Math.Abs(dy), _view.Y + (float)Math.Abs(dx));
                            else if (angle > a3 && angle < a4)
                                View = new PointF(_view.X + (float)Math.Abs(dy), _view.Y - (float)Math.Abs(dx));
                        }
                    }
                    CalcPoints(phase);
                }
            }
        }

        public List<Point> AttackRange { get; set; } = null;
        public Dictionary<Point, double> Mobility { get; set; } = null;
        private Visibility[,,] visibilities;

        public int Size
        {
            get => this.size;
            set
            {
                this.size = value;

                u = Height;
                v = Width;

                CalcPoints();
            }
        }

        public double Width => Math.Cos(Math.PI / 6) * size;
        public double Height => Math.Sin(Math.PI / 6) * size;

        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }

        public Engine(Map map, int size, int Width, int Height)
        {
            this.map = map;
            points = new PointF[map.Width + 1, map.Height + 1];
            visibilities = new Visibility[8, map.Width, map.Height];
            this.size = size;

            w = Width;
            h = Height;

            u = this.Height;
            v = this.Width;

            center = new PointF((float)((map.Width - map.Height) * -v / 2), (float)((map.Width + map.Height) * -u / 2));
            CalcPoints();
        }

        public PointF[,] GetMiniMap(double size)
        {
            PointF[,] points = new PointF[map.Width + 1, map.Height + 1];
            for (int i = 0; i < map.Width + 1; i++)
            {
                for (int j = 0; j < map.Height + 1; j++)
                {
                    double x = (i - j) * size + (float)((map.Width - map.Height) * -size / 2);
                    double y = (i + j) * size + (float)((map.Width + map.Height) * -size / 2);

                    points[i, j] = new PointF((float)x, (float)y);
                }
            }
            minimap = points;
            return points;
        }

        public List<PointF> GetMiniMapPoints(PointF[,] matrix, int x, int y)
        {
            return new List<PointF>() { matrix[x, y], matrix[x + 1, y], matrix[x, y + 1], matrix[x + 1, y + 1] };
        }

        public void LoadVisibility(int player = 0)
        {
            for (int i = 0; i < map.Width; i++)
            {
                for (int j = 0; j < map.Height; j++)
                {
                    if (visibilities[player, i, j] == MapDriver.Visibility.Sighted)
                        visibilities[player, i, j] = MapDriver.Visibility.Hidden;
                }
            }
        }

        public void CalcPoints(int phase = 1)
        {
            this.phase = phase;

            PointF view = View;
            for (int i = 0; i < map.Width + 1; i++)
            {
                for (int j = 0; j < map.Height + 1; j++)
                {
                    double x = (i - j) * v + center.X + view.X + Center.X;
                    double y = (i + j) * u + center.Y + view.Y + Center.Y + map.GetElevation(i, j) * size / 2.5;

                    points[i, j] = new PointF((float)x, (float)y);
                    if (phase != 1)
                    {
                        for (int q = 0; q < 2; q++)
                        {
                            if (j != map.Height && i != map.Width && visibilities[q, i, j] == MapDriver.Visibility.Visible)
                                visibilities[q, i, j] = MapDriver.Visibility.Sighted;
                        }
                    }
                }
            }

            if (phase == 1)
            {
                Dictionary<Point, Unit> units = map.Units;
                foreach (KeyValuePair<Point, Unit> kvp in units)
                {
                    Point p = kvp.Key;
                    Unit u = kvp.Value;

                    visibilities[kvp.Value.Player - 1, p.X, p.Y] = Visibility.Visible;

                    for (double angle = 0; angle < 2 * Math.PI; angle += Math.PI / 90)
                    {
                        int lx = p.X;
                        int ly = p.Y;
                        double source_elev = map.GetTerrain(lx, ly).Elevation;
                        double max = 0;

                        for (double i = 1; i < Math.Floor(u.LineOfSight + max); i++)
                        {
                            double px = Math.Cos(angle) * i;
                            double py = Math.Sin(angle) * i;

                            int x = p.X + (int)Math.Round(px);
                            int y = p.Y + (int)Math.Round(py);

                            if (map.TryCoordinates(x, y))
                            {
                                double nelev = map.GetTerrain(x, y).Elevation;
                                double elev = map.GetTerrain(lx, ly).Elevation;
                                if (nelev < elev)
                                {
                                    max += 0.5;
                                    if (nelev > source_elev) break;
                                }
                                else if (nelev > elev)
                                    max -= 0.5;

                                visibilities[kvp.Value.Player - 1, x, y] = Visibility.Visible;

                                MapObject obj = map.GetMapObject(x, y);
                                if (obj != null && obj.Transparent == false) break;

                                lx = x;
                                ly = y;
                            }
                        }
                    }
                }
            }
        }

        public List<Point> GetLineOfSight(int player)
        {
            List<Point> los = new List<Point>();
            Dictionary<Point, Unit> units = map.Units.Where(u => u.Value.Player == player).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            foreach (KeyValuePair<Point, Unit> kvp in units)
            {
                Point p = kvp.Key;
                Unit u = kvp.Value;
                los.Add(new Point(p.X, p.Y));

                for (double angle = 0; angle < 2 * Math.PI; angle += Math.PI / 90)
                {
                    int lx = p.X;
                    int ly = p.Y;
                    double source_elev = map.GetTerrain(lx, ly).Elevation;
                    double max = 0;

                    for (double i = 1; i < Math.Floor(u.LineOfSight + max); i++)
                    {
                        double px = Math.Cos(angle) * i;
                        double py = Math.Sin(angle) * i;

                        int x = p.X + (int)Math.Round(px);
                        int y = p.Y + (int)Math.Round(py);

                        if (map.TryCoordinates(x, y))
                        {
                            double nelev = map.GetTerrain(x, y).Elevation;
                            double elev = map.GetTerrain(lx, ly).Elevation;
                            if (nelev < elev)
                            {
                                max += 0.5;
                                if (nelev > source_elev) break;
                            }
                            else if (nelev > elev)
                                max -= 0.5;

                            Point key = new Point(x, y);
                            if (!los.Contains(key))
                                los.Add(key);

                            MapObject obj = map.GetMapObject(x, y);
                            if (obj != null && obj.Transparent == false) break;

                            lx = x;
                            ly = y;
                        }
                    }
                }
            }

            return los;
        }

        public Visibility GetVisibility(int x, int y, int player = 0)
        {
            return visibilities[player, x, y];
        }

        public void SetVisibility(int x, int y, Visibility data)
        {
            visibilities[0, x, y] = data;
        }

        public PointF GetCenter(int x, int y)
        {
            var a = GetPoint(x, y);
            var b = GetPoint(x + 1, y + 1);
            var c = GetPoint(x, y + 1);
            var d = GetPoint(x + 1, y);

            double sx = b.X - a.X, sy = b.Y - a.Y;
            double ux = d.X - c.X, uy = d.Y - c.Y;

            double t = (c.X - a.X) / (sx - ux);

            double px = a.X + sx * t;
            double py = a.Y + sy * t;
            return new PointF((float)px, (float)py);
        }

        public PointF GetPoint(int x, int y) => points[x, y];
        public PointF GetMiniMapPoint(int x, int y) => minimap[x, y];
        public PointF[,] MiniMapPoints => minimap;

        public List<Point> GetMove(Dictionary<Point, double> mobility, int sx, int sy, int px, int py)
        {
            List<Point> points = new List<Point>();

            void get(int x, int y)
            {
                points.Add(new Point(x, y));
                if (sx == x && sy == y)
                    return;

                Point point = new Point(0, 0);
                double max = 0;
                for (int i = 0; i < 8; i++)
                {
                    int nx = x;
                    int ny = y;

                    #region direction
                    switch (i)
                    {
                        case 0:
                            ny = y - 1;
                            break;
                        case 4:
                            ny = y - 1;
                            nx = x + 1;
                            break;
                        case 1:
                            nx = x + 1;
                            break;
                        case 5:
                            nx = x + 1;
                            ny = y + 1;
                            break;
                        case 2:
                            ny = y + 1;
                            break;
                        case 6:
                            ny = y + 1;
                            nx = x - 1;
                            break;
                        case 3:
                            nx = x - 1;
                            break;
                        case 7:
                            nx = x - 1;
                            ny = y - 1;
                            break;
                    }
                    #endregion

                    Point key = new Point(nx, ny);
                    if (mobility.ContainsKey(key))
                    {
                        if (mobility[key] > max)
                        {
                            max = mobility[key];
                            point = key;
                        }
                    }
                }

                if (!points.Contains(point))
                    get(point.X, point.Y);
            }

            get(px, py);
            points.Reverse();
            return points;
        }

        public Dictionary<Point, double> GetMobility(int px, int py, Unit unit = null, double? stamina = null)
        {
            Dictionary<Point, double> mobility = new Dictionary<Point, double>();

            if (unit == null) unit = map.GetUnit(px, py);

            double per = unit.StaminaPerMove;

            if (stamina == null) stamina = unit.Stamina;
            else if (stamina < 0)
            {
                per = per / 1000;
                stamina = unit.Stamina;
            }

            mobility.Add(new Point(px, py), stamina.Value);

            for (int j = 0; j < mobility.Count; j++)
            {
                Point pos = mobility.Keys.ToArray()[j];
                int x = pos.X;
                int y = pos.Y;

                for (int i = 0; i < 8; i++)
                {
                    int nx = x;
                    int ny = y;
                  
                    #region direction
                    switch (i)
                    {
                        case 0:
                            ny = y - 1;
                            break;
                        case 4:
                            ny = y - 1;
                            nx = x + 1;
                            break;
                        case 1:
                            nx = x + 1;
                            break;
                        case 5:
                            nx = x + 1;
                            ny = y + 1;
                            break;
                        case 2:
                            ny = y + 1;
                            break;
                        case 6:
                            ny = y + 1;
                            nx = x - 1;
                            break;
                        case 3:
                            nx = x - 1;
                            break;
                        case 7:
                            nx = x - 1;
                            ny = y - 1;
                            break;
                    }
                    #endregion

                    if (map.TryCoordinates(nx, ny) && map.IsTroughput(this, unit, nx, ny, unit.Player))
                    {
                        Point p = new Point(nx, ny);
                        if (!mobility.ContainsKey(p))
                        {
                            Terrain t = map.GetTerrain(nx, ny);
                            MapObject o = map.GetMapObject(nx, ny);
                            float mob = t.Mobility + (o == null ? 0 : o.Mobility);
                            if (i > 3)
                                mob = mob * 1.2f;

                            double st = mobility[new Point(x, y)] - per * mob;
                            if (st >= 0)
                                mobility.Add(p, st);
                        }
                    }
                }
            }

            return mobility;
        }

        public List<Point> GetAttackRange(int sx, int sy, Unit unit = null, int rrange = 0)
        {
            List<Point> range = new List<Point>();
            if (unit == null) unit = map.GetUnit(sx, sy);

            int attack_range = unit.Range;
            bool reverse = rrange != 0;

            if (reverse)
                attack_range = rrange;

            double s = 0.5;
            double source_elev = map.GetTerrain(sx, sy).Elevation;
            double circuit = 2 * Math.PI * attack_range;

            for (double angle = 0; angle < 2 * Math.PI; angle += (Math.PI * 2) / (2 * circuit))
            {
                double max = 0;

                int lx = sx;
                int ly = sy;

                for (double i = 0; i < attack_range + max; i++)
                {
                    double px = Math.Cos(angle) * (i + 1);
                    double py = Math.Sin(angle) * (i + 1);

                    int x = sx + (int)Math.Round(px);
                    int y = sy + (int)Math.Round(py);

                    if (map.TryCoordinates(x, y) && !(lx == x && ly == y))
                    {
                        if (unit.IsRanged)
                        {
                            double nelev = map.GetTerrain(x, y).Elevation;
                            double elev = map.GetTerrain(lx, ly).Elevation;

                            if (nelev < elev)
                            {
                                max += (reverse) ? -s : s;
                                if (nelev > source_elev) break;
                            }
                            else if (elev < nelev)
                            {
                                max -= (reverse) ? -s : s;
                            }
                        }

                        Point key = new Point(x, y);
                        if (!range.Contains(key))
                            range.Add(key);

                        lx = x;
                        ly = y;
                    }

                }
            }

            return range;
        }

        public void SetViewToPostion(int px, int py)
        {
            int x = map.Width / 2 - px;
            int y = map.Height / 2 - py;

            float nx = (float)((x - y) * v);
            float ny = (float)((x + y) * u);

            int w = ScreenWidth / 2;
            int h = ScreenHeight / 2;

            Rectangle rectangle = new Rectangle((int)(View.X - w), (int)(View.Y - h),  2 * w, 2 * h);
            if (!rectangle.Contains((int)nx, (int)ny))
                this.View = new PointF(nx, ny);
        }

        private bool is_in_area(PointF[] area, PointF point) => is_in_polygon(new PointF[] { area[0], area[1], area[3] }, point) || is_in_polygon(new PointF[] { area[0], area[2], area[3] }, point);

        private bool is_in_polygon(PointF[] polygon, PointF point)
        {
            int len = polygon.Length;
            bool inside = false;

            float px = point.X, py = point.Y;
            float sx, sy, ex, ey;

            PointF ep = polygon[len - 1];
            ex = ep.X;
            ey = ep.Y;

            int i = 0;
            while (i < len)
            {
                sx = ex;
                sy = ey;

                ep = polygon[i++];
                ex = ep.X;
                ey = ep.Y;

                inside ^= (ey > py ^ sy > py) && ((px - ex) < (py - ey) * (sx - ex) / (sy - ey));
            }
            return inside;
        }
    }
}
