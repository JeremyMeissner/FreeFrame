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
using FreeFrame.Components;
using FreeFrame.Components.Shapes;
using FreeFrame.Lib.FilePicker;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static FreeFrame.Selector;
using System.Reflection;
using Emgu.CV;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using AnimatedGif;
using System.Drawing.Drawing2D;

namespace FreeFrame
{
    public class Window : GameWindow
    {
        enum UserMode
        {
            Idle,
            Edit,
            Create
        }
        enum ImportMode
        {
            Add,
            Override
        }
        enum CreateMode
        {
            Line,
            Rectangle,
            Circle
        }
        int _ioAngle;
        int _ioCornerRadius;
        int _ioX;
        int _ioY;
        int _ioWidth;
        int _ioHeight;
        System.Numerics.Vector4 _ioColor;

        System.Numerics.Vector3 _ioBgColor;
        bool _dialogFilePicker = false;
        bool _dialogCompatibility = false;


        Vector2i _mouseOriginalState;


        Timeline _timeline;

        SelectorType _selectorType = SelectorType.None;

        Selector _selector;
        Shape? _selectedShape;
        Shape? _selectedShapeBefore;

        UserMode _userMode;
        CreateMode _createMode;
        ImportMode _importMode;

        ImGuiController _ImGuiController;

        List<Shape> _shapes;

        public Shape? SelectedShape { get => _selectedShape; set => _selectedShape = value; }
        public List<Shape> Shapes { get => _shapes; set => _shapes = value; }

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            //Helper.EnableDebugMode();
            _ioBgColor = new System.Numerics.Vector3(0.1f, 0.1f, 0.1f);
            GL.ClearColor(_ioBgColor.X, _ioBgColor.Y, _ioBgColor.Z, 1.0f);

            GL.Enable(EnableCap.Multisample);

            _userMode = UserMode.Idle;
            _createMode = CreateMode.Rectangle;

            Shapes = new List<Shape>();

            _selector = new Selector();

            _mouseOriginalState = new Vector2i(0, 0);

            _ImGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);

            _timeline = new Timeline();

            // TODO: default values for io 
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

            Title = String.Format("FreeFrame - (height: {0} width: {1}) (x: {2} y: {3})", ClientSize.X, ClientSize.Y, MouseState.X, MouseState.Y);

