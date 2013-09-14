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

using QScalp.History.Internals;

namespace QScalp.History.Writer
{
  sealed class QshWriter : IDisposable
  {
    // **********************************************************************

    const byte FileVersion = 3;

    readonly FileStream fs;
    readonly DataWriter dw;

    int streamsCount;

    // **********************************************************************

    public int RecordsCount { get { return dw.RecordsCount; } }
    public long FileSize { get { return fs.Length; } }

    // **********************************************************************

    public QshWriter(string path, string appName, string comment,
      DateTime recDateTime, int streamsCount)
    {
      if(streamsCount > byte.MaxValue)
        throw new OverflowException("Слишком много потоков для записи");

      fs = new FileStream(path, FileMode.Create, FileAccess.Write);
      dw = new DataWriter(fs, recDateTime, streamsCount > 1);

      DataFile.WritePrefix(dw, FileVersion);

      dw.Write(appName);
      dw.Write(comment);
      dw.Write(recDateTime.Ticks);
      dw.Write((byte)streamsCount);
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

    public StockStream CreateStockStream(Security s)
    {
      return new StockStream(dw, streamsCount++, s);
    }

    public DealsStream CreateDealsStream(Security s)
    {
      return new DealsStream(dw, streamsCount++, s);
    }

    public OrdersStream CreateOrdersStream(Security s)
    {
      return new OrdersStream(dw, streamsCount++, s);
    }

    public TradesStream CreateTradesStream(Security s)
    {
      return new TradesStream(dw, streamsCount++, s);
    }

    public MessagesStream CreateMessagesStream()
    {
      return new MessagesStream(dw, streamsCount++);
    }

    public AuxInfoStream CreateAuxInfoStream(Security s)
    {
      return new AuxInfoStream(dw, streamsCount++, s);
    }

    // **********************************************************************
  }
}
