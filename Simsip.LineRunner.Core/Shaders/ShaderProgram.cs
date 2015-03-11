using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simsip.LineRunner.Utils;

namespace Simsip.LineRunner.Shaders
{
    public class ShaderProgram
    {
        public int ProgramID = -1;
        public int VShaderID = -1;
        public int FShaderID = -1;

        public Dictionary<String, int> Attributes = new Dictionary<string, int>();
        public Dictionary<String, int> Uniforms = new Dictionary<string, int>();
        public Dictionary<String, int> Buffers = new Dictionary<string, int>();
        public Dictionary<String, int> Textures = new Dictionary<string, int>();

        #region Construction

        /// <summary>
        /// Bare-bones constructor.
        /// 
        /// TODO: May want to build this out later for incremental
        /// construction of complex shader programs.
        /// </summary>
        public ShaderProgram()
        {
#if DEBUG
            ClearErrors();
#endif

            ProgramID = GL.CreateProgram();

#if DEBUG
            CheckError();
#endif
        }

        /// <summary>
        /// Complete load/compile/link prep for a shader program.
        /// </summary>
        /// <param name="vshader">If from file the name of the vertex shader otherwise
        /// a string containing the vertex shader source code.</param>
        /// <param name="fshader">If from file the name of the fragment shader, otherwise
        /// a string containing the fragment shader source code.</param>
        /// <param name="fromFile">Boolean to specify if we are loading source from
        /// files or not.
        /// 
        /// Defaults to true.</param>
        public ShaderProgram(String vshader, String fshader, bool fromFile = true)
        {

#if DEBUG
            // Don't let any previous errors seep into our error checking.
            ClearErrors();
#endif

            ProgramID = GL.CreateProgram();

#if DEBUG
            CheckError();
#endif

            if (fromFile)
            {
                LoadShaderFromFile(vshader, All.VertexShader);
                LoadShaderFromFile(fshader, All.FragmentShader);
            }
            else
            {
                LoadShaderFromString(vshader, All.VertexShader);
                LoadShaderFromString(fshader, All.FragmentShader);
            }
 
            Link();
        }

        public void LoadShaderFromString(String code, All type)
        {
            if (type == All.VertexShader)
            {
                LoadShader(code, type, out VShaderID);
            }
            else if (type == All.FragmentShader)
            {
                LoadShader(code, type, out FShaderID);
            }
        }
 
        public void LoadShaderFromFile(String filename, All type)
        {
            // To make the naming of the location of the file the same as all
            // other asset locations
            string fullFilename = System.IO.Path.Combine(new string[] { "Content/Shaders/Core", filename });
            string content = string.Empty;

#if ANDROID
            using (var sr = new StreamReader(Program.SharedProgram.Assets.Open(fullFilename)))
            {
                content = sr.ReadToEnd();
            }
#endif

            if (type == All.VertexShader)
            {
                LoadShader(content, type, out VShaderID);
            }
            else if (type == All.FragmentShader)
            {
                LoadShader(content, type, out FShaderID);
            }
        }

        public void Link()
        {
            GL.LinkProgram(ProgramID);
#if DEBUG
            CheckError();
#endif
            int status = -1;
            GL.GetProgram(ProgramID, All.LinkStatus, ref status);
#if DEBUG
            CheckError();
#endif

            if (status == (int)All.False)
            {
                int length = -1;
                GL.GetProgram(ProgramID, All.InfoLogLength, ref length);
#if DEBUG
                CheckError();
#endif

                StringBuilder infoLog = new StringBuilder(length);
                GL.GetProgramInfoLog(ProgramID, infoLog.Capacity, ref length, infoLog);
#if DEBUG
                CheckError();
#endif

                throw new Exception(infoLog.ToString());
            }
        }

        #endregion

        #region Program

        /// <summary>
        /// Set up or disable a program to be used in a subsequent Draw or DrawElement call.
        /// </summary>
        /// <param name="use">True if we are using the program, false if we revert back to an undefined program.</param>
        public void UseProgram(bool use)
        {
            if (use)
            {
                GL.UseProgram(ProgramID);
#if DEBUG
                CheckError();
#endif
            }
            else
            {
                GL.UseProgram(0);
#if DEBUG
                CheckError();
#endif
            }
        }

