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
  sealed class QshWriter : IDisposable
  {
    // **********************************************************************

    const byte fileVersion = 4;

    readonly Stream fs;
    readonly DataWriter dw;

    int streamCount;

    // **********************************************************************

    public int RecordCount { get { return dw.RecordCount; } }
    public long FileSize { get { return fs.Length; } }

    // **********************************************************************

    public QshWriter(string path, bool compress, string appName,
      string comment, DateTime recDateTime, int streamCount)
    {
      if(streamCount < 0)
        throw new ArgumentOutOfRangeException("streamCount");

      if(streamCount > byte.MaxValue)
        throw new OverflowException("Слишком много потоков для записи");

      fs = QshFile.Create(path, compress, fileVersion);
      dw = new DataWriter(fs, recDateTime, streamCount > 1);

      dw.Write(appName);
      dw.Write(comment);
      dw.Write(recDateTime.Ticks);
      dw.Write((byte)streamCount);
    }

    // **********************************************************************

    public void Dispose()
    {
      if(dw != null)
        dw.Dispose();

      if(fs != null)
        fs.Dispose();
    }

    // **********************************************************************

    public QuotesStream CreateQuotesStream(Security s)
    {
      return new QuotesStream(dw, streamCount++, s);
    }

    public DealsStream CreateDealsStream(Security s)
    {
      return new DealsStream(dw, streamCount++, s);
    }

    public OwnOrdersStream CreateOwnOrdersStream(Security s)
    {
      return new OwnOrdersStream(dw, streamCount++, s);
    }

    public OwnTradesStream CreateOwnTradesStream(Security s)
    {
      return new OwnTradesStream(dw, streamCount++, s);
    }

    public MessagesStream CreateMessagesStream()
    {
      return new MessagesStream(dw, streamCount++);
    }

    public AuxInfoStream CreateAuxInfoStream(Security s)
    {
      return new AuxInfoStream(dw, streamCount++, s);
    }

    public OrdLogStream CreateOrdLogStream(Security s)
    {
      return new OrdLogStream(dw, streamCount++, s);
    }

    // **********************************************************************
  }
}
