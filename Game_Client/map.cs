using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal
{
    class map
    {
        public int[][] mapint;
        public Texture2D Texture;
        public List<Texture2D> TexList = new List<Texture2D>();
        public List<List<Texture2D>> TexMap = new List<List<Texture2D>>();
        public Vector2 TileSize;

        public void Initialize(int[][] map,Texture2D Texture,int H,int W,GraphicsDevice gd)
        {
            mapint = map;
            this.Texture = Texture;
            TileSize = new Vector2(Texture.Width / W, Texture.Height / H);

            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    Rectangle newBounds = new Rectangle(
                        Convert.ToInt16(x * TileSize.X), 
                        Convert.ToInt16(y * TileSize.Y), 
                        Convert.ToInt16(TileSize.X), 
                        Convert.ToInt16(TileSize.Y));
                    Texture2D croppedTexture = new Texture2D(gd, newBounds.Width, newBounds.Height);
                    Color[] data = new Color[newBounds.Width * newBounds.Height];
                    Texture.GetData(0, newBounds, data, 0, newBounds.Width * newBounds.Height);
                    croppedTexture.SetData(data);
                    TexList.Add(croppedTexture);
                }
            }
            for (int x = 0; x < mapint.Length; x++)
            {
                TexMap.Add(new List<Texture2D>());
                for (int y = 0; y < mapint[x].Length; y++)
                {
                    TexMap[x].Add(TexList[mapint[x][y]]);
                }
            }
        }

        public void update()
        {

        }

        public void Draw(SpriteBatch sb)
        {
            for (int x = 0; x < TexMap.Count; x++)
            {
                for (int y = 0; y < TexMap[x].Count; y++)
                {
                    sb.Draw(TexMap[x][y], new Vector2(TileSize.X*x,TileSize.Y*y),null, Color.White);
                }
            }
        }
    }
}
