using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeFrame.Components.Shapes
{

    public class Hitbox
    {
        // TODO: Absolute or relative ?
        public struct Area
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public Area(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }
        }
        List<Area> _areas = new List<Area>();

        public List<Area> Areas { get => _areas; set => _areas = value; }

        public Hitbox()
        {
        }

        public bool IsThereSomething(float x, float y) // mouse position
        {
            foreach (Area area in Areas)
                if (x >= area.X && x <= area.X+area.Width && y >= area.Y && y <= area.Y+area.Height) // TODO: To check if it's working
                    return true;
            return false;
        }
    }
}
