using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeFrame.Lib.ImGuiTools;

namespace FreeFrame
{
    class Window : GameWindow
    {
        int _vertexBuffer;
        int _vertexArray;
        int _indexBuffer;
        Shader _shader;
        ImGuiController _ImGuiController;

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
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            _ImGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            // TODO: map the new NDC to the window

            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            _ImGuiController.WindowResized(ClientSize.X, ClientSize.Y);
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


            _shader.Use(); // Select current shader
            GL.BindVertexArray(_vertexArray); // Bind VAO for workspace (because ImGui binds another)

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            _ImGuiController.Update(this, (float)e.Time); // TODO: Explain what's the point of this. Also explain why this order is necessary
            //ImGui.ShowDemoWindow();
            UI.Show(this);
            _ImGuiController.Render(); // Render ImGui elements

            SwapBuffers();
        }
        
        protected override void OnUnload()
        {
            base.OnUnload();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBuffer);
            GL.DeleteBuffer(_indexBuffer);
            GL.DeleteVertexArray(_vertexArray);
        }
    }
}