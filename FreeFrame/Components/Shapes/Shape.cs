using FreeFrame.Components.Shapes.Path;
using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace FreeFrame.Components.Shapes
{
    public abstract class Shape : IDrawable
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

        private List<Renderer> renderers;
        public int X { get => _x; set => _x = value; }
        public int Y { get => _y; set => _y = value; }
        public int Width { get => _width; set => _width = Math.Max(0, value); }
        public int Height { get => _height; set => _height = Math.Max(0, value); }
        public Color4 Color { get => _color; set => _color = value; }
        public int Angle { get => _angle; set => _angle = value; }
        public bool IsMoveable { get => _isMoveable; set => _isMoveable = value; }
        public bool IsResizeable { get => _isResizeable; set => _isResizeable = value; }
        public bool IsAngleChangeable { get => _isAngleChangeable; set => _isAngleChangeable = value; }
        public bool IsCornerRadiusChangeable { get => _isCornerRadiusChangeable; set => _isCornerRadiusChangeable = value; }
        public int CornerRadius { get => _cornerRadius; set => _cornerRadius = value; }
        public Guid Id { get => _id; set => _id = value; }
        public List<Renderer> Renderers { get => renderers; set => renderers = value; }
        public string ShortId
        {
            get => Id.ToString().Substring(0, 6);
        }
        public Shape()
        {
            Renderers = new List<Renderer>();
            Color = Color4.Black;
            Id = Guid.NewGuid();
        }

        public virtual void Draw(Vector2i clientSize)
        {
            //Console.WriteLine("Draw {0}, {1}, {2}", GetType().Name, Id, GetHashCode());
            foreach (Renderer vao in Renderers)
                vao.Draw(clientSize, Color, this);
        }
        public void DeleteObjects()
        {
            foreach (Renderer vao in Renderers)
                vao.DeleteObjects();
            Renderers.Clear();
        }
        public Shape ShallowCopy() => (Shape)MemberwiseClone();
        public Shape DeepCopy()
        {
            Shape shape = (Shape)MemberwiseClone();
            shape.Id = Guid.NewGuid();
            shape.DeleteObjects();
            shape.Renderers = new List<Renderer>();
            shape.ImplementObject();
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

        public static string ColorToHexadecimal(Color4 color)
        {
            int r, g, b, a;
            r = (int)(color.R * 255);
            g = (int)(color.G * 255);
            b = (int)(color.B * 255);
            a = (int)(color.A * 255);
            return '#' + r.ToString("X2") + g.ToString("X2") + b.ToString("X2") + a.ToString("X2");
        }

    }

}
