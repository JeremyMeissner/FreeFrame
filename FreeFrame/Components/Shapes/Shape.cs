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
        private static Window? _window;
        private Shader _shader;
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _indexBufferObject;

        private int _indexCount;

        public Color4 Color { get => _color; set => _color = value; }

        public Shape() { }
        public static void BindWindow(GameWindow window) => _window = (Window)window;
        public void GenerateObjects()
        {
            _vertexArrayObject = GL.GenVertexArray();
            _vertexBufferObject = GL.GenBuffer();
            _indexBufferObject = GL.GenBuffer();
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
        }

        /// <summary>
        /// Add the given vertex and index to the OpenGL buffers. And link those with an VAO.
        /// </summary>
        /// <param name="vertex">vertex array</param>
        /// <param name="index">index array</param>
        public void ImplementObjects()
        {
            //float[] vertices =
            //{
            //    0.5f, 0.5f, // Top-Right
            //    0.5f, 0.0f,  // Bottom-Right
            //    0.0f, 0.0f, // Bottom-Left
            //    0.0f, 0.5f, // Top-Left
            //};

            //uint[] indexes =
            //{
            //    0, 1, 2, // First triangle
            //    0, 2, 3, // Second triangle
            //};

            float[] vertices = GetVertices();
            uint[] indexes = GetVerticesIndexes();

            // VAO
            GL.BindVertexArray(_vertexArrayObject);

            string label = $"VAO {GetType().Name}:";
            GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, _vertexArrayObject, label.Length, label);

            // VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            label = $"VBO {GetType().Name}:";
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, _vertexBufferObject, label.Length, label);

            // IBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indexes.Length * sizeof(uint), indexes, BufferUsageHint.StaticDraw);

            label = $"IBO {GetType().Name}:";
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, _indexBufferObject, label.Length, label);

            // Link Attributes
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0); // x, y;
            GL.EnableVertexAttribArray(0);

            _indexCount = indexes.Length;
        }

        /// <summary>
        /// Trigge draw element throw OpenGL context
        /// </summary>
        public void Draw()
        {
            if (_window == null)
                throw new Exception("Trying to convert to NDC but no Window is binded");

            _shader.Use();

            // Applied projection matrix
            int uModelToNDC = _shader.GetUniformLocation("u_Model_To_NDC"); // TODO: Don't need to apply projection matrix at each frame I think
            Matrix4 matrix = Matrix4.CreateOrthographicOffCenter(0, _window.ClientSize.X, _window.ClientSize.Y, 0, -1.0f, 1.0f);
            _shader.SetUniformMat4(uModelToNDC, matrix);

            // Applied common geometry color
            int uColor = _shader.GetUniformLocation("u_Color");
            _shader.SetUniformVec4(uColor, (Vector4)Color);

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
        }
        public void DeleteObjects()
        {
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_indexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            _shader.Delete();
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
