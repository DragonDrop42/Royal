using GlobalClassLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class GameHandler
    {
        //public List<GameObject> Lst_ameObjects { get; set; }
        public List<Player> Lst_PlayerObj { get; set; }
        public List<Bullet> Lst_BulletObj { get; set; }


        static int[][] map = new int[200][];    //0-... Textures


        //Events
        public ManualResetEvent UpdateNecessary = new ManualResetEvent(false);  //Update durchführen
        private bool SomethingChanged = false;

        public GameHandler()
        {
            Init(); //map erzeugen

            Lst_PlayerObj = new List<Player>();
            Lst_BulletObj = new List<Bullet>();

            Thread gameLoop = new Thread(GameLoopHandler);
            gameLoop.Start();
        }
        
        private static void Init()
        {
            for (int i = 0; i < map.Length; i++)
            {
                int[] row = new int[200];
                for (int j = 0; j < 200; j++)
                {
                    row[j] = 255;
                }
                map[i] = row;
            }
        }

        const int GameTimeMiliseconds = 10;     //gameTime++++++++++++++++++++++++++++++++++++++++++++++++

        private void GameLoopHandler()
        {
            Stopwatch sw = new Stopwatch();
            while (true)
            {
                sw.Reset();
                sw.Start();

                bool update = GameLoop();   //This takes Time

                sw.Stop();

                Console.WriteLine("Time needed for GameLoop: " + sw.Elapsed);

                if (sw.ElapsedMilliseconds < GameTimeMiliseconds)
                {
                    Thread.Sleep(GameTimeMiliseconds - sw.Elapsed.Milliseconds);    //hällt Framerate stabil
                }

                if (update || SomethingChanged) //if update necesarry
                {
                    //Update packet an alle Client senden
                    UpdateNecessary.Set();
                    SomethingChanged = false;
                }
            }
        }
        //GameLoop++++++++++++++++++++++++++++++++++++++++++++++++++
        private bool GameLoop()
        {
            try
            {
                bool update = false;
                //MoveBullets
                List<Object> Lst_removeObj = new List<object>();
                Parallel.ForEach(Lst_BulletObj, bullet =>
                {
                    bullet.Lifetime--;
                    if (bullet.Lifetime <= 0)
                    {
                        Lst_removeObj.Add(bullet);
                        Console.WriteLine("Bullet remove");
                    }
                    else
                    {
                        bullet.Position = new Vector(bullet.Position.X + bullet.Dir.X, bullet.Position.Y + bullet.Dir.Y);
                        update = true;
                    }
                });
                //remove old Bullets
                Parallel.ForEach(Lst_removeObj, b =>
                {
                    Lst_BulletObj.Remove((Bullet)b);
                    update = true;
                });
                Lst_removeObj = new List<object>();

                return update;
            }
            catch
            {
                Console.WriteLine("GameLoop Error-----------------------");
                return false;
            }
        }


        //Add Objects to Game+++++++++++++++++++++++++++++++++++++++++++++++

        public void AddPlayer(string id)
        {
            Player obj = new Player
            {
                Position = new Vector(0, 0),
                Rotation = 0f,
                GameObjType = GameObjType.Player,
                Name = "tmpName",
                ID = id //mit ID verbinden
            };
            Lst_PlayerObj.Add(obj);
        }

        public void AddBullet(Bullet b)
        {
            //Server setup
            b.Lifetime = 100;
            //add to List
            Lst_BulletObj.Add(b);
        }

        //Packet verarbeiten++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public Packet PacketManager(Packet request, string ID)
        {
            //finde gameObject
            Player player = Lst_PlayerObj.Find(o => o.ID == ID);

            Packet responseP = null;
            switch (request.PacketType)
            {
                case PacketType.Default:
                    Console.WriteLine("Work ++++++++++++++++++++++++");
                    break;

                case PacketType.LoginRequest:
                    player.Name = request.PlayerObj.Name;
                    //..........
                    responseP = new Packet
                    {
                        PacketType = PacketType.ID_Response,
                        stringData = new string[] { player.ID }
                    };
                    break;

                case PacketType.PlayerMove:
                    CheckCollision();
                    player.Position = request.PlayerObj.Position;
                    player.Rotation = request.PlayerObj.Rotation;
                    SomethingChanged = true;  //zum aktualisieren
                    break;

                case PacketType.AddBullet:
                    AddBullet(request.BulletObj);
                    break;

                default:
                    break;
            }

            return responseP;
        }

        //Physik und Kollision+++++++++++++++++++++++++++++++++++++++++++++++++++++
        private void CheckCollision()
        {
            //throw new NotImplementedException();
        }
    }
}
