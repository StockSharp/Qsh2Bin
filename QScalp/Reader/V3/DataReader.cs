#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;
using System.IO;

using QScalp.History.Internals;

namespace QScalp.History.Reader.V3
{
  sealed class DataReader : BinaryReader
  {
    // **********************************************************************

    public DataReader(Stream stream) : base(stream) { }

    // **********************************************************************

    public DateTime ReadDateTime(ref DateTime baseDateTime)
    {
      int offset = ReadUInt16();

      if(offset == ushort.MaxValue)
        return baseDateTime = new DateTime(ReadInt64());
      else
        return baseDateTime.AddMilliseconds(offset);
    }

    // **********************************************************************

    public int ReadRelative(ref int baseValue)
    {
      int offset = ReadSByte();

      if(offset == sbyte.MinValue)
        return baseValue = ReadInt32();
      else
        return baseValue + offset;
    }

    // **********************************************************************

    public int ReadPackInt()
    {
      int prefix = ReadByte();
      int bytes, value;

      if((prefix & 0x78) == 0x78)
      {
        bytes = 4;
        value = (prefix & 0x7) << 32;
      }
      else if((prefix & 0x70) == 0x70)
      {
        bytes = 3;
        value = (prefix & 0xf) << 24;
      }
      else if((prefix & 0x60) == 0x60)
      {
        bytes = 2;
        value = (prefix & 0x1f) << 16;
      }
      else if((prefix & 0x40) == 0x40)
      {
        bytes = 1;
        value = (prefix & 0x3f) << 8;
      }
      else
      {
        bytes = 0;
        value = prefix & 0x7f;
      }

      for(int i = 0; i < bytes; i++)
        value |= ReadByte() << (i << 3);

      if((prefix & 0x80) == 0x80)
        value = ~value;

      return value;
    }

    // **********************************************************************

    public long ReadRelative(ref long lastValue)
    {
      int offset = ReadSByte();

      if(offset == sbyte.MinValue)
        return lastValue = Leb128.Read(BaseStream);
      else
        return lastValue += offset;
    }

    // **********************************************************************
  }
}
