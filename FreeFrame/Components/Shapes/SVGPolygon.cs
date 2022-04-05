using System;
using System.Collections.Generic;
using System.Text;

namespace FreeFrame.Components.Shapes
{
    public class SVGPolygon : Shape
    {
        #region Geometry properties
        List<float> points = new List<float>();
        #endregion
        public SVGPolygon()
        {

        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
