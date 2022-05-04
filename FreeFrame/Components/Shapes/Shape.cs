using FreeFrame.Components.Shapes.Path;
using MathNet.Numerics.LinearAlgebra;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace FreeFrame.Components.Shapes
{
    public abstract class Shape
    {
        bool _moveable = true;
        bool _resizeable = true;
        #region Common Geometry Properties
        private int _x, _y, _width, _height;
        private Color4 _color;
        #endregion

        protected List<VertexArrayObject> _vaos;
        public virtual int X { get => _x; set => _x = value; }
        public virtual int Y { get => _y; set => _y = value; }
        public virtual int Width { get => _width; set => _width = value; }
        public virtual int Height { get => _height; set => _height = value; }
        public Color4 Color { get => _color; set => _color = value; }
        public bool Moveable { get => _moveable; protected set => _moveable = value; }
        public bool Resizeable { get => _resizeable; protected set => _resizeable = value; }

        List<Shape>[] _timeline;

        public Shape() 
        {
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
        public Shape Clone()
        {
            Shape shape = (Shape)this.MemberwiseClone();
            return shape;
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
        /// <summary>
        /// Reset the vaos and create new ones (use when update any properties of the shape)
        /// </summary>
        public abstract void ImplementObject();
        public abstract void Move(Vector2i position);
        public abstract void Resize(Vector2i size);
        public abstract Hitbox Hitbox();
        public abstract override string ToString();
    }

}
