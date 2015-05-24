#region Copyright (c) 2011-2015 Николай Морошкин, http://www.moroshkin.com/
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
  sealed class TradesStream
  {
    // **********************************************************************

    readonly DataWriter dw;
    readonly int sid;

    long lastMilliseconds;
    long lastTradeId;
    long lastOrderId;
    int lastPrice;

    // **********************************************************************

    public TradesStream(DataWriter dw, int sid, Security s)
    {
      this.dw = dw;
      this.sid = sid;

      dw.Write((byte)StreamType.Trades);
      dw.Write(s.Entry);
    }

    // **********************************************************************

    public void Write(DateTime dateTime, OwnTradeReply reply)
    {
      dw.WriteRecHeader(sid, dateTime);

      dw.WriteGrowing(DateTimeHelper.ToMs(reply.DateTime), ref lastMilliseconds);

      dw.WriteLeb128(reply.TradeId - lastTradeId);
      lastTradeId = reply.TradeId;

      dw.WriteLeb128(reply.OrderId - lastOrderId);
      lastOrderId = reply.OrderId;

      dw.WriteLeb128(reply.Price - lastPrice);
      lastPrice = reply.Price;

      dw.WriteLeb128(reply.Quantity);
    }

    // **********************************************************************
  }
}
