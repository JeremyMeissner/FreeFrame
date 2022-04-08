using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;

namespace FreeFrame.Components.Shapes
{
    public class SVGCircle : Shape
    {
        #region Geometry properties
        private int _cx;
        private int _cy;
        private int _r;
        #endregion
        /*
        
        // https://www.w3.org/TR/SVG2/shapes.html#CircleElement
        TODO: take in account the attributes (listed on the doc Basic Shapes)

        private Color _fill;
        private int _fillOpacity;
        private int _opacity;

        public SVGCircle(XmlReader reader)
        {
            for (int i = 0; i < reader.AttributeCount; i++)
            {

            }
        }
        */
        public SVGCircle(XmlReader reader) : this(
            Convert.ToInt32(reader["r"]),
            Convert.ToInt32(reader["cx"]),
            Convert.ToInt32(reader["cy"])) // TODO: Error handler if r, cx or cy are not here
        { }
        public SVGCircle() : this(0, 0, 0) { }
        public SVGCircle(int r, int cx, int cy)
        {
            _cx = cx;
            _cy = cy;
            _r = r;
        }

        public override string ToString() => $"cx: {_cx}, cy: {_cy}, r: {_r}";
    }
}