            if (_userMode == UserMode.Edit && _selectorType == SelectorType.Move)
            {
                float x = MouseState.X, y = MouseState.Y;
                if (KeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    if (Math.Abs(_mouseOriginalState.X - x) > Math.Abs(_mouseOriginalState.Y - y))
                        y = _mouseOriginalState.Y;
                    else
                        x = _mouseOriginalState.X;
                }
                Title += String.Format(" (delta x: {0} delta y: {1})", x - _mouseOriginalState.X, y - _mouseOriginalState.Y);
            }

        }
        /// <summary>
        /// Triggered as often as possible (fps). (Drawing, etc.)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit); // Clear the color

            _timeline.OnRenderFrame(e, this);

            if (KeyboardState.IsKeyDown(Keys.Escape))
                ResetSelection();

            if (KeyboardState.IsKeyDown(Keys.Delete))
            {
                if (SelectedShape != null)
                {
                    _timeline.RemoveElementInTimeline(SelectedShape);
                    //RemoveElementInTimeline(SelectedShape);

                    Shapes.Remove(SelectedShape);
                    SelectedShape.DeleteObjects();

                    ResetSelection();
                }
            }

            if (KeyboardState.IsKeyDown(Keys.LeftControl) && KeyboardState.IsKeyDown(Keys.D) && (KeyboardState.WasKeyDown(Keys.LeftControl) == false || KeyboardState.WasKeyDown(Keys.D) == false)) // TODO: Fix duplication
            {
                if (SelectedShape != null)
                {
                    Shape shape = SelectedShape.DeepCopy();
                    ResetSelection();
                    Shapes.Add(shape);
                    SelectedShape = shape;
                }
            }
            // Change to ButtonPressed instead of was and is
            if (MouseState.WasButtonDown(MouseButton.Left) == false && MouseState.IsButtonDown(MouseButton.Left) == true) // First left click
                OnLeftMouseDown();
            else if (MouseState.WasButtonDown(MouseButton.Left) == true && MouseState.IsButtonDown(MouseButton.Left) == true) // Long left click
                OnLeftMouseEnter();
            else if (MouseState.WasButtonDown(MouseButton.Left) == true && MouseState.IsButtonDown(MouseButton.Left) == false) // Release left click
                OnLeftMouseUp();

            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            foreach (Shape shape in Shapes)
                shape.Draw(ClientSize);

            switch (_userMode)
            {
                case UserMode.Edit:
                    _selector.Draw(ClientSize); // Only draw selector on edit mode
                    break;
                case UserMode.Create:
                    break;
                case UserMode.Idle:
                default:
                    break;
            }
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            if (SelectedShape != null)
            {
                if (SelectedShape != _selectedShapeBefore) // New shape
                {
                    UpdateIO_UI();
                    SelectedShape.ImplementObject();
                    _selectedShapeBefore = SelectedShape;
                }
                else
                {
                    if (SelectedShape.X != _ioX ||
                        SelectedShape.Y != _ioY ||
                        SelectedShape.Width != _ioWidth ||
                        SelectedShape.Height != _ioHeight ||
                        SelectedShape.Color != new Color4(_ioColor.X, _ioColor.Y, _ioColor.Z, _ioColor.W) ||
                        SelectedShape.Angle != _ioAngle ||
                        SelectedShape.CornerRadius != _ioCornerRadius) // If a properties need to be updated
                    {
                        Console.WriteLine("Update properties");
                        SelectedShape.X = _ioX;
                        SelectedShape.Y = _ioY;
                        SelectedShape.Width = _ioWidth;
                        SelectedShape.Height = _ioHeight;
                        SelectedShape.Color = new Color4(_ioColor.X, _ioColor.Y, _ioColor.Z, _ioColor.W);
                        SelectedShape.Angle = _ioAngle;
                        SelectedShape.CornerRadius = _ioCornerRadius;

                        // Update the shape in the timeline
                        _timeline.UpdateShapeInTimeline(SelectedShape);

                        SelectedShape.ImplementObject();
                        _selector.Select(SelectedShape);
                    }
                }
            }

            _ImGuiController.Update(this, (float)e.Time); // TODO: Explain what's the point of this. Also explain why this order is necessary
            ImGui.ShowDemoWindow();
            ShowUI();
            ShowUIDebug();

            _ImGuiController.Render(); // Render ImGui elements

            SwapBuffers();
        }
        protected override void OnUnload()
        {
            base.OnUnload();

            Console.WriteLine("Program stops");

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);

            Shapes.ForEach(shape => shape.DeleteObjects());
        }
        /// <summary>
        /// Update shape properties windows using shape properties
        /// </summary>
        public void UpdateIO_UI()
        {
            if (SelectedShape != null)
            {
                _ioX = SelectedShape.X;
                _ioY = SelectedShape.Y;
                _ioWidth = SelectedShape.Width;
                _ioHeight = SelectedShape.Height;
                _ioColor = new System.Numerics.Vector4(SelectedShape.Color.R, SelectedShape.Color.G, SelectedShape.Color.B, SelectedShape.Color.A);
                _ioAngle = SelectedShape.Angle;
                _ioCornerRadius = SelectedShape.CornerRadius;
            }
        }
        public void ResetSelection()
        {
            _userMode = UserMode.Idle;
            _selectorType = SelectorType.None;
            SelectedShape = null;
            _selectedShapeBefore = null;

            _ioX = 0;
            _ioY = 0;
            _ioWidth = 0;
            _ioHeight = 0;
            _ioColor = new System.Numerics.Vector4(0);
            _ioAngle = 0;
            _ioCornerRadius = 0;
        }
        public Shape? GetNearestShape(Vector2i currentLocation)
        {
            (Shape? shape, double pythagore) nearest = (null, double.MaxValue);

            foreach (Shape shape in Shapes)
            {
                List<Vector2i> points = shape.GetSelectablePoints();

                foreach (Vector2i point in points)
                {
                    double pythagore = Math.Sqrt(Math.Pow(point.X - currentLocation.X, 2) + Math.Pow(point.Y - currentLocation.Y, 2));

                    if (pythagore < nearest.pythagore) // Get the nearest pythagore value
                        nearest = (shape, pythagore);
                }
            }
            return nearest.shape;
        }
        public void OnLeftMouseDown()
        {
            switch (_userMode)
            {
                case UserMode.Idle:
                case UserMode.Edit:
                    if (ImGui.GetIO().WantCaptureMouse == false) // If it's not ImGui click
                    {
                        _mouseOriginalState.X = (int)MouseState.X;
                        _mouseOriginalState.Y = (int)MouseState.Y;

                        (bool click, SelectorType? type) = _selector.HitBox(new Vector2i((int)MouseState.X, (int)MouseState.Y));
                        if (click)
                        {
                            Console.WriteLine("Click on selector");
                            _selectorType = type ?? SelectorType.None;

                            if (_selectorType == SelectorType.Move && SelectedShape != null)
                            {
                                _mouseOriginalState.X = SelectedShape.X;
                                _mouseOriginalState.Y = SelectedShape.Y;
                            }
                        }
                        Shape? nearestShape = GetNearestShape(new Vector2i((int)MouseState.X, (int)MouseState.Y));
                        if (nearestShape != null)
                        {
                            if (nearestShape != SelectedShape)
                            {
                                Console.WriteLine("New shape selected -> {0} >> {1}", nearestShape.GetType().Name, nearestShape.ToString()); // TODO: Add a debug mode
                                _userMode = UserMode.Edit;
                                _selector.Select(nearestShape);
                                SelectedShape = nearestShape;
                            }
                        }
                    }
                    break;
                case UserMode.Create:
                default:
                    break;
            }
        }
        public void OnLeftMouseEnter() // TODO: Rename this
        {
            // Console.WriteLine("Mouse is down and usermode is {0}", _userMode.ToString());
            switch (_userMode)
            {
                case UserMode.Create: // Create mode
                    SelectedShape = _createMode switch
                    {
                        CreateMode.Line => new SVGLine((int)MouseState.X, (int)MouseState.Y, (int)MouseState.X, (int)MouseState.Y, "#000000FF"),
                        CreateMode.Rectangle => new SVGRectangle(0, 0, (int)MouseState.X, (int)MouseState.Y),
                        CreateMode.Circle => new SVGCircle(0, (int)MouseState.X, (int)MouseState.Y, "#000000FF"),
                        _ => throw new Exception("A create mode need to be selected"),
                    };
                    Shapes.Add(SelectedShape);
                    _selectorType = SelectorType.Resize;
                    _userMode = UserMode.Edit;
                    OnLeftMouseEnter(); // Change user mode and call same function in order to switch to edit mode
                    break;
                case UserMode.Edit: // Edit mode
                    if (SelectedShape != null)
                    {
                        switch (_selectorType)
                        {
                            case SelectorType.Move:
                                if (SelectedShape.IsMoveable)
                                {
                                    float x = MouseState.X, y = MouseState.Y;
                                    if (KeyboardState.IsKeyDown(Keys.LeftShift)) // SHIFT
                                    {
                                        if (Math.Abs(_mouseOriginalState.X - x) > Math.Abs(_mouseOriginalState.Y - y))
                                            y = _mouseOriginalState.Y;
                                        else
                                            x = _mouseOriginalState.X;
                                    }
                                    SelectedShape.Move(new Vector2i((int)x, (int)y));

                                    // Update shape in timeline
                                    _timeline.UpdateShapeInTimeline(SelectedShape);

                                    _selector.Select(SelectedShape);
                                    UpdateIO_UI();
                                }
                                break;
                            case SelectorType.Resize:
                                if (SelectedShape.IsResizeable)
                                {
                                    float width, height;
                                    width = MouseState.X - SelectedShape.X;
                                    height = MouseState.Y - SelectedShape.Y;
                                    if (KeyboardState.IsKeyDown(Keys.LeftShift) || SelectedShape.GetType() == typeof(SVGCircle)) // SHIFT
                                    {
                                        if (width > height)
                                            height = width;
                                        else
                                            width = height;
                                    }
                                    SelectedShape.Resize(new Vector2i((int)width, (int)height));

                                    // Update shape in timeline
                                    _timeline.UpdateShapeInTimeline(SelectedShape);

                                    _selector.Select(SelectedShape);
                                    UpdateIO_UI();
                                }
                                break;
                            case SelectorType.Edge:
                            case SelectorType.None:
                            default:
                                break;
                        }
                    }
                    break;
                case UserMode.Idle:
                default:
                    break;
            }

        }
        public void OnLeftMouseUp()
        {
            _selectorType = SelectorType.None;
        }

        public void ShowUIDebug()
        {
            ImGui.Begin("Debug");
            ImGui.Text("Selected shape:");
            if (SelectedShape != null)
            {
                ImGui.Text(string.Format("Name: {0}", SelectedShape.GetType().Name));
                ImGui.Text(string.Format("UID: {0}", SelectedShape.Id));
                ImGui.Text(string.Format("Hash: {0}", SelectedShape.GetHashCode()));
            }
            else
                ImGui.Text("none");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (SelectedShape != null)
            {
                foreach (Renderer vao in SelectedShape.Vaos)
                {
                    ImGui.Text(String.Format("VAO: {0}", vao.VertexArrayObjectID));
                    ImGui.Text(String.Format("VBO: {0}", vao.VertexBufferObjectID));
                    ImGui.Text(String.Format("IBO: {0}", vao.IndexBufferObjectID));
                }
            }
            else
                ImGui.Text("none");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.Text("Context:");
            ImGui.Text(String.Format("User mode: {0}", _userMode.ToString()));
            ImGui.Text(String.Format("Create mode: {0}", _createMode.ToString()));

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            _timeline.DrawDebugUI(this);
            ImGui.End();
        }

        public void ShowUI()
        {
            // ImGui settings
            ImGui.GetStyle().WindowRounding = 0.0f;
            ImGui.GetStyle().ScrollbarRounding = 0.0f;
            ImGui.GetStyle().LogSliderDeadzone = 0.0f;
            ImGui.GetStyle().TabRounding = 0.0f;

            // Parameters side
            ImGui.Begin("Parameters", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(200, ClientSize.Y / 2));
            ImGui.SetWindowPos(new System.Numerics.Vector2(ClientSize.X - ImGui.GetWindowWidth(), 0));

            ImGui.Text("Parameters");
            ImGui.Spacing();
            if (SelectedShape == null || SelectedShape.IsMoveable == false)
                ImGui.BeginDisabled();

            // Position parameters
            if (ImGui.BeginTable("Position", 2))
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("X");
                ImGui.SameLine();
                if (SelectedShape == null || SelectedShape.IsMoveable == false)
                {
                    ImGui.EndDisabled();
                    //HelpMarker("This shape is not moveable");
                    ImGui.BeginDisabled();
                }
                ImGui.TableSetColumnIndex(1);
                ImGui.Text("Y");
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.PushItemWidth(82);
                ImGui.InputInt("##X", ref _ioX);
                ImGui.TableSetColumnIndex(1);
                ImGui.PushItemWidth(82);
                ImGui.InputInt("##Y", ref _ioY);

                ImGui.EndTable();
            }
            if (SelectedShape == null || SelectedShape.IsMoveable == false)
                ImGui.EndDisabled();

            ImGui.Spacing();
            if (SelectedShape == null || SelectedShape.IsResizeable == false)
                ImGui.BeginDisabled();

            // Size parameters
            if (ImGui.BeginTable("Size", 2))
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Width");
                ImGui.SameLine();
                if (SelectedShape == null || SelectedShape.IsResizeable == false)
                {
                    ImGui.EndDisabled();
                    //HelpMarker("This shape is not resizeable");
                    ImGui.BeginDisabled();
                }
                ImGui.TableSetColumnIndex(1);
                ImGui.Text("Height");
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.PushItemWidth(82);
                if (ImGui.InputInt("##Width", ref _ioWidth))
                {
                    if (SelectedShape != null) // Constraint verification
                        if (SelectedShape.GetType() == typeof(SVGCircle))
                            _ioHeight = _ioWidth;
                }
                ImGui.TableSetColumnIndex(1);
                ImGui.PushItemWidth(82);
                if (ImGui.InputInt("##Height", ref _ioHeight))
                {
                    if (SelectedShape != null) // Constraint verification
                        if (SelectedShape.GetType() == typeof(SVGCircle))
                            _ioWidth = _ioHeight;
                }

                ImGui.EndTable();
            }
            if (SelectedShape == null || SelectedShape.IsResizeable == false)
                ImGui.EndDisabled();

            // Other parameters
            if (ImGui.BeginTable("Other", 2))
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                if (SelectedShape == null || SelectedShape.IsAngleChangeable == false)
                    ImGui.BeginDisabled();
                ImGui.Text("Angle");
                if (SelectedShape == null || SelectedShape.IsAngleChangeable == false)
                    ImGui.EndDisabled();


                ImGui.TableSetColumnIndex(1);
                if (SelectedShape == null || SelectedShape.IsCornerRadiusChangeable == false)
                    ImGui.BeginDisabled();
                ImGui.Text("Radius");
                if (SelectedShape == null || SelectedShape.IsCornerRadiusChangeable == false)
                    ImGui.EndDisabled();


                ImGui.TableNextRow();


                ImGui.TableSetColumnIndex(0);
                ImGui.PushItemWidth(82);
                if (SelectedShape == null || SelectedShape.IsAngleChangeable == false)
                    ImGui.BeginDisabled();
                ImGui.InputInt("##Angle", ref _ioAngle);
                if (SelectedShape == null || SelectedShape.IsAngleChangeable == false)
                    ImGui.EndDisabled();


                ImGui.TableSetColumnIndex(1);
                ImGui.PushItemWidth(82);
                if (SelectedShape == null || SelectedShape.IsCornerRadiusChangeable == false)
                    ImGui.BeginDisabled();
                if (ImGui.InputInt("##Radius", ref _ioCornerRadius))
                    _ioCornerRadius = Math.Max(_ioCornerRadius, 0);
                if (SelectedShape == null || SelectedShape.IsCornerRadiusChangeable == false)
                    ImGui.EndDisabled();

                ImGui.EndTable();
            }

            ImGui.Spacing();

            if (SelectedShape == null)
                ImGui.BeginDisabled();
            ImGui.Text("Color");
            ImGui.ColorEdit4("##Color", ref _ioColor);
            if (SelectedShape == null)
                ImGui.EndDisabled();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.Text("Background Color");
            ImGui.Spacing();
            if (ImGui.ColorEdit3("##BgColor", ref _ioBgColor))
                GL.ClearColor(_ioBgColor.X, _ioBgColor.Y, _ioBgColor.Z, 1.0f);
            ImGui.End();

            // Tree view side
            ImGui.Begin("Tree view", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(200, ClientSize.Y / 2));
            ImGui.SetWindowPos(new System.Numerics.Vector2(ClientSize.X - ImGui.GetWindowWidth(), ClientSize.Y / 2));
            ImGui.Text("Tree View");
            ImGui.Spacing();
            for (int i = Shapes.Count - 1; i >= 0; i--)
            {
                if (ImGui.Selectable(String.Format("{0} - {1}##{2}", Shapes[i].ShortId, Shapes[i].GetType().Name, Shapes[i].GetHashCode()), SelectedShape == Shapes[i]))
                {
                    SelectedShape = Shapes[i];
                    _selector.Select(Shapes[i]);
                    _userMode = UserMode.Edit;
                    Console.WriteLine("New shape selected through tree view");
                }
                if (i - 1 >= 0)
                {
                    if (ImGui.ArrowButton(String.Format("Down##d{0}", i), ImGuiDir.Down))
                    {
                        InvertShape(i, i - 1);
                        SelectShape(Shapes[i - 1]);
                    }
                }
                if (i + 1 < Shapes.Count)
                {
                    if (i - 1 >= 0)
                        ImGui.SameLine();
                    if (ImGui.ArrowButton(String.Format("Up##u{0}", i), ImGuiDir.Up))
                    {
                        InvertShape(i, i + 1);
                        SelectShape(Shapes[i + 1]);
                    }
                }
                ImGui.Separator();
            }
            ImGui.End();

            // Animation side
            ImGui.Begin("Animation", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(ClientSize.X / 2, 200));
            ImGui.SetWindowPos(new System.Numerics.Vector2(0, ClientSize.Y - ImGui.GetWindowHeight()));
            _timeline.DrawAnimationList(this);
            ImGui.End();


            // Timeline side
            ImGui.Begin("Timeline", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(ClientSize.X / 2 - 200, 200));
            ImGui.SetWindowPos(new System.Numerics.Vector2(ClientSize.X / 2, ClientSize.Y - ImGui.GetWindowHeight()));
            _timeline.DrawUI(this);
            ImGui.End();


            // Navbar side
            ImGui.Begin("NavBar", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.MenuBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(ClientSize.X - 200, 0));
            ImGui.SetWindowPos(new System.Numerics.Vector2(0, 0));

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.BeginMenu("Open"))
                    {
                        if (ImGui.MenuItem("New project", "Ctrl+O"))
                        {
                            _dialogFilePicker = true;
                            _importMode = ImportMode.Override;
                        }
                        if (ImGui.MenuItem("Import a file", "Ctrl+I"))
                        {
                            _dialogFilePicker = true;
                            _importMode = ImportMode.Add;
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("Save"))
                    {
                        if (ImGui.MenuItem("Save as SVG", "Ctrl+S"))
                            SaveCurrentScreenToSVG();
                        if (ImGui.MenuItem("Save as MP4"))
                            SaveCurrentScreenToMP4();
                        if (ImGui.MenuItem("Save as GIF"))
                            SaveCurrentScreenToGIF();
                        if (ImGui.MenuItem("Save as PNG"))
                            SaveCurrentScreenToPNG();
                        ImGui.EndMenu();
                    }
                    if (ImGui.MenuItem("Close", "Ctrl+W")) { /* Do stuff */ }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
            if (ImGui.Button("Line"))
            {
                _userMode = UserMode.Create;
                _createMode = CreateMode.Line;
            }
            ImGui.SameLine();

            if (ImGui.Button("Primitive Shape"))
                ImGui.OpenPopup("primitive_popup");

            if (ImGui.BeginPopup("primitive_popup"))
            {
                ImGui.Text("Select a shape");
                ImGui.Separator();
                if (ImGui.Selectable("Circle"))
                {
                    _userMode = UserMode.Create;
                    _createMode = CreateMode.Circle;
                }
                if (ImGui.Selectable("Rectangle"))
                {
                    _userMode = UserMode.Create;
                    _createMode = CreateMode.Rectangle;
                }
                if (ImGui.Selectable("Triangle")) { /* Do stuff */ }
                ImGui.EndPopup();
            }

            // File picker dialog
            if (_dialogFilePicker)
                ImGui.OpenPopup("open-file");
            if (ImGui.BeginPopupModal("open-file")) // ImGuiWindowFlags.AlwaysAutoResize
            {
                var picker = FilePicker.GetFilePicker(this, Path.Combine(Environment.CurrentDirectory, "Content/Atlases"), ".svg");
                if (picker.Draw())
                {
                    ResetSelection();
                    bool compatibilityFlag;

                    switch (_importMode)
                    {
                        case ImportMode.Add:
                            (List<Shape> newShapes, compatibilityFlag) = Importer.ImportFromFile(picker.SelectedFile);
                            Shapes.AddRange(newShapes);
                            break;
                        case ImportMode.Override:
                        default:
                            (Shapes, compatibilityFlag) = Importer.ImportFromFile(picker.SelectedFile);
                            _timeline.ResetTimeline();
                            break;
                    }
                    FilePicker.RemoveFilePicker(this);
                    if (compatibilityFlag)
                        _dialogCompatibility = true;

                }
                _dialogFilePicker = false;
                ImGui.EndPopup();
            }

            // Compatibility alert
            if (_dialogCompatibility)
                ImGui.OpenPopup("Compatibility Problem");
            if (ImGui.BeginPopupModal("Compatibility Problem")) // ImGuiWindowFlags.AlwaysAutoResize
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
        public void SelectShape(Shape shape)
        {
            SelectedShape = shape;
            _selector.Select(shape);
            _userMode = UserMode.Edit;
        }
        public void InvertShape(int index1, int index2)
        {
            Shape tmpShape = Shapes[index2].ShallowCopy();
            Shapes[index2] = Shapes[index1].ShallowCopy();
            Shapes[index1] = tmpShape;
        }

        static void HelpMarker(string desc)
        {
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(desc);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        public void SaveCurrentScreenToMP4()
        {
            using VideoWriter w = new VideoWriter("output.mp4", _timeline.IoFps, new System.Drawing.Size(ClientSize.X, ClientSize.Y), true);
            for (int i = Timeline.MIN_TIMELINE; i <= Timeline.MAX_TIMELINE; i++) // TODO: please dont hardcode this
            {
                RenderFrameBySecondIndex(i);
                w.Write(TakeSnap().ToMat());
            }
        }
        public void SaveCurrentScreenToGIF()
        {
            using (var gif = AnimatedGif.AnimatedGif.Create("output.gif", 1000 / _timeline.IoFps))
            {
                for (int i = Timeline.MIN_TIMELINE; i <= Timeline.MAX_TIMELINE; i++) // TODO: please dont hardcode this
                {
                    RenderFrameBySecondIndex(i);
                    gif.AddFrame(TakeSnap(), delay: -1, quality: GifQuality.Bit8);
                }
            }
        }
        public void SaveCurrentScreenToSVG()
        {
            Importer.ExportToFile(Shapes, ClientSize);
        }
        public void SaveCurrentScreenToPNG()
        {
            RenderFrameBySecondIndex(_timeline.IoTimeline);
            TakeSnap().Save("output.png", ImageFormat.Png);
        }

        // [SupportedOSPlatform("windows")]
        public void RenderFrameBySecondIndex(int second)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit); // Clear the colorV
            ResetSelection();
            _timeline.IoTimeline = second;
            _timeline.RenderInterpolation(this);
            foreach (Shape shape in Shapes)
                shape.Draw(ClientSize);
        }
        public Bitmap TakeSnap()
        {
            //Matrix matrix = new Matrix();

            //matrix.Scale(1, -1);

            //return Bitmap

            Console.WriteLine("Snap time");

            Bitmap bmp = new Bitmap(ClientSize.X, ClientSize.Y);

            // Lock the bits
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, ClientSize.X, ClientSize.Y), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            // Fill with current window
            GL.ReadPixels(0, 0, ClientSize.X, ClientSize.Y, OpenTK.Graphics.OpenGL4.PixelFormat.Bgr, PixelType.UnsignedByte, bmpData.Scan0);

            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}