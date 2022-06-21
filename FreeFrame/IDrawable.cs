using OpenTK.Mathematics;

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
