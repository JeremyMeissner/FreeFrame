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
        /// <summary>
        /// Trigger draw element through OpenGL context
        /// </summary>
        /// <param name="clientSize">Window size</param>
        public void Draw(Vector2i clientSize);

        /// <summary>
        /// Deletes the objects saved in OpenGL context
        /// </summary>
        public void DeleteObjects();
    }
}
