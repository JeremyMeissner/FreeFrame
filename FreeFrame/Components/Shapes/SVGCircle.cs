using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;

namespace FreeFrame.Components.Shapes
{
    class SVGCircle : Shape
    {
        #region Geometry properties
        private float _cx;
        private float _cy;
        private float _r;
        #endregion
        /*
        
        // https://www.w3.org/TR/SVG2/shapes.html#CircleElement
        TODO: take in account the attributes (listed on the doc Basic Shapes)

        private Color _fill;
        private float _fillOpacity;
        private float _opacity;

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
            Convert.ToInt32(reader["cy"]))
        { }
        public SVGCircle() : this(0, 0, 0) { }
        public SVGCircle(float r, float cx, float cy)
        {
            _cx = cx;
            _cy = cy;
            _r = r;
        }

        public override string ToString() => $"cx: {_cx}, cy: {_cy}, r: {_r}";
    }
}
