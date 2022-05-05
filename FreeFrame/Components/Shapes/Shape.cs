using FreeFrame.Components.Shapes.Path;
using MathNet.Numerics.LinearAlgebra;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace FreeFrame.Components.Shapes
{
    public abstract class Shape
    {
        bool _isMoveable = true;
        bool _isResizeable = true;
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
        public bool IsMoveable { get => _isMoveable; protected set => _isMoveable = value; }
        public bool IsResizeable { get => _isResizeable; protected set => _isResizeable = value; }

        List<Shape>[] _timeline;

        public Shape() 
        {
            _vaos = new List<VertexArrayObject>();
            Color = Color4.Black;
        }

        /// <summary>
        /// Trigge draw element through OpenGL context
        /// </summary>
        public virtual void Draw(Vector2i clientSize)
        {
            foreach (VertexArrayObject vao in _vaos)
                vao.Draw(clientSize, Color, this);
        }
        public void DeleteObjects()
        {
            foreach (VertexArrayObject vao in _vaos)
                vao.DeleteObjects();
            _vaos.Clear();
        }
        public Shape Clone()
        {
            Shape shape = (Shape)MemberwiseClone();
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
        public abstract override string ToString();
    }

}
