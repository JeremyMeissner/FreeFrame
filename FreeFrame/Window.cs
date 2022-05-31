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
            Circle,
            Triangle
        }
        int _ioAngle;
        int _ioCornerRadius;
        int _ioX;
        int _ioY;
        int _ioWidth;
        int _ioHeight;
        int _ioTimeline;
        int _ioFps;
        bool _ioIsPlaying;
        System.Numerics.Vector4 _ioColor;

        System.Numerics.Vector3 _ioBgColor;
        bool _dialogFilePicker = false;
        bool _dialogCompatibility = false;
        bool _dialogError = false;


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

            _ioFps = Timeline.DEFAULT_FPS;

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

            _ioTimeline = _timeline.TimelineIndex;

            if (KeyboardState.IsKeyDown(Keys.Escape))
                ResetSelection();

            if (KeyboardState.IsKeyDown(Keys.Delete))
            {
                if (_selectedShape != null)
                {
                    _timeline.RemoveElementInTimeline(_selectedShape);
                    //RemoveElementInTimeline(SelectedShape);

                    Shapes.Remove(_selectedShape);
                    _selectedShape.DeleteObjects();

                    ResetSelection();
                }
            }

            if (KeyboardState.IsKeyDown(Keys.LeftControl) && KeyboardState.IsKeyDown(Keys.D) && (KeyboardState.WasKeyDown(Keys.LeftControl) == false || KeyboardState.WasKeyDown(Keys.D) == false)) // TODO: Fix duplication
            {
                if (_selectedShape != null)
                {
                    Shape shape = _selectedShape.DeepCopy();
                    ResetSelection();
                    Shapes.Add(shape);
                    _selectedShape = shape;
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
                        _selectedShape.CornerRadius != _ioCornerRadius) // If a property needs to be updated
                    {
                        _selectedShape.X = _ioX;
                        _selectedShape.Y = _ioY;
                        _selectedShape.Width = _ioWidth;
                        _selectedShape.Height = _ioHeight;
                        _selectedShape.Color = new Color4(_ioColor.X, _ioColor.Y, _ioColor.Z, _ioColor.W);
                        _selectedShape.Angle = _ioAngle;
                        _selectedShape.CornerRadius = _ioCornerRadius;

                        // Update the shape in the timeline
                        _timeline.UpdateShapeInTimeline(_selectedShape);

                        _selectedShape.ImplementObject();
                        _selector.Select(_selectedShape);
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
        private void OnLeftMouseDown()
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

                            if (_selectorType == SelectorType.Move && _selectedShape != null)
                            {
                                _mouseOriginalState.X = _selectedShape.X;
                                _mouseOriginalState.Y = _selectedShape.Y;
                            }
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
        private void OnLeftMouseEnter() // TODO: Rename this
        {
            // Console.WriteLine("Mouse is down and usermode is {0}", _userMode.ToString());
            switch (_userMode)
            {
                case UserMode.Create: // Create mode
                    _selectedShape = _createMode switch
                    {
                        CreateMode.Line => new SVGLine((int)MouseState.X, (int)MouseState.Y, (int)MouseState.X, (int)MouseState.Y, "#000000FF"),
                        CreateMode.Rectangle => new SVGRectangle(0, 0, (int)MouseState.X, (int)MouseState.Y),
                        CreateMode.Circle => new SVGCircle(0, (int)MouseState.X, (int)MouseState.Y, "#000000FF"),
                        CreateMode.Triangle => new SVGPolygon((int)MouseState.X, (int)MouseState.Y, 0, 0, "#000000FF"),
                        _ => throw new Exception("A create mode need to be selected"),
                    };
                    Shapes.Add(_selectedShape);
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

                                    // Update shape in timeline
                                    _timeline.UpdateShapeInTimeline(_selectedShape);

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

                                    // Update shape in timeline
                                    _timeline.UpdateShapeInTimeline(_selectedShape);

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
        private void OnLeftMouseUp()
        {
            _selectorType = SelectorType.None;
        }

        private void ShowUIDebug()
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
                foreach (Renderer vao in _selectedShape.Vaos)
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

            numberColumns = _timeline.SortedTimeline.Count;
            if (_timeline.SortedTimeline.Count > 0)
                numberRows = _timeline.SortedTimeline.Max(i => i.Value != null ? i.Value.Count : 0);

            if (numberColumns > 0)
            {
                if (ImGui.BeginTable("elements", numberColumns, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                {
                    foreach (KeyValuePair<int, List<Shape>> shapes in _timeline.SortedTimeline)
                        if (shapes.Value != null)
                            ImGui.TableSetupColumn(shapes.Key.ToString());
                    ImGui.TableHeadersRow();

                    for (int row = 0; row < numberRows; row++)
                    {
                        int columnIndex = 0;
                        ImGui.TableNextRow();
                        foreach (KeyValuePair<int, List<Shape>> shapes in _timeline.SortedTimeline)
                        {
                            if (shapes.Value != null)
                            {
                                if (shapes.Value.Count > row)
                                {
                                    ImGui.TableSetColumnIndex(columnIndex);
                                    ImGui.Text(string.Format("{0}", shapes.Value[row].GetType().Name));
                                    ImGui.Text(string.Format("{0}", shapes.Value[row].Id));
                                    ImGui.Text(string.Format("{0}", shapes.Value[row].GetHashCode()));
                                    foreach (Renderer vao in shapes.Value[row].Vaos)
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

        private void ShowUI()
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

            if (_selectedShape == null)
                ImGui.BeginDisabled();
            ImGui.Text("Color");
            ImGui.ColorEdit4("##Color", ref _ioColor);
            if (_selectedShape == null)
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
                if (ImGui.Selectable(String.Format("{0} - {1}##{2}", Shapes[i].ShortId, Shapes[i].GetType().Name, Shapes[i].GetHashCode()), _selectedShape == Shapes[i]))
                {
                    _selectedShape = Shapes[i];
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
            ImGui.Text("Animation");
            ImGui.Spacing();

            int numberColumns = _timeline.SortedTimeline.Count;
            if (numberColumns > 0)
            {
                foreach (Shape shape in Shapes)
                {
                    if (ImGui.BeginTable("shape", 2, ImGuiTableFlags.Resizable))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(string.Format("{0} - {1}", shape.ShortId, shape.GetType().Name));
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.BeginTable(String.Format("elements##{0}", shape.Id), numberColumns + 1, ImGuiTableFlags.Borders))
                        {
                            ImGui.TableSetupColumn("Properties");
                            foreach (KeyValuePair<int, List<Shape>> shapes in _timeline.SortedTimeline)
                                ImGui.TableSetupColumn(shapes.Key.ToString());
                            ImGui.TableHeadersRow();


                            int i = 0;

                            if (shape.IsMoveable)
                            {
                                i = 0;
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(i);
                                ImGui.Text("X");
                                foreach (KeyValuePair<int, List<Shape>> timeline in _timeline.SortedTimeline)
                                {
                                    i++;
                                    Shape? sibling = timeline.Value.Find(x => x.Id == shape.Id);
                                    if (sibling != null)
                                    {
                                        ImGui.TableSetColumnIndex(i);
                                        ImGui.Text(sibling.X.ToString());
                                    }
                                }

                                i = 0;
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(i);
                                ImGui.Text("Y");
                                foreach (KeyValuePair<int, List<Shape>> timeline in _timeline.SortedTimeline)
                                {
                                    i++;
                                    Shape? sibling = timeline.Value.Find(x => x.Id == shape.Id);
                                    if (sibling != null)
                                    {
                                        ImGui.TableSetColumnIndex(i);
                                        ImGui.Text(sibling.Y.ToString());
                                    }
                                }
                            }

                            if (shape.IsResizeable)
                            {
                                i = 0;
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(i);
                                ImGui.Text("Width");
                                foreach (KeyValuePair<int, List<Shape>> timeline in _timeline.SortedTimeline)
                                {
                                    i++;
                                    Shape? sibling = timeline.Value.Find(x => x.Id == shape.Id);
                                    if (sibling != null)
                                    {
                                        ImGui.TableSetColumnIndex(i);
                                        ImGui.Text(sibling.Width.ToString());
                                    }
                                }

                                i = 0;
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(i);
                                ImGui.Text("Height");
                                foreach (KeyValuePair<int, List<Shape>> timeline in _timeline.SortedTimeline)
                                {
                                    i++;
                                    Shape? sibling = timeline.Value.Find(x => x.Id == shape.Id);
                                    if (sibling != null)
                                    {
                                        ImGui.TableSetColumnIndex(i);
                                        ImGui.Text(sibling.Height.ToString());
                                    }
                                }
                            }

                            if (shape.IsAngleChangeable)
                            {
                                i = 0;
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(i);
                                ImGui.Text("Angle");
                                foreach (KeyValuePair<int, List<Shape>> timeline in _timeline.SortedTimeline)
                                {
                                    i++;
                                    Shape? sibling = timeline.Value.Find(x => x.Id == shape.Id);
                                    if (sibling != null)
                                    {
                                        ImGui.TableSetColumnIndex(i);
                                        ImGui.Text(String.Format("{0}°", sibling.Angle));
                                    }
                                }
                            }

                            if (shape.IsCornerRadiusChangeable)
                            {
                                i = 0;
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(i);
                                ImGui.Text("Corner Radius");
                                foreach (KeyValuePair<int, List<Shape>> timeline in _timeline.SortedTimeline)
                                {
                                    i++;
                                    Shape? sibling = timeline.Value.Find(x => x.Id == shape.Id);
                                    if (sibling != null)
                                    {
                                        ImGui.TableSetColumnIndex(i);
                                        ImGui.Text(sibling.CornerRadius.ToString());
                                    }
                                }
                            }

                            i = 0;
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(i);
                            ImGui.Text("Color");
                            foreach (KeyValuePair<int, List<Shape>> timeline in _timeline.SortedTimeline)
                            {
                                i++;
                                Shape? sibling = timeline.Value.Find(x => x.Id == shape.Id);
                                if (sibling != null)
                                {
                                    ImGui.TableSetColumnIndex(i);
                                    ImGui.Text(String.Format("RGBA({0}, {1}, {2}, {3})", sibling.Color.R, sibling.Color.G, sibling.Color.B, sibling.Color.A));
                                }
                            }

                            ImGui.EndTable();
                        }
                        ImGui.EndTable();
                        ImGui.Spacing();
                    }
                }
            }
            ImGui.End();


            // Timeline side
            ImGui.Begin("Timeline", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowSize(new System.Numerics.Vector2(ClientSize.X / 2 - 200, 200));
            ImGui.SetWindowPos(new System.Numerics.Vector2(ClientSize.X / 2, ClientSize.Y - ImGui.GetWindowHeight()));

            ImGui.Text("Timeline");
            ImGui.Spacing();
            if (ImGui.SliderInt("frame", ref _ioTimeline, Timeline.MIN_TIMELINE, Timeline.MAX_TIMELINE))
            {
                _timeline.TimelineIndex = _ioTimeline;
                _timeline.RenderInterpolation(this);
            }

            ImGui.SameLine();

            ImGui.PushItemWidth(80f);
            if (ImGui.InputInt("fps", ref _ioFps))
            {
                _ioFps = Math.Clamp(_ioFps, Timeline.MIN_FPS, Timeline.MAX_FPS);
                _timeline.Fps = _ioFps;
            }
            ImGui.PopItemWidth();

            if (ImGui.Button(_timeline.IsPlaying == false ? "Play" : "Pause"))
                _timeline.IsPlaying = !_timeline.IsPlaying;

            ImGui.SameLine();

            if (_selectedShape != null)
            {
                if (_timeline.SortedTimeline.ContainsKey(_timeline.TimelineIndex) && _timeline.SortedTimeline[_timeline.TimelineIndex] != null && _timeline.SortedTimeline[_timeline.TimelineIndex].Any(x => x.Id == _selectedShape.Id))
                {
                    if (ImGui.Button(String.Format("Remove keyframe {0} for {1}", _timeline.TimelineIndex, _selectedShape.GetType().Name)))
                    {
                        _timeline.SortedTimeline[_timeline.TimelineIndex].Remove(_timeline.SortedTimeline[_timeline.TimelineIndex].Find(x => x.Id == _selectedShape.Id)!); // Can't be null
                        if (_timeline.SortedTimeline[_timeline.TimelineIndex].Count == 0)
                            _timeline.SortedTimeline.Remove(_timeline.TimelineIndex);
                        // If list _timeline[_ioTimeline] empty then null it
                    }
                }
                else
                {
                    if (ImGui.Button(String.Format("Create keyframe for {0}", _selectedShape.GetType().Name)))
                    {
                        if (_timeline.SortedTimeline.ContainsKey(_timeline.TimelineIndex) == false || _timeline.SortedTimeline[_timeline.TimelineIndex] == null)
                            _timeline.SortedTimeline[_timeline.TimelineIndex] = new List<Shape>();

                        foreach (Shape shape in _timeline.SortedTimeline[_timeline.TimelineIndex])
                        {
                            if (shape.Id == _selectedShape.Id)
                            {
                                Console.WriteLine("Already exist");
                                _timeline.SortedTimeline[_timeline.TimelineIndex].Remove(shape);
                                break;
                            }
                        }
                        _timeline.SortedTimeline[_timeline.TimelineIndex].Add(_selectedShape.ShallowCopy());
                    }
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
                if (ImGui.Selectable("Triangle"))
                {
                    _userMode = UserMode.Create;
                    _createMode = CreateMode.Triangle;
                }
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
                    //bool compatibilityFlag = false;
                    switch (_importMode)
                    {
                        case ImportMode.Add:
                            (List<Shape> newShapes, SortedDictionary<int, List<Shape>> newTimeline, _dialogCompatibility) = Importer.ImportFromFile(picker.SelectedFile);
                            Shapes.AddRange(newShapes);
                            // TODO: merde both timelines
                            break;
                        case ImportMode.Override:
                        default:
                            (Shapes, _timeline.SortedTimeline, _dialogCompatibility) = Importer.ImportFromFile(picker.SelectedFile);
                            _timeline.ResetTimeline();
                            break;
                    }
                    try //TODO: fix trycatch
                    {
                    }
                    catch (Exception)
                    {
                        _dialogError = true;
                    }

                    FilePicker.RemoveFilePicker(this);
                    //if (compatibilityFlag)
                    //    _dialogCompatibility = true;

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

            // Error alert
            if (_dialogError)
                ImGui.OpenPopup("Error Problem");
            if (ImGui.BeginPopupModal("Error Problem")) // ImGuiWindowFlags.AlwaysAutoResize
            {
                ImGui.Text("There was an error while importing the file, please check the syntax");
                ImGui.Separator();
                if (ImGui.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                    _dialogError = false;
                }
                ImGui.EndPopup();
            }
            ImGui.End();
        }
        public void SelectShape(Shape shape)
        {
            _selectedShape = shape;
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
            using VideoWriter w = new VideoWriter("output.mp4", _timeline.Fps, new System.Drawing.Size(ClientSize.X, ClientSize.Y), true);
            for (int i = Timeline.MIN_TIMELINE; i <= Timeline.MAX_TIMELINE; i++) // TODO: please dont hardcode this
            {
                RenderFrameBySecondIndex(i);
                w.Write(TakeSnap().ToMat());
            }
        }
        public void SaveCurrentScreenToGIF()
        {
            using (var gif = AnimatedGif.AnimatedGif.Create("output.gif", 1000 / _timeline.Fps))
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
            RenderFrameBySecondIndex(_timeline.TimelineIndex);
            TakeSnap().Save("output.png", ImageFormat.Png);
        }

        // [SupportedOSPlatform("windows")]
        public void RenderFrameBySecondIndex(int second)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit); // Clear the colorV
            ResetSelection();
            _timeline.TimelineIndex = second;
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

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bmp;
        }
    }
}