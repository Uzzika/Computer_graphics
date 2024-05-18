using OpenTK.Graphics.ES30;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace Lab3_ray_tracing
{
    internal class View
    {
        private int BasicVertexShader;
        private int BasicFragmentShader;
        private int BasicProgramID;
        private int vbo_position;
        private int attribute_vpos;
        private int uniform_pos;
        private Vector3 campos;
        private int uniform_aspect, aspect;
        private Vector3[] vertdata;

        void loadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (System.IO.StreamReader sr = new System.IO.StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }
        public void InitShaders()
        {
            BasicProgramID = GL.CreateProgram();
            loadShader("C:\\Users\\79200\\source\\Computer_graphics\\Lab3_ray_tracing\\raytracing.frag", 
                ShaderType.VertexShader, BasicProgramID,
                out BasicVertexShader);
            loadShader("C:\\Users\\79200\\source\\Computer_graphics\\Lab3_ray_tracing\\raytracing.vert",
                ShaderType.FragmentShader, BasicProgramID,
                out BasicFragmentShader);
            GL.LinkProgram(BasicProgramID);
            int status = 0;
            GL.GetProgram(BasicProgramID, GetProgramParameterName.LinkStatus, out status);
            Console.WriteLine(GL.GetProgramInfoLog(BasicProgramID));
        }
        public void drawQuad()
        {
            vertdata = new Vector3[] {
                new Vector3(-1f,-1f,0f),
                new Vector3(1f,-1f,0f),
                new Vector3(1f,1f,0f),
                new Vector3(-1f,1f,0f)};
            GL.GenBuffers(1, out vbo_position);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length *
                Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.Uniform3(uniform_pos, campos);
            GL.Uniform1(uniform_aspect, aspect);
            GL.UseProgram(BasicProgramID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}
