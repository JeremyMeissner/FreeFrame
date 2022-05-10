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
        bool _isAngleChangeable = true;
        bool _isCornerRadiusChangeable = true;
        #region Common Geometry Properties
        private int _x, _y, _width, _height, _angle, _cornerRadius;
        private Color4 _color;
        Guid _id;
        #endregion

        private List<VertexArrayObject> vaos;
        public virtual int X { get => _x; set => _x = value; }
        public virtual int Y { get => _y; set => _y = value; }
        public virtual int Width { get => _width; set => _width = value; }
        public virtual int Height { get => _height; set => _height = value; }
        public Color4 Color { get => _color; set => _color = value; }
        public bool IsMoveable { get => _isMoveable; protected set => _isMoveable = value; }
        public bool IsResizeable { get => _isResizeable; protected set => _isResizeable = value; }
        public int Angle { get => _angle; set => _angle = value; }
        public bool IsAngleChangeable { get => _isAngleChangeable; set => _isAngleChangeable = value; }
        public bool IsCornerRadiusChangeable { get => _isCornerRadiusChangeable; set => _isCornerRadiusChangeable = value; }
        public int CornerRadius { get => _cornerRadius; set => _cornerRadius = value; }
        public Guid Id { get => _id; private  set => _id = value; }
        public List<VertexArrayObject> Vaos { get => vaos; protected set => vaos = value; }

        public Shape() 
        {
            Vaos = new List<VertexArrayObject>();
            Color = Color4.Black;
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Trigge draw element through OpenGL context
        /// </summary>
        public virtual void Draw(Vector2i clientSize)
        {
            //Console.WriteLine("Draw {0}, {1}, {2}", GetType().Name, Id, GetHashCode());
            foreach (VertexArrayObject vao in Vaos)
                vao.Draw(clientSize, Color, this);
        }
        public void DeleteObjects()
        {
            foreach (VertexArrayObject vao in Vaos)
                vao.DeleteObjects();
            Vaos.Clear();
        }
        public Shape Clone() => (Shape)MemberwiseClone();


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
