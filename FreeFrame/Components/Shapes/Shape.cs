using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;

namespace FreeFrame.Components.Shapes
{
    public abstract class Shape
    {
        static GameWindow? _window;
        private Shader _shader;
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _indexBufferObject;

        private int _indexCount;

        public Shape()
        {
            _vertexArrayObject = GL.GenVertexArray();
            _vertexBufferObject = GL.GenBuffer();
            _indexBufferObject = GL.GenBuffer();
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
        }
        public static void BindWindow(GameWindow window) => _window = window;

        /// <summary>
        /// Convert given vertex position attributes in px to NDC 
        /// </summary>
        /// <param name="vertexPositions">vertex position attribute</param>
        /// <returns>vertex position attribute in NDC</returns>
        /// <exception cref="Exception">Window should be binded before calling</exception>
        protected static float[] ConvertToNDC(params int[] vertexPositions)
        {
            if (_window == null)
                throw new Exception("Trying to convert to NDC but no Window is binded");

            float[] result = new float[vertexPositions.Length];
            for (int i = 0; i < vertexPositions.Length; i+=2)
            {
                result[i] = vertexPositions[i] / (float)_window.ClientSize.X / 2;
                result[i+1] = vertexPositions[i + 1] / (float)_window.ClientSize.Y / 2;
            }
            return result;
        }

        /// <summary>
        /// Add the given vertex and index to the OpenGL buffers. And link those with an VAO.
        /// </summary>
        /// <param name="vertex">vertex array</param>
        /// <param name="index">index array</param>
        protected void ImplementObjects(float[] vertex, uint[] index)
        {
            // VAO
            GL.BindVertexArray(_vertexArrayObject);

            string label = $"VAO {GetType().Name}:";
            GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, _vertexArrayObject, label.Length, label);

            // VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertex.Length * sizeof(float), vertex, BufferUsageHint.StaticDraw);

            label = $"VBO {GetType().Name}:";
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, _vertexBufferObject, label.Length, label);

            // IBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, index.Length * sizeof(uint), index, BufferUsageHint.StaticDraw);

            label = $"IBO {GetType().Name}:";
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, _indexBufferObject, label.Length, label);

            // Link Attributes
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0); // x, y;
            GL.EnableVertexAttribArray(0);

            _indexCount = index.Length;
        }
        /// <summary>
        /// Trigge draw element throw OpenGL context
        /// </summary>
        public void Draw()
        {
            _shader.Use();
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
        }

        /// <summary>
        /// Should update the size and the position to the new Window size
        /// </summary>
        public abstract void UpdateProperties();
        public abstract override string ToString();
    }

}
