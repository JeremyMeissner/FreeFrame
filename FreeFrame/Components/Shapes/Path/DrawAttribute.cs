using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace FreeFrame.Components.Shapes.Path
{
    /// <summary>
    /// DrawAttribute.
    /// Attribute: d.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d
    /// </summary>
    public abstract class DrawAttribute
    {
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
        int _x;
        int _y;
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
            _x = x;
            _y = y;
            _isRelative = isRelative;
        }
        public override string ToString() => String.Format("{0} {1},{2}", _isRelative ? 'm' : 'M', _x, _y);
    }
    /// <summary>
    /// LineTo, L or l.
    /// Use to draw a straight line from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#lineto_path_commands
    /// </summary>
    public class LineTo : DrawAttribute
    {
        bool _isRelative;
        int _x;
        int _y;
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
            _x = x;
            _y = y;
            _isRelative = isRelative;
        }

        public override string ToString() => String.Format("{0} {1},{2}", _isRelative ? 'l' : 'L', _x, _y);
    }
    /// <summary>
    /// HorizontalLineTo, H or h.
    /// Use to draw a horizontal line from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#lineto_path_commands
    /// </summary>
    public class HorizontalLineTo : DrawAttribute
    {
        bool _isRelative;
        int _x;

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
            _x = x;
            _isRelative = isRelative;
        }
        public override string ToString() => String.Format("{0} {1}", _isRelative ? 'h' : 'H', _x);
    }
    /// <summary>
    /// VerticalLineTo, V or v.
    /// Use to draw a vertical line from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#lineto_path_commands
    /// </summary>
    public class VerticalLineTo : DrawAttribute
    {
        bool _isRelative;
        int _y;

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
            _y = y;
            _isRelative = isRelative;
        }

        public override string ToString() => String.Format("{0} {1}", _isRelative ? 'v' : 'V', _y);
    }
    /// <summary>
    /// CurveTo, Cubic Bézier Curve, C or c.
    /// Use to draw a cubic Bézier curve from the current point to the given point.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#cubic_b%C3%A9zier_curve
    /// </summary>
    public class CurveTo : DrawAttribute
    {
        bool _isRelative;
        int _x1;
        int _y1;
        int _x2;
        int _y2;
        int _x;
        int _y;

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
            _x1 = x1;
            _y1 = y1;
            _x2 = x2;
            _y2 = y2;
            _x = x;
            _y = y;
            _isRelative = isRelative;
        }

        public override string ToString() => String.Format("{0} {1},{2} {3},{4} {5},{6}", _isRelative ? 'c' : 'C', _x1, _y1, _x2, _y2, _x, _y);
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
        int _x;
        int _y;

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
            _x2 = x2;
            _y2 = y2;
            _x = x;
            _y = y;
            _isRelative = isRelative;
        }

        public override string ToString() => String.Format("{0} {1},{2} {3},{4}", _isRelative ? 's' : 'S', _x2, _y2, _x, _y);
    }
    /// <summary>
    /// QuadraticBezierCurveTo, Q, q.
    /// Use to draw a quadratic Bézier curve.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#quadratic_b%C3%A9zier_curve
    /// </summary>
    public class QuadraticBezierCurveTo : DrawAttribute
    {
        bool _isRelative;
        int _x1;
        int _y1;
        int _x;
        int _y;

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
            _x1 = x1;
            _y1 = y1;
            _x = x;
            _y = y;
            _isRelative = isRelative;
        }

        public override string ToString() => String.Format("{0} {1},{2} {3},{4}", _isRelative ? 'q' : 'Q', _x1, _y1, _x, _y);
    }
    /// <summary>
    /// SmoothQuadraticBezierCurveTo, T, t.
    /// Use to draw a smooth quadratic Bézier curve.
    /// More info: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d#quadratic_b%C3%A9zier_curve
    /// </summary>
    public class SmoothQuadraticBezierCurveTo : DrawAttribute
    {
        bool _isRelative;
        int _x;
        int _y;

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
            _x = x;
            _y = y;
            _isRelative = isRelative;
        }

        public override string ToString() => String.Format("{0} {1},{2}", _isRelative ? 't' : 'T', _x, _y);
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
        int _x;
        int _y;

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
            _x = x;
            _y = y;
            _isRelative = isRelative;
        }
        public override string ToString() => String.Format("{0} {1} {2} {3} {4} {5} {6},{7}", _isRelative ? 'a' : 'A', _rx, _ry, _angle, Convert.ToInt32(_largeArcFlag), Convert.ToInt32(_sweepFlag), _x, _y);
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

        public override string ToString() => "z";
    }
}
