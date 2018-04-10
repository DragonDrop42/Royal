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

        List<string> Lst_removeClientsIDs = new List<string>();

        public void RemoveClientFromGame(string id)
        {
            Lst_removeClientsIDs.Add(id);
        }

        const int GameTimeMiliseconds = 10;     //gameTime++++++++++++++++++++++++++++++++++++++++++++++++

        private void GameLoopHandler()
        {
            Stopwatch sw = new Stopwatch();
            while (true)
            {
                sw.Reset();
                sw.Start();

                GameLoop();   //This takes Time

                sw.Stop();

                //Console.WriteLine("Time needed for GameLoop: " + sw.Elapsed);

                if (sw.ElapsedMilliseconds < GameTimeMiliseconds)
                {
                    Thread.Sleep(GameTimeMiliseconds - sw.Elapsed.Milliseconds);    //hällt Framerate stabil
                }

                if (SomethingChanged) //if update necesarry
                {
                    //Update packet an alle Client senden
                    UpdateNecessary.Set();
                    SomethingChanged = false;
                }
            }
        }
        //GameLoop++++++++++++++++++++++++++++++++++++++++++++++++++
        private void GameLoop()
        {
            try
            {
                HandleBullets();

                //Spieler entfernen
                if(Lst_removeClientsIDs.Count > 0)
                {
                    foreach(string id in Lst_removeClientsIDs)
                    {
                        Player p_remove = Lst_PlayerObj.Single(p => p.ID == id);
                        Lst_PlayerObj.Remove(p_remove);
                    }
                    Lst_removeClientsIDs.Clear();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("GameLoop Error-----------------------" + e.Message);
            }
        }

        private void HandleBullets()
        {
            //MoveBullets
            List<Object> Lst_removeObj = new List<object>();
            Parallel.ForEach(Lst_BulletObj, bullet =>
            {
                bullet.Lifetime--;
                if (bullet.Lifetime <= 0)
                {
                    Lst_removeObj.Add(bullet);
                }
                else
                {
                    //Move
                    bullet.Position = new Vector(bullet.Position.X + (bullet.Dir.X * bullet.Speed), bullet.Position.Y + (bullet.Dir.Y * bullet.Speed));

                    //check Collision
                    Player objHit = (Player)CheckCollision_List_Object(Lst_PlayerObj.ConvertAll(x => (GameObject)x), bullet);
                    if(objHit != null)  //hit
                    {
                        objHit.Life--;
                        Console.WriteLine("Hit++++++++++++ " + objHit.Life);

                        //remove Bullet
                        Lst_removeObj.Add(bullet);
                    }

                    SomethingChanged = true;
                }
            });
            //remove old Bullets
            Parallel.ForEach(Lst_removeObj, b =>
            {
                Lst_BulletObj.Remove((Bullet)b);
                Console.WriteLine("Bullet remove");
                SomethingChanged = true;
            });
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
                Life = 100,
                //collision
                CircleColiderRad = 15,
                ID = id //mit ID verbinden
            };
            Lst_PlayerObj.Add(obj);
        }

        public void AddBullet(Bullet b, string id)
        {
            //Server setup
            b.Lifetime = 100;
            b.Speed = 4;
            b.CircleColiderRad = 8;
            b.ID = id;
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
                    AddBullet(request.BulletObj, ID);
                    break;

                default:
                    break;
            }

            return responseP;
        }

        //Physik und Kollision+++++++++++++++++++++++++++++++++++++++++++++++++++++
        private GameObject CheckCollision_List_Object(List<GameObject> lstGameObj, GameObject obj2)
        {
            GameObject collision = null;
            Parallel.ForEach(lstGameObj, obj =>
            {
                if ((CalculateDist(obj, obj2) < (obj.CircleColiderRad + obj2.CircleColiderRad)) && (obj.ID != obj2.ID))     //man kann sich nicht selber treffen
                {
                    collision = obj;    //erste Kollision
                    return;
                }
            });
            return collision;
        }

        private void CheckCollision()
        {
            //throw new NotImplementedException();
        }

        private float CalculateDist(GameObject o1, GameObject o2)
        {
            float dx = Math.Abs(o1.Position.X - o2.Position.X);
            float dy = Math.Abs(o1.Position.Y - o2.Position.Y);

            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            return dist;
        }
    }
}
