using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeFrame
{
    class Window : GameWindow
    {
        int _vertexBuffer;
        int _vertexArray;
        int _indexBuffer;
        int _shader;

        public readonly float[] _vertices =
        {
             0.5f,  0.5f, 0.0f, // Top-Right
             0.5f, -0.5f, 0.0f,  // Bottom-Right
            -0.5f, -0.5f, 0.0f, // Bottom-Left
            -0.5f,  0.5f, 0.0f, // Top-Left
        };

        public readonly uint[] _indices =
        {
            0, 1, 2, // First triangle
            0, 2, 3, // Second triangle
        };

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }
        
        static int CompileShader(string uri, ShaderType type)
        {
            int shader = GL.CreateShader(type);

            GL.ShaderSource(shader, File.ReadAllText(uri)); // Import the source code of the shader
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int compileStatus); // compileStatus is 0 if compile error
            if (compileStatus == 0)
            {
                Console.WriteLine("{0}: {1}", type.ToString(), GL.GetShaderInfoLog(shader));
                throw new Exception(); // TODO: Remove this exception
            }

            return shader;
        }

        static int CreateShader(string uriVertexShader, string uriFragementShader)
        {
            int program = GL.CreateProgram();

            int vertexShader = CompileShader(uriVertexShader, ShaderType.VertexShader);
            int fragmentShader = CompileShader(uriFragementShader, ShaderType.FragmentShader);

            // TODO: Handling error

            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);

            GL.LinkProgram(program); // Put the shaders in their respective processor
            GL.ValidateProgram(program); // Check if everything correct and store the information on the logs 
            // TODO: Explain what's the difference with GL.ShaderInfoLog

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }
        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

            // Vertex Buffer Object
            _vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // Vertex Array Object
            _vertexArray = GL.GenVertexArray(); // TODO: Why a vertex array is necessary? Why not only use VertexAttribPointer?
            GL.BindVertexArray(_vertexArray);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Index Buffer Object
            _indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // Shaders
            _shader = CreateShader("Shaders/shader.vert", "Shaders/shader.frag");
            GL.UseProgram(_shader);
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            // TODO: map the new NDC to the window
        }
        /// <summary>
        /// Triggered at a fixed interval. (Logic, etc.)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }
        /// <summary>
        /// Triggered as often as possible (fps). (Drawing, etc.)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit); // Clear the color

            //GL.UseProgram(_shader);
            //GL.BindVertexArray(_vertexArray);

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }
        
        protected override void OnUnload()
        {
            base.OnUnload();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteProgram(_shader);
            GL.DeleteBuffer(_vertexBuffer);
            GL.DeleteBuffer(_indexBuffer);
            GL.DeleteVertexArray(_vertexArray);
        }
    }
}