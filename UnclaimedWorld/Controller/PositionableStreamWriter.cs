using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace UWGame.Contol
{
    class PositionableStreamWriter
    {
        public StreamWriter writer;
        public FileStream fileStream;
       
        public PositionableStreamWriter(string filePath)
        {
            fileStream = File.Create(filePath);
            writer = new StreamWriter(fileStream);
        }

        public void SetPositionFromCurrentPosition(int position)
        {
            writer.BaseStream.Seek(position, SeekOrigin.Current);
        }
        public void SetPositionFromEnd(int position)
        {
            writer.BaseStream.Seek(position, SeekOrigin.End);
        }
        public void SetPositionFromBeginning(int position)
        {
            writer.BaseStream.Seek(position, SeekOrigin.Begin);
        }
        public void Close()
        {
            writer.Close();
        }
    }
}
