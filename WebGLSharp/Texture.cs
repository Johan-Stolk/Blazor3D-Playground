using Blazor.Extensions.Canvas.WebGL;
using System;
using System.Threading.Tasks;

namespace WebGLSharp
{
    public class Texture
    {
        WebGLContext _gl;
        WebGLTexture _texture;
        int _width, _height;

        public static async Task<Texture> BuildAsync(WebGLContext gl, int[] image, int width = 100, int height = 100)
        {
            var texture = await gl.CreateTextureAsync();
            await gl.BindTextureAsync(TextureType.TEXTURE_2D, texture);
            //TODO: Create a new version without width&height
            await gl.TexImage2DAsync(Texture2DType.TEXTURE_2D, 0, PixelFormat.RGBA, width, height, 0, PixelFormat.RGBA, PixelType.UNSIGNED_BYTE, image);
            // WebGL1 has different requirements for power of 2 images vs non power of 2 images
            //if ((width & (width - 1)) == 0 && (height & (height - 1)) == 0)
            //{
            //    await gl.GenerateMipmapAsync(TextureType.TEXTURE_2D);
            //}
            //else
            //{
            //await gl.TexParameterAsync(TextureType.TEXTURE_2D, TextureParameter.TEXTURE_MAG_FILTER, (int)TextureParameterValue.LINEAR);
            await gl.TexParameterAsync(TextureType.TEXTURE_2D, TextureParameter.TEXTURE_MIN_FILTER, (int)TextureParameterValue.LINEAR);
            await gl.TexParameterAsync(TextureType.TEXTURE_2D, TextureParameter.TEXTURE_WRAP_S, (int)TextureParameterValue.CLAMP_TO_EDGE);
            await gl.TexParameterAsync(TextureType.TEXTURE_2D, TextureParameter.TEXTURE_WRAP_T, (int)TextureParameterValue.CLAMP_TO_EDGE);
            return new Texture(gl, texture, width, height);
        }

        public static async Task<Texture> LoadUrl(WebGLContext gl, string url)
        {
            var texture = await gl.CreateTextureAsync();
            await gl.BindTextureAsync(TextureType.TEXTURE_2D, texture);

            // Because images have to be downloaded over the internet
            // they might take a moment until they are ready.
            // Until then put a single pixel in the texture so we can
            // use it immediately. When the image has finished downloading
            // we'll update the texture with the contents of the image.
            int level = 0;
            PixelFormat internalFormat = PixelFormat.RGBA;
            int width = 1;
            int height = 1;
            int border = 0;
            PixelFormat srcFormat = PixelFormat.RGBA;
            PixelType srcType = PixelType.UNSIGNED_BYTE;
            ushort[] pixel = new ushort[] { 0, 0, 255, 255 };  // opaque blue
            await gl.TexImage2DAsync(Texture2DType.TEXTURE_2D, level, internalFormat,
                          width, height, border, srcFormat, srcType,
                          pixel);
            /*
            var image = new Image();
            image.onload = function() {
                gl.bindTexture(gl.TEXTURE_2D, texture);
                gl.texImage2D(gl.TEXTURE_2D, level, internalFormat,
                              srcFormat, srcType, image);

                // WebGL1 has different requirements for power of 2 images
                // vs non power of 2 images so check if the image is a
                // power of 2 in both dimensions.
                if (isPowerOf2(image.width) && isPowerOf2(image.height))
                {
                    // Yes, it's a power of 2. Generate mips.
                    gl.generateMipmap(gl.TEXTURE_2D);
                }
                else
                {
                    // No, it's not a power of 2. Turn off mips and set
                    // wrapping to clamp to edge
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                }
            };
            image.src = url;
            */

            return new Texture(gl, texture, width, height);
        }

        public async Task UpdateData(int[] image)
        {
            await _gl.BindTextureAsync(TextureType.TEXTURE_2D, _texture);
            await _gl.TexImage2DAsync(Texture2DType.TEXTURE_2D, 0, PixelFormat.RGBA, _width, _height, 0, PixelFormat.RGBA, PixelType.UNSIGNED_BYTE, image);
            //await _gl.TexParameterAsync(TextureType.TEXTURE_2D, TextureParameter.TEXTURE_MIN_FILTER, (int)TextureParameterValue.LINEAR);
            //await _gl.TexParameterAsync(TextureType.TEXTURE_2D, TextureParameter.TEXTURE_WRAP_S, (int)TextureParameterValue.CLAMP_TO_EDGE);
            //await _gl.TexParameterAsync(TextureType.TEXTURE_2D, TextureParameter.TEXTURE_WRAP_T, (int)TextureParameterValue.CLAMP_TO_EDGE);
        }

        internal Texture(WebGLContext gl, WebGLTexture texture, int width, int height)
        {
            _gl = gl;
            _texture = texture;
            _width = width;
            _height = height;
        }

        internal async Task UseAsync(WebGLUniformLocation uniform, int binding)
        {
            await _gl.ActiveTextureAsync((Blazor.Extensions.Canvas.WebGL.Texture)Enum.Parse(typeof(Blazor.Extensions.Canvas.WebGL.Texture), "TEXTURE" + binding));
            await _gl.BindTextureAsync(TextureType.TEXTURE_2D, _texture);
            await _gl.UniformAsync(uniform, binding);
        }

        internal static Texture Load(string textureFileContent)
        {
            throw new NotImplementedException();
        }
    }
}