        /// <summary>
        /// Helpful in debug builds to determine if program validates completely.
        /// </summary>
        /// <returns></returns>
        public void ValidateProgram()
        {
            // Attempt to validate
            GL.ValidateProgram(ProgramID);
#if DEBUG
            CheckError();
#endif

            // Did we validate?
            int valid = -1;
            GL.GetProgram(ProgramID, All.ValidateStatus, ref valid);
#if DEBUG
            CheckError();
#endif

            if (valid == (int)All.False)
            {
                throw new Exception("GL Program failed validation.");
            }
        }

        /// <summary>
        /// Deletes a program object from opengl.
        /// </summary>
        public void DeleteProgram()
        {
            GL.DeleteProgram(this.ProgramID);
#if DEBUG
            CheckError();
#endif
        }

        #endregion

        #region Uniforms

        /// <summary>
        /// Get the index for a uniform and store away for easy reference
        /// by string name.
        /// </summary>
        /// <param name="uniformName">The name of the uniform we need the index for.</param>
        public void InitUniformLocation(string uniformName)
        {
            int positionUniformIndex = GL.GetUniformLocation(ProgramID, uniformName);
#if DEBUG
            CheckError();
#endif

            if (positionUniformIndex == -1)
            {
                throw new Exception("Invalid uniform name in attempting to get uniform location: " + uniformName);
            }

            Uniforms.Add(uniformName, positionUniformIndex);
        }

        public void Uniform1(string uniformName, float x)
        {
            GL.Uniform1(Uniforms[uniformName], x);
#if DEBUG
            CheckError();
#endif
        }

        public void Uniform1(string uniformName, int x)
        {
            GL.Uniform1(Uniforms[uniformName], x);
#if DEBUG
            CheckError();
#endif
        }

        public void Uniform2(string uniformName, Vector2 vector)
        {
            GL.Uniform2(Uniforms[uniformName], vector.X, vector.Y);
#if DEBUG
            CheckError();
#endif
        }

        public void Uniform3(string uniformName, Vector3 vector)
        {
            GL.Uniform3(Uniforms[uniformName], vector.X, vector.Y, vector.Z);
#if DEBUG
            CheckError();
#endif
        }

        public void Uniform4(string uniformName, Vector4 vector)
        {
            GL.Uniform4(Uniforms[uniformName], vector.X, vector.Y, vector.Z, vector.W);
#if DEBUG
            CheckError();
#endif
        }

        public void UniformMatrix(string uniformName, Matrix matrix)
        {
            GL.UniformMatrix4(Uniforms[uniformName], 1, false, Matrix.ToFloatArray(matrix));
#if DEBUG
            CheckError();
#endif
        }

        public int GetUniform(string name)
        {
            if (Uniforms.ContainsKey(name))
            {
                return Uniforms[name];
            }
            else
            {
                return -1;
            }
        }

        #endregion

        #region Attributes

        /// <summary>
        /// Get the index for an attribute and store away for easy reference
        /// by string name.
        /// </summary>
        /// <param name="attribName">The name of the attribute we need the index for.</param>
        public void InitAttribLocation(string attribName)
        {
            int positionAttributeIndex = GL.GetAttribLocation(ProgramID, attribName);
#if DEBUG
            CheckError();
#endif

            if (positionAttributeIndex == -1)
            {
                throw new Exception("Invalid attribute name in attempting to get attribute location: " + attribName);
            }

            Attributes.Add(attribName, positionAttributeIndex);
        }

        /// <summary>
        /// Point an attribute at an attribute array stored in a vertex buffer.
        /// </summary>
        /// <param name="attribName">The name we use to reference the attribute.</param>
        /// <param name="size">The size of the attribute array.</param>
        /// <param name="type">The type corresponding to the size of the attribute array.</param>
        /// <param name="stride">How far to go to get to next attribute</param>
        /// <param name="ptr">The offset into the vertex buffer to start reading attributes from.</param>
        public void VertexAttribPointer(string attribName, int size, All type, int stride, int ptr)
        {
            GL.VertexAttribPointer(Attributes[attribName], size, type, false, stride, new IntPtr(ptr));
#if DEBUG
            CheckError();
#endif
        }

        /// <summary>
        /// Point an attribute at an attribute array not stored stored in a vertex buffer.
        /// </summary>
        /// <param name="attribName">The name we use to reference the attribute.</param>
        /// <param name="size">The size of the attribute array.</param>
        /// <param name="type">The type corresponding to the size of the attribute array.</param>
        /// <param name="stride">How far to go to get to next attribute</param>
        /// <param name="ptr">An array of floats representing the data for the attribute array.</param>
        public void VertexAttribPointer(string attribName, int size, All type, int stride, float[] ptr)
        {
            GL.VertexAttribPointer(Attributes[attribName], size, type, false, stride, ptr);
#if DEBUG
            CheckError();
#endif
        }

