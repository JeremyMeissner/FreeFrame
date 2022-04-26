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
        Color4 _color = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
        #endregion

        protected List<VertexArrayObject> _vaos;

        private static Window? _window;

        public Color4 Color { get => _color; set => _color = value; }
        public static Window? Window { get => _window; set => _window = value; }

        public Shape() { }
        public void GenerateObjects()
        {
            _vaos = new List<VertexArrayObject>();
        }

        /// <summary>
        /// Add the given vertex and index to the OpenGL buffers. And link those with an VAO.
        /// </summary>
        /// <param name="vertex">vertex array</param>
        /// <param name="index">index array</param>
        public void ImplementObjects()
        {
        }

        /// <summary>
        /// Trigge draw element through OpenGL context
        /// </summary>
        public virtual void Draw(Vector2i clientSize)
        {
            if (Window == null)
                throw new Exception("Trying to convert to NDC but no Window is binded");

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
        }
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
        public abstract Hitbox Hitbox();
        public abstract override string ToString();
    }

}
