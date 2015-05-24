#region Copyright (c) 2011-2015 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

namespace QScalp.History.Reader.V3
{
  abstract class QshStream : IQshStream
  {
    // **********************************************************************

    public StreamType Type { get; private set; }

    // **********************************************************************

    protected readonly DataReader dr;

    // **********************************************************************

    protected QshStream(StreamType type, DataReader dr)
    {
      this.Type = type;
      this.dr = dr;
    }

    // **********************************************************************

    public abstract void Read(bool push);

    // **********************************************************************
  }
}
