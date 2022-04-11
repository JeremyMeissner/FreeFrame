using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeFrame
{
    class Shader
    {
        private int _program;
        int CompileShader(string uri, ShaderType type)
        {
            int shader = GL.CreateShader(type);

            GL.ShaderSource(shader, File.ReadAllText(uri)); // Import the source code of the shader
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int compileStatus); // compileStatus is 0 if compile error
            if (compileStatus == 0)
            {
                Console.WriteLine("{0}: {1}", type.ToString(), GL.GetShaderInfoLog(shader));
                throw new Exception(); // TODO: Remove this exception
            }

            return shader;
        }
        public Shader(string uriVertexShader, string uriFragementShader)
        {
            _program = GL.CreateProgram();

            int vertexShader = CompileShader(uriVertexShader, ShaderType.VertexShader);
            int fragmentShader = CompileShader(uriFragementShader, ShaderType.FragmentShader);

            // TODO: Handling error

            GL.AttachShader(_program, vertexShader);
            GL.AttachShader(_program, fragmentShader);

            GL.LinkProgram(_program); // Put the shaders in their respective processor
            GL.ValidateProgram(_program); // Check if everything correct and store the information on the logs 
            // TODO: Explain what's the difference with GL.ShaderInfoLog

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }
        public int GetUniformLocation(string uniform) => GL.GetUniformLocation(_program, uniform);
        public void Use() => GL.UseProgram(_program);
        ~Shader() => GL.DeleteProgram(_program);
    }
}
