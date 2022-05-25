using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeFrame
{
    public interface IDrawable
    {
        //List<Renderer> Vaos
        //{
        //    get;
        //}
        /// <summary>
        /// Trigge draw element through OpenGL context
        /// </summary>
        public void Draw(Vector2i clientSize);
        public void DeleteObjects();
    }
}
