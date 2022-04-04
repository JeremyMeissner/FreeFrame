using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace FreeFrame.Components.Shapes
{
    abstract class Shape
    {
        public Shape()
        {

        }

        //TODO: add method to convert shape position and size to NDC
        public abstract override string ToString();
    }
}
