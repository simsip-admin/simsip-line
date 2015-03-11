using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Simsip.LineRunner.Utils
{
    public class TextureUtils
    {
        public static Color[,] TextureTo2DColorArray(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);

            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colors2D[x, y] = colors1D[x + y * texture.Width];

            return colors2D;
        }

        public static byte[] TextureToByteArray(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);

            byte[] colors2D = new byte[4 * (texture.Width * texture.Height)];
            int colors2DCounter = 0;
            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    Color pixelColor = colors1D[x + (y * texture.Width)];

                    colors2D[colors2DCounter++] = pixelColor.R;
                    colors2D[colors2DCounter++] = pixelColor.G;
                    colors2D[colors2DCounter++] = pixelColor.B;
                    colors2D[colors2DCounter++] = pixelColor.A;
                }
            }

            return colors2D;
        }



    }
}