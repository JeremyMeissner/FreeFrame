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

        private int _timelineIndex;
        private int _fps;
        private bool _isPlaying;

        private double _secondsEllapsed;

        private SortedDictionary<int, List<Shape>> _sortedTimeline;

        public SortedDictionary<int, List<Shape>> SortedTimeline { get => _sortedTimeline; set => _sortedTimeline = value; }
        public int TimelineIndex
        {
            get => _timelineIndex;
            set
            {
                if (value > MAX_TIMELINE) // Avoid blinking index
                    value -= MAX_TIMELINE;

                _timelineIndex = Math.Clamp(value, MIN_TIMELINE, MAX_TIMELINE);
            }
        }
        public int Fps { get => _fps; set => _fps = value; }
        public bool IsPlaying { get => _isPlaying; set => _isPlaying = value; }

        public Timeline()
        {
            TimelineIndex = MIN_TIMELINE;
            Fps = DEFAULT_FPS;
            IsPlaying = false;
            _secondsEllapsed = 0;
            SortedTimeline = new SortedDictionary<int, List<Shape>>();
        }

        /// <summary>
        /// If IsPlaying is True, render the timeline in loop
        /// </summary>
        /// <param name="e"></param>
        /// <param name="shapes"></param>
        public void OnRenderFrame(FrameEventArgs e, Window window)
        {
            if (IsPlaying)
            {
                double frameDuration = 1.0 / Fps;
                _secondsEllapsed += e.Time;
                if (_secondsEllapsed >= frameDuration)
                {
                    while (_secondsEllapsed >= frameDuration)
                    {
                        _secondsEllapsed -= frameDuration;
                        TimelineIndex++;
                    }
                }
                RenderInterpolation(window.Shapes);
                window.ResetSelection();
            }
        }
        /// <summary>
        /// Render timeline
        /// </summary>
        /// <param name="shapes">Shapes available</param>
        public void RenderInterpolation(List<Shape> shapes)
        {
            foreach (Shape shape in shapes)
            {
                if (SortedTimeline.ContainsKey(TimelineIndex) == true && SortedTimeline[TimelineIndex] != null && SortedTimeline[TimelineIndex].Any(x => x.Id == shape.Id))
                {
                    // Draw the current one in this list
                    Shape sibling = SortedTimeline[TimelineIndex].Find(x => x.Id == shape.Id)!;

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
                    if (SortedTimeline.Any(i => i.Value.Any(j => j.Id == shape.Id))) // If exist somewhere else but not here
                    {
                        // Retrieve all the keyframes
                        int[] keys = SortedTimeline.Where(pair => pair.Value.Any(x => x.Id == shape.Id)).Select(pair => pair.Key).ToArray(); // Key key everywhere it exist

                        (int first, int second) nearest = (int.MaxValue, int.MaxValue);

                        nearest.first = 0;
                        nearest.second = 0;

                        int timelineIndex = TimelineIndex;

                        if (timelineIndex >= keys[keys.Length - 1]) // Handle max value
                        {
                            nearest.first = keys[keys.Length - 1];
                            nearest.second = keys[keys.Length - 1];
                        }
                        else if (timelineIndex <= keys[0]) // Handle min value
                        {
                            nearest.first = keys[0];
                            nearest.second = keys[0];
                        }
                        else
                        {
                            // Retrieve the nearest keyframe to the current index in the timeline
                            for (int i = 0; i < keys.Length; i++)
                            {
                                if (keys[i] >= timelineIndex)
                                {
                                    nearest.second = keys[i];
                                    if (i - 1 >= 0) // If second possible
                                        nearest.first = keys[i - 1];
                                    break;
                                }
                            }
                        }
                        Shape first = SortedTimeline[nearest.first].Find(x => x.Id == shape.Id)!;
                        Shape second = SortedTimeline[nearest.second].Find(x => x.Id == shape.Id)!;
                                
                        // Interpolate each properties
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
                        // Doesnt exist somewhere else, so just draw the one on the screen.
                        shape.ImplementObject();
                    }
                }
            }
        }

        /// <summary>
        /// Update the given shape found in the timeline
        /// </summary>
        /// <param name="shape">Shape that need to be updated in the timeline</param>
        public void UpdateShapeInTimeline(Shape shape)
        {
            if (SortedTimeline.ContainsKey(TimelineIndex) == true && SortedTimeline[TimelineIndex] != null)
            {
                Shape? existingShape = SortedTimeline[TimelineIndex].Find(x => x.Id == shape.Id);
                if (existingShape != null) // If the shape really exist in the timeline, update it
                {
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

        /// <summary>
        /// Basic linear interpolate.
        /// See https://en.wikipedia.org/wiki/Linear_interpolation
        /// </summary>
        /// <param name="x"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public int LinearInterpolate(int x, int x1, int x2, int y1, int y2)
        {
            float y = y1 + (x - x1) * ((y2 - y1) / ((x2 - x1) == 0 ? 1 : (float)(x2 - x1)));

            if (y1 <= y2)
                return (int)Math.Clamp(y, y1, y2);
            else
                return (int)Math.Clamp(y, y2, y1);
        }
        /// <summary>
        /// Basic linear interpolate for Color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public Color4 LinearInterpolate(int x, int x1, int x2, Color4 y1, Color4 y2)
        {
            int r = LinearInterpolate(x, x1, x2, (int)(y1.R * 255), (int)(y2.R * 255));
            int g = LinearInterpolate(x, x1, x2, (int)(y1.G * 255), (int)(y2.G * 255));
            int b = LinearInterpolate(x, x1, x2, (int)(y1.B * 255), (int)(y2.B * 255));
            int a = LinearInterpolate(x, x1, x2, (int)(y1.A * 255), (int)(y2.A * 255));
            return new Color4(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        /// <summary>
        /// Remove given shape in the timeline
        /// </summary>
        /// <param name="shapeToRemove">The shape to remove</param>
        public void RemoveElementInTimeline(Shape shapeToRemove)
        {
            List<int> emptyKeyframes = new();
            foreach (KeyValuePair<int, List<Shape>> keyframe in SortedTimeline) // Remove element in the timeline
            {
                foreach (Shape shape in keyframe.Value)
                {
                    if (shape.Id == shapeToRemove.Id)
                    {
                        shape.DeleteObjects();
                        keyframe.Value.Remove(shape);
                        break; // Because we know that only one shape id is in the current list
                    }
                }
                if (keyframe.Value.Count == 0) // Remove shapes in timeline
                    emptyKeyframes.Add(keyframe.Key);
            }
            foreach (int id in emptyKeyframes) // Remove empty keyframes
                SortedTimeline.Remove(id);
        }

        /// <summary>
        /// Empty the timeline
        /// </summary>
        public void ResetTimeline()
        {
            foreach (KeyValuePair<int, List<Shape>> keyframe in SortedTimeline) // Remove element in the timeline
            {
                foreach (Shape shape in keyframe.Value)
                    shape.DeleteObjects();
                keyframe.Value.Clear();
            }
            SortedTimeline.Clear();
        }
    }
}
