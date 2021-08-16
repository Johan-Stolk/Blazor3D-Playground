using Blazor.Extensions;
using Blazor.Extensions.Canvas.WebGL;
using GLMatrixSharp;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WebGLSharp;
using WebGLSharp.YoaGames;

namespace WebGLCanvasExtension_Playground.Pages
{
    public partial class CameraMouseControlSample : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; }

        BECanvasComponent _canvasReference;
        ShaderProgram _shaderProgram;
        private Random rnd;
        Mesh _cylinderMesh;
        Light _light;

        bool _isDragging = false;
        double _lastX = 0, _lastY = 0;
        double _rotationX = 0, _rotationY = 0;

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                gl = await this._canvasReference.CreateWebGLAsync(new WebGLContextAttributes
                {
                    PreserveDrawingBuffer = true,
                    PowerPreference = WebGLContextAttributes.POWER_PREFERENCE_HIGH_PERFORMANCE
                });
                await gl.ClearColorAsync(0.1f, 0.1f, 0.3f, 1);
                await gl.EnableAsync(EnableCap.DEPTH_TEST);
                await gl.ClearAsync(BufferBits.COLOR_BUFFER_BIT | BufferBits.DEPTH_BUFFER_BIT);
                await gl.EnableAsync(EnableCap.CULL_FACE);
                await gl.FrontFaceAsync(FrontFaceDirection.CCW);
                await gl.CullFaceAsync(Face.BACK);
                _shaderProgram = await ShaderProgram.InitShaderProgram(gl,
            await Http.GetStringAsync("shaders/basic.vert"),
            await Http.GetStringAsync("shaders/basic.frag"),
            new List<string>() { "position", "normal", "uv" },
            new List<string>() { "model", "projection", "ambientLight", "lightDirection", "diffuse" });

                rnd = new Random();
                //geometry = Geometry.ParseObjFile(await Http.GetStringAsync("models/susan.obj"));

                geometry = SpaceModels.cuirck().ToGeometry();
#if WASRNDCOLOR  || true
                var textureData = new int[40000];
                for (int i = 0; i < textureData.Length; i = i + 4)
                {
                    //Colors between 0-255
                    textureData[i] = rnd.Next(40, 250);
                    textureData[i + 1] = rnd.Next(40, 250);
                    textureData[i + 2] = rnd.Next(40, 250);
                    textureData[i + 3] = 255;
                }
                var texture = await WebGLSharp.Texture.BuildAsync(gl, textureData);
#else
                int dim = 8;
                var textureData = new int[dim * dim * 4];
                int i = 0;
                for (int u = 0; u < dim; u++)
                {
                    for (int v = 0; v < dim; v++)
                    {
                        bool on = (v & 1) == 1 || (u & 1) == 0;
                        textureData[i++] = 255;
                        textureData[i++] = on ? 255 : 0;
                        textureData[i++] = on ? 255 : 0;
                        textureData[i++] = 255;
                    }
                }
                var texture = await WebGLSharp.Texture.BuildAsync(gl, textureData, dim, dim);
#endif
                await MakePos2(gl, rnd, geometry, 0);
                //var initialPosition = Mat4.Translate(Mat4.Create(), new float[] { -0.0f, 0.0f, -6f });
                _cylinderMesh = await Mesh.BuildAsync(gl, geometry, texture);
                _light = new Light();

                await DrawSceneAsync();
            }
        }

        private async Task MakePos2(WebGLContext gl, Random rnd, Geometry geometry, float factor)
        {
            float[] posar = geometry.GetPositions();
            for (int j = 0; j < posar.Length; j++)
                posar[j] += (float)Math.Cos(posar[j]) * factor;
            pos2 = await VBO.BuildAsync(gl, posar.Length / 3, posar);
        }

        VBO pos2;
        private Geometry geometry;
        private WebGLContext gl;

        private async Task DrawSceneAsync()
        {
            await _shaderProgram.GlContext.ClearAsync(BufferBits.COLOR_BUFFER_BIT | BufferBits.DEPTH_BUFFER_BIT);
            await _shaderProgram.GlContext.UseProgramAsync(_shaderProgram.Program);
            await _light.UseAsync(_shaderProgram);
            var projectionMatrix = Mat4.Perspective((float)(45 * Math.PI / 180), 1f, 0.1f, 100f);
            projectionMatrix = Mat4.Translate(projectionMatrix, new float[] { 0, 0, -6f });
            projectionMatrix = Mat4.Rotate(projectionMatrix, (float)_rotationX, new float[3] { 1f, 0, 0f });
            projectionMatrix = Mat4.Rotate(projectionMatrix, (float)_rotationY, new float[3] { 0, 1f, 0f });
            await _shaderProgram.GlContext.UniformMatrixAsync(_shaderProgram.Uniforms.GetValueOrDefault("projection"), false, projectionMatrix);
            if (_isDragging)
            {
                await _cylinderMesh.DrawAsync2(_shaderProgram, pos2, true);
            }
            else
                await _cylinderMesh.DrawAsync(_shaderProgram);
        }

        private void MouseDown(MouseEventArgs e)
        {
            _isDragging = true;
            _lastX = e.ClientX;
            _lastY = e.ClientY;
        }

        private void MouseUp(MouseEventArgs e)
        {
            _isDragging = false;
        }

        private async Task MouseMove(MouseEventArgs e)
        {
            var x = e.ClientX;
            var y = e.ClientY;
            if (_isDragging)
            {
                var factor = 10d / _canvasReference.Height;
                var dx = factor * (x - _lastX);
                var dy = factor * (y - _lastY);
                _rotationX += dy;
                _rotationY += dx;
                // await MakePos2(gl, rnd, geometry, (float)_rotationX);
                await DrawSceneAsync();
            }
            _lastX = x;
            _lastY = y;
        }
    }
}
