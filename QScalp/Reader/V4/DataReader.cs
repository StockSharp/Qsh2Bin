#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System.IO;
using QScalp.History.Internals;

namespace QScalp.History.Reader.V4
{
  sealed class DataReader : BinaryReader
  {
    // **********************************************************************

    public DataReader(Stream stream) : base(stream) { }

    // **********************************************************************

    public long ReadGrowing(long lastValue)
    {
      uint offset = ULeb128.Read(BaseStream);

      if(offset == ULeb128.Max4BValue)
        return lastValue + Leb128.Read(BaseStream);
      else
        return lastValue + offset;
    }

    // **********************************************************************

    public long ReadLeb128() { return Leb128.Read(BaseStream); }

    // **********************************************************************
  }
}
