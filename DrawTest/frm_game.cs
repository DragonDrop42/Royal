using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GlobalClassLib;
using System.Threading;

namespace DrawTest
{
    public partial class frm_game : Form
    {
        ClientConnection client;

        //game
        private string ID;

        public frm_game()
        {
            InitializeComponent();
;
            //setup Connect to Server
            int port = 4444;
            string localAddr = (GlobalMethods.GetIPAddress());

            client = new ClientConnection(localAddr, port, InvokeForm, OnError);

            SendLoginRequest();
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

        //packet empfangen+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        private void InvokeForm(object sender, Packet packet)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { OnpacketReceived(packet); });
            }
            else
            {
                OnpacketReceived(packet);
            }
        }
        private void OnpacketReceived(Packet packet)
        {
            try {
                switch (packet.PacketType)
                {
                    case PacketType.LoginRequest:
                        ID = packet.stringData[0];
                        break;
                    case PacketType.UpdateAllObjects:
                        DrawAllObjects(packet);
                        break;
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("draw" + e.Message);
            }
        }

        private void DrawAllObjects(Packet packet)
        {
            Bitmap bmp = new Bitmap(200, 200);

            Graphics g = Graphics.FromImage(bmp);

            foreach (Player playerObj in packet.LstPlayerObj)
            {
                g.FillEllipse(Brushes.Blue, playerObj.Position.X, playerObj.Position.Y, 10, 10);
            }

            foreach (Bullet bulletObj in packet.LstBulletObj)
            {
                g.FillEllipse(Brushes.Blue, bulletObj.Position.X, bulletObj.Position.Y, 2, 2);
            }

            p_draw.Image = bmp;
            p_draw.Refresh();
        }
        //----------------------------------------------------------------------------

        private void p_draw_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Packet packet = new Packet
                {
                    PacketType = PacketType.PlayerMove,
                    PlayerObj = new Player
                    {
                        GameObjType = GameObjType.Player,
                        Position = new Vector( e.X, e.Y )
                        //Name = "Player_One"
                    }
                };

                client.SendPacket(packet);
            }
        }

        
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++Error
        private void OnError(object sender, string e)
        {
            MessageBox.Show(e);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void p_draw_MouseDown(object sender, MouseEventArgs e)
        {
            Packet packet = new Packet
            {
                PacketType = PacketType.AddBullet,
                BulletObj = new Bullet
                {
                    GameObjType = GameObjType.Bullet,
                    Position = new Vector(e.X, e.Y),
                    Dir = new Vector(0.5f, -0.5f)
                    //Name = "Player_One"
                }
            };

            client.SendPacket(packet);
        }
    }
}
