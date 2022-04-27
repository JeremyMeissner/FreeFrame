using FreeFrame.Components.Shapes.Path;
using MathNet.Numerics.LinearAlgebra;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace FreeFrame.Components.Shapes
{
    public abstract class Shape
    {
        #region Common Geometry Properties
        public struct DefaultProperties
        {
            public int x;
            public int y;
            public int width;
            public int height;
            public Color4 color;
            public static bool operator == (DefaultProperties p1, DefaultProperties p2) => p1.Equals(p2);
            public static bool operator != (DefaultProperties p1, DefaultProperties p2) => !p1.Equals(p2);
        }
        protected int _propertiesHashCode;
        DefaultProperties _properties;
        #endregion

        protected List<VertexArrayObject> _vaos;
        public DefaultProperties Properties { get => _properties; set => _properties = value; }

        public Shape() 
        {
            Properties = new DefaultProperties()
            {
                x = 0, y = 0, width = 0, height = 0, color = Color4.White
            };
            _vaos = new List<VertexArrayObject>();
        }

        /// <summary>
        /// Trigge draw element through OpenGL context
        /// </summary>
        public abstract void Draw(Vector2i clientSize);
            //if (Window == null)
            //    throw new Exception("Trying to convert to NDC but no Window is binded");

            // Call me using a child that override me

            //GL.DrawElements(PrimitiveType.Lines, _indexCount, DrawElementsType.UnsignedInt, 0);
            //    if (GetType() == typeof(SVGPath))
            //        GL.DrawElements(PrimitiveType.LineStrip, _indexCount, DrawElementsType.UnsignedInt, 0);

            //    // Can't do a switch because a switch need a const and a type is not
            //    if (GetType() == typeof(SVGLine))
            //    {
            //        GL.Enable(EnableCap.LineSmooth);
            //        GL.LineWidth(1.0f); // TODO: Lines are not really great (needed anti aliasing)
            //        GL.DrawElements(PrimitiveType.Lines, _indexCount, DrawElementsType.UnsignedInt, 0);
            //        GL.Disable(EnableCap.LineSmooth);
            //    }
            //    else if (GetType() == typeof(SVGPath))
            //    {
            //        GL.Enable(EnableCap.LineSmooth);
            //        GL.LineWidth(1.0f); // TODO: Lines are not really great (needed anti aliasing)
            //        GL.DrawElements(PrimitiveType.Lines, _indexCount, DrawElementsType.UnsignedInt, 0);
            //        GL.Disable(EnableCap.LineSmooth);
            //    }
            //    else
            //        GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
        public void DeleteObjects()
        {
            foreach (VertexArrayObject vao in _vaos)
                vao.DeleteObjects();
        }
        /// <summary>
        /// Should return the vertices position in NDC format
        /// </summary>
        /// <returns>array of vertices position. x, y, x, y, ... (clockwise)</returns>
        public abstract float[] GetVertices();
        /// <summary>
        /// Should return the indexes position of the triangles
        /// </summary>
        /// <returns>array of indexes</returns>
        public abstract uint[] GetVerticesIndexes();
        public abstract List<Vector2i> GetSelectablePoints();
        public abstract void UpdateProperties(DefaultProperties properties);
        public abstract void ImplementObject();
        public abstract Hitbox Hitbox();
        public abstract override string ToString();
    }

}
