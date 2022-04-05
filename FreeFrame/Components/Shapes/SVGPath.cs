using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace FreeFrame.Components.Shapes
{
    public class SVGPath : Shape
    {
        // https://developer.mozilla.org/en-US/docs/Web/SVG/Tutorial/Paths
        // https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d
        // Bézier Curves and Quadratique
        #region Geometry properties
        private float _Mx;
        private float _My;

        private float _Cx1;
        private float _Cy1;

        private float _Cx2;
        private float _Cy2;

        private float _Cx;
        private float _Cy;
        #endregion


        //public SVGPath(XmlReader reader) : this(){ } # TODO: This constructor
        public SVGPath(float Mx, float My, float Cx1, float Cy1, float Cx2, float Cy2, float Cx, float Cy)
        {
            _Mx = Mx;
            _My = My;
            _Cx1 = Cx1;
            _Cy1 = Cy1;
            _Cx2 = Cx2;
            _Cy2 = Cy2;
            _Cx = Cx;
            _Cy = Cy;
        }
        
        public override string ToString() => $"M: {_Mx} {_My}, C: {_Cx1} {_Cy1}, {_Cx2} {_Cy2}, {_Cx} {_Cy}";
    }
}
