﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using OpenTK.Mathematics;

namespace FreeFrame.Components.Shapes.Path
{
    /// <summary>
    /// DrawAttribute.
    /// Attribute: d.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d
    /// </summary>
    public abstract class DrawAttribute
    {
        int _x, _y, _x1, _y1 = 0;
        public int Y1 { get => _y1; protected set => _y1 = value; }
        public int X1 { get => _x1; protected set => _x1 = value; }
        public int X { get => _x; protected set => _x = value; }
        public int Y { get => _y; protected set => _y = value; }


        public static (int X, int Y, int X1, int Y1) Last = (0, 0, 0, 0);

        /// <summary>
        /// Should return the vertices position in NDC format
        /// </summary>
        /// <returns>array of vertices position. x, y, x, y, ... (clockwise)</returns>
        /// 
        public abstract (Vector2i position, Vector2i? controlPosition) Information();
        public abstract float[] GetVertices();
        public abstract uint[] GetVerticesIndexes();
        public abstract void MoveDelta(Vector2i deltaPosition);
        public abstract void ResizeDelta(Vector2i size);
        public abstract List<Vector2i> GetSelectablePoints();
        public abstract override string ToString();
    }
    /// <summary>
    /// MoveTo, M or m.
    /// Moving the current point to another point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#moveto_path_commands
    /// </summary>
    public class MoveTo : DrawAttribute
    {
        bool _isRelative;
        
        public bool IsRelative { get => _isRelative; private set => _isRelative = value; }

        /// <summary>
        /// Moving the current point to another point.
        /// </summary>
        /// <param name="x">position x</param>
        /// <param name="y">positin y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public MoveTo(Group x, Group y, bool isRelative = false) : this(Convert.ToInt32(x.Value), Convert.ToInt32(y.Value), isRelative) { }
        /// <summary>
        /// Moving the current point to another point.
        /// </summary>
        /// <param name="x">position x</param>
        /// <param name="y">positin y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public MoveTo(int x, int y, bool isRelative = false)
        {
            X = x;
            Y = y;
            IsRelative = isRelative;
        }
        public override float[] GetVertices()
        {
            if (IsRelative)
            {
                Last.X += X;
                Last.Y += Y;
            }
            else
            {
                Last.X = X;
                Last.Y = Y;
            }

            return new float[] { }; // Move doesnt have any vertices
        }
        public override uint[] GetVerticesIndexes()
        {
            return new uint[] { };
        }

        public override string ToString() => String.Format("{0} {1},{2}", IsRelative ? 'm' : 'M', X, Y);

        public override List<Vector2i> GetSelectablePoints() => new List<Vector2i>();

        public override void MoveDelta(Vector2i deltaPosition)
        {
            X += deltaPosition.X;
            Y += deltaPosition.Y;
            //throw new NotImplementedException();
        }

        public override void ResizeDelta(Vector2i size)
        {
            //throw new NotImplementedException();
        }

        public override (Vector2i position, Vector2i? controlPosition) Information() => (new Vector2i(X, Y), null);
    }
    /// <summary>
    /// LineTo, L or l.
    /// Use to draw a straight line from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#lineto_path_commands
    /// </summary>
    public class LineTo : DrawAttribute
    {
        bool _isRelative;
        public bool IsRelative { get => _isRelative; private set => _isRelative = value; }

        /// <summary>
        /// Use to draw a straight line from the current point to the given point.
        /// </summary>
        /// <param name="x">end point x</param>
        /// <param name="y">end point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public LineTo(Group x, Group y, bool isRelative = false) : this(Convert.ToInt32(x.Value), Convert.ToInt32(y.Value), isRelative) { }
        /// <summary>
        /// Use to draw a straight line from the current point to the given point.
        /// </summary>
        /// <param name="x">end point x</param>
        /// <param name="y">end point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public LineTo(int x, int y, bool isRelative = false)
        {
            X = x;
            Y = y;
            IsRelative = isRelative;
        }

