#region Copyright (c) 2011-2015 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;

namespace QScalp.History.Reader.V3
{
  sealed class TradesStream : QshStream, ITradesStream
  {
    // **********************************************************************

    DateTime baseDateTime;
    int basePrice;

    // **********************************************************************

    public Security Security { get; private set; }
    public event Action<int, OwnTradeReply> Handler;

    // **********************************************************************

    public TradesStream(DataReader dr)
      : base(StreamType.Trades, dr)
    {
      Security = new Security(dr.ReadString());
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      long orderId = dr.ReadInt64();
      DateTime dateTime = dr.ReadDateTime(ref baseDateTime);
      int price = dr.ReadRelative(ref basePrice);
      int quantity = dr.ReadPackInt();

      OwnTradeReply reply = new OwnTradeReply(OwnTradeSource.History,
        dateTime, 0, orderId, price, quantity);

      if(push && Handler != null)
        Handler(Security.Key, reply);
    }

    // **********************************************************************
  }
}
