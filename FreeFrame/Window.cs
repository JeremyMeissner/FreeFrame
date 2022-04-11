﻿using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeFrame.Lib.ImGuiTools;
using FreeFrame.Components;
using FreeFrame.Components.Shapes;
using FreeFrame.Lib.FilePicker;

namespace FreeFrame
{
    class Window : GameWindow
    {
        int _ioX;
        int _ioY;
        int _ioWidth;
        int _ioHeight;
        System.Numerics.Vector4 _ioColor;
        int _ioTimeline;
        bool _dialogFilePicker = false;
        bool _dialogCompatibility = false;

        ImGuiController _ImGuiController;

        List<Shape> _shapes;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            Helper.DebugMode();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

            Shape.BindWindow(this);

            //_shapes = Importer.ImportFromFile("test.svg");

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

            if (_shapes != null)
            {

                _shapes[0].UpdateProperties();
                _shapes[0].Draw();
            }

            _ImGuiController.Update(this, (float)e.Time); // TODO: Explain what's the point of this. Also explain why this order is necessary
            //ImGui.ShowDemoWindow();
            ShowUI();
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

            // TODO: Delete the buffer in the Shape class
        }
        public void ShowUI()
        {
            ImGui.GetStyle().WindowRounding = 0.0f;
            ImGui.GetStyle().ScrollbarRounding = 0.0f;
            ImGui.GetStyle().LogSliderDeadzone = 0.0f;
            ImGui.GetStyle().TabRounding = 0.0f;


            ImGui.Begin("Parameters", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(200, ClientSize.Y / 2));
            ImGui.SetWindowPos(new System.Numerics.Vector2(ClientSize.X - ImGui.GetWindowWidth(), 0));

            ImGui.Text("Parameters");
            ImGui.Spacing();
            ImGui.InputInt("X", ref _ioX);
            ImGui.InputInt("Y", ref _ioY);
            ImGui.Spacing();
            ImGui.InputInt("Width", ref _ioWidth);
            ImGui.InputInt("Height", ref _ioHeight);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.Text("Color");
            ImGui.Spacing();
            ImGui.ColorEdit4("Color", ref _ioColor);

            ImGui.End();


            ImGui.Begin("Tree view", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(200, ClientSize.Y / 2));
            ImGui.SetWindowPos(new System.Numerics.Vector2(ClientSize.X - ImGui.GetWindowWidth(), ClientSize.Y / 2));
            ImGui.Text("Tree View");
            ImGui.Spacing();

            ImGui.Selectable("Polygon");
            ImGui.Selectable("Circle", true);
            ImGui.Selectable("Rectangle");
            ImGui.End();


            ImGui.Begin("Animation", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(ClientSize.X / 2, 200));
            ImGui.SetWindowPos(new System.Numerics.Vector2(0, ClientSize.Y - ImGui.GetWindowHeight()));
            ImGui.Text("Animation");
            ImGui.End();


            ImGui.Begin("Timeline", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(ClientSize.X / 2 - 200, 200));
            ImGui.SetWindowPos(new System.Numerics.Vector2(ClientSize.X / 2, ClientSize.Y - ImGui.GetWindowHeight()));
            ImGui.Text("Timeline");
            ImGui.Spacing();
            ImGui.SliderInt("(seconds)", ref _ioTimeline, 0, 60);

            ImGui.End();


            ImGui.Begin("NavBar", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.MenuBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(ClientSize.X - 200, 0));
            ImGui.SetWindowPos(new System.Numerics.Vector2(0, 0));

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open..", "Ctrl+O"))
                    {
                        _dialogFilePicker = true;
                    }
                    if (ImGui.BeginMenu("Save"))
                    {
                        if (ImGui.MenuItem("Save as PNG", "Ctrl+S"))
                        {
                            // Save the current screen
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.MenuItem("Close", "Ctrl+W")) { /* Do stuff */ }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
            if (ImGui.Button("Line")) { /* Do stuff */ }
            ImGui.SameLine();

            if (ImGui.Button("Primitive Shape"))
                ImGui.OpenPopup("primitive_popup");

            if (ImGui.BeginPopup("primitive_popup"))
            {
                ImGui.Text("Select a shape");
                ImGui.Separator();
                if (ImGui.Selectable("Circle")) { /* Do stuff */ }
                if (ImGui.Selectable("Rectangle")) { /* Do stuff */ }
                if (ImGui.Selectable("Triangle")) { /* Do stuff */ }
                ImGui.EndPopup();
            }

            if (_dialogFilePicker)
                ImGui.OpenPopup("open-file");

            if (ImGui.BeginPopupModal("open-file"))
            {
                var picker = FilePicker.GetFilePicker(this, Path.Combine(Environment.CurrentDirectory, "Content/Atlases"), ".svg");
                if (picker.Draw())
                {
                    (_shapes, bool compatibilityFlag) = Importer.ImportFromFile(picker.SelectedFile);
                    _shapes.ForEach(shape => shape.GenerateObjects());
                    FilePicker.RemoveFilePicker(this);
                    if (compatibilityFlag)
                    {
                        _dialogCompatibility = true;
                    }
                    
                }
                _dialogFilePicker = false;
                ImGui.EndPopup();
            }

            if (_dialogCompatibility)
                ImGui.OpenPopup("Compatibility Problem");
            if (ImGui.BeginPopupModal("Compatibility Problem"))
            {
                ImGui.Text("Some SVG elements are not compatible. Go to the list of compatible SVG elements");
                ImGui.Separator();
                if (ImGui.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                    _dialogCompatibility = false;
                }
                ImGui.EndPopup();
            }
            ImGui.End();
        }
    }
}