using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class ObjectEventArgs : EventArgs
    {
        private Action _Success = () => { };
        private Action _Fail = () => { };

        public Stack<ObjectEventArgs> Trace = new Stack<ObjectEventArgs>();
        public INetwork Network;
        public byte[] Data;
        public Message.Types Type;
        public GameObject Sender, Target;
        public object[] Parameters;
        public Action Success { get { return _Success; } set { _Success = value; } }
        public Action Fail { get { return _Fail; } set { _Fail = value; } }
        public Vector3 Face;

        public ObjectEventArgs()
        {
            this.Parameters = new object[0];
            this.Data = new byte[0];
        }
        
        public ObjectEventArgs(Message.Types type)
        {
            this.Target = null;
            this.Parameters = new object[0];
            this.Sender = null;
            Type = type;
            this.Face = Vector3.Zero;
        }
        public ObjectEventArgs(Message.Types type, GameObject sender, params object[] parameters)
        {
            this.Target = null;
            this.Face = Vector3.Zero;
            Type = type;
            Sender = sender;
            Parameters = parameters;
        }
        public ObjectEventArgs(Message.Types type, Vector3 face, GameObject sender, params object[] parameters)
        {
            this.Target = null;
            this.Face = face;
            Type = type;
            Sender = sender;
            Parameters = parameters;
        }
        public ObjectEventArgs(Message.Types type, Vector3 face, GameObject sender, GameObject target, params object[] parameters)
        {
            this.Face = face;
            Type = type;
            Sender = sender;
            Parameters = parameters;
            this.Target = target;
        }

        static public ObjectEventArgs Create(Message.Types type, object[] parameters)
        {
            return new ObjectEventArgs() { Type = type, Parameters = parameters };
        }
        static public ObjectEventArgs Create(Message.Types type, byte[] data)
         {
             return new ObjectEventArgs() { Type = type, Data = data};
         }
        static public ObjectEventArgs Create(Message.Types type, Action<BinaryWriter> writer)
         {
            using BinaryWriter w = new(new MemoryStream());
            writer(w);
            return new ObjectEventArgs() { Type = type, Data = (w.BaseStream as MemoryStream).ToArray() };
        }
        
        public static void Write(BinaryWriter writer, Message.Types type, Action<BinaryWriter> argsWriter)
         {
             writer.Write((byte)type);
             argsWriter(writer);
         }

        public ObjectEventArgs Read(INetwork net, BinaryReader reader)
        {
            this.Type = (Message.Types)reader.ReadByte();
            // WARNING!!! better prefix manually
            int dataLength = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
            this.Data = reader.ReadBytes(dataLength);
            this.Network = net;
            return this;
        }

        public void Translate(Action<BinaryReader> read)
        {
            this.Data.Translate(this.Network, read);
        }
    }
}
