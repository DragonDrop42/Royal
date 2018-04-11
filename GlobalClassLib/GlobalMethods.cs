using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalClassLib
{
    public static class GlobalMethods
    {
        public delegate void ErrorMessageCallback(string s);    //standard errorDelegate
        public delegate void UpdateFormCallback(Packet p);    //statndard update

        //password Hash
        private static string salt = "492";   //random seed

        public static void SpeedTest()
        {
            Thread.Sleep(2000);
        }

        public static string passwordToHash(string pass)
        {
            if (pass.Length < 5)
            {
                throw new Exception("Passwort muss mindestens 5 Zeichen lang sein.");   //keine Passwörter unter 5 zeiichen
            }
            //passwort verschlüsslung
            System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create();
            byte[] preHash = Encoding.UTF32.GetBytes(pass + salt);
            byte[] hash = sha.ComputeHash(preHash);
            string password = Convert.ToBase64String(hash, 0, hash.Length);  //immer 15 Stellen lang
            //
            //errorCallback(password);
            return password;
        }

        public static bool check_email(string email)
        {
            //check
            if (email.Contains('@') == false)// || (email.Contains(".com")||email.Contains(".net")||email.Contains(".de")) == false)
            {
                throw new Exception("Die eingetragene email ist fehlerhaft!");
            }
            return true;
        }

        public static string GetIPAddress()
        {
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1"; //Loopback
        }

        
    }
}
