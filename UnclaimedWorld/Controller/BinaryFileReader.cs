using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UWGame.Control.Replays;

namespace UWGame.Control
{
    public class BinaryFileReader
    {
        private System.IO.BinaryReader reader;
        //public IFormatter serializationReader = new BinaryFormatter();
        
        private long readPosition;
        private long documentLength;
        FileStream inStream;
        
        public BinaryFileReader(string filePath)
        {
            inStream = File.OpenRead(filePath);
            reader = new System.IO.BinaryReader(inStream);

            //serializationReader.Deserialize

            documentLength = inStream.Length;
            readPosition = 0;
        }
        /*
        MyObject obj = new MyObject();
        obj.n1 = 1;
        obj.n2 = 24;
        obj.str = "Some String";
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream("MyFile.bin", 
                                 FileMode.Create, 
                                 FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, obj);
        stream.Close();
         */
        /*public object Deserialize()
        {
            return serializationReader.Deserialize(inStream);
        }*/
        public bool HasReachedEndOfFile()
        {
            if (readPosition < documentLength)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool ReadBool()
        {
            AdvanceReadPosition(sizeof(bool));
            return reader.ReadBoolean();
        }
        public int ReadInt()
        {
            AdvanceReadPosition(sizeof(int));
            return reader.ReadInt32();
        }
        public long ReadLong()
        {
            AdvanceReadPosition(sizeof(long));
            return reader.ReadInt64();
        }
        public float ReadFloat()
        {
            AdvanceReadPosition(sizeof(float));
            return reader.ReadSingle();
        }
        public short ReadShort()
        {
            AdvanceReadPosition(sizeof(short));
            return reader.ReadInt16();
        }
        /*public char[] ReadChars(short charCount)
        {
            AdvanceReadPosition(sizeof(char) * charCount);
            return reader.ReadChars((int)charCount);
        }*/
        public char ReadChar()
        {
            AdvanceReadPosition(sizeof(char));
            return reader.ReadChar();
        }
        private void AdvanceReadPosition(long amountToAdvance)
        {
            readPosition += amountToAdvance;
            if (readPosition > documentLength)
            {
                throw new Exception("Tried to read outside file!");
            }
        }
        public void Close()
        {
            reader.Close();
        }
    }
}
