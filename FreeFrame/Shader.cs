using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FreeFrame
{
    public class Shader
    {
        private int _program;

        public Shader(string uriVertexShader, string uriFragementShader)
        {
            _program = GL.CreateProgram(); // Handler for the shaders

            int vertexShader = CompileShader(uriVertexShader, ShaderType.VertexShader);
            int fragmentShader = CompileShader(uriFragementShader, ShaderType.FragmentShader);

            GL.AttachShader(_program, vertexShader);
            GL.AttachShader(_program, fragmentShader);

            GL.LinkProgram(_program); // Put the shaders in their respective processor

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }
        int CompileShader(string uri, ShaderType type)
        {
            int shader = GL.CreateShader(type);

            GL.ShaderSource(shader, File.ReadAllText(uri)); // Import the source code of the shader

            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int compileStatus); // compileStatus is 0 if compile error
            if (compileStatus == 0)
            {
                Console.WriteLine("{0}: {1}", type.ToString(), GL.GetShaderInfoLog(shader));
                throw new Exception("Error while compiling shader");
            }

            return shader;
        }
        /// <summary>
        /// Retrieve the Uniform location in a shader
        /// </summary>
        /// <param name="uniform">The uniform</param>
        /// <returns>The index of the uniform in the shader</returns>
        public int GetUniformLocation(string uniform) => GL.GetUniformLocation(_program, uniform);

        // Set the given uniform
        public void SetUniformVec4(int uniform, Vector4 vector) => GL.Uniform4(uniform, vector);
        public void SetUniformVec2(int uniform, Vector2 vector) => GL.Uniform2(uniform, vector);
        public void SetUniformFloat(int uniform, float value) => GL.Uniform1(uniform, value);
        public void SetUniformMat4(int uniform, Matrix4 matrix) => GL.UniformMatrix4(uniform, false, ref matrix);

        /// <summary>
        /// Use the current shaders
        /// </summary>
        public void Use() => GL.UseProgram(_program);
    }
}
