using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalClassLib;

namespace Royal
{
    class DrawPlayer
    {
        public Texture2D Texture;
        public Vector2 Position;
        public int speed = 10;
        public float angle;
        public Vector2 centerOffset;
        public ClientConnection Client;
        public string ID;
        public float oldangle;

        public void Initialize(Texture2D Texture,Vector2 Position, ClientConnection Client)
        {
            this.Texture = Texture;
            this.Position = Position;
            centerOffset = new Vector2(Texture.Width / 2, Texture.Height / 2);
            this.Client = Client;
        }

        public void Update()
        {
            Vector2 anglel = this.Position - Mouse.GetState().Position.ToVector2();
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                Client.SendPacket(new Packet
                {
                    PacketType = PacketType.AddBullet,
                    BulletObj = new Bullet
                    {
                        Position = new Vector (this.Position.X,this.Position.Y),
                        Dir = new Vector(anglel.X,anglel.Y)
                        
                    }
                });


            Vector2 MousePos = Mouse.GetState().Position.ToVector2();
            oldangle = angle;
            angle = Convert.ToSingle(Math.Atan2(MousePos.Y - Position.Y - centerOffset.Y, MousePos.X - Position.X - centerOffset.X));
            if (Keyboard.GetState().GetPressedKeys().Length == 0)
                return;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                Position.X -= speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                Position.X += speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                Position.Y -= speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                Position.Y += speed;

            Client.SendPacket(new Packet
            {
                PacketType = PacketType.PlayerMove,
                PlayerObj = new Player
                {
                    GameObjType = GameObjType.Player,
                    Position = new Vector(Position.X, Position.Y),
                    Rotation = this.angle
                    //Name = "Player_One"
                }
            });
        }
    }
}
