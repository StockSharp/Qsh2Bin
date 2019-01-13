#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;
using QScalp.History.Internals;

namespace QScalp.History.Reader.V4
{
  sealed class OwnTradesStream : QshStream, IOwnTradesStream
  {
    // **********************************************************************

    long lastMilliseconds;
    long lastTradeId;
    long lastOrderId;
    int lastPrice;

    // **********************************************************************

    public Security Security { get; private set; }
    public event Action<OwnTrade> Handler;

    // **********************************************************************

    public OwnTradesStream(DataReader dr)
      : base(StreamType.OwnTrades, dr)
    {
      Security = new Security(dr.ReadString());
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      lastMilliseconds = dr.ReadGrowing(lastMilliseconds);

      lastTradeId += dr.ReadLeb128();
      lastOrderId += dr.ReadLeb128();
      lastPrice += (int)dr.ReadLeb128();

      int quantity = (int)dr.ReadLeb128();

      if(push && Handler != null)
        Handler(new OwnTrade(
          OwnTradeSource.History, DateTimeHelper.FromMs(lastMilliseconds),
          lastTradeId, lastOrderId, lastPrice, quantity));
    }

    // **********************************************************************
  }
}