        public override float[] GetVertices()
        {
            float[] vertices;

            if (IsRelative)
            {
                vertices = new float[] { Last.X, Last.Y, Last.X + X, Last.Y + Y };
                Last.X += X; // Update last position
                Last.Y += Y; // Update last position
            }
            else
            {
                vertices = new float[] { Last.X, Last.Y, X, Y };
                Last.X = X; // Update last position
                Last.Y = Y; // Update last position
            }

            return vertices;
        }
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1 }; // TODO: Please dont hardcode this

        public override string ToString() => String.Format("{0} {1},{2}", IsRelative ? 'l' : 'L', X, Y);

        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new List<Vector2i>();

            if (IsRelative)
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(Last.X + X, Last.Y + Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(X, Y));
            }
            return points;
        }

        public override void MoveDelta(Vector2i deltaPosition)
        {
            X += deltaPosition.X;
            Y += deltaPosition.Y;
        }

        public override void ResizeDelta(Vector2i deltaSize)
        {
            X += deltaSize.X;
            Y += deltaSize.Y;
        }
        public override (Vector2i position, Vector2i? controlPosition) Information() => (new Vector2i(X, Y), null);
    }
    /// <summary>
    /// HorizontalLineTo, H or h.
    /// Use to draw a horizontal line from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#lineto_path_commands
    /// </summary>
    public class HorizontalLineTo : DrawAttribute
    {
        bool _isRelative;
        public bool IsRelative { get => _isRelative; private set => _isRelative = value; }

        /// <summary>
        /// Use to draw a horizontal line from the current point to the given point.
        /// </summary>
        /// <param name="x">offset end point x</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public HorizontalLineTo(Group x, bool isRelative = false) : this(Convert.ToInt32(x.Value), isRelative) { }
        /// <summary>
        /// Use to draw a horizontal line from the current point to the given point.
        /// </summary>
        /// <param name="x">offset end point x</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public HorizontalLineTo(int x, bool isRelative = false)
        {
            X = x;
            IsRelative = isRelative;
        }

        public override float[] GetVertices()
        {
            float[] vertices;

            if (IsRelative)
            {
                vertices = new float[] { Last.X, Last.Y, Last.X + X, Last.Y };
                Last.X += X; // Update last position
            }
            else
            {
                vertices = new float[] { Last.X, Last.Y, X, Last.Y };
                Last.Y = X; // Update last position
            }

            return vertices;
        }

        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1 }; // TODO: Please dont hardcode this

        public override string ToString() => String.Format("{0} {1}", IsRelative ? 'h' : 'H', X);

        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new List<Vector2i>();

            if (IsRelative)
            {
                points.Add(new Vector2i(Last.X - X, Last.Y));
                points.Add(new Vector2i(Last.X, Last.Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X - X, Last.Y));
                points.Add(new Vector2i(X, Last.Y));
            }
            return points;
        }

        public override void MoveDelta(Vector2i deltaPosition)
        {
            X += deltaPosition.X;
            //LastY += deltaPosition.Y;
        }

        public override void ResizeDelta(Vector2i size)
        {
            X += size.X;
            //LastY += size.Y;
        }
        public override (Vector2i position, Vector2i? controlPosition) Information() => (new Vector2i(X, Last.Y), null);
    }
    /// <summary>
    /// VerticalLineTo, V or v.
    /// Use to draw a vertical line from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#lineto_path_commands
    /// </summary>
    public class VerticalLineTo : DrawAttribute
    {
        bool _isRelative;
        public bool IsRelative { get => _isRelative; set => _isRelative = value; }

        /// <summary>
        /// Use to draw a vertical line from the current point to the given point.
        /// </summary>
        /// <param name="y">offset end point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public VerticalLineTo(Group y, bool isRelative = false) : this(Convert.ToInt32(y.Value), isRelative) { }
        /// <summary>
        /// Use to draw a vertical line from the current point to the given point.
        /// </summary>
        /// <param name="y">offset end point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public VerticalLineTo(int y, bool isRelative = false)
        {
            Y = y;
            IsRelative = isRelative;
        }

        public override float[] GetVertices()
        {
            float[] vertices;

            if (IsRelative)
            {
                vertices = new float[] { Last.X, Last.Y, Last.X, Last.Y + Y };
                Last.Y += Y; // Update last position
            }
            else
            {
                vertices = new float[] { Last.X, Last.Y, Last.X, Y };
                Last.Y = Y; // Update last position
            }

            return vertices;
        }
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1 }; // TODO: Please dont hardcode this

        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new List<Vector2i>();

            if (IsRelative)
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(Last.X, Last.Y + Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(Last.X, Y));
            }
            return points;
        }

        public override string ToString() => String.Format("{0} {1}", IsRelative ? 'v' : 'V', Y);

        public override void MoveDelta(Vector2i deltaPosition)
        {
            throw new NotImplementedException();
        }

        public override void ResizeDelta(Vector2i size)
        {
            throw new NotImplementedException();
        }
        public override (Vector2i position, Vector2i? controlPosition) Information()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// CurveTo, Cubic Bézier Curve, C or c.
    /// Use to draw a cubic Bézier curve from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#cubic_b%C3%A9zier_curve
    /// </summary>
    public class CurveTo : DrawAttribute
    {
        bool _isRelative;
        int _x2;
        int _y2;

        public int X2 { get => _x2; private set => _x2 = value; }
        public int Y2 { get => _y2; private set => _y2 = value; }

        /// <summary>
        /// Use to draw a cubic Bézier curve from the current point to the given point.
        /// </summary>
        /// <param name="x1">start control point x</param>
        /// <param name="y1">start control point y</param>
        /// <param name="x2">end control point x</param>
        /// <param name="y2">end control point y</param>
        /// <param name="x">end point x</param>
        /// <param name="y">end point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public CurveTo(Group x1, Group y1, Group x2, Group y2, Group x, Group y, bool isRelative = false) : this(Convert.ToInt32(x1.Value), Convert.ToInt32(y1.Value), Convert.ToInt32(x2.Value), Convert.ToInt32(y2.Value), Convert.ToInt32(x.Value), Convert.ToInt32(y.Value), isRelative) { }
        /// <summary>
        /// Use to draw a cubic Bézier curve from the current point to the given point.
        /// </summary>
        /// <param name="x1">start control point x</param>
        /// <param name="y1">start control point y</param>
        /// <param name="x2">end control point x</param>
        /// <param name="y2">end control point y</param>
        /// <param name="x">end point x</param>
        /// <param name="y">end point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public CurveTo(int x1, int y1, int x2, int y2, int x, int y, bool isRelative = false)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            X = x;
            Y = y;
            _isRelative = isRelative;
        }
        public override float[] GetVertices()
        {
            List<float> vertices = new List<float>();
            double t;
            float x, y;

            // Only edges
            if (_isRelative)
            {
                for (int i = 0; i < 100; i++) // TODO: Magic value please dont hard code this
                {
                    t = i / 100.0f;

                    x = (float)(Math.Pow((1 - t), 3) * Last.X + 3 * Math.Pow((1 - t), 2) * t * (Last.X + X1) + 3 * (1 - t) * Math.Pow(t, 2) * (Last.X + X2) + Math.Pow(t, 3) * (Last.X + X));
                    y = (float)(Math.Pow((1 - t), 3) * Last.Y + 3 * Math.Pow((1 - t), 2) * t * (Last.Y + Y1) + 3 * (1 - t) * Math.Pow(t, 2) * (Last.Y + Y2) + Math.Pow(t, 3) * (Last.Y + Y));

                    vertices.AddRange(new float[] { x, y });
                }
                if (X > X2)
                    Last.X1 += X + X2;
                else if (X < X2)
                    Last.X1 += X - X2;
                else
                    Last.X1 += X;
                Last.X += X;
                Last.Y += Y;
            }
            else
            {
                for (int i = 0; i < 100; i++) // TODO: Magic value please dont hard code this
                {
                    t = i / 100.0f;

                    x = (float)(Math.Pow((1 - t), 3) * Last.X + 3 * Math.Pow((1 - t), 2) * t * X1 + 3 * (1 - t) * Math.Pow(t, 2) * X2 + Math.Pow(t, 3) * X);
                    y = (float)(Math.Pow((1 - t), 3) * Last.Y + 3 * Math.Pow((1 - t), 2) * t * Y1 + 3 * (1 - t) * Math.Pow(t, 2) * Y2 + Math.Pow(t, 3) * Y);

                    vertices.AddRange(new float[] { x, y });
                }
                if (X > X2)
                    Last.X1 = X + X2;
                else if (X < X2)
                    Last.X1 = X - X2;
                else
                    Last.X1 = X;
                Last.X = X;
                Last.Y = Y;
            }

            return vertices.ToArray();
        }
        public override uint[] GetVerticesIndexes() => Enumerable.Range(0, 100).Select(i => (uint)i).ToArray(); // Magic value please dont hard code this

        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new List<Vector2i>();

            if (_isRelative)
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(Last.X + X1, Last.Y + Y1));
                points.Add(new Vector2i(Last.X + X2, Last.Y + X2));
                points.Add(new Vector2i(Last.X + X, Last.Y + Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(X1, Y1));
                points.Add(new Vector2i(X2, X2));
                points.Add(new Vector2i(X, Y));
            }
            return points;
        }

        public override string ToString() => String.Format("{0} {1},{2} {3},{4} {5},{6}", _isRelative ? 'c' : 'C', X1, Y1, X2, Y2, X, Y);

        public override void MoveDelta(Vector2i deltaPosition)
        {
            X1 += deltaPosition.X;
            Y1 += deltaPosition.Y;
            X2 += deltaPosition.X;
            Y2 += deltaPosition.Y;
            X += deltaPosition.X;
            Y += deltaPosition.Y;
        }

        public override void ResizeDelta(Vector2i size)
        {
            X1 += size.X;
            Y1 += size.Y;
            X2 += size.X;
            Y2 += size.Y;
            X += size.X;
            Y += size.Y;
        }

        public override (Vector2i position, Vector2i? controlPosition) Information()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// SmoothCurveTo, Smooth Cubic Bézier Curbe, S or s.
    /// Use to draw a smooth cubic Bézier curve from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#cubic_b%C3%A9zier_curve
    /// </summary>
    public class SmoothCurveTo : DrawAttribute
    {
        bool _isRelative;
        int _x2;
        int _y2;

        public int X2 { get => _x2; private set => _x2 = value; }
        public int Y2 { get => _y2; private set => _y2 = value; }
        public bool IsRelative { get => _isRelative; private set => _isRelative = value; }

        /// <summary>
        /// Use to draw a smooth cubic Bézier curve from the current point to the given point.
        /// </summary>
        /// <param name="x2">end control point x</param>
        /// <param name="y2">end control point y</param>
        /// <param name="x">end point x</param>
        /// <param name="y">end point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public SmoothCurveTo(Group x2, Group y2, Group x, Group y, bool isRelative = false) : this(Convert.ToInt32(x2.Value), Convert.ToInt32(y2.Value), Convert.ToInt32(x.Value), Convert.ToInt32(y.Value), isRelative) { }
        /// <summary>
        /// Use to draw a smooth cubic Bézier curve from the current point to the given point.
        /// </summary>
        /// <param name="x2">end control point x</param>
        /// <param name="y2">end control point y</param>
        /// <param name="x">end point x</param>
        /// <param name="y">end point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public SmoothCurveTo(int x2, int y2, int x, int y, bool isRelative = false)
        {
            X2 = x2;
            Y2 = y2;
            X = x;
            Y = y;
            IsRelative = isRelative;
        }
        public override float[] GetVertices()
        {
            List<float> vertices = new List<float>();
            double t;
            float x, y;

            // Only edges
            if (IsRelative)
            {
                for (int i = 0; i < 100; i++) // TODO: Magic value please dont hard code this
                {
                    t = i / 100.0f;

                    x = (float)(Math.Pow((1 - t), 3) * Last.X + 3 * Math.Pow((1 - t), 2) * t * (Last.X + Last.X1) + 3 * (1 - t) * Math.Pow(t, 2) * (Last.X + X2) + Math.Pow(t, 3) * (Last.X + X));
                    y = (float)(Math.Pow((1 - t), 3) * Last.Y + 3 * Math.Pow((1 - t), 2) * t * (Last.Y + Last.Y1) + 3 * (1 - t) * Math.Pow(t, 2) * (Last.Y + Y2) + Math.Pow(t, 3) * (Last.Y + Y));

                    vertices.AddRange(new float[] { x, y });
                }
                if (X > X2)
                    Last.X1 += X + X2;
                else if (X < X2)
                    Last.X1 += X - X2;
                else
                    Last.X1 += X;
                Last.X += X;
                Last.Y += Y;
            }
            else
            {
                for (int i = 0; i < 100; i++) // TODO: Magic value please dont hard code this
                {
                    t = i / 100.0f;

                    x = (float)(Math.Pow((1 - t), 3) * Last.X + 3 * Math.Pow((1 - t), 2) * t * Last.X1 + 3 * (1 - t) * Math.Pow(t, 2) * X2 + Math.Pow(t, 3) * X);
                    y = (float)(Math.Pow((1 - t), 3) * Last.Y + 3 * Math.Pow((1 - t), 2) * t * Last.X1 + 3 * (1 - t) * Math.Pow(t, 2) * Y2 + Math.Pow(t, 3) * Y);

                    vertices.AddRange(new float[] { x, y });
                }
                if (X > X2)
                    Last.X1 = X + X2;
                else if (X < X2)
                    Last.X1 = X - X2;
                else
                    Last.X1 = X;
                Last.X = X;
                Last.Y = Y;
            }

            return vertices.ToArray();
        }
        public override uint[] GetVerticesIndexes() => Enumerable.Range(0, 100).Select(i => (uint)i).ToArray(); // Magic value please dont hard code this

        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new List<Vector2i>();

            if (IsRelative)
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(Last.X + Last.X1, Last.Y + Last.Y1));
                points.Add(new Vector2i(Last.X + X2, Last.Y + X2));
                points.Add(new Vector2i(Last.X + X, Last.Y + Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(Last.X1, Last.Y1));
                points.Add(new Vector2i(X2, X2));
                points.Add(new Vector2i(X, Y));
            }
            return points;
        }

        public override string ToString() => String.Format("{0} {1},{2} {3},{4}", IsRelative ? 's' : 'S', X2, Y2, X, Y);

        public override void MoveDelta(Vector2i deltaPosition)
        {
            throw new NotImplementedException();
        }

        public override void ResizeDelta(Vector2i size)
        {
            throw new NotImplementedException();
        }

        public override (Vector2i position, Vector2i? controlPosition) Information()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// QuadraticBezierCurveTo, Q, q.
    /// Use to draw a quadratic Bézier curve.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#quadratic_b%C3%A9zier_curve
    /// </summary>
    public class QuadraticBezierCurveTo : DrawAttribute
    {
        bool _isRelative;
        public bool IsRelative { get => _isRelative; set => _isRelative = value; }

        /// <summary>
        /// Use to draw a quadratic Bézier curve.
        /// </summary>
        /// <param name="x1">control point x</param>
        /// <param name="y1">control point y</param>
        /// <param name="x">end point x</param>
        /// <param name="y">end point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public QuadraticBezierCurveTo(Group x1, Group y1, Group x, Group y, bool isRelative = false) : this(Convert.ToInt32(x1.Value), Convert.ToInt32(y1.Value), Convert.ToInt32(x.Value), Convert.ToInt32(y.Value), isRelative) { }

        /// <summary>
        /// Use to draw a quadratic Bézier curve.
        /// </summary>
        /// <param name="x1">control point x</param>
        /// <param name="y1">control point y</param>
        /// <param name="x">end point x</param>
        /// <param name="y">end point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public QuadraticBezierCurveTo(int x1, int y1, int x, int y, bool isRelative = false)
        {
            X1 = x1;
            Y1 = y1;
            X = x;
            Y = y;
            IsRelative = isRelative;
        }
        public override float[] GetVertices()
        {
            List<float> vertices = new List<float>();
            double t;
            float x, y;

            // Only edges
            if (_isRelative)
            {
                for (int i = 0; i < 100; i++) // TODO: Magic value please dont hard code this
                {
                    t = i / 100.0f;

                    x = (float)(Math.Pow((1 - t), 2) * Last.X + 2 * (1 - t) * t * (Last.X + X1) + Math.Pow(t, 2) * (Last.X + X));
                    y = (float)(Math.Pow((1 - t), 2) * Last.Y + 2 * (1 - t) * t * (Last.Y + Y1) + Math.Pow(t, 2) * (Last.Y + Y));

                    vertices.AddRange(new float[] { x, y });
                }
                if (X > X1)
                    Last.X1 += X + X1;
                else if (X < X1)
                    Last.X1 += X - X1;
                else
                    Last.X1 += X;
                Last.X += X;
                Last.Y += Y;
            }
            else
            {
                for (int i = 0; i < 100; i++) // TODO: Magic value please dont hard code this
                {
                    t = i / 100.0f;

                    x = (float)(Math.Pow((1 - t), 2) * Last.X + 2 * (1 - t) * t * X1 + Math.Pow(t, 2) * X);
                    y = (float)(Math.Pow((1 - t), 2) * Last.Y + 2 * (1 - t) * t * Y1 + Math.Pow(t, 2) * Y);

                    vertices.AddRange(new float[] { x, y });
                }
                if (X > X1)
                    Last.X1 = X + X1;
                else if (X < X1)
                    Last.X1 = X - X1;
                else
                    Last.X1 = X;
                Last.X = X;
                Last.Y = Y;
            }

            return vertices.ToArray();
        }
        public override uint[] GetVerticesIndexes() => Enumerable.Range(0, 100).Select(i => (uint)i).ToArray(); // Magic value please dont hard code this

        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new List<Vector2i>();

            if (IsRelative)
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(Last.X + X1, Last.Y + Y1));
                points.Add(new Vector2i(Last.X + X, Last.Y + Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(X1, Y1));
                points.Add(new Vector2i(X, Y));
            }
            return points;
        }

        public override string ToString() => String.Format("{0} {1},{2} {3},{4}", IsRelative ? 'q' : 'Q', X1, Y1, X, Y);

        public override void MoveDelta(Vector2i deltaPosition)
        {
            throw new NotImplementedException();
        }

        public override void ResizeDelta(Vector2i size)
        {
            throw new NotImplementedException();
        }

        public override (Vector2i position, Vector2i? controlPosition) Information()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// SmoothQuadraticBezierCurveTo, T, t.
    /// Use to draw a smooth quadratic Bézier curve.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#quadratic_b%C3%A9zier_curve
    /// </summary>
    public class SmoothQuadraticBezierCurveTo : DrawAttribute
    {
        bool _isRelative;
        public bool IsRelative { get => _isRelative; set => _isRelative = value; }

        /// <summary>
        /// Use to draw a smooth quadratic Bézier curve.
        /// </summary>
        /// <param name="x">end point x</param>
        /// <param name="y">end point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public SmoothQuadraticBezierCurveTo(Group x, Group y, bool isRelative = false) : this(Convert.ToInt32(x.Value), Convert.ToInt32(y.Value), isRelative) { }
        /// <summary>
        /// Use to draw a smooth quadratic Bézier curve.
        /// </summary>
        /// <param name="x">end point x</param>
        /// <param name="y">end point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public SmoothQuadraticBezierCurveTo(int x, int y, bool isRelative = false)
        {
            X = x;
            Y = y;
            IsRelative = isRelative;
        }

        public override float[] GetVertices()
        {
            List<float> vertices = new List<float>();
            double t;
            float x, y;

            // Only edges
            if (IsRelative)
            {
                for (int i = 0; i < 100; i++) // TODO: Magic value please dont hard code this
                {
                    t = i / 100.0f;

                    x = (float)(Math.Pow((1 - t), 2) * Last.X + 2 * (1 - t) * t * (Last.X + Last.X1) + Math.Pow(t, 2) * (Last.X + X));
                    y = (float)(Math.Pow((1 - t), 2) * Last.Y + 2 * (1 - t) * t * (Last.Y + Last.Y1) + Math.Pow(t, 2) * (Last.Y + Y));

                    vertices.AddRange(new float[] { x, y });
                }
                if (X > Last.X1)
                    Last.X1 += X + Last.X1;
                else if (X < Last.X1)
                    Last.X1 += X - Last.X1;
                else
                    Last.X1 += X;
                Last.X += X;
                Last.Y += Y;
            }
            else
            {
                for (int i = 0; i < 100; i++) // TODO: Magic value please dont hard code this
                {
                    t = i / 100.0f;

                    x = (float)(Math.Pow((1 - t), 2) * Last.X + 2 * (1 - t) * t * Last.X1 + Math.Pow(t, 2) * X);
                    y = (float)(Math.Pow((1 - t), 2) * Last.Y + 2 * (1 - t) * t * Last.Y1 + Math.Pow(t, 2) * Y);

                    vertices.AddRange(new float[] { x, y });
                }
                if (X > Last.X1)
                    Last.X1 = X + Last.X1;
                else if (X < Last.X1)
                    Last.X1 = X - Last.X1;
                else
                    Last.X1 = X;
                Last.X = X;
                Last.Y = Y;
            }

            return vertices.ToArray();
        }
        public override uint[] GetVerticesIndexes() => Enumerable.Range(0, 100).Select(i => (uint)i).ToArray(); // Magic value please dont hard code this

        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new List<Vector2i>();

            if (IsRelative)
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(Last.X + Last.X1, Last.Y + Last.Y1));
                points.Add(new Vector2i(Last.X + X, Last.Y + Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(Last.X1, Last.Y1));
                points.Add(new Vector2i(X, Y));
            }
            return points;
        }

        public override string ToString() => String.Format("{0} {1},{2}", IsRelative ? 't' : 'T', X, Y);

        public override void MoveDelta(Vector2i deltaPosition)
        {
            throw new NotImplementedException();
        }

        public override void ResizeDelta(Vector2i size)
        {
            throw new NotImplementedException();
        }

        public override (Vector2i position, Vector2i? controlPosition) Information()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// EllipticalArc, Elliptical Arc Curve, A or a.
    /// Use to draw an ellipse.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#elliptical_arc_curve
    /// </summary>
    public class EllipticalArc : DrawAttribute
    {
        bool _isRelative;
        int _rx;
        int _ry;
        int _angle;
        bool _largeArcFlag;
        bool _sweepFlag;

        /// <summary>
        /// Use to draw an ellipse. 
        /// </summary>
        /// <param name="rx">radius x of the ellipse</param>
        /// <param name="ry">radius y of the ellipse</param>
        /// <param name="angle">rotation (in degrees) relative to the x-axis</param>
        /// <param name="largeArcFlag">either 0 (large arc) or 1 (small arc)</param>
        /// <param name="sweepFlag">either 0 (clockwise arc) or 1 (counterclockwise arc)</param>
        /// <param name="x">new current point x</param>
        /// <param name="y">new current point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public EllipticalArc(Group rx, Group ry, Group angle, Group largeArcFlag, Group sweepFlag, Group x, Group y, bool isRelative = false) : this(Convert.ToInt32(rx.Value), Convert.ToInt32(ry.Value), Convert.ToInt32(angle.Value), Convert.ToInt32(largeArcFlag.Value) == 0 ? false : true, Convert.ToInt32(sweepFlag.Value) == 0 ? false : true, Convert.ToInt32(x.Value), Convert.ToInt32(y.Value), isRelative) { }
        /// <summary>
        /// Use to draw an ellipse. 
        /// </summary>
        /// <param name="rx">radius x of the ellipse</param>
        /// <param name="ry">radius y of the ellipse</param>
        /// <param name="angle">rotation (in degrees) relative to the x-axis</param>
        /// <param name="largeArcFlag">either 0 (large arc) or 1 (small arc)</param>
        /// <param name="sweepFlag">either 0 (clockwise arc) or 1 (counterclockwise arc)</param>
        /// <param name="x">new current point x</param>
        /// <param name="y">new current point y</param>
        /// <param name="isRelative">if true, the points becames relatives to the last point</param>
        public EllipticalArc(int rx, int ry, int angle, bool largeArcFlag, bool sweepFlag, int x, int y, bool isRelative = false)
        {
            _rx = rx;
            _ry = ry;
            _angle = angle;
            _largeArcFlag = largeArcFlag;
            _sweepFlag = sweepFlag;
            X = x;
            Y = y;
            _isRelative = isRelative;
        }

        public override List<Vector2i> GetSelectablePoints()
        {
            throw new NotImplementedException();
        }

        public override float[] GetVertices()
        {
            throw new NotImplementedException();
        }
        public override uint[] GetVerticesIndexes()
        {
            throw new NotImplementedException();
        }

        public override void MoveDelta(Vector2i deltaPosition)
        {
            throw new NotImplementedException();
        }

        public override void ResizeDelta(Vector2i size)
        {
            throw new NotImplementedException();
        }
        public override (Vector2i position, Vector2i? controlPosition) Information()
        {
            throw new NotImplementedException();
        }
        public override string ToString() => String.Format("{0} {1} {2} {3} {4} {5} {6},{7}", _isRelative ? 'a' : 'A', _rx, _ry, _angle, Convert.ToInt32(_largeArcFlag), Convert.ToInt32(_sweepFlag), X, Y);
    }
    /// <summary>
    /// ClosePath, Z or z.
    /// Use to draw a straight line from the current position to the first point in the path.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#closepath
    /// </summary>
    public class ClosePath : DrawAttribute
    {
        /// <summary>
        /// Draw a straight line from the current position to the first point in the path.
        /// </summary>
        public ClosePath() { }

        public override List<Vector2i> GetSelectablePoints()
        {
            throw new NotImplementedException();
        }

        public override float[] GetVertices()
        {
            throw new NotImplementedException();
        }
        public override uint[] GetVerticesIndexes()
        {
            throw new NotImplementedException();
        }

        public override void MoveDelta(Vector2i deltaPosition)
        {
            throw new NotImplementedException();
        }

        public override void ResizeDelta(Vector2i size)
        {
            throw new NotImplementedException();
        }
        public override (Vector2i position, Vector2i? controlPosition) Information()
        {
            throw new NotImplementedException();
        }
        public override string ToString() => "z";
    }
}
