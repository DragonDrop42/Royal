using GlobalClassLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Royal
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        DrawPlayer player = new DrawPlayer();
        Texture2D cursor;
        map Map = new map();
        private ClientConnection client;
        private string ID;
        Packet DrawPacket;
        private Texture2D playertexture;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            // TODO: Add your initialization logic here
            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y);
            playertexture = Content.Load<Texture2D>("Player/player");
            cursor = Content.Load<Texture2D>("Player/cursor");
            Texture2D MapTex = Content.Load<Texture2D>("Map");
            int[][] intMap = new int[10][];
            int i = 0;

            for(int x = 0; x < intMap.Length; x++)
	        {
                intMap[x] = new int[10];
		        for(int y= 0; y < intMap[x].Length;y++){
                    intMap[x][y] = i;
                }
            }
            Map.Initialize(intMap, MapTex , 20, 27, GraphicsDevice);
            
            int port = 4444;
            string localAddr = (GlobalMethods.GetIPAddress());
            client = new ClientConnection(localAddr, port, PaketIn, OnError);
            SendLoginRequest();

            player.Initialize(Content.Load<Texture2D>("Player/player"), playerPosition,client);

            base.Initialize();
        }

        private void OnError(object sender, string e)
        {
            System.Console.WriteLine(e);
        }

        private void PaketIn(object sender, Packet p)
        {
            switch (p.PacketType)
            {
                case PacketType.LoginRequest:
                    ID = p.stringData[0];
                    player.ID = ID;
                    break;
                case PacketType.UpdateAllObjects:
                    DrawPacket = p;
                    break;
            }
        }

        private void SendLoginRequest()
        {
            Packet request = new Packet
            {
                PacketType = PacketType.LoginRequest,
                PlayerObj = new Player
                {
                    Name = "moritz"
                }
            };
            client.SendPacket(request);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            player.Update();
            
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            Map.Draw(spriteBatch);
            Vector2 cursorCenter = new Vector2(cursor.Width/2,cursor.Height/2);
            DrawGameObject();
            spriteBatch.Draw(cursor, Mouse.GetState().Position.ToVector2(), null, Color.White, 0f, cursorCenter, 0.1f, SpriteEffects.None, 0f);
            spriteBatch.End();
            // TODO: Add your drawing code here
            base.Draw(gameTime);
        }

        public void DrawGameObject()
        {
            if (DrawPacket == null)
                return;
            Vector2 centerOffset = new Vector2(playertexture.Width / 2, playertexture.Height / 2);
            foreach (Player pl in DrawPacket.LstPlayerObj)
            {
                Vector2 pos = new Vector2(pl.Position.X, pl.Position.Y);
                spriteBatch.Draw(playertexture, pos, null, Color.White, pl.Rotation, centerOffset, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}
