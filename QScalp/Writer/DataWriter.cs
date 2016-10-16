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

namespace QScalp.History.Writer
{
  sealed class DataWriter : BinaryWriter
  {
    // **********************************************************************

    readonly bool multistreamed;
    long lastMilliseconds;

    // **********************************************************************

    public int RecordCount { get; private set; }

    // **********************************************************************

    public DataWriter(Stream output, DateTime recDateTime, bool multistreamed)
      : base(output)
    {
      this.lastMilliseconds = DateTimeHelper.ToMs(recDateTime);
      this.multistreamed = multistreamed;
    }

    // **********************************************************************

    public void WriteRecHeader(int sid, DateTime dateTime)
    {
      WriteGrowing(DateTimeHelper.ToMs(dateTime), ref lastMilliseconds);

      if(multistreamed)
        Write((byte)sid);

      RecordCount++;
    }

    // **********************************************************************

    public void WriteLeb128(long value) { Leb128.Write(BaseStream, value); }

    // **********************************************************************

    public void WriteGrowing(long value, ref long lastValue)
    {
      long offset = value - lastValue;

      if(offset >= ULeb128.Max4BValue || offset < 0)
      {
        ULeb128.Write(BaseStream, ULeb128.Max4BValue);
        Leb128.Write(BaseStream, offset);
      }
      else
        ULeb128.Write(BaseStream, (uint)offset);

      lastValue = value;
    }

    // **********************************************************************
  }
}
