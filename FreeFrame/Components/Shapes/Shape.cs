using OpenTK.Mathematics;

namespace FreeFrame.Components.Shapes
{
    public abstract class Shape : IDrawable
    {
        #region Default values
        public const string DefaultColor = "#000000FF";
        #endregion

        #region Common Geometry Properties
        private int _x, _y, _width, _height, _angle, _cornerRadius;
        private Color4 _color;
        Guid _id;
        bool _isMoveable = true;
        bool _isResizeable = true;
        bool _isAngleChangeable = true;
        bool _isCornerRadiusChangeable = true;
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

        /// <summary>
        /// Retrieve the Id but shorter (only used for display)
        /// </summary>
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

        /// <summary>
        /// Trigger draw element through OpenGL context
        /// </summary>
        /// <param name="clientSize">Window size</param>
        public virtual void Draw(Vector2i clientSize)
        {
            foreach (Renderer render in Renderers)
                render.Draw(clientSize, Color, this);
        }

        /// <summary>
        /// Deletes the objects saved in OpenGL context
        /// </summary>
        public void DeleteObjects()
        {
            foreach (Renderer render in Renderers)
                render.DeleteObjects();
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
        /// Return the vertices position in NDC format
        /// </summary>
        /// <returns>array of vertices position. x, y, x, y, ... (clockwise)</returns>
        public abstract float[] GetVertices();

        /// <summary>
        /// Return the indexes position of the triangles
        /// </summary>
        /// <returns>array of indexes</returns>
        public abstract uint[] GetVerticesIndexes();

        /// <summary>
        /// Retrieve the points that made the shape detectable
        /// </summary>
        /// <returns>Position of all the points</returns>
        public abstract List<Vector2i> GetSelectablePoints();

        /// <summary>
        /// Reset the renderers and create new ones (use when update any properties of the shape)
        /// </summary>
        public abstract void ImplementObject();

        /// <summary>
        /// Move the current shape to the given position
        /// </summary>
        /// <param name="position">New position</param>
        public abstract void Move(Vector2i position);

        /// <summary>
        /// Resize the current shape to the given size
        /// </summary>
        /// <param name="size">New size</param>
        public abstract void Resize(Vector2i size);

        /// <summary>
        /// Retrieve the Shape in the SVG format
        /// </summary>
        /// <returns>string of the SVG format</returns>
        public abstract override string ToString();

        /// <summary>
        /// Convert the given Color4 into a hexadecimal string
        /// </summary>
        /// <param name="color">Color to convert</param>
        /// <returns>format: #XXXXXXXX</returns>
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
