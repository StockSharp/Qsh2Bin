#region Copyright (c) 2011-2013 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;
using System.IO;

namespace QScalp.History.Writer
{
  sealed class DataWriter : BinaryWriter
  {
    // **********************************************************************

    readonly bool multistreamed;
    DateTime baseDateTime;

    // **********************************************************************

    public int RecordsCount { get; private set; }

    // **********************************************************************

    public DataWriter(Stream output, DateTime baseDateTime, bool multistreamed)
      : base(output)
    {
      this.baseDateTime = baseDateTime;
      this.multistreamed = multistreamed;
    }

    // **********************************************************************

    public void WriteRecHeader(int sid, DateTime dateTime)
    {
      WriteDateTime(dateTime, ref baseDateTime);

      if(multistreamed)
        Write((byte)sid);

      RecordsCount++;
    }

    // **********************************************************************

    public void WriteDateTime(DateTime dateTime, ref DateTime baseDateTime)
    {
      double offset = (dateTime - baseDateTime).TotalMilliseconds;

      if(offset >= ushort.MaxValue || offset < ushort.MinValue)
      {
        baseDateTime = dateTime;

        Write(ushort.MaxValue);
        Write(dateTime.Ticks);
      }
      else
        Write((ushort)offset);
    }

    // **********************************************************************

    public void WriteRelative(int value, ref int baseValue)
    {
      int offset = value - baseValue;

      if(offset <= sbyte.MinValue || offset > sbyte.MaxValue)
      {
        baseValue = value;

        Write(sbyte.MinValue);
        Write(value);
      }
      else
        Write((sbyte)offset);
    }

    // **********************************************************************

    public void WritePackInt(int value)
    {
      int prefix;

      if(value >= 0)
        prefix = 0;
      else
      {
        prefix = 0x80;
        value = ~value;
      }

      int bytes;

      if(value < 0xff)
        bytes = 0;
      else if(value < 0xffff)
        bytes = 1;
      else if(value < 0xffffff)
        bytes = 2;
      else
        bytes = 3;

      int pv = value >> (bytes << 3);
      int pvbits = 0;

      int tmp = pv;
      while(tmp > 0)
      {
        pvbits++;
        tmp >>= 1;
      }

      if(bytes + pvbits > 6)
        bytes++;
      else
        prefix |= pv;

      switch(bytes)
      {
        case 1: prefix |= 0x40; break;
        case 2: prefix |= 0x60; break;
        case 3: prefix |= 0x70; break;
        case 4: prefix |= 0x78; break;
      }

      Write((byte)prefix);

      for(int i = 0; i < bytes; i++)
      {
        Write((byte)value);
        value >>= 8;
      }
    }

    // **********************************************************************
  }
}
