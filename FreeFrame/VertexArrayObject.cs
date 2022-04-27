using OpenTK.Graphics.OpenGL4;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using FreeFrame.Components.Shapes;

namespace FreeFrame
{
    public class VertexArrayObject
    {
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _indexBufferObject;
        private int _indexCount;
        private PrimitiveType _primitiveType;
        private Shader _shader;

        public VertexArrayObject(PrimitiveType primitiveType)
        {
            _primitiveType = primitiveType;
            _vertexArrayObject = GL.GenVertexArray();
            _vertexBufferObject = GL.GenBuffer();
            _indexBufferObject = GL.GenBuffer();
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
        }
        public VertexArrayObject(float[] vertices, uint[] indexes, PrimitiveType primitiveType) : this(primitiveType)
        {
            ImplementObjects(vertices, indexes);
        }

        public void Draw(Vector2i clientSize)
        {
            _shader.Use();

            // Applied projection matrix
            int uModelToNDC = _shader.GetUniformLocation("u_Model_To_NDC"); // TODO: Don't need to apply projection matrix at each frame I think
            Matrix4 matrix = Matrix4.CreateOrthographicOffCenter(0, clientSize.X, clientSize.Y, 0, -1.0f, 1.0f);
            _shader.SetUniformMat4(uModelToNDC, matrix);

            // Applied common geometry color
            int uColor = _shader.GetUniformLocation("u_Color");
            _shader.SetUniformVec4(uColor, new Vector4(255, 255, 255, 255)); // (Vector4)Color

            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawElements(_primitiveType, _indexCount, DrawElementsType.UnsignedInt, 0);
        }
        public void ImplementObjects(float[] vertices, uint[] indexes)
        {
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
        public void DeleteObjects()
        {
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_indexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            _shader.Delete();
        }
    }
}
