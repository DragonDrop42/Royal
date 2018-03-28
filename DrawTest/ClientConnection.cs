using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GlobalClassLib;

namespace DrawTest
{
    public class ClientConnection
    {
        //async server model++++++++++++++++++++++++++++++++++++++++++
        class ClientContext //clientData
        {
            public TcpClient Client;
            public NetworkStream Stream;
            public byte[] Buffer = new byte[4]; //liest anzahl bytes
            public MemoryStream Message = new MemoryStream(); //buffert data
            public ManualResetEvent sendDone = new ManualResetEvent(false);
        }

        ClientContext clientData;
        EventHandler<string> errorEvent;
        EventHandler<Packet> packetReceivedE;

        public ClientConnection(string ip, int port, EventHandler<Packet> PacketReceivedEvent, EventHandler<string> ErrorEvent)
        {
            try
            {
                //Fehlerausgabe
                errorEvent = ErrorEvent;
                packetReceivedE = PacketReceivedEvent;

                //Connect
                clientData = new ClientContext();
                clientData.Client = new TcpClient(ip, port);
                clientData.Stream = clientData.Client.GetStream();

                //Thread listen = new Thread(ListenToServer);
                //listen.Start();

                clientData.Stream.BeginRead(clientData.Buffer, 0, clientData.Buffer.Length, OnDataRead, clientData);

                errorEvent.Invoke(this, "Connected");
            }
            catch (Exception e)
            {
                errorEvent.Invoke(this, e.Message);
            }
        }

        public void SendPacket(Packet p)
        {
            try
            {
                //Antwort senden++++++++++++++++++++++++++++++++
                clientData.sendDone.Reset();
                byte[] data = PacketHandler.SerializePacket(p);
                byte[] lengthBytes = BitConverter.GetBytes(data.Length);

                clientData.Stream.BeginWrite(lengthBytes, 0, lengthBytes.Length, OnClientWrite, clientData);
                clientData.sendDone.WaitOne(); //warten

                clientData.sendDone.Reset();

                clientData.Stream.BeginWrite(data, 0, data.Length, OnClientWrite, clientData);
                clientData.sendDone.WaitOne();
                //Console.WriteLine("Packet: " + p.PacketType + " wurde gesendet");
                clientData.Stream.Flush();
            }
            catch (Exception e)
            {
                errorEvent.Invoke(this, "SendPacket " + e.Message);
                CloseConnection(clientData);
            }
        }

        void OnClientWrite(IAsyncResult ar)
        {
            ClientContext ClientData = ar.AsyncState as ClientContext;
            if (ClientData == null) { return; }

            ClientData.Stream.EndWrite(ar);
            ClientData.Stream.Flush();
            ClientData.sendDone.Set();
        }


        public async void ListenToServer()
        {
            try
            {
                while (clientData.Client.Connected)
                {
                    int read = await clientData.Stream.ReadAsync(clientData.Buffer, 0, clientData.Buffer.Length);
                    int length = BitConverter.ToInt32(clientData.Buffer, 0);
                    byte[] buffer = new byte[1024]; //buffer wird mehrfach gelesen
                    while (length > 0)
                    {
                        read = await clientData.Stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, length));
                        clientData.Message.Write(buffer, 0, read);
                        length -= read;
                    }
                    //clientData.Stream.Flush();  //Stream leeren !!!
                    Packet response = PacketHandler.DeserializePacket(clientData.Message.ToArray());
                    clientData.Stream.Flush();
                    clientData.Message.Flush();
                    packetReceivedE.Invoke(this, response);
                }
            }
            catch (Exception e)
            {
                errorEvent.Invoke(this, "Listen to Server " + e.Message);
            }
        }

        //test+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        private void OnDataRead(IAsyncResult ar)
        {
            ClientContext clientData = ar.AsyncState as ClientContext;
            if (clientData == null)
                return;

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

                //Packet entpacken+++++++++++++++++++++++++++++
                Packet response = PacketHandler.DeserializePacket(clientData.Message.ToArray());
                clientData.Message.Dispose();   //stream löschen
                clientData.Message = new MemoryStream();

                packetReceivedE.Invoke(this, response); //Packet übergeben
            }
            catch (Exception exc)
            {
                errorEvent.Invoke(this, "On data read " +  exc.Message);
                CloseConnection(clientData);
            }
            finally
            {
                if (clientData.Client.Connected)
                {
                    clientData.Stream.BeginRead(clientData.Buffer, 0, clientData.Buffer.Length, OnDataRead, clientData);    //selbstaufruf
                }
            }
        }
        private void CloseConnection(ClientContext clientData)
        {
            clientData.Client.Close();
            clientData.Stream.Dispose();
            clientData.Message.Dispose();
            clientData = null;
        }
    }
}
        

