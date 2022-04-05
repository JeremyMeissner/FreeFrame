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

        public readonly float[] _vertices =
        {
             0.0f, -0.5f, 0.0f, // Top
            -0.5f,  0.5f, 0.0f, // Bottom-Left
             0.5f,  0.5f, 0.0f  // Bottom-Right
        };
        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }
        protected override void OnLoad()
        {
            base.OnLoad();
            _vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

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
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }
        
        protected override void OnUnload()
        {
            base.OnUnload();
        }
    }
}