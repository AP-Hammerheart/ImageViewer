// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ImageViewer.Content
{
    internal class D2
    {
        internal D2(double x, double y)
        {
            X = x;
            Y = y;
        }

        internal double X { get; set; }
        internal double Y { get; set; }
        internal static double Epsilon { get; } = 1e-8;

        internal double Length()
        {
            return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
        }

        public static D2 operator +(D2 a, D2 b)
        {
            return new D2(a.X + b.X, a.Y + b.Y);
        }

        public static D2 operator -(D2 a, D2 b)
        {
            return new D2(a.X - b.X, a.Y - b.Y);
        }

        public static D2 operator *(D2 a, double s)
        {
            return new D2(s * a.X, s * a.Y);
        }

        public static bool operator ==(D2 a, D2 b)
        {
            return (Math.Abs(a.X - b.X) < Epsilon)
                && (Math.Abs(a.Y - b.Y) < Epsilon);
        }

        public static bool operator !=(D2 a, D2 b)
        {
            return (Math.Abs(a.X - b.X) >= Epsilon)
                || (Math.Abs(a.Y - b.Y) >= Epsilon);
        }

        internal static double Dot(D2 a, D2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        internal static double Angle(D2 a, D2 b)
        {
            var p = new D2(-b.Y, b.X);
            var angle = Math.Atan2(D2.Dot(a, p), D2.Dot(a, b));

            return (angle < 0) ? 2.0 * Math.PI + angle : angle;
        }

        public override bool Equals(object obj)
        {
            var vector = obj as D2;
            return vector != null &&
                   X == vector.X &&
                   Y == vector.Y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
    }

    internal class Rotator
    {
        private static D2 RotatePoint(D2 p, double angle)
        {
            var s = Math.Sin(angle);
            var c = Math.Cos(angle);

            return new D2(
                (p.X * c) - (p.Y * s),
                (p.X * s) + (p.Y * c));
        }

        private static D2 MovePoint(D2 p, double dx, double dy)
        {
            return new D2(p.X + dx, p.Y + dy);
        }

        private static int TileIndex(D2 p, double size)
        {
            return (int)
                ((Math.Floor(((3.5 * size) - p.Y) / size) * 7) +
                Math.Floor((p.X + (3.5 * size)) / size));
        }

        internal static bool InsideSquare(D2[] square, D2 p)
        {
            var ap = p - square[0];
            var ab = square[1] - square[0];
            var ad = square[3] - square[0];

            var ap_dot_ab = D2.Dot(ap, ab);
            var ap_dot_ad = D2.Dot(ap, ad);

            return
                (0 < ap_dot_ab) &&
                (0 < ap_dot_ad) &&
                (ap_dot_ab < D2.Dot(ab, ab)) &&
                (ap_dot_ad < D2.Dot(ad, ad));
        }

        private static List<D2> IntersectionPoints(D2 a, D2 b, D2 c, D2 d, double size)
        {
            var list = new List<D2>();

            var e = new D2(c.X - a.X, c.Y - a.Y);
            var r = new D2(b.X - a.X, b.Y - a.Y);
            var s = new D2(d.X - c.X, d.Y - c.Y);

            var exr = e.X * r.Y - e.Y * r.X;
            var exs = e.X * s.Y - e.Y * s.X;
            var rxs = r.X * s.Y - r.Y * s.X;

            if (exr == 0f && rxs == 0f)
            {
                list.Add(c);
                list.Add(d);

                if (a.Y == b.Y)
                {
                    if (d.X > c.X)
                    {
                        var x = a.X + (Math.Floor(e.X / size)) * size + size;
                        while (x < d.X) { list.Add(new D2(x, c.Y)); x += size; }
                    }
                    else
                    {
                        var x = a.X + (Math.Floor(e.X / size)) * size - size;
                        while (x > d.X) { list.Add(new D2(x, c.Y)); x -= size; }
                    }
                }
                else
                {
                    if (d.Y > c.Y)
                    {
                        var y = a.Y + (Math.Floor(e.Y / size)) * size + size;
                        while (y < d.Y) { list.Add(new D2(c.X, y)); y += size; }
                    }
                    else
                    {
                        var y = a.Y + (Math.Floor(e.Y / size)) * size - size;
                        while (y > d.Y) { list.Add(new D2(c.X, y)); y -= size; }
                    }
                }

                return list;
            }

            if (rxs != 0f)
            {
                var rxsr = 1f / rxs;
                var t = exs * rxsr;
                var u = exr * rxsr;

                if ((t >= 0f) && (t <= 1f) && (u >= 0f) && (u <= 1f))
                {
                    list.Add(a + (b - a) * t);
                }
            }

            return list;
        }

        private static Vector2 GetUV(D2 p, D2 uv0_0, D2 uv1_1)
        {
            float u = (float)((p.X - uv0_0.X) / (uv1_1.X - uv0_0.X));
            float v = (float)((p.Y - uv0_0.Y) / (uv1_1.Y - uv0_0.Y));

            return new Vector2(u, v);
        }

        private static ushort[] CreatePlaneIndices(int count)
        {
            var indices = new List<ushort>();

            for (var i = 0; i < count - 2; i++)
            {
                indices.Add(0);
                indices.Add((ushort)(i + 1));
                indices.Add((ushort)(i + 2));
            }

            return indices.ToArray();
        }

        private static VertexPlane[] CreateVertices(
            List<D2> points,
            Tuple<D2, D2> corners)
        {
            var minX = points[0].X;
            var maxX = points[0].X;

            var minY = points[0].Y;
            var maxY = points[0].Y;

            for (var i = 1; i < points.Count; i++)
            {
                if (points[i].X < minX) minX = points[i].X;
                if (points[i].X > minX) maxX = points[i].X;
                if (points[i].Y < minY) minY = points[i].Y;
                if (points[i].Y > maxY) maxY = points[i].Y;
            }

            var center = new D2((minX + maxX) / 2.0, (minY + maxY) / 2.0);
            var angles = new Tuple<int, double>[points.Count];
            var zeroLine = new D2(1, 0);

            for (var i = 0; i < points.Count; i++)
            {
                angles[i] = new Tuple<int, double>(
                    i, D2.Angle(points[i] - center, zeroLine));
            }

            Array.Sort(angles, (a, b) =>
                (a.Item2 > b.Item2 ? -1 :
                    a.Item2 < b.Item2 ? 1 : 0));

            var vertices = new VertexPlane[points.Count];

            for (var i = 0; i < points.Count; i++)
            {
                vertices[i] = new VertexPlane(
                    new Vector3(
                        (float)(points[angles[i].Item1].X), 
                        (float)(points[angles[i].Item1].Y), 0.0f), 
                    GetUV(points[angles[i].Item1], corners.Item1, corners.Item2));                
            }

            return vertices;
        }

        internal static List<Tuple<int, VertexPlane[], ushort[]>> Tiles(
            double angle,
            double dx,
            double dy,
            double size)
        {
            D2[] square =
            {
                new D2(1.5 * size, 1.5 * size),
                new D2(-1.5 * size, 1.5 * size),
                new D2(-1.5 * size, -1.5 * size),
                new D2(1.5 * size, -1.5 * size)
            };

            var index = new int[4];

            for (var idx = 0; idx < 4; idx++)
            {
                square[idx] = MovePoint(RotatePoint(square[idx], angle), dx, dy);
                index[idx] = TileIndex(square[idx], size);
            }

            var cornerPoints = new Tuple<D2, bool>[64];

            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    var p = new D2((-3.5 + x) * size, (3.5 - y) * size);
                    cornerPoints[y * 8 + x] = new Tuple<D2, bool>(p, InsideSquare(square, p));
                }
            }

            var horizontalLines = new List<D2>[8];
            var verticalLines = new List<D2>[8];

            for (var i = 0; i < 8; i++)
            {
                horizontalLines[i] = new List<D2>();
                verticalLines[i] = new List<D2>();
            }

            // First and last line will never intersect with square
            for (var i = 1; i < 7; i++)
            {
                var hp1 = new D2(-3.5 * size, (3.5 - i) * size);
                var hp2 = new D2(3.5 * size, (3.5 - i) * size);

                var vp1 = new D2((-3.5 + i) * size, 3.5 * size);
                var vp2 = new D2((-3.5 + i) * size, -3.5 * size);

                for (var s = 0; s < 4; s++)
                {
                    horizontalLines[i].AddRange(IntersectionPoints(hp1, hp2, square[s], square[(s + 1) % 4], size));
                    verticalLines[i].AddRange(IntersectionPoints(vp1, vp2, square[s], square[(s + 1) % 4], size));
                }
            }

            var tiles = new List<D2>[49];
            var corners = new Tuple<D2, D2>[49];

            int[] inactiveTiles = { 0, 1, 5, 6, 7, 13, 35, 41, 42, 43, 47, 48 };

            for (var y = 0; y < 7; y++)
            {
                for (var x = 0; x < 7; x++)
                {
                    var idx = y * 7 + x;

                    if (inactiveTiles.Contains<int>(idx)) continue;

                    tiles[idx] = new List<D2>();

                    Tuple<D2, bool>[] pt =
                    {
                        cornerPoints[y * 8 + (x + 1)],
                        cornerPoints[y * 8 + x],
                        cornerPoints[(y + 1) * 8 + x],
                        cornerPoints[(y + 1) * 8 + (x + 1)]
                    };

                    corners[idx] = new Tuple<D2, D2>(pt[1].Item1, pt[3].Item1);

                    foreach (var p in horizontalLines[y])
                    {
                        if (p.X + D2.Epsilon >= pt[1].Item1.X && p.X <= pt[0].Item1.X + D2.Epsilon)
                        {
                            tiles[idx].Add(p);
                        }
                    }

                    foreach (var p in horizontalLines[y + 1])
                    {
                        if (p.X + D2.Epsilon >= pt[1].Item1.X && p.X <= pt[0].Item1.X + D2.Epsilon)
                        {
                            tiles[idx].Add(p);
                        }
                    }

                    foreach (var p in verticalLines[x])
                    {
                        if (p.Y + D2.Epsilon >= pt[2].Item1.Y && p.Y <= pt[1].Item1.Y + D2.Epsilon)
                        {
                            tiles[idx].Add(p);
                        }
                    }

                    foreach (var p in verticalLines[x + 1])
                    {
                        if (p.Y + D2.Epsilon >= pt[2].Item1.Y && p.Y <= pt[1].Item1.Y + D2.Epsilon)
                        {
                            tiles[idx].Add(p);
                        }
                    }

                    for (var s = 0; s < 4; s++)
                    {
                        if (pt[s].Item2) tiles[idx].Add(pt[s].Item1);
                        if (idx == index[s])
                        {
                            tiles[idx].Add(square[s]);
                        }
                    }
                }
            }

            var mesh = new List<Tuple<int, VertexPlane[], ushort[]>>();

            for (var i = 0; i < 49; i++)
            {
                if (tiles[i] == null || tiles[i].Count < 3) continue;

                tiles[i].Sort((a, b) =>
                        Math.Abs(a.Y - b.Y) < D2.Epsilon ?
                            (Math.Abs(a.X - b.X) < D2.Epsilon ?
                                0 : a.X < b.X ? -1 : 1) :
                            a.Y > b.Y ? -1 : 1);

                var idx = 1;
                while (idx < tiles[i].Count)
                {
                    if (tiles[i][idx] == tiles[i][idx - 1])
                    {
                        tiles[i].RemoveAt(idx);
                    }
                    else idx++;
                }

                if (tiles[i].Count > 2)
                {
                    mesh.Add(new Tuple<int, VertexPlane[], ushort[]>(
                        i,
                        CreateVertices(tiles[i], corners[i]),
                        CreatePlaneIndices(tiles[i].Count)));
                }
            }

            return mesh;
        }
    }
}
