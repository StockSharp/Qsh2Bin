#region Copyright (c) 2011-2013 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System.IO;

namespace QScalp.History.Reader.V3
{
  abstract class QshStream3 : IQshStream
  {
    // **********************************************************************

    public StreamType Type { get; private set; }

    // **********************************************************************

    protected readonly BinaryReader br;

    // **********************************************************************

    protected QshStream3(StreamType type, BinaryReader br)
    {
      this.Type = type;
      this.br = br;
    }

    // **********************************************************************

    public abstract void Read(bool push);

    // **********************************************************************
  }
}
