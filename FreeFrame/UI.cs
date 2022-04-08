using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeFrame
{
    /// <summary>
    /// UI ImGui Handler
    /// </summary>
    static class UI
    {
        static int _ioX;
        static int _ioY;
        static int _ioWidth;
        static int _ioHeight;
        static System.Numerics.Vector4 _ioColor;
        static int _ioTimeline;

        // TODO: Make multiple UI maybe?

        /// <summary>
        /// Show a default UI using ImGui context
        /// </summary>
        /// <param name="window"></param>
        public static void Show(GameWindow window)
        {
            //if (ImGui.Button("Import asset"))
            //{
            //    ImGui.OpenPopup("open-file");
            //}

            //if (ImGui.BeginPopupModal("open-file"))
            //{
            //    // Open File Dialog
            //    ImGui.EndPopup();
            //}

            ImGui.GetStyle().WindowRounding = 0.0f;
            ImGui.GetStyle().ScrollbarRounding = 0.0f;
            ImGui.GetStyle().LogSliderDeadzone = 0.0f;
            ImGui.GetStyle().TabRounding = 0.0f;


            ImGui.Begin("Parameters", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(200, window.ClientSize.Y / 2));
            ImGui.SetWindowPos(new System.Numerics.Vector2(window.ClientSize.X - ImGui.GetWindowWidth(), 0));

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
            ImGui.SetWindowSize(new System.Numerics.Vector2(200, window.ClientSize.Y / 2));
            ImGui.SetWindowPos(new System.Numerics.Vector2(window.ClientSize.X - ImGui.GetWindowWidth(), window.ClientSize.Y / 2));
            ImGui.Text("Tree View");
            ImGui.Spacing();

            ImGui.Selectable("Polygon");
            ImGui.Selectable("Circle", true);
            ImGui.Selectable("Rectangle");
            ImGui.End();


            ImGui.Begin("Animation", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(window.ClientSize.X / 2, 200));
            ImGui.SetWindowPos(new System.Numerics.Vector2(0, window.ClientSize.Y - ImGui.GetWindowHeight()));
            ImGui.Text("Animation");
            ImGui.End();


            ImGui.Begin("Timeline", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(window.ClientSize.X / 2 - 200, 200));
            ImGui.SetWindowPos(new System.Numerics.Vector2(window.ClientSize.X / 2, window.ClientSize.Y - ImGui.GetWindowHeight()));
            ImGui.Text("Timeline");
            ImGui.Spacing();
            ImGui.SliderInt("(seconds)", ref _ioTimeline, 0, 60);
            ImGui.End();


            ImGui.Begin("NavBar", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.MenuBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(window.ClientSize.X - 200, 0));
            ImGui.SetWindowPos(new System.Numerics.Vector2(0, 0));

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open..", "Ctrl+O")) { /* Do stuff */ }
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

            ImGui.End();
        }
    }
}
