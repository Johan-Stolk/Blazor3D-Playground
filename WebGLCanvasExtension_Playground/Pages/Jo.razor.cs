﻿using Blazor.Extensions;
using Blazor.Extensions.Canvas.WebGL;
using GLMatrixSharp;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using WebGLSharp;

namespace WebGLCanvasExtension_Playground.Pages
{
    public partial class Jo : ComponentBase
    {
        #region Variables Declaration
        //General use variables
        BECanvasComponent _canvasReference;
        WebGLContext _context;
        Dictionary<string, WebGLBuffer> _buffers;
        ShaderProgram _program;

        //Matrices used to display object
        float[] _projectionMatrix = Mat4.Perspective((float)(45 * Math.PI / 180), 640f / 640, 0.01f, 100f);
        float[] _modelViewMatrix;

        //FPS Counter variables
        DateTime _fpsLastTime = DateTime.Now;
        int _framesRendered;
        int _fpsDisplay;

        //Animation related varaibles
        DateTime _lastTime = DateTime.Now;
        double _deltaTime;
        float _xRotation = 0f; //For simplicity we are only animating the rotation around X axis
        float _xRotationSpeed = 0.2f;
        System.Timers.Timer _animationTimer;
        #endregion

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await SetupContext();
                //Demand each frame every 10 ms => 100fps. On my pc rendering takes about 7ms, so the browser is idle for 3ms
                _animationTimer = new System.Timers.Timer(10);
                _animationTimer.Elapsed += new ElapsedEventHandler(async (o, e) => await AnimationTick());
                _animationTimer.Start();
            }
        }

        async Task SetupContext()
        {
            _context = await this._canvasReference.CreateWebGLAsync(new WebGLContextAttributes
            {
                PowerPreference = WebGLContextAttributes.POWER_PREFERENCE_HIGH_PERFORMANCE
            });

            //Initial setup of settings
            await _context.ClearColorAsync(0.15f, 0.35f, 0.5f, 1);
            await _context.ClearAsync(BufferBits.COLOR_BUFFER_BIT);
            await _context.EnableAsync(EnableCap.DEPTH_TEST);
            await _context.EnableAsync(EnableCap.CULL_FACE);
            await _context.FrontFaceAsync(FrontFaceDirection.CCW);
            await _context.CullFaceAsync(Face.BACK);

            //ShaderProgram is provided by my library
            _program = await ShaderProgram.InitShaderProgram(_context, await Http.GetStringAsync("shaders/vs.glsl"), await Http.GetStringAsync("shaders/fs.glsl"));

            //Vertices positions are typed by hand so we can't use other helpers (like Mesh etc..) to initialize buffers
            _buffers = await InitBuffers(_context);
        }

        async Task<Dictionary<string, WebGLBuffer>> InitBuffers(WebGLContext gl)
        {
            var buffers = new Dictionary<string, WebGLBuffer>();

            #region Vertex Buffer
            var vertices = new[]
        {
                // Front face
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,

                // Back face
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,

                // Top face
                -1.0f,  1.0f, -1.5f,
                -1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f, -1.0f,

                // Bottom face
                -1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                // Right face
                 1.0f, -1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,

                // Left face
                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f, -1.0f
         };
            var vertexBuffer = await gl.CreateBufferAsync();
            await gl.BindBufferAsync(BufferType.ARRAY_BUFFER, vertexBuffer);
            await gl.BufferDataAsync(BufferType.ARRAY_BUFFER, vertices, BufferUsageHint.STATIC_DRAW);
            buffers.Add("position", vertexBuffer);

            #endregion

            #region Color Buffer
            var faceColors = new[]
    {
            new float[] {1.0f, 1.0f, 1.0f, 1.0f},    // Front face: white

            new float[] {1.0f, 0.0f, 0.0f, 1.0f},    // Back face: red

            new float[] {0.0f, 1.0f, 0.0f, 1.0f},    // Top face: green

            new float[] {0.0f, 0.0f, 1.0f, 1.0f},    // Bottom face: blue

            new float[] {1.0f, 1.0f, 0.0f, 1.0f},    // Right face: yellow

            new float[] {1.0f, 0.0f, 1.0f, 1.0f},    // Left face: purple
        };
            List<float> colors = new List<float>();
            //copying every face color for each of 4 verticies for 1 face
            foreach (var colorFace in faceColors)
            {
                for (int i = 0; i < 4; i++)
                {
                    colors.AddRange(colorFace);
                }
            }
            var colorBuffer = await gl.CreateBufferAsync();
            await gl.BindBufferAsync(BufferType.ARRAY_BUFFER, colorBuffer);
            await gl.BufferDataAsync(BufferType.ARRAY_BUFFER, colors.ToArray(), BufferUsageHint.STATIC_DRAW);
            buffers.Add("color", colorBuffer);

            #endregion

            #region Index Buffer
            short[] indices = new short[]
    {
                0,  1,  2,      0,  2,  3,    // front
                4,  5,  6,      4,  6,  7,    // back
                8,  9,  10,     8,  10, 11,   // top
                12, 13, 14,     12, 14, 15,   // bottom
                16, 17, 18,     16, 18, 19,   // right
                20, 21, 22,     20, 22, 23   // left
    };
            var indexBuffer = await gl.CreateBufferAsync();
            await gl.BindBufferAsync(BufferType.ELEMENT_ARRAY_BUFFER, indexBuffer);
            await gl.BufferDataAsync(BufferType.ELEMENT_ARRAY_BUFFER, indices, BufferUsageHint.STATIC_DRAW);
            buffers.Add("indices", indexBuffer);
            #endregion

            await gl.BindBufferAsync(BufferType.ARRAY_BUFFER, buffers.GetValueOrDefault("position"));
            await gl.VertexAttribPointerAsync(0, 3, DataType.FLOAT, false, 3 * sizeof(float), 0);
            await gl.EnableVertexAttribArrayAsync(0);

            await gl.BindBufferAsync(BufferType.ARRAY_BUFFER, buffers.GetValueOrDefault("color"));
            await gl.VertexAttribPointerAsync(1, 4, DataType.FLOAT, false, 4 * sizeof(float), 0);
            await gl.EnableVertexAttribArrayAsync(1);

            await gl.BindBufferAsync(BufferType.ELEMENT_ARRAY_BUFFER, buffers.GetValueOrDefault("indices"));
            await gl.UseProgramAsync(_program.Program);

            return buffers;
        }

        async Task DrawScene(Dictionary<string, WebGLBuffer> buffers, WebGLContext gl, WebGLProgram program)
        {
            //Clear old render
            await gl.ClearColorAsync(0.15f, 0.35f, 0.5f, 1);
            await gl.ClearAsync(BufferBits.COLOR_BUFFER_BIT | BufferBits.DEPTH_BUFFER_BIT);

            //Matrices manipulations
            //TODO: Refactor by only rotating the matrix during the render phase => only one method call
            _modelViewMatrix = Mat4.Create();
            _modelViewMatrix = Mat4.Translate(_modelViewMatrix, new float[] { -0.0f, 0.0f, -6f });
            _modelViewMatrix = Mat4.Rotate(_modelViewMatrix, (float)(_xRotation * Math.Tau), new float[] { 1f, 0f, 0f });
            _modelViewMatrix = Mat4.Rotate(_modelViewMatrix, (float)(0.1f * Math.Tau), new float[] { 0f, 1f, 0f });
            _modelViewMatrix = Mat4.Rotate(_modelViewMatrix, (float)(0.5f * Math.Tau), new float[] { 0f, 0f, 1f });

            //Send updated view matrices to the GPU
            await gl.UniformMatrixAsync(await gl.GetUniformLocationAsync(program, "uModelViewMatrix"), false, _modelViewMatrix);
            await gl.UniformMatrixAsync(await gl.GetUniformLocationAsync(program, "uProjectionMatrix"), false, _projectionMatrix);

            await gl.DrawElementsAsync(Primitive.TRIANGLES, 36, DataType.UNSIGNED_SHORT, 0);
        }

        async Task AnimationTick()
        {
            //TODO: Examine if try catch is necessary => it slows down the execution
            try
            {
                //Animation logic
                _deltaTime = (DateTime.Now - _lastTime).TotalSeconds;
                _lastTime = DateTime.Now;
                _xRotation += _xRotationSpeed * (float)_deltaTime;

                await DrawScene(_buffers, _context, _program.Program);

                //FPS counter logic
                _framesRendered++;
                if ((DateTime.Now - _fpsLastTime).TotalSeconds >= 1)
                {
                    // one second has elapsed
                    _fpsDisplay = _framesRendered;
                    _framesRendered = 0;
                    _fpsLastTime = DateTime.Now;
                    StateHasChanged();
                }
            }
            catch
            {
                Console.WriteLine("error in animation loop -> user probably navigated from the page");
            }
        }

        public void Dispose()
        {
            _animationTimer.Stop();
            _animationTimer.Dispose();
        }

    }
}
