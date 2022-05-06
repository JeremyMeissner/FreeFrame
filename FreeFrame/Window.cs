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
        int _ioAngle;
        int _ioCornerRadius;
        int _ioX;
        int _ioY;
        int _ioWidth;
        int _ioHeight;
        System.Numerics.Vector4 _ioColor;
        int _ioTimeline;
        bool _dialogFilePicker = false;
        bool _dialogCompatibility = false;

        Vector2i _mouseOriginalState;

        SortedDictionary<int, List<Shape>> _timeline;

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

            _mouseOriginalState = new Vector2i(0, 0);

            _timeline = new SortedDictionary<int, List<Shape>>();

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

            Title = String.Format("FreeFrame - x: {0} y: {1}", MouseState.X, MouseState.Y);
            // Update title based on current context
            //if (_selectedShape != null)
            //    Title = String.Format("FreeFrame - Selected: {0} Mode: {1}", _selectedShape.GetType().Name, _userMode.ToString());
            //else
            //    Title = "FreeFrame";

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

            if (KeyboardState.IsKeyDown(Keys.Q))
            {
                //Console.WriteLine("Draw me please");
                foreach (KeyValuePair<int, List<Shape>> shapes in _timeline)
                {
                    foreach (Shape shape in shapes.Value)
                    {
                        shape.ImplementObject();
                        shape.Draw(ClientSize);
                    }
                }
                if (_selectedShape != null)
                    _selectedShape.ImplementObject(); // Reset VAOS for selected shape
            }

            if (KeyboardState.IsKeyDown(Keys.Escape))
                ResetSelection();

            if (KeyboardState.IsKeyDown(Keys.Delete))
            {
                if (_selectedShape != null)
                {
                    RemoveElementInTimeline(_selectedShape);

                    _shapes.Remove(_selectedShape);
                    _selectedShape.DeleteObjects();

                    ResetSelection();
                }
            }

            if (KeyboardState.IsKeyDown(Keys.LeftControl) && KeyboardState.IsKeyDown(Keys.D) && (KeyboardState.WasKeyDown(Keys.LeftControl) == false || KeyboardState.WasKeyDown(Keys.D) == false)) // TODO: Fix duplication
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
                //shape.ImplementObject(); // Reset VAOs
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
                    if (_selectedShape.X != _ioX ||
                        _selectedShape.Y != _ioY ||
                        _selectedShape.Width != _ioWidth ||
                        _selectedShape.Height != _ioHeight ||
                        _selectedShape.Color != new Color4(_ioColor.X, _ioColor.Y, _ioColor.Z, _ioColor.W) ||
                        _selectedShape.Angle != _ioAngle ||
                        _selectedShape.CornerRadius != _ioCornerRadius) // If a properties need to be updated
                    {
                        Console.WriteLine("Update properties");
                        _selectedShape.X = _ioX;
                        _selectedShape.Y = _ioY;
                        _selectedShape.Width = _ioWidth;
                        _selectedShape.Height = _ioHeight;
                        _selectedShape.Color = new Color4(_ioColor.X, _ioColor.Y, _ioColor.Z, _ioColor.W);
                        _selectedShape.Angle = _ioAngle;
                        _selectedShape.CornerRadius = _ioCornerRadius;

                        _selectedShape.ImplementObject();
                        _selector.Select(_selectedShape);
                    }
                }
            }
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

            _ImGuiController.Update(this, (float)e.Time); // TODO: Explain what's the point of this. Also explain why this order is necessary
            ImGui.ShowDemoWindow();
            ShowUI();
            ShowUIDebug();

            _ImGuiController.Render(); // Render ImGui elements

            SwapBuffers();
        }
        public void RemoveElementInTimeline(Shape shapeToRemove)
        {
            List<int> shapesToDelete = new();
            foreach (KeyValuePair<int, List<Shape>> shapes in _timeline) // Remove element in the timeline
            {
                foreach (Shape shape in shapes.Value)
                {
                    if (shape.Id == shapeToRemove.Id)
                    {
                        shape.DeleteObjects();
                        shapes.Value.Remove(shape);
                        break; // Because we know that there is no more of this shape in the current list
                    }
                }
                if (shapes.Value.Count == 0) // Remove shapes in timeline
                    shapesToDelete.Add(shapes.Key);
            }
            foreach (int id in shapesToDelete)
                _timeline.Remove(id);
        }
        public void ResetTimeline()
        {
            foreach (KeyValuePair<int, List<Shape>> shapes in _timeline) // Remove element in the timeline
            {
                foreach (Shape shape in shapes.Value)
                    shape.DeleteObjects();
                shapes.Value.Clear();
            }
            _timeline.Clear();
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
                _ioAngle = _selectedShape.Angle;
                _ioCornerRadius = _selectedShape.CornerRadius;
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
            _ioAngle = 0;
            _ioCornerRadius = 0;
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
                        _mouseOriginalState.X = (int)MouseState.X;
                        _mouseOriginalState.Y = (int)MouseState.Y;

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
                case UserMode.Create: // Create mode
                    _selectedShape = _createMode switch
                    {
                        CreateMode.Line => new SVGLine((int)MouseState.X, (int)MouseState.Y, (int)MouseState.X, (int)MouseState.Y),
                        CreateMode.Rectangle => new SVGRectangle(0, 0, (int)MouseState.X, (int)MouseState.Y),
                        CreateMode.Circle => new SVGCircle(0, (int)MouseState.X, (int)MouseState.Y),
                        _ => throw new Exception("A create mode need to be selected"),
                    };
                    _shapes.Add(_selectedShape);
                    _selectorType = SelectorType.Resize;
                    _userMode = UserMode.Edit;
                    OnLeftMouseEnter(); // Change user mode and call same function in order to switch to edit mode
                    break;
                case UserMode.Edit: // Edit mode
                    if (_selectedShape != null)
                    {
                        switch (_selectorType)
                        {
                            case SelectorType.Move:
                                if (_selectedShape.IsMoveable)
                                {
                                    float x = MouseState.X, y = MouseState.Y;
                                    if (KeyboardState.IsKeyDown(Keys.LeftShift)) // SHIFT
                                    {
                                        if (Math.Abs(_mouseOriginalState.X - x) > Math.Abs(_mouseOriginalState.Y - y))
                                            y = _mouseOriginalState.Y;
                                        else
                                            x = _mouseOriginalState.X;
                                    }
                                    _selectedShape.Move(new Vector2i((int)x, (int)y));
                                    _selector.Select(_selectedShape);
                                    UpdateIO_UI();
                                }
                                break;
                            case SelectorType.Resize:
                                if (_selectedShape.IsResizeable)
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

        public void ShowUIDebug()
        {
            ImGui.Begin("Debug");
            ImGui.Text("Selected shape:");
            if (_selectedShape != null)
            {
                ImGui.Text(string.Format("Name: {0}", _selectedShape.GetType().Name));
                ImGui.Text(string.Format("UID: {0}", _selectedShape.Id));
                ImGui.Text(string.Format("Hash: {0}", _selectedShape.GetHashCode()));
            }
            else
                ImGui.Text("none");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (_selectedShape != null)
            {
                foreach (VertexArrayObject vao in _selectedShape.Vaos)
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
            ImGui.Text("Timeline");
            int numberColumns = 0, numberRows = 0;

            numberColumns = _timeline.Count;
            if (_timeline.Count > 0)
                numberRows = _timeline.Max(i => i.Value != null ? i.Value.Count : 0);

            if (numberColumns > 0)
            {
                if (ImGui.BeginTable("elements", numberColumns, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                {
                    foreach (KeyValuePair<int, List<Shape>> shapes in _timeline)
                        if (shapes.Value != null)
                            ImGui.TableSetupColumn(shapes.Key.ToString());
                    ImGui.TableHeadersRow();

                    for (int row = 0; row < numberRows; row++)
                    {
                        int columnIndex = 0;
                        ImGui.TableNextRow();
                        foreach (KeyValuePair<int, List<Shape>> shapes in _timeline)
                        {
                            if (shapes.Value != null)
                            {
                                if (shapes.Value.Count > row)
                                {
                                    ImGui.TableSetColumnIndex(columnIndex);
                                    ImGui.Text(string.Format("{0}", shapes.Value[row].GetType().Name));
                                    ImGui.Text(string.Format("{0}", shapes.Value[row].Id));
                                    ImGui.Text(string.Format("{0}", shapes.Value[row].GetHashCode()));
                                    foreach (VertexArrayObject vao in shapes.Value[row].Vaos)
                                    {
                                        ImGui.Text(String.Format("VAO: {0}", vao.VertexArrayObjectID));
                                        ImGui.Text(String.Format("VBO: {0}", vao.VertexBufferObjectID));
                                        ImGui.Text(String.Format("IBO: {0}", vao.IndexBufferObjectID));
                                    }
                                }
                                columnIndex++;
                            }
                        }
                    }
                    ImGui.EndTable();
                }
            }
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
            if (_selectedShape == null || _selectedShape.IsMoveable == false)
                ImGui.BeginDisabled();

            // Position parameters
            if (ImGui.BeginTable("Position", 2))
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("X");
                ImGui.SameLine();
                if (_selectedShape == null || _selectedShape.IsMoveable == false)
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
            if (_selectedShape == null || _selectedShape.IsMoveable == false)
                ImGui.EndDisabled();

            ImGui.Spacing();
            if (_selectedShape == null || _selectedShape.IsResizeable == false)
                ImGui.BeginDisabled();

            // Size parameters
            if (ImGui.BeginTable("Size", 2))
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Width");
                ImGui.SameLine();
                if (_selectedShape == null || _selectedShape.IsResizeable == false)
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
                    if (_selectedShape != null) // Constraint verification
                        if (_selectedShape.GetType() == typeof(SVGCircle))
                            _ioHeight = _ioWidth;
                }
                ImGui.TableSetColumnIndex(1);
                ImGui.PushItemWidth(82);
                if (ImGui.InputInt("##Height", ref _ioHeight))
                {
                    if (_selectedShape != null) // Constraint verification
                        if (_selectedShape.GetType() == typeof(SVGCircle))
                            _ioWidth = _ioHeight;
                }

                ImGui.EndTable();
            }
            if (_selectedShape == null || _selectedShape.IsResizeable == false)
                ImGui.EndDisabled();

            // Other parameters
            if (ImGui.BeginTable("Other", 2))
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                if (_selectedShape == null || _selectedShape.IsAngleChangeable == false)
                    ImGui.BeginDisabled();
                ImGui.Text("Angle");
                if (_selectedShape == null || _selectedShape.IsAngleChangeable == false)
                    ImGui.EndDisabled();


                ImGui.TableSetColumnIndex(1);
                if (_selectedShape == null || _selectedShape.IsCornerRadiusChangeable == false)
                    ImGui.BeginDisabled();
                ImGui.Text("Radius");
                if (_selectedShape == null || _selectedShape.IsCornerRadiusChangeable == false)
                    ImGui.EndDisabled();


                ImGui.TableNextRow();


                ImGui.TableSetColumnIndex(0);
                ImGui.PushItemWidth(82);
                if (_selectedShape == null || _selectedShape.IsAngleChangeable == false)
                    ImGui.BeginDisabled();
                ImGui.InputInt("##Angle", ref _ioAngle);
                if (_selectedShape == null || _selectedShape.IsAngleChangeable == false)
                    ImGui.EndDisabled();


                ImGui.TableSetColumnIndex(1);
                ImGui.PushItemWidth(82);
                if (_selectedShape == null || _selectedShape.IsCornerRadiusChangeable == false)
                    ImGui.BeginDisabled();
                if (ImGui.InputInt("##Radius", ref _ioCornerRadius))
                    _ioCornerRadius = Math.Max(_ioCornerRadius, 0);
                if (_selectedShape == null || _selectedShape.IsCornerRadiusChangeable == false)
                    ImGui.EndDisabled();

                ImGui.EndTable();
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
                    _selectedShape = shape;
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
            ImGui.Spacing();


            if (_timeline.Count > 0 && _timeline.ContainsKey(_ioTimeline))
            {
                foreach (Shape shape in _timeline[_ioTimeline])
                {
                    // TODO: Only show the edited fields
                    ImGui.Text(shape.GetType().Name);
                    ImGui.Indent();
                    ImGui.Text(string.Format("X: {0}", shape.X));
                    ImGui.Text(string.Format("Y: {0}", shape.Y));
                    ImGui.Text(string.Format("Width: {0}", shape.Width));
                    ImGui.Text(string.Format("Height: {0}", shape.Height));
                    ImGui.Text(string.Format("Color: {0}", shape.Color.ToString()));
                    ImGui.Unindent();
                }
            }


            ImGui.End();


            // Timeline side
            ImGui.Begin("Timeline", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(ClientSize.X / 2 - 200, 200));
            ImGui.SetWindowPos(new System.Numerics.Vector2(ClientSize.X / 2, ClientSize.Y - ImGui.GetWindowHeight()));
            ImGui.Text("Timeline");
            ImGui.Spacing();
            ImGui.SliderInt("(seconds)", ref _ioTimeline, 0, 60);
            if (_selectedShape != null)
            {
                if (ImGui.Button(String.Format("Create keyframe for {0}", _selectedShape.GetType().Name)))
                {
                    if (_timeline.ContainsKey(_ioTimeline) == false || _timeline[_ioTimeline] == null)
                        _timeline[_ioTimeline] = new List<Shape>();

                    foreach (Shape shape in _timeline[_ioTimeline])
                    {
                        if (shape.Id == _selectedShape.Id)
                        {
                            Console.WriteLine("Already exist");
                            _timeline[_ioTimeline].Remove(shape);
                            break;
                        }
                    }
                    _timeline[_ioTimeline].Add(_selectedShape.Clone());
                }
            }
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
                    ResetSelection();
                    ResetTimeline();
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