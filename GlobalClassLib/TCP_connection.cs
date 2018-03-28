using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalClassLib
{
    public static class TCP_connection
    {

        public static bool SendPacket(NetworkStream stream, Packet p)
        {
            try
            {
                byte[] data = PacketHandler.SerializePacket(p); //APcket to byte[] Encoding.ASCII.GetBytes(File.ReadAllText("test.txt"));  // 
                                                                //Anzahl bytes senden
                byte[] lengthBuffer = BitConverter.GetBytes(data.Length);
                if (lengthBuffer.Length > 4)
                {
                    throw new Exception("Zu viele Daten");
                }
                //senden
                stream.Write(lengthBuffer, 0, lengthBuffer.Length);
                //Daten senden
                stream.Write(data, 0, data.Length);
                stream.Flush();
                return true;
            }
            catch
            {
                throw new Exception("Fehler beim Senden (Timeout)");
            }
        }

        public static Packet WaitForPacket(NetworkStream stream)
        {
            MemoryStream ms = new MemoryStream();   //zwischenspweicher
            byte[] lengthBuffer = new byte[4];   //Zahl bis zu 16 bytes  //uint 32

            try
            {
                int read = stream.Read(lengthBuffer, 0, lengthBuffer.Length);

                int length = BitConverter.ToInt32(lengthBuffer, 0);
                byte[] buffer = new byte[1024]; //buffer wird mehrfach gelesen
                while (length > 0)
                {
                    read = stream.Read(buffer, 0, Math.Min(buffer.Length, length));
                    ms.Write(buffer, 0, read);
                    length -= read;
                }
                Packet response = PacketHandler.DeserializePacket(ms.ToArray());
                return response;

            }
            catch(Exception e)
            {
                throw new Exception("Timeout " + e.Message);
            }
            finally
            {
                ms.Dispose();
                stream.Flush();
            }
        }
    }
}
