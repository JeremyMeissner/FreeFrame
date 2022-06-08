using System;
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
        public const int CURVE_ACCURACY = 100;

        int _x, _y, _x1, _y1 = 0;

        private bool _isRelative;
        public bool IsRelative { get => _isRelative; set => _isRelative = value; }

        // Current X1,Y1 point
        public int Y1 { get => _y1; set => _y1 = value; }
        public int X1 { get => _x1; set => _x1 = value; }

        // Current X,Y point
        public int X { get => _x; set => _x = value; }
        public int Y { get => _y; set => _y = value; }

        // Last drawn values
        public static (int X, int Y, int X1, int Y1) Last = (0, 0, 0, 0);

        /// <summary>
        /// Update the Last drawn values
        /// </summary>
        public abstract void UpdateLast();

        /// <summary>
        /// Return the vertices position in NDC format
        /// </summary>
        /// <returns>array of vertices position. x, y, x, y, ... (clockwise)</returns>
        public abstract float[] GetVertices();

        /// <summary>
        /// Return the indexes position of the triangles/lines
        /// </summary>
        /// <returns>array of indexes</returns>
        public abstract uint[] GetVerticesIndexes();

        /// <summary>
        /// Retrieve the points that made the attribute detectable
        /// </summary>
        /// <returns>Position of all the points</returns>
        public abstract List<Vector2i> GetSelectablePoints();

        /// <summary>
        /// Retrieve the attribute for the d attribute
        /// </summary>
        /// <returns>string of the attribute</returns>
        public abstract override string ToString();
    }

    /// <summary>
    /// MoveTo, M or m.
    /// Moving the current point to another point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#moveto_path_commands
    /// </summary>
    public class MoveTo : DrawAttribute
    {
        public MoveTo() { }
        
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
        public override float[] GetVertices() => Array.Empty<float>(); // Move doesnt have any vertices

        public override uint[] GetVerticesIndexes() => Array.Empty<uint>();

        public override string ToString() => String.Format("{0} {1},{2}", IsRelative ? 'm' : 'M', X, Y);

        public override List<Vector2i> GetSelectablePoints() => new List<Vector2i>();

        public override void UpdateLast()
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
        }
    }

    /// <summary>
    /// LineTo, L or l.
    /// Use to draw a straight line from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#lineto_path_commands
    /// </summary>
    public class LineTo : DrawAttribute
    {
        public LineTo() { }
        
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
                vertices = new float[] { Last.X, Last.Y, Last.X + X, Last.Y + Y };
            else
                vertices = new float[] { Last.X, Last.Y, X, Y };

            return vertices;
        }
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1 };

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

        public override void UpdateLast()
        {
            if (IsRelative)
            {
                Last.X += X; // Update last position
                Last.Y += Y; // Update last position
            }
            else
            {
                Last.X = X; // Update last position
                Last.Y = Y; // Update last position
            }
        }
    }

    /// <summary>
    /// HorizontalLineTo, H or h.
    /// Use to draw a horizontal line from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#lineto_path_commands
    /// </summary>
    public class HorizontalLineTo : DrawAttribute
    {
        public HorizontalLineTo() { }

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
                vertices = new float[] { Last.X, Last.Y, Last.X + X, Last.Y };
            else
                vertices = new float[] { Last.X, Last.Y, X, Last.Y };

            return vertices;
        }

        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1 };

        public override string ToString() => String.Format("{0} {1}", IsRelative ? 'h' : 'H', X);

        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new List<Vector2i>();

            if (IsRelative)
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(Last.X + X, Last.Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(X + X, Last.Y));
            }
            return points;
        }

        public override void UpdateLast()
        {
            if (IsRelative)
                Last.X += X; // Update last position
            else
                Last.X = X; // Update last position
        }
    }

    /// <summary>
    /// VerticalLineTo, V or v.
    /// Use to draw a vertical line from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#lineto_path_commands
    /// </summary>
    public class VerticalLineTo : DrawAttribute
    {
        public VerticalLineTo() { }

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
                vertices = new float[] { Last.X, Last.Y, Last.X, Last.Y + Y };
            else
                vertices = new float[] { Last.X, Last.Y, Last.X, Y };

            return vertices;
        }

        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1 };

        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new();

            if (IsRelative)
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(Last.X, Last.Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(Last.X, Y));
            }
            return points;
        }

        public override string ToString() => String.Format("{0} {1}", IsRelative ? 'v' : 'V', Y);

        public override void UpdateLast()
        {
            if (IsRelative)
                Last.Y += Y; // Update last position
            else
                Last.Y = Y; // Update last position
        }
    }

    /// <summary>
    /// CurveTo, Cubic Bézier Curve, C or c.
    /// Use to draw a cubic Bézier curve from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#cubic_b%C3%A9zier_curve
    /// </summary>
    public class CurveTo : DrawAttribute
    {
        int _x2;
        int _y2;

        public int X2 { get => _x2; set => _x2 = value; }
        public int Y2 { get => _y2; set => _y2 = value; }

        public CurveTo() { }

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
                for (int i = 0; i < CURVE_ACCURACY; i++)
                {
                    t = i / CURVE_ACCURACY;

                    x = (float)(Math.Pow((1 - t), 3) * Last.X + 3 * Math.Pow((1 - t), 2) * t * (Last.X + X1) + 3 * (1 - t) * Math.Pow(t, 2) * (Last.X + X2) + Math.Pow(t, 3) * (Last.X + X));
                    y = (float)(Math.Pow((1 - t), 3) * Last.Y + 3 * Math.Pow((1 - t), 2) * t * (Last.Y + Y1) + 3 * (1 - t) * Math.Pow(t, 2) * (Last.Y + Y2) + Math.Pow(t, 3) * (Last.Y + Y));

                    vertices.AddRange(new float[] { x, y });
                }
            }
            else
            {
                for (int i = 0; i < CURVE_ACCURACY; i++)
                {
                    t = i / CURVE_ACCURACY;

                    x = (float)(Math.Pow((1 - t), 3) * Last.X + 3 * Math.Pow((1 - t), 2) * t * X1 + 3 * (1 - t) * Math.Pow(t, 2) * X2 + Math.Pow(t, 3) * X);
                    y = (float)(Math.Pow((1 - t), 3) * Last.Y + 3 * Math.Pow((1 - t), 2) * t * Y1 + 3 * (1 - t) * Math.Pow(t, 2) * Y2 + Math.Pow(t, 3) * Y);

                    vertices.AddRange(new float[] { x, y });
                }
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
                points.Add(new Vector2i(Last.X + X, Last.Y + Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(X, Y));
            }
            return points;
        }

        public override string ToString() => String.Format("{0} {1},{2} {3},{4} {5},{6}", IsRelative ? 'c' : 'C', X1, Y1, X2, Y2, X, Y);

        public override void UpdateLast()
        {
            if (IsRelative)
            {
                if (X > X2)
                    Last.X1 = X - X2 + X;
                else if (X < X2)
                    Last.X1 = X2 - X + X2;
                else
                    Last.X1 = X2;

                if (Y > Y2)
                    Last.Y1 = Y - Y2 + Y;
                else if (Y < Y2)
                    Last.Y1 = Y2 - Y + Y2;
                else
                    Last.Y1 += Y2;

                Last.X += X;
                Last.Y += Y;
            }
            else
            {
                if (X > X2)
                    Last.X1 = X - X2 + X;
                else if (X < X2)
                    Last.X1 = X2 - X + X2;
                else
                    Last.X1 = X;

                if (Y > Y2)
                    Last.Y1 = Y - Y2 + Y;
                else if (Y < Y2)
                    Last.Y1 = Y2 - Y + Y2;
                else
                    Last.Y1 = Y;

                Last.X = X;
                Last.Y = Y;
            }
        }
    }

    /// <summary>
    /// SmoothCurveTo, Smooth Cubic Bézier Curbe, S or s.
    /// Use to draw a smooth cubic Bézier curve from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#cubic_b%C3%A9zier_curve
    /// </summary>
    public class SmoothCurveTo : DrawAttribute
    {
        int _x2;
        int _y2;

        public int X2 { get => _x2; set => _x2 = value; }
        public int Y2 { get => _y2; set => _y2 = value; }

        public SmoothCurveTo() { }

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
                for (int i = 0; i < CURVE_ACCURACY; i++)
                {
                    t = i / CURVE_ACCURACY;

                    x = (float)(Math.Pow((1 - t), 3) * Last.X + 3 * Math.Pow((1 - t), 2) * t * (Last.X + Last.X1) + 3 * (1 - t) * Math.Pow(t, 2) * (Last.X + X2) + Math.Pow(t, 3) * (Last.X + X));
                    y = (float)(Math.Pow((1 - t), 3) * Last.Y + 3 * Math.Pow((1 - t), 2) * t * (Last.Y + Last.Y1) + 3 * (1 - t) * Math.Pow(t, 2) * (Last.Y + Y2) + Math.Pow(t, 3) * (Last.Y + Y));

                    vertices.AddRange(new float[] { x, y });
                }
            }
            else
            {
                for (int i = 0; i < CURVE_ACCURACY; i++)
                {
                    t = i / CURVE_ACCURACY;

                    x = (float)(Math.Pow((1 - t), 3) * Last.X + 3 * Math.Pow((1 - t), 2) * t * Last.X1 + 3 * (1 - t) * Math.Pow(t, 2) * X2 + Math.Pow(t, 3) * X);
                    y = (float)(Math.Pow((1 - t), 3) * Last.Y + 3 * Math.Pow((1 - t), 2) * t * Last.Y1 + 3 * (1 - t) * Math.Pow(t, 2) * Y2 + Math.Pow(t, 3) * Y);

                    vertices.AddRange(new float[] { x, y });
                }
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
                points.Add(new Vector2i(Last.X + X, Last.Y + Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(X, Y));
            }
            return points;
        }

        public override string ToString() => String.Format("{0} {1},{2} {3},{4}", IsRelative ? 's' : 'S', X2, Y2, X, Y);

        public override void UpdateLast()
        {
            if (IsRelative)
            {
                if (X > X2)
                {
                    Last.X1 = X - X2 + X;
                }
                else if (X < X2)
                    Last.X1 = X2 - X + X2;
                else
                    Last.X1 = X;

                if (Y > Y2)
                    Last.Y1 = Y - Y2 + Y;
                else if (Y < Y2)
                    Last.Y1 = Y2 - Y + Y2;
                else
                    Last.Y1 = Y;

                Last.X += X;
                Last.Y += Y;
            }
            else
            {
                if (X > X2)
                    Last.X1 = X - X2 + X;
                else if (X < X2)
                    Last.X1 = X2 - X + X2;
                else
                    Last.X1 = X;

                if (Y > Y2)
                    Last.Y1 = Y - Y2 + Y;
                else if (Y < Y2)
                    Last.Y1 = Y2 - Y + Y2;
                else
                    Last.Y1 = Y;

                Last.X = X;
                Last.Y = Y;
            }
        }
    }
    /// <summary>
    /// QuadraticBezierCurveTo, Q, q.
    /// Use to draw a quadratic Bézier curve.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#quadratic_b%C3%A9zier_curve
    /// </summary>
    public class QuadraticBezierCurveTo : DrawAttribute
    {
        public QuadraticBezierCurveTo() { }
        
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
            if (IsRelative)
            {
                for (int i = 0; i < CURVE_ACCURACY; i++)
                {
                    t = i / CURVE_ACCURACY;

                    x = (float)(Math.Pow((1 - t), 2) * Last.X + 2 * (1 - t) * t * (Last.X + X1) + Math.Pow(t, 2) * (Last.X + X));
                    y = (float)(Math.Pow((1 - t), 2) * Last.Y + 2 * (1 - t) * t * (Last.Y + Y1) + Math.Pow(t, 2) * (Last.Y + Y));

                    vertices.AddRange(new float[] { x, y });
                }
            }
            else
            {
                for (int i = 0; i < CURVE_ACCURACY; i++)
                {
                    t = i / CURVE_ACCURACY;

                    x = (float)(Math.Pow((1 - t), 2) * Last.X + 2 * (1 - t) * t * X1 + Math.Pow(t, 2) * X);
                    y = (float)(Math.Pow((1 - t), 2) * Last.Y + 2 * (1 - t) * t * Y1 + Math.Pow(t, 2) * Y);

                    vertices.AddRange(new float[] { x, y });
                }
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
                points.Add(new Vector2i(Last.X + X, Last.Y + Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(X, Y));
            }
            return points;
        }

        public override string ToString() => String.Format("{0} {1},{2} {3},{4}", IsRelative ? 'q' : 'Q', X1, Y1, X, Y);

        public override void UpdateLast()
        {
            if (IsRelative)
            {
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
                if (X > X1)
                    Last.X1 = X + X1;
                else if (X < X1)
                    Last.X1 = X - X1;
                else
                    Last.X1 = X;
                Last.X = X;
                Last.Y = Y;
            }
        }
    }
    /// <summary>
    /// SmoothQuadraticBezierCurveTo, T, t.
    /// Use to draw a smooth quadratic Bézier curve.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#quadratic_b%C3%A9zier_curve
    /// </summary>
    public class SmoothQuadraticBezierCurveTo : DrawAttribute
    {
        public SmoothQuadraticBezierCurveTo() { }
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
                for (int i = 0; i < CURVE_ACCURACY; i++)
                {
                    t = i / CURVE_ACCURACY;

                    x = (float)(Math.Pow((1 - t), 2) * Last.X + 2 * (1 - t) * t * (Last.X + Last.X1) + Math.Pow(t, 2) * (Last.X + X));
                    y = (float)(Math.Pow((1 - t), 2) * Last.Y + 2 * (1 - t) * t * (Last.Y + Last.Y1) + Math.Pow(t, 2) * (Last.Y + Y));

                    vertices.AddRange(new float[] { x, y });
                }
            }
            else
            {
                for (int i = 0; i < CURVE_ACCURACY; i++)
                {
                    t = i / CURVE_ACCURACY;

                    x = (float)(Math.Pow((1 - t), 2) * Last.X + 2 * (1 - t) * t * Last.X1 + Math.Pow(t, 2) * X);
                    y = (float)(Math.Pow((1 - t), 2) * Last.Y + 2 * (1 - t) * t * Last.Y1 + Math.Pow(t, 2) * Y);

                    vertices.AddRange(new float[] { x, y });
                }
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
                points.Add(new Vector2i(Last.X + X, Last.Y + Y));
            }
            else
            {
                points.Add(new Vector2i(Last.X, Last.Y));
                points.Add(new Vector2i(X, Y));
            }
            return points;
        }

        public override string ToString() => String.Format("{0} {1},{2}", IsRelative ? 't' : 'T', X, Y);

        public override void UpdateLast()
        {
            if (IsRelative)
            {
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
                if (X > Last.X1)
                    Last.X1 = X + Last.X1;
                else if (X < Last.X1)
                    Last.X1 = X - Last.X1;
                else
                    Last.X1 = X;
                Last.X = X;
                Last.Y = Y;
            }
        }
    }

    /// <summary>
    /// EllipticalArc, Elliptical Arc Curve, A or a.
    /// Use to draw an ellipse.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#elliptical_arc_curve
    /// </summary>
    public class EllipticalArc : DrawAttribute
    {
        int _rx;
        int _ry;
        int _angle;
        bool _largeArcFlag;
        bool _sweepFlag;

        public EllipticalArc() { }

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
            IsRelative = isRelative;
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

        public override string ToString() => String.Format("{0} {1} {2} {3} {4} {5} {6},{7}", IsRelative ? 'a' : 'A', _rx, _ry, _angle, Convert.ToInt32(_largeArcFlag), Convert.ToInt32(_sweepFlag), X, Y);

        public override void UpdateLast()
        {
            throw new NotImplementedException();
        }
    }
}
