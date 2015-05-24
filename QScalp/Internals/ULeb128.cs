#region Copyright (c) 2011-2015 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System.IO;

namespace QScalp.History.Internals
{
  static class ULeb128
  {
    // **********************************************************************

    public const uint Max1BValue = 127;
    public const uint Max2BValue = 16383;
    public const uint Max3BValue = 2097151;
    public const uint Max4BValue = 268435455;

    // **********************************************************************

    public static void Write(Stream stream, uint value)
    {
      while(value > 127)
      {
        stream.WriteByte((byte)(value | 0x80));
        value >>= 7;
      }

      stream.WriteByte((byte)value);
    }

    // **********************************************************************

    public static uint Read(Stream stream)
    {
      uint value = 0;
      int shift = 0;

      for(; ; )
      {
        uint b = (uint)stream.ReadByte();

        if(b == 0xffffffff)
          throw new EndOfStreamException();

        value |= (b & 0x7f) << shift;

        if((b & 0x80) == 0)
          return value;

        shift += 7;
      }
    }

    // **********************************************************************
  }
}
