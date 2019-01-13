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

namespace QScalp.History.Writer
{
  sealed class OwnTradesStream
  {
    // **********************************************************************

    readonly DataWriter dw;
    readonly int sid;

    long lastMilliseconds;
    long lastTradeId;
    long lastOrderId;
    int lastPrice;

    // **********************************************************************

    public OwnTradesStream(DataWriter dw, int sid, Security s)
    {
      this.dw = dw;
      this.sid = sid;

      dw.Write((byte)StreamType.OwnTrades);
      dw.Write(s.Entry);
    }

    // **********************************************************************

    public void Write(DateTime dateTime, OwnTrade trade)
    {
      dw.WriteRecHeader(sid, dateTime);

      dw.WriteGrowing(DateTimeHelper.ToMs(trade.DateTime), ref lastMilliseconds);

      dw.WriteLeb128(trade.TradeId - lastTradeId);
      lastTradeId = trade.TradeId;

      dw.WriteLeb128(trade.OrderId - lastOrderId);
      lastOrderId = trade.OrderId;

      dw.WriteLeb128(trade.Price - lastPrice);
      lastPrice = trade.Price;

      dw.WriteLeb128(trade.Quantity);
    }

    // **********************************************************************
  }
}
