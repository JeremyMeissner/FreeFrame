using FreeFrame.Components.Shapes;
using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeFrame
{
    public class Timeline
    {
        public const int DEFAULT_FPS = 24;
        public const int MIN_FPS = 1;
        public const int MAX_FPS = 120;
        public const int MIN_TIMELINE = 1;
        public const int MAX_TIMELINE = 100;

        private int _ioTimeline;
        private int _ioFps;
        private bool _ioIsPlaying;

        private double _secondsEllapsed;

        private SortedDictionary<int, List<Shape>> _timeline;

        public int IoTimeline
        {
            get => _ioTimeline;
            set
            {
                if (value > MAX_TIMELINE)
                    value -= MAX_TIMELINE;

                _ioTimeline = Math.Clamp(value, MIN_TIMELINE, MAX_TIMELINE);
            }
        }
        public int IoFps { get => _ioFps; private set => _ioFps = value; }

        public Timeline()
        {
            IoTimeline = MIN_TIMELINE;
            _ioIsPlaying = false;
            IoFps = DEFAULT_FPS;
            _secondsEllapsed = 0;
            _timeline = new SortedDictionary<int, List<Shape>>();
        }
        public void OnRenderFrame(FrameEventArgs e, Window window)
        {
            if (_ioIsPlaying)
            {
                double frameDuration = 1.0 / IoFps;
                _secondsEllapsed += e.Time;
                if (_secondsEllapsed >= frameDuration)
                {
                    while (_secondsEllapsed >= frameDuration)
                    {
                        _secondsEllapsed -= frameDuration;
                        IoTimeline++;
                    }

                }
                RenderInterpolation(window);
            }
        }
        public void DrawUI(Window window)
        {
            ImGui.Text("Timeline");
            ImGui.Spacing();
            if (ImGui.SliderInt("frame", ref _ioTimeline, MIN_TIMELINE, MAX_TIMELINE)) // TODO: please dont hardcode this
                RenderInterpolation(window);

            ImGui.SameLine();

            ImGui.PushItemWidth(80f);
            if (ImGui.InputInt("fps", ref _ioFps))
                IoFps = Math.Clamp(IoFps, MIN_FPS, MAX_FPS); // TODO: please dont hardcode this
            ImGui.PopItemWidth();

            if (ImGui.Button(_ioIsPlaying == false ? "Play" : "Pause"))
                _ioIsPlaying = !_ioIsPlaying;

            ImGui.SameLine();

            if (window.SelectedShape != null)
            {
                if (_timeline.ContainsKey(IoTimeline) && _timeline[IoTimeline] != null && _timeline[IoTimeline].Any(x => x.Id == window.SelectedShape.Id))
                {
                    if (ImGui.Button(String.Format("Remove keyframe {0} for {1}", IoTimeline, window.SelectedShape.GetType().Name)))
                    {
                        _timeline[IoTimeline].Remove(_timeline[IoTimeline].Find(x => x.Id == window.SelectedShape.Id)!); // Can't be null
                        if (_timeline[IoTimeline].Count == 0)
                            _timeline.Remove(IoTimeline);
                        // If list _timeline[_ioTimeline] empty then null it
                    }
                }
                else
                {
                    if (ImGui.Button(String.Format("Create keyframe for {0}", window.SelectedShape.GetType().Name)))
                    {
                        if (_timeline.ContainsKey(IoTimeline) == false || _timeline[IoTimeline] == null)
                            _timeline[IoTimeline] = new List<Shape>();

                        foreach (Shape shape in _timeline[IoTimeline])
                        {
                            if (shape.Id == window.SelectedShape.Id)
                            {
                                Console.WriteLine("Already exist");
                                _timeline[IoTimeline].Remove(shape);
                                break;
                            }
                        }
                        _timeline[IoTimeline].Add(window.SelectedShape.ShallowCopy());
                    }
                }
            }
        }

        public void DrawAnimationList(Window window)
        {
            ImGui.Text("Animation");
            ImGui.Spacing();

            int numberColumns = _timeline.Count;
            if (numberColumns > 0)
            {
                foreach (Shape shape in window.Shapes)
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
                            foreach (KeyValuePair<int, List<Shape>> shapes in _timeline)
                                ImGui.TableSetupColumn(shapes.Key.ToString());
                            ImGui.TableHeadersRow();


                            int i = 0;

                            if (shape.IsMoveable)
                            {
                                i = 0;
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(i);
                                ImGui.Text("X");
                                foreach (KeyValuePair<int, List<Shape>> timeline in _timeline)
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
                                foreach (KeyValuePair<int, List<Shape>> timeline in _timeline)
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
                                foreach (KeyValuePair<int, List<Shape>> timeline in _timeline)
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
                                foreach (KeyValuePair<int, List<Shape>> timeline in _timeline)
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
                                foreach (KeyValuePair<int, List<Shape>> timeline in _timeline)
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
                                foreach (KeyValuePair<int, List<Shape>> timeline in _timeline)
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
                            foreach (KeyValuePair<int, List<Shape>> timeline in _timeline)
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
        }
        public void DrawDebugUI(Window window)
        {
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
        }
        public void UpdateShapeInTimeline(Shape shape)
        {
            if (_timeline.ContainsKey(IoTimeline) == true && _timeline[IoTimeline] != null)
            {
                Shape? existingShape = _timeline[IoTimeline].Find(x => x.Id == shape.Id);
                if (existingShape != null)
                {
                    Console.WriteLine("There is a shape like meee");
                    existingShape.X = shape.X;
                    existingShape.Y = shape.Y;
                    existingShape.Width = shape.Width;
                    existingShape.Height = shape.Height;
                    existingShape.Angle = shape.Angle;
                    existingShape.CornerRadius = shape.CornerRadius;
                    existingShape.Color = shape.Color;
                }
            }
        }
        public void RenderInterpolation(Window window)
        {
            foreach (Shape shape in window.Shapes)
            {
                if (_timeline.ContainsKey(IoTimeline) == true && _timeline[IoTimeline] != null && _timeline[IoTimeline].Any(x => x.Id == shape.Id))
                {
                    // Draw the current one in this list
                    Shape sibling = _timeline[IoTimeline].Find(x => x.Id == shape.Id)!;
                    Console.WriteLine("One already exist");

                    shape.X = sibling.X;
                    shape.Y = sibling.Y;
                    shape.Width = sibling.Width;
                    shape.Height = sibling.Height;
                    shape.Angle = sibling.Angle;
                    shape.CornerRadius = sibling.CornerRadius;
                    shape.Color = sibling.Color;
                    shape.ImplementObject();
                }
                else // Doesn't exist in the list
                {
                    if (_timeline.Any(i => i.Value.Any(j => j.Id == shape.Id))) // If exist somewhere else but not here
                    {
                        int[] keys = _timeline.Where(pair => pair.Value.Any(x => x.Id == shape.Id)).Select(pair => pair.Key).ToArray(); // Key key everywhere it exist

                        // Find two nearest
                        (int first, int second) nearest = (int.MaxValue, int.MaxValue);
                        //(int first, int second) deltas = (int.MaxValue, int.MaxValue);
                        nearest.first = 0;
                        nearest.second = 0; //Math.Min(keys[keys.Length - 1], _ioTimeline);

                        int timelineIndex = IoTimeline;

                        if (timelineIndex >= keys[keys.Length - 1])
                        {
                            Console.WriteLine("ihhhh ioTimeline == {0}", timelineIndex);
                            nearest.first = keys[keys.Length - 1];
                            nearest.second = keys[keys.Length - 1];
                        }
                        else if (timelineIndex <= keys[0])
                        {
                            Console.WriteLine("ohhh ioTimeline == {0}", timelineIndex);
                            nearest.first = keys[0];
                            nearest.second = keys[0];
                        }
                        else
                        {
                            for (int i = 0; i < keys.Length; i++)
                            {
                                //Console.WriteLine("key[{0}] = {1}", i, keys[i]);
                                if (keys[i] >= timelineIndex)
                                {
                                    nearest.second = keys[i];
                                    if (i - 1 >= 0) // If second possible
                                        nearest.first = keys[i - 1];
                                    break;
                                }
                            }
                        }
                        Console.WriteLine("HHHHHHHHHHHHHHHHHHHHHHHHHHHHH first: {0}   second: {1}", nearest.first, nearest.second);

                        Shape first = _timeline[nearest.first].Find(x => x.Id == shape.Id)!; // can't be null
                        Shape second = _timeline[nearest.second].Find(x => x.Id == shape.Id)!; // can't be null;
                                                                                               // If reverse and loop invert the two shape every odd

                        shape.X = LinearInterpolate(timelineIndex, nearest.first, nearest.second, first.X, second.X);
                        shape.Y = LinearInterpolate(timelineIndex, nearest.first, nearest.second, first.Y, second.Y);
                        shape.Width = LinearInterpolate(timelineIndex, nearest.first, nearest.second, first.Width, second.Width);
                        shape.Height = LinearInterpolate(timelineIndex, nearest.first, nearest.second, first.Height, second.Height);
                        shape.Angle = LinearInterpolate(timelineIndex, nearest.first, nearest.second, first.Angle, second.Angle);
                        shape.CornerRadius = LinearInterpolate(timelineIndex, nearest.first, nearest.second, first.CornerRadius, second.CornerRadius);
                        shape.Color = LinearInterpolate(timelineIndex, nearest.first, nearest.second, first.Color, second.Color);

                        shape.ImplementObject();
                    }
                    else
                    {
                        // doesnt exist somewhere else, so just draw the one on the screen.
                        Console.WriteLine("Draw the current one");
                        shape.ImplementObject();
                    }
                }
            }
            window.ResetSelection();
        }

        public int LinearInterpolate(int x, int x1, int x2, int y1, int y2)
        {
            float y = y1 + (x - x1) * ((y2 - y1) / ((x2 - x1) == 0 ? 1 : (float)(x2 - x1)));

            if (y1 <= y2)
                return (int)Math.Clamp(y, y1, y2);
            else
                return (int)Math.Clamp(y, y2, y1);
        }
        public Color4 LinearInterpolate(int x, int x1, int x2, Color4 y1, Color4 y2)
        {
            int r = LinearInterpolate(x, x1, x2, (int)(y1.R * 255), (int)(y2.R * 255));
            int g = LinearInterpolate(x, x1, x2, (int)(y1.G * 255), (int)(y2.G * 255));
            int b = LinearInterpolate(x, x1, x2, (int)(y1.B * 255), (int)(y2.B * 255));
            int a = LinearInterpolate(x, x1, x2, (int)(y1.A * 255), (int)(y2.A * 255));
            return new Color4(r / 255f, g / 255f, b / 255f, a / 255f);
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
    }
}
