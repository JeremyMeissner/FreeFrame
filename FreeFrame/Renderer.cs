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
    public class Renderer
    {
        private int _vertexBufferObjectID;
        private int _vertexArrayObjectID;
        private int _indexBufferObjectID;
        private int _indexCount;
        private PrimitiveType _primitiveType;
        private Shader _shader;

        // VBO
        public int VertexBufferObjectID { get => _vertexBufferObjectID; private set => _vertexBufferObjectID = value; }

        // VAO
        public int VertexArrayObjectID { get => _vertexArrayObjectID; private set => _vertexArrayObjectID = value; }

        // IBO
        public int IndexBufferObjectID { get => _indexBufferObjectID; private set => _indexBufferObjectID = value; }
        

        public Renderer() : this(PrimitiveType.Triangles) { }
        public Renderer(PrimitiveType primitiveType)
        {
            _primitiveType = primitiveType;
            VertexArrayObjectID = GL.GenVertexArray();
            VertexBufferObjectID = GL.GenBuffer();
            IndexBufferObjectID = GL.GenBuffer();
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
        }

        public Renderer(float[] vertices, uint[] indexes, PrimitiveType primitiveType) : this(primitiveType)
        {
            // Create all the objects and index for OpenGL and send data
            ImplementObjects(vertices, indexes);
        }

        public Renderer(float[] vertices, uint[] indexes, PrimitiveType primitiveType, Shape shape) : this(primitiveType)
        {
            Type type = shape.GetType();
            if (type == typeof(SVGCircle)) // Shader depend on the shape
                _shader = new Shader("Shaders/shader.vert", "Shaders/circle.frag"); 
            else if (type == typeof(SVGRectangle))
                _shader = new Shader("Shaders/shader.vert", "Shaders/rectangle.frag");

            // Create all the objects and index for OpenGL and send data
            ImplementObjects(vertices, indexes);
        }
        /// <summary>
        /// Call the Draw method of OpenGL
        /// </summary>
        /// <param name="clientSize">Window size</param>
        /// <param name="color">Element color</param>
        public void Draw(Vector2i clientSize, Color4 color)
        {
            _shader.Use();

            // Applied projection matrix
            int uModelToNDC = _shader.GetUniformLocation("u_Model_To_NDC"); 
            Matrix4 matrix = Matrix4.CreateOrthographicOffCenter(0, clientSize.X, clientSize.Y, 0, -1.0f, 1.0f);
            _shader.SetUniformMat4(uModelToNDC, matrix);

            // Applied common geometry color
            int uColor = _shader.GetUniformLocation("u_Color");
            _shader.SetUniformVec4(uColor, (Vector4)color);

            // Applied window size
            int uResolution = _shader.GetUniformLocation("u_Resolution");
            _shader.SetUniformVec2(uResolution, (Vector2)clientSize);

            // Applied transformation
            int uTransformation = _shader.GetUniformLocation("u_Transformation");
            _shader.SetUniformMat4(uTransformation, Matrix4.Identity);

            // Bind VAO
            GL.BindVertexArray(VertexArrayObjectID);

            // Draw
            GL.DrawElements(_primitiveType, _indexCount, DrawElementsType.UnsignedInt, 0);
        }

        /// <summary>
        /// Call the Draw method of OpenGL
        /// </summary>
        /// <param name="clientSize">Window size</param>
        /// <param name="color">Element color</param>
        /// <param name="shape">Shape type</param>
        public void Draw(Vector2i clientSize, Color4 color, Shape shape)
        {
            _shader.Use();

            // Applied projection matrix
            int uModelToNDC = _shader.GetUniformLocation("u_Model_To_NDC");
            Matrix4 matrix = Matrix4.CreateOrthographicOffCenter(0, clientSize.X, clientSize.Y, 0, -1.0f, 1.0f);
            _shader.SetUniformMat4(uModelToNDC, matrix);

            // Applied common geometry color
            int uColor = _shader.GetUniformLocation("u_Color");
            _shader.SetUniformVec4(uColor, (Vector4)color);

            // Applied window size
            int uResolution = _shader.GetUniformLocation("u_Resolution");
            _shader.SetUniformVec2(uResolution, (Vector2)clientSize);

            // Applied transformation
            int uTransformation = _shader.GetUniformLocation("u_Transformation");
            Matrix4 rotation = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(shape.Angle));
            _shader.SetUniformMat4(uTransformation, rotation);

            // Applied corner radius
            int uRadius = _shader.GetUniformLocation("u_Radius");
            _shader.SetUniformFloat(uRadius, shape.CornerRadius);

            // Applied width
            int uSize = _shader.GetUniformLocation("u_Size");
            _shader.SetUniformVec2(uSize, new Vector2(shape.Width, shape.Height));

            // Applied position
            int uPosition = _shader.GetUniformLocation("u_Position");
            _shader.SetUniformVec2(uPosition, new Vector2(shape.X, shape.Y));

            // Bind VAO
            GL.BindVertexArray(VertexArrayObjectID);

            // Draw
            GL.Enable(EnableCap.Blend);
            GL.DrawElements(_primitiveType, _indexCount, DrawElementsType.UnsignedInt, 0);
            GL.Disable(EnableCap.Blend);
        }
        public void ImplementObjects(float[] vertices, uint[] indexes)
        {
            // VAO
            GL.BindVertexArray(VertexArrayObjectID);

            string label = $"VAO {GetType().Name}:";
            GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, VertexArrayObjectID, label.Length, label);

            // VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObjectID);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            label = $"VBO {GetType().Name}:";
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, VertexBufferObjectID, label.Length, label);

            // IBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferObjectID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indexes.Length * sizeof(uint), indexes, BufferUsageHint.StaticDraw);

            label = $"IBO {GetType().Name}:";
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, IndexBufferObjectID, label.Length, label);

            // Link Attributes
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0); // x, y;
            GL.EnableVertexAttribArray(0);

            _indexCount = indexes.Length;
        }
        /// <summary>
        /// Delete all the buffer objects and shader
        /// </summary>
        public void DeleteObjects()
        {
            GL.DeleteBuffer(VertexBufferObjectID);
            GL.DeleteBuffer(IndexBufferObjectID);
            GL.DeleteVertexArray(VertexArrayObjectID);
            _shader.Delete();
        }
    }
}