        /// <summary>
        /// Simple enable of just one attribute array.
        /// </summary>
        /// <param name="attribName"></param>
        public void EnableVertexAttribArray(string attribName)
        {
            GL.EnableVertexAttribArray(Attributes[attribName]);
#if DEBUG
            CheckError();
#endif
        }

        public void DisableVertexAttribArray(string attribName)
        {
            GL.DisableVertexAttribArray(Attributes[attribName]);
#if DEBUG
            CheckError();
#endif
        }

        public void EnableAllVertexAttribArrays()
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                GL.EnableVertexAttribArray(Attributes.Values.ElementAt(i));
#if DEBUG
                CheckError();
#endif
            }
        }

        public void DisableAllVertexAttribArrays()
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                GL.DisableVertexAttribArray(Attributes.Values.ElementAt(i));
#if DEBUG
                CheckError();
#endif
            }
        }

        public int GetAttribute(string name)
        {
            if (Attributes.ContainsKey(name))
            {
                return Attributes[name];
            }
            else
            {
                return -1;
            }
        }

        #endregion

        #region Vertex Buffers

        /// <summary>
        /// Create a vertex buffer and load it with data
        /// </summary>
        /// <param name="bufferName">The name we want to associate with the vertex buffer index.</param>
        /// <param name="bufferSize">The size of the buffer. This will be multiplied by the size of float to get final size.</param>
        /// <param name="bufferData">An array of floats to populate the vertex buffer with.</param>
        /// <param name="usage">The opengl usage, defaults to All.StaticDraw</param>
        public void LoadVertexBuffer(string bufferName,
                                            int bufferSize,
                                            float[] bufferData,
                                            All usage = All.StaticDraw)
        {
            // Create the opengl vertex buffer object
            int buffer = -1;
            GL.GenBuffers(1, ref buffer);
#if DEBUG
            CheckError();
#endif
            // Store it for easy reference using the buffer name passed in
            Buffers.Add(bufferName, buffer);

            // Bind the vertex buffer for subsequent load of data
            GL.BindBuffer(All.ArrayBuffer, buffer);
#if DEBUG
            CheckError();
#endif

            // Calculate the correct size of the buffer based on sizeof float
            int size = sizeof(float) * bufferSize;

            // Load the vertex buffer with data
            GL.BufferData(All.ArrayBuffer,
                          new IntPtr(size),
                          bufferData,
                          usage);

#if DEBUG
            CheckError();
#endif
        }

        /// <summary>
        /// Create a vertex buffer for the purpose of indexing and load it with data
        /// </summary>
        /// <param name="bufferName">The name we want to associate with the vertex buffer index.</param>
        /// <param name="bufferSize">The size of the buffer. This will be multiplied by the size of int to get final size.</param>
        /// <param name="bufferData">An array of ints to populate the vertex buffer with.</param>
        /// <param name="usage">The opengl usage, defaults to All.StaticDraw</param>

        public void LoadIndexBuffer(string bufferName,
                                    int bufferSize,
                                    int[] bufferData,
                                    All usage = All.StaticDraw)
        {
            // Create the opengl vertex buffer object
            int buffer = -1;
            GL.GenBuffers(1, ref buffer);
#if DEBUG
            CheckError();
#endif
            // Store it for easy reference using the buffer name passed in
            Buffers.Add(bufferName, buffer);

            // Bind the vertex buffer for subsequent load of data
            GL.BindBuffer(All.ElementArrayBuffer, buffer);
#if DEBUG
            CheckError();
#endif
            // Calculate the correct size of the buffer based on sizeof int
            int size = sizeof(int) * bufferSize;

            // Load the vertex buffer with data
            GL.BufferData(All.ElementArrayBuffer,
                          new IntPtr(size),
                          bufferData,
                          usage);
#if DEBUG
            CheckError();
#endif

        }
        /// <summary>
        /// Simple bind of buffer only.
        /// 
        /// Useful when you want to bind one buffer to support multiple attribute arrays.
        /// </summary>
        /// <param name="bufferName"></param>
        public void BindVertexBuffer(string bufferName)
        {
            // Bind the vertex buffer for subseqent enable of attribute array to reference it
            GL.BindBuffer(All.ArrayBuffer, Buffers[bufferName]);
#if DEBUG
            CheckError();
#endif
        }

        /// <summary>
        /// Simple unbind of buffer only.
        /// </summary>
        /// <param name="bufferName"></param>
        public void UnbindVertexBuffer()
        {
            GL.BindBuffer(All.ArrayBuffer, 0);
#if DEBUG
            CheckError();
#endif
        }

        /// <summary>
        /// Simple unbind of buffer for indices only.
        /// </summary>
        /// <param name="bufferName"></param>
        public void UnbindVertexBufferForIndices()
        {
            GL.BindBuffer(All.ElementArrayBuffer, 0);
#if DEBUG
            CheckError();
#endif
        }

        /// <summary>
        /// Bind a vertex buffer to an attribute array.
        /// </summary>
        /// <param name="bufferName">The buffer name we used to reference the vertex buffer.</param>
        /// <param name="attribName">The attribute name we used to reference the attribute.</param>
        public void BindVertexBufferToAttribArray(string bufferName, string attribName)
        {
            // Bind the vertex buffer for subseqent enable of attribute array to reference it
            GL.BindBuffer(All.ArrayBuffer, Buffers[bufferName]);
#if DEBUG
            CheckError();
#endif
            // Have the attribute array reference the just bound vertex buffer
            GL.EnableVertexAttribArray(Attributes[attribName]);
#if DEBUG
            CheckError();
#endif
        }

        /// <summary>
        /// Bind a vertex buffer with indices so it can be used in a call to DrawElements.
        /// </summary>
        /// <param name="bufferName">The buffer name we used to reference the vertex buffer.</param>
        /// </summary>
        public void BindVertexBufferToIndices(string bufferName)
        {
            // Bind the vertex buffer so it can be used in a subsequent call to DrawElements
            GL.BindBuffer(All.ElementArrayBuffer, Buffers[bufferName]);
#if DEBUG
            CheckError();
#endif
        }

        public int GetBuffer(string name)
        {
            if (Buffers.ContainsKey(name))
            {
                return Buffers[name];
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Delete a vertex buffer we are tracking by name.
        /// </summary>
        /// <param name="name">The name of the verex buffer.
        /// Will be used to lookup the vertex buffer index to delete.</param>
        public void DeleteBuffer(string name)
        {
            int buffer = Buffers[name];
            GL.DeleteBuffers(1, new int[] { buffer });
#if DEBUG
            CheckError();
#endif
        }

        #endregion

        #region Textures

        public void LoadTexture(string textureName, Texture2D texture, bool IsRepeated = false, bool IsSmooth = true)
        {
#if DEBUG
            // Leaving in here for debugging purposes if needed
            int size = -1;
            GL.GetInteger(All.MaxTextureSize, ref size);
#endif

            // Genererate a texture id
            int id = -1;
            GL.GenTextures(1, ref id);
#if DEBUG
            CheckError();
#endif
            // Bind the texture id for the subsequent load process
            GL.BindTexture(All.Texture2D, id);
#if DEBUG
            CheckError();
#endif

            // Get a byte array of our texture
            var colors = TextureUtils.TextureToByteArray(texture);

            // Load the texture id just bound to
            GL.TexImage2D(All.Texture2D, 
                          0,
                          (int)All.Rgba, 
                          texture.Width,
                          texture.Height,
                          0,
                          All.Rgba, 
                          All.UnsignedByte,
                          colors);
#if DEBUG
            CheckError();
#endif

            // Setup filtering
            GL.TexParameter(All.Texture2D, All.TextureWrapS, IsRepeated ? Convert.ToInt32(All.Repeat) : Convert.ToInt32(All.ClampToEdge));
#if DEBUG
            CheckError();
#endif
            GL.TexParameter(All.Texture2D, All.TextureWrapT, IsRepeated ? Convert.ToInt32(All.Repeat) : Convert.ToInt32(All.ClampToEdge));
#if DEBUG
            CheckError();
#endif
            GL.TexParameter(All.Texture2D, All.TextureMagFilter, IsSmooth ? Convert.ToInt32(All.Linear) : Convert.ToInt32(All.Nearest));
#if DEBUG
            CheckError();
#endif
            GL.TexParameter(All.Texture2D, All.TextureMinFilter, IsSmooth ? Convert.ToInt32(All.Linear) : Convert.ToInt32(All.Nearest));
#if DEBUG
            CheckError();
#endif

            // Store away a string based key to our texture object
            Textures.Add(textureName, id);
        }

        /// <summary>
        /// Centralized handler to setup the connections between texture unit, texture object and uniform.
        /// </summary>
        /// <param name="textureUnit">The texture unit we want to bind with.</param>
        /// <param name="textureName">The name of our texture object we want to bind with.</param>
        /// <param name="uniformName">The name of the uniform we want to bind with.</param>
        public void BindTextureToUniform(int textureUnit, string textureName, string uniformName)
        {
            // Set active texture unit
            All activeTextureUnit;
            switch (textureUnit)
            {
                case 0:
                    {
                        activeTextureUnit = All.Texture0;
                        break;
                    }
                case 1:
                    {
                        activeTextureUnit = All.Texture1;
                        break;
                    }
                case 2:
                    {
                        activeTextureUnit = All.Texture2;
                        break;
                    }
                case 3:
                    {
                        activeTextureUnit = All.Texture3;
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
            GL.ActiveTexture(activeTextureUnit);
#if DEBUG
            CheckError();
#endif

            // Bind texture object to currently active texture unit
            GL.BindTexture(All.Texture2D, Textures[textureName]);
#if DEBUG
            CheckError();
#endif

            // Bind uniform sampler to texture unit
            GL.Uniform1(Uniforms[uniformName], textureUnit);
#if DEBUG
            CheckError();
#endif
        }

        #endregion
                
        #region Drawing

        /// <summary>
        /// A simplified version of DrawArrays.
        /// </summary>
        /// <param name="mode">The mode to draw (e.g., Triangle, TriangleList, etc.</param>
        /// <param name="count">The count of the vertices.</param>
        public void DrawArrays(All mode, int count)
        {
            GL.DrawArrays(mode, 0, count);
#if DEBUG
            CheckError();
#endif
        }

        /// <summary>
        /// A simplified version of DrawElements.
        /// </summary>
        /// <param name="mode">The mode to draw (e.g., Triangle, TriangleList, etc.</param>
        /// <param name="count">The count of the indices we will use to draw with.</param>
        /// <param name="indices">The offset into the vertex buffer that has just bound to hold indices.</param>
        public void DrawElements(All mode, int count, int indices)
        {
            GL.DrawElements(mode, count, All.UnsignedInt, new IntPtr(indices));
#if DEBUG
            CheckError();
#endif
        }

        #endregion

        #region Errors
        /// <summary>
        /// Clears out all previous errors.
        /// </summary>
        public static void ClearErrors()
        {
            while (GL.GetError() != All.NoError)
            {
                // No-op
            }
        }

        /// <summary>
        /// Check for the OpenGL Error
        /// </summary>
        public static void CheckError()
        {
            All errorCode = GL.GetError();

            if (errorCode == All.NoError)
            {
                return;
            }

            string error = "Unknown GL Error: ";
            string description = "No Description";

            // Decode the error code
            switch (errorCode)
            {
                case All.InvalidEnum:
                    {
                        error = "GL_INVALID_ENUM: ";
                        description = "An unacceptable value has been specified for an enumerated argument";
                        break;
                    }

                case All.InvalidValue:
                    {
                        error = "GL_INVALID_VALUE: ";
                        description = "A numeric argument is out of range";
                        break;
                    }

                case All.InvalidOperation:
                    {
                        error = "GL_INVALID_OPERATION: ";
                        description = "The specified operation is not allowed in the current state";
                        break;
                    }

                case All.OutOfMemory:
                    {
                        error = "GL_OUT_OF_MEMORY: ";
                        description = "There is not enough memory left to execute the command";
                        break;
                    }

                default:
                    {
                        error += "(" + errorCode.ToString() +")";
                        break;
                    }
            }

            throw new Exception(error + description);
        }

        #endregion

        #region Helper methods

        private void LoadShader(String code, All type, out int address)
        {
            address = GL.CreateShader(type);

#if DEBUG
            CheckError();
#endif

            GL.ShaderSource(address, 1, new string[] { code }, new int[] { code.Length });
#if DEBUG
            CheckError();
#endif

            GL.CompileShader(address);
#if DEBUG
            CheckError();
#endif
            int status = -1;
            GL.GetShader(address, All.CompileStatus, ref status);
#if DEBUG
            CheckError();
#endif

            if (status == (int)All.False)
            {
                int length = -1;
                GL.GetShader(address, All.InfoLogLength, ref length);
#if DEBUG
                CheckError();
#endif
                    
                StringBuilder infoLog = new StringBuilder(length);
                GL.GetShaderInfoLog(address, infoLog.Capacity, ref length, infoLog);
#if DEBUG
                CheckError();
#endif

                throw new Exception(infoLog.ToString());
            }

            GL.AttachShader(ProgramID, address);
#if DEBUG
            CheckError();
#endif
        }

        #endregion
    }
}