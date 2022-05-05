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
        enum CreateMode
        {
            Line,
            Rectangle,
            Circle
        }
        int _ioX;
        int _ioY;
        int _ioWidth;
        int _ioHeight;
        System.Numerics.Vector4 _ioColor;
        int _ioTimeline;
        bool _dialogFilePicker = false;
        bool _dialogCompatibility = false;

        SelectorType _selectorType = SelectorType.None;

        Selector _selector;
        Shape? _selectedShape;
        Shape? _selectedShapeBefore;

        UserMode _userMode;
        CreateMode _createMode;

        ImGuiController _ImGuiController;

        List<Shape> _shapes;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            Helper.EnableDebugMode();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f); // TODO: Magic value
            GL.Enable(EnableCap.Multisample);

            _userMode = UserMode.Idle;
            _createMode = CreateMode.Rectangle;

            _shapes = new List<Shape>();

            _selector = new Selector();

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

            //if (_selectedShape != null)
            //    Console.WriteLine("x: {0}; y: {1}; width: {2}; height: {3}", _selectedShape.X, _selectedShape.Y, _selectedShape.Width, _selectedShape.Height);
        }
        /// <summary>
        /// Triggered as often as possible (fps). (Drawing, etc.)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit); // Clear the color


            if (KeyboardState.IsKeyDown(Keys.Escape))
                ResetSelection();

            if (KeyboardState.IsKeyDown(Keys.Delete))
            {
                if (_selectedShape != null)
                {
                    _shapes.Remove(_selectedShape);
                    _selectedShape.DeleteObjects();

                    ResetSelection();
                }
            }

            if (KeyboardState.IsKeyDown(Keys.LeftControl) && KeyboardState.IsKeyDown(Keys.D)) // TODO: Fix duplication
            {
                if (_selectedShape != null)
                {
                    Shape shape = _selectedShape.Clone();
                    _shapes.Add(shape);
                    ResetSelection();
                    _selectedShape = shape;
                }
            }

            if (MouseState.WasButtonDown(MouseButton.Left) == false && MouseState.IsButtonDown(MouseButton.Left) == true) // First left click
                OnLeftMouseDown();
            else if (MouseState.WasButtonDown(MouseButton.Left) == true && MouseState.IsButtonDown(MouseButton.Left) == true) // Long left click
                OnLeftMouseEnter();
            else if (MouseState.WasButtonDown(MouseButton.Left) == true && MouseState.IsButtonDown(MouseButton.Left) == false) // Release left click
                OnLeftMouseUp();


            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            foreach (Shape shape in _shapes)
            {
                // shape.ImplementObject();
                shape.Draw(ClientSize);
            }

            if (_selectedShape != null)
            {
                if (_selectedShape != _selectedShapeBefore) // New shape
                {
                    UpdateIO_UI();
                    _selectedShape.ImplementObject();
                    _selectedShapeBefore = _selectedShape;
                }
                else
                {
                    //_selectedShape.UpdateProperties(properties);
                    if (_selectedShape.X != _ioX ||
                        _selectedShape.Y != _ioY ||
                        _selectedShape.Width != _ioWidth ||
                        _selectedShape.Height != _ioHeight ||
                        _selectedShape.Color != new Color4(_ioColor.X, _ioColor.Y, _ioColor.Z, _ioColor.W))
                    {
                        _selectedShape.X = _ioX;
                        _selectedShape.Y = _ioY;
                        _selectedShape.Width = _ioWidth;
                        _selectedShape.Height = _ioHeight;
                        _selectedShape.Color = new Color4(_ioColor.X, _ioColor.Y, _ioColor.Z, _ioColor.W);

                        _selectedShape.ImplementObject();
                        _selector.Select(_selectedShape);
                    }
                }
            }
            switch (_userMode)
            {
                case UserMode.Edit:
                    _selector.Draw(ClientSize);
                    break;
                case UserMode.Create:
                    break;
                case UserMode.Idle:
                default:
                    break;
            }
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            _ImGuiController.Update(this, (float)e.Time); // TODO: Explain what's the point of this. Also explain why this order is necessary
            //ImGui.ShowDemoWindow();
            ShowUI();
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

            _shapes.ForEach(shape => shape.DeleteObjects());
        }
        /// <summary>
        /// Update shape properties windows using shape properties
        /// </summary>
        public void UpdateIO_UI()
        {
            if (_selectedShape != null)
            {
                _ioX = _selectedShape.X;
                _ioY = _selectedShape.Y;
                _ioWidth = _selectedShape.Width;
                _ioHeight = _selectedShape.Height;
                _ioColor = new System.Numerics.Vector4(_selectedShape.Color.R, _selectedShape.Color.G, _selectedShape.Color.B, _selectedShape.Color.A);
            }
        }
        public void ResetSelection()
        {
            _userMode = UserMode.Idle;
            _selectorType = SelectorType.None;
            _selectedShape = null;
            _selectedShapeBefore = null;

            _ioX = 0;
            _ioY = 0;
            _ioWidth = 0;
            _ioHeight = 0;
            _ioColor = new System.Numerics.Vector4(0);
        }
        public Shape? GetNearestShape(Vector2i currentLocation)
        {
            (Shape? shape, double pythagore) nearest = (null, double.MaxValue);

            foreach (Shape shape in _shapes)
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
                        (bool click, SelectorType? type) = _selector.HitBox(new Vector2i((int)MouseState.X, (int)MouseState.Y));
                        if (click)
                        {
                            Console.WriteLine("Click on selector");
                            _selectorType = type ?? SelectorType.None;
                        }
                        Shape? nearestShape = GetNearestShape(new Vector2i((int)MouseState.X, (int)MouseState.Y));
                        if (nearestShape != null)
                        {
                            if (nearestShape != _selectedShape)
                            {
                                Console.WriteLine("New shape selected -> {0} >> {1}", nearestShape.GetType().Name, nearestShape.ToString()); // TODO: Add a debug mode
                                _userMode = UserMode.Edit;
                                _selector.Select(nearestShape);
                                _selectedShape = nearestShape;
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
                case UserMode.Create:
                    _selectedShape = _createMode switch
                    {
                        CreateMode.Line => new SVGLine((int)MouseState.X, (int)MouseState.Y, (int)MouseState.X, (int)MouseState.Y),
                        CreateMode.Rectangle => new SVGRectangle(0, 0, (int)MouseState.X, (int)MouseState.Y),
                        CreateMode.Circle => new SVGCircle(0, (int)MouseState.X, (int)MouseState.Y),
                        _ => throw new Exception("A create mode need to be selected"),
                    };
                    _shapes.Add(_selectedShape);
                    _userMode = UserMode.Edit;
                    _selectorType = SelectorType.Resize;
                    OnLeftMouseEnter();
                    break;
                case UserMode.Edit:
                    if (_selectedShape != null)
                    {
                        switch (_selectorType)
                        {
                            case SelectorType.Move:
                                if (_selectedShape.Moveable)
                                {
                                    _selectedShape.Move(new Vector2i((int)MouseState.X, (int)MouseState.Y));
                                    _selector.Select(_selectedShape);
                                    UpdateIO_UI();
                                }
                                break;
                            case SelectorType.Resize:
                                if (_selectedShape.Resizeable)
                                {
                                    float width, height;
                                    width = MouseState.X - _selectedShape.X;
                                    height = MouseState.Y - _selectedShape.Y;
                                    if (KeyboardState.IsKeyDown(Keys.LeftShift) || _selectedShape.GetType() == typeof(SVGCircle)) // SHIFT
                                    {
                                        if (width > height)
                                            height = width;
                                        else
                                            width = height;
                                    }
                                    _selectedShape.Resize(new Vector2i((int)width, (int)height));
                                    _selector.Select(_selectedShape);
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
            if (_selectedShape == null || _selectedShape.Moveable == false)
                ImGui.BeginDisabled();
            ImGui.InputInt("X", ref _ioX);
            ImGui.InputInt("Y", ref _ioY);
            if (_selectedShape == null || _selectedShape.Moveable == false)
            {
                ImGui.EndDisabled();
                HelpMarker("This shape is not moveable");
            }

            ImGui.Spacing();
            if (_selectedShape == null || _selectedShape.Resizeable == false)
                ImGui.BeginDisabled();
            if (ImGui.InputInt("Width", ref _ioWidth))
            {
                if (_selectedShape != null) // Constraint verification
                    if (_selectedShape.GetType() == typeof(SVGCircle))
                        _ioHeight = _ioWidth;
            }
            if (ImGui.InputInt("Height", ref _ioHeight))
            {
                if (_selectedShape != null) // Constraint verification
                    if (_selectedShape.GetType() == typeof(SVGCircle))
                        _ioWidth = _ioHeight;
            }
            if (_selectedShape == null || _selectedShape.Resizeable == false)
            {
                ImGui.EndDisabled();
                HelpMarker("This shape is not resizeable");
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.Text("Color");
            ImGui.Spacing();
            if (_selectedShape == null)
                ImGui.BeginDisabled();
            ImGui.ColorEdit4("Color", ref _ioColor);
            if (_selectedShape == null)
                ImGui.EndDisabled();
            ImGui.End();

            // Tree view side
            ImGui.Begin("Tree view", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(200, ClientSize.Y / 2));
            ImGui.SetWindowPos(new System.Numerics.Vector2(ClientSize.X - ImGui.GetWindowWidth(), ClientSize.Y / 2));
            ImGui.Text("Tree View");
            ImGui.Spacing();
            foreach (Shape shape in _shapes)
            {
                if (ImGui.Selectable(String.Format("{0}##{1}", shape.GetType().Name, shape.GetHashCode()), _selectedShape == shape))
                {
                    _selectedShape = shape; // TODO: Impossible to select an element from the Tree View
                    _selector.Select(shape);
                    _userMode = UserMode.Edit;
                    Console.WriteLine("New shape selected through tree view");
                }
            }
            ImGui.End();

            // Animation side
            ImGui.Begin("Animation", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(ClientSize.X / 2, 200));
            ImGui.SetWindowPos(new System.Numerics.Vector2(0, ClientSize.Y - ImGui.GetWindowHeight()));
            ImGui.Text("Animation");
            ImGui.End();


            // Timeline side
            ImGui.Begin("Timeline", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(ClientSize.X / 2 - 200, 200));
            ImGui.SetWindowPos(new System.Numerics.Vector2(ClientSize.X / 2, ClientSize.Y - ImGui.GetWindowHeight()));
            ImGui.Text("Timeline");
            ImGui.Spacing();
            ImGui.SliderInt("(seconds)", ref _ioTimeline, 0, 60);
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
                            _dialogFilePicker = true;
                        if (ImGui.MenuItem("Import a file", "Ctrl+I"))
                        { }
                        ImGui.EndMenu();
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
                    (_shapes, bool compatibilityFlag) = Importer.ImportFromFile(picker.SelectedFile);
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
    }
}