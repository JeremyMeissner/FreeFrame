using FreeFrame.Components.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeFrame
{
    public class Timeline
    {
        private int _ioTimeline;
        private SortedDictionary<int, List<Shape>> _timeline;
        public Timeline()
        {
            _ioTimeline = 1;
            _timeline = new SortedDictionary<int, List<Shape>>();
        }

        //public void RemoveElementInTimeline(Shape shapeToRemove)
        //{
        //    List<int> shapesToDelete = new();
        //    foreach (KeyValuePair<int, List<Shape>> shapes in _timeline) // Remove element in the timeline
        //    {
        //        foreach (Shape shape in shapes.Value)
        //        {
        //            if (shape.Id == shapeToRemove.Id)
        //            {
        //                shape.DeleteObjects();
        //                shapes.Value.Remove(shape);
        //                break; // Because we know that there is no more of this shape in the current list
        //            }
        //        }
        //        if (shapes.Value.Count == 0) // Remove shapes in timeline
        //            shapesToDelete.Add(shapes.Key);
        //    }
        //    foreach (int id in shapesToDelete)
        //        _timeline.Remove(id);
        //}
        //public void ResetTimeline()
        //{
        //    foreach (KeyValuePair<int, List<Shape>> shapes in _timeline) // Remove element in the timeline
        //    {
        //        foreach (Shape shape in shapes.Value)
        //            shape.DeleteObjects();
        //        shapes.Value.Clear();
        //    }
        //    _timeline.Clear();
        //}
    }
}
