using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Net;
using System.Data;
using System.Collections.Specialized;
using ZeroFormatter;

namespace GlobalClassLib
{
    public enum PacketType  //Für alle sichtbar
    {
        Default,
        LoginRequest,
        ID_Response,

        PlayerMove,
        AddBullet,

        //Server
        UpdateAllObjects
    }
    public enum GameObjType
    {
        Default,
        Player,
        Bullet
    }

    public enum BulletType
    {
        Default,
        Small,
        Big
    }

    public enum ItemType
    {
        Default
    }

    [ZeroFormattable]
    public class Packet
    {
        // Index is key of serialization
        [Index(0)]
        public virtual PacketType PacketType { get; set; }

        [Index(1)]
        public virtual int[][] Map { get; set; }

        [Index(2)]
        public virtual string[] stringData { get; set; }

        //Single Objects
        [Index(3)]
        public virtual Player PlayerObj { get; set; }
        [Index(4)]
        public virtual Bullet BulletObj { get; set; }

        //List Objects
        [Index(5)]
        public virtual List<Player> LstPlayerObj { get; set; }
        [Index(6)]
        public virtual List<Bullet> LstBulletObj { get; set; }
    }


    [ZeroFormattable]
    public class GameObject
    {
        [Index(7)]
        public virtual GameObjType GameObjType { get; set; }

        [Index(8)]
        public virtual Vector Position { get; set; }

        [Index(9)]
        public virtual string ID { get; set; }

        [IgnoreFormat]
        public virtual int CircleColiderRad { get; set; }
    }
    [ZeroFormattable]
    public class Bullet : GameObject 
    {
        [Index(10)]
        public virtual float Speed { get; set; }

        [Index(11)]
        public virtual Vector Dir { get; set; }

        [Index(12)]
        public virtual BulletType BulletType { get; set; }

        [IgnoreFormat]
        public virtual int Lifetime { get; set; }
    }

    [ZeroFormattable]
    public class Player : GameObject
    {
        [Index(13)]
        public virtual string Name { get; set; }

        [Index(14)]
        public virtual List<Item> Lst_Items { get; set; }

        [Index(15)]
        public virtual float Rotation { get; set; }

        [Index(16)]
        public virtual int Life { get; set; }
    }
    [ZeroFormattable]
    public class Item : GameObject
    {
        [Index(17)]
        public virtual ItemType ItemType { get; set; }
    }
}
