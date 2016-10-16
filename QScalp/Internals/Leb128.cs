#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
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
  static class Leb128
  {
    // **********************************************************************

    public const int Min1BValue = -64;
    public const int Max1BValue = -Min1BValue - 1;

    public const int Min2BValue = -8192;
    public const int Max2BValue = -Min2BValue - 1;

    public const int Min3BValue = -1048576;
    public const int Max3BValue = -Min3BValue - 1;

    public const int Min4BValue = -134217728;
    public const int Max4BValue = -Min4BValue - 1;

    public const long Min5BValue = -17179869184;
    public const long Max5BValue = -Min5BValue - 1;

    public const long Min6BValue = -2199023255552;
    public const long Max6BValue = -Min6BValue - 1;

    public const long Min7BValue = -281474976710656;
    public const long Max7BValue = -Min7BValue - 1;

    public const long Min8BValue = -36028797018963968;
    public const long Max8BValue = -Min8BValue - 1;

    public const long Min9BValue = -4611686018427387904;
    public const long Max9BValue = -Min9BValue - 1;

    // **********************************************************************

    public static void Write(Stream stream, long value)
    {
      if(value >= 0)
        for(; ; )
        {
          int b = (int)(value & 0x7f);
          value >>= 7;

          if(value == 0 && (b & 0x40) == 0)
          {
            stream.WriteByte((byte)b);
            return;
          }

          stream.WriteByte((byte)(b | 0x80));
        }
      else
        for(; ; )
        {
          int b = (int)(value & 0x7f);
          value >>= 7;

          if(value == -1 && (b & 0x40) != 0)
          {
            stream.WriteByte((byte)b);
            return;
          }

          stream.WriteByte((byte)(b | 0x80));
        }
    }

    // **********************************************************************

    public static long Read(Stream stream)
    {
      long value = 0;
      int shift = 0;

      for(; ; )
      {
        int b = stream.ReadByte();

        if(b == -1)
          throw new EndOfStreamException();

        value |= (long)(b & 0x7f) << shift;
        shift += 7;

        if((b & 0x80) == 0)
        {
          if(shift < sizeof(long) * 8 && (b & 0x40) != 0)
            value |= -(1L << shift);

          return value;
        }
      }
    }

    // **********************************************************************
  }
}
