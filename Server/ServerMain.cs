using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GlobalClassLib;
using System.Windows;

namespace Server
{
    class Program
    {
        //async server model++++++++++++++++++++++++++++++++++++++++++
        class ClientContext //clientData
        {
            public TcpClient Client;
            public NetworkStream Stream;
            public byte[] Buffer = new byte[4]; //liest anzahl bytes
            public MemoryStream Message = new MemoryStream(); //buffert data
            public ManualResetEvent sendDone = new ManualResetEvent(false);

            public string ID = "";  //verbindung zum GameObject
        }

        //public static ManualResetEvent acceptDone = new ManualResetEvent(false);  //fertig gesendet
        //public static ManualResetEvent sendDone = new ManualResetEvent(false);  //fertig gesendet

        static List<ClientContext> lst_connectedClients = new List<ClientContext>();

        static GameHandler game;

        static void Main(string[] args)
        {
            //Server starten
            Console.WriteLine("Server startet on " + GlobalMethods.GetIPAddress() + "\n");
            //Game setup
            game = new GameHandler();

            Thread listen = new Thread(StartListening);
            listen.Start();

            //endlessloop
            SendClientUpdate();

            Console.Read();
        }

        private static void SendClientUpdate()
        {
            try
            {
                while (true)
                {
                    game.UpdateNecessary.WaitOne();

                    Packet sendAll = new Packet
                    {
                        PacketType = PacketType.UpdateAllObjects,
                        LstPlayerObj = game.Lst_PlayerObj,
                        LstBulletObj = game.Lst_BulletObj
                    };

                    Parallel.For(0, lst_connectedClients.Count,
                       index =>
                       {
                           SendPacket(lst_connectedClients[index], sendAll);
                       });

                    game.UpdateNecessary.Reset();
                    //Thread.Sleep(4);    //Puffer
                }
            }
            catch
            {
                Console.WriteLine("ClientUpdate Error--------------------");
            }
        }

        private static void StartListening()
        {
            int port = 4444;
            IPAddress localAddr = IPAddress.Parse(GlobalMethods.GetIPAddress());
            TcpListener listener = new TcpListener(localAddr, port);
            listener.Start();

            listener.BeginAcceptTcpClient(OnClientAccepted, listener);

            //Console.WriteLine("=>Press enter to close the Server");
            //Console.ReadLine();
            //listener.Stop();
        }

        static void OnClientAccepted(IAsyncResult ar)
        {
            TcpListener listener = ar.AsyncState as TcpListener;
            if (listener == null)
            {
                return;
            }
            try
            {
                ClientContext clientData = new ClientContext();
                clientData.Client = listener.EndAcceptTcpClient(ar);
                clientData.Stream = clientData.Client.GetStream();
                //new ID
                clientData.ID = Guid.NewGuid().ToString();
                //Add GameObject player to Game
                game.AddPlayer(clientData.ID);
                //Add Client to List
                lst_connectedClients.Add(clientData);

                //Start listen to Client
                clientData.Stream.BeginRead(clientData.Buffer, 0, clientData.Buffer.Length, OnClientRead, clientData);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.WriteLine("wait for new Client");
                listener.BeginAcceptTcpClient(OnClientAccepted, listener);  //nächsten Client verbinden
            }
        }

        static void OnClientRead(IAsyncResult ar)
        {
            ClientContext clientData = ar.AsyncState as ClientContext;
            if (clientData == null)
            {
                return;
            }

            try
            {
                int read = clientData.Stream.EndRead(ar);

                int length = BitConverter.ToInt32(clientData.Buffer, 0);
                //Console.WriteLine("length: " + length);

                byte[] buffer = new byte[1024]; //buffer wird mehrfach gelesen
                while (length > 0)
                {
                    read = clientData.Stream.Read(buffer, 0, Math.Min(buffer.Length, length));
                    clientData.Message.Write(buffer, 0, read);
                    length -= read;
                }
                clientData.Stream.Flush();  //Stream leeren !!!
                
                //Packet bearbeiten+++++++++++++++++++++++++++++
                Packet response = OnPacketReceived(clientData);
                
                if (response != null)
                {
                    SendPacket(clientData, response);
                    Console.WriteLine("´Response wurde gesendet => " + response.PacketType);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                CloseConnection(clientData);
            }
            finally
            {
                //clientData.Client.Close();
                //clientData.Stream.Dispose();
                clientData.Buffer = new byte[4];
                if (clientData.Client.Connected)
                {
                    try
                    {
                        clientData.Stream.BeginRead(clientData.Buffer, 0, clientData.Buffer.Length, OnClientRead, clientData);
                    }
                    catch
                    {
                        Console.WriteLine("clienTReadExceptionm--------");
                    }
                }
                else
                {
                    Console.WriteLine("--------Client Disconnected");
                }
                
            }
        }

        static Packet OnPacketReceived(ClientContext context)
        {
            try
            {
                Packet request = PacketHandler.DeserializePacket(context.Message.ToArray());
                context.Message.Dispose();  //stream löschen
                context.Message = new MemoryStream();
                //packet empfangen
                Console.WriteLine("Packet: " + request.PacketType + " wurde empfangen");
                //Console.WriteLine(Encoding.ASCII.GetString(context.Message.ToArray()));
                Packet response = game.PacketManager(request, context.ID);  //packet an Game weiterleiten
                return response;
            }
            catch (Exception e)
            {
                throw new Exception("-----------------Error OnPacketReceived " + e.Message);
            }
        }

        //SendToClient
        static void SendPacket(ClientContext clientData, Packet p)
        {
            try
            {
                if (!clientData.Client.Connected)
                {
                    Console.WriteLine("Client disconnected");
                    lst_connectedClients.Remove(clientData);
                }

                //Antwort senden++++++++++++++++++++++++++++++++
                clientData.sendDone.Reset();
                byte[] data = PacketHandler.SerializePacket(p);
                byte[] lengthBytes = BitConverter.GetBytes(data.Length);

               // Console.WriteLine("*********************" + lengthBytes.Length);

                clientData.Stream.BeginWrite(lengthBytes, 0, lengthBytes.Length, OnClientWrite, clientData);
                clientData.sendDone.WaitOne(); //warten

                clientData.sendDone.Reset();

                clientData.Stream.BeginWrite(data, 0, data.Length, OnClientWrite, clientData);
                clientData.sendDone.WaitOne();
                Console.WriteLine("Packet: " + p.PacketType + " wurde gesendet");

                clientData.Stream.Flush();
            }
            catch
            {
                CloseConnection(clientData);
            }
        }

        static void OnClientWrite(IAsyncResult ar)
        {
            ClientContext ClientData = ar.AsyncState as ClientContext;
            if (ClientData == null) { return; }

            ClientData.Stream.EndWrite(ar);
            ClientData.sendDone.Set();
        }
        //Verbindung trennen
        private static void CloseConnection(ClientContext clientData)
        {
            //GameClient löschen
            game.RemoveClientFromGame(clientData.ID);

            //Close Socket
            clientData.Client.Close();
            clientData.Stream.Dispose();
            clientData.Message.Dispose();
            lst_connectedClients.Remove(clientData);
            clientData = null;
        }
       
    }
}

