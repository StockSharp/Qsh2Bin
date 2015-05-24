#region Copyright (c) 2011-2015 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;
using System.Collections.Generic;

using QScalp.History.Internals;
using QScalp.Shared;

namespace QScalp.History.Reader.V4
{
  sealed class OrdLogStream : QshStream, IOrdLogStream, IStockStream, IDealsStream, IAuxInfoStream
  {
    // **********************************************************************

    readonly TimeSpan stockPushInterval = new TimeSpan(0, 0, 0, 0, 5);

    // **********************************************************************

    readonly RawQuotes rawQuotes;

    Action<int, Quote[], Spread> stockHandler;
    Action<Deal> dealHandler;
    Action<int, AuxInfo> auxInfoHandler;

    DateTime lastStockPush;
    long lastPushedDealId;

    // **********************************************************************

    long lastMilliseconds;
    long lastOrderId;

    int lastPrice;

    int lastAmount;
    int lastAmountRest;

    long lastDealId;
    int lastDealPrice;
    int lastOI;

    // **********************************************************************

    public Security Security { get; private set; }
    public event Action<int, OrdLogEntry> Handler;

    // **********************************************************************

    event Action<int, Quote[], Spread> IStockStream.Handler
    {
      add { stockHandler += value; }
      remove { stockHandler -= value; }
    }

    event Action<Deal> IDealsStream.Handler
    {
      add { dealHandler += value; }
      remove { dealHandler -= value; }
    }

    event Action<int, AuxInfo> IAuxInfoStream.Handler
    {
      add { auxInfoHandler += value; }
      remove { auxInfoHandler -= value; }
    }

    // **********************************************************************

    public OrdLogStream(DataReader dr)
      : base(StreamType.OrdLog, dr)
    {
      Security = new Security(dr.ReadString());
      rawQuotes = new RawQuotes();
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      DateTime dateTime;
      long orderId;
      int amountRest;
      long dealId;
      int dealPrice;
      int oi;

      // ------------------------------------------------------------

      OrdLogEntryFlags flags = (OrdLogEntryFlags)dr.ReadByte();
      OrdLogFlags ordLogFlags = (OrdLogFlags)dr.ReadUInt16();

      bool isAdd = (ordLogFlags & OrdLogFlags.Add) != 0;
      bool isFill = (ordLogFlags & OrdLogFlags.Fill) != 0;

      bool isBuy = (ordLogFlags & OrdLogFlags.Buy) != 0;
      bool isSell = (ordLogFlags & OrdLogFlags.Sell) != 0;

      // ------------------------------------------------------------

      if((flags & OrdLogEntryFlags.DateTime) != 0)
        lastMilliseconds = dr.ReadGrowing(lastMilliseconds);

      dateTime = DateTimeHelper.FromMs(lastMilliseconds);

      if((flags & OrdLogEntryFlags.OrderId) == 0)
        orderId = lastOrderId;
      else if(isAdd)
        orderId = lastOrderId = dr.ReadGrowing(lastOrderId);
      else
        orderId = lastOrderId + dr.ReadLeb128();

      if((flags & OrdLogEntryFlags.Price) != 0)
        lastPrice += (int)dr.ReadLeb128();

      if((flags & OrdLogEntryFlags.Amount) != 0)
        lastAmount = (int)dr.ReadLeb128();

      if((flags & OrdLogEntryFlags.AmountRest) != 0)
        amountRest = lastAmountRest = (int)dr.ReadLeb128();
      else if(isAdd)
        amountRest = lastAmount;
      else
        amountRest = 0;

      if((flags & OrdLogEntryFlags.DealId) != 0)
        dealId = lastDealId = (int)dr.ReadGrowing(lastDealId);
      else if(isFill)
        dealId = lastDealId;
      else
        dealId = 0;

      if((flags & OrdLogEntryFlags.DealPrice) != 0)
        dealPrice = lastDealPrice += (int)dr.ReadLeb128();
      else if(isFill)
        dealPrice = lastDealPrice;
      else
        dealPrice = 0;

      if((flags & OrdLogEntryFlags.OI) != 0)
        oi = lastOI += (int)dr.ReadLeb128();
      else if(isFill)
        oi = lastOI;
      else
        oi = 0;

      // ------------------------------------------------------------

      if(Handler != null && push)
        Handler(Security.Key, new OrdLogEntry(
          ordLogFlags, dateTime, orderId, lastPrice,
          lastAmount, amountRest, dealId, dealPrice, oi));

      // ------------------------------------------------------------

      if((ordLogFlags & OrdLogFlags.SessIdChanged) != 0)
        rawQuotes.Clear();

      if(!(isBuy ^ isSell)
        || (ordLogFlags & OrdLogFlags.NonSystem) != 0
        || (ordLogFlags & OrdLogFlags.NonZeroReplAct) != 0)
      {
        return;
      }

      // ------------------------------------------------------------

      int quantity;
      rawQuotes.TryGetValue(lastPrice, out quantity);

      if(isAdd ? isSell : isBuy)
        quantity += lastAmount;
      else
        quantity -= lastAmount;

      if(quantity == 0)
        rawQuotes.Remove(lastPrice);
      else
        rawQuotes[lastPrice] = quantity;

      // ------------------------------------------------------------

      if(push)
      {
        if((ordLogFlags & OrdLogFlags.EndOfTransaction) != 0)
        {
          DateTime now = DateTime.UtcNow;

          if(now - lastStockPush > stockPushInterval)
          {
            lastStockPush = now;

            if(stockHandler != null)
            {
              Quote[] quotes;
              Spread spread;

              rawQuotes.GetQuotes(out quotes, out spread);

              if(spread.Ask > 0 && spread.Bid > 0)
                stockHandler(Security.Key, quotes, spread);
            }

            if(auxInfoHandler != null)
            {
              int askTotal = 0;
              int bidTotal = 0;

              foreach(KeyValuePair<int, int> kvp in rawQuotes)
                if(kvp.Value > 0)
                  askTotal += kvp.Value;
                else
                  bidTotal -= kvp.Value;

              auxInfoHandler(Security.Key, new AuxInfo(
                dateTime, lastDealPrice, askTotal,
                bidTotal, lastOI, 0, 0, 0, 0, null));
            }
          }
        }

        if(lastPushedDealId < dealId)
        {
          if(dealHandler != null)
            dealHandler(new Deal()
            {
              SecKey = Security.Key,
              Type = isSell ? DealType.Sell : DealType.Buy,
              Id = dealId,
              DateTime = dateTime,
              Price = Security.GetPrice(dealPrice),
              Volume = lastAmount,
              OI = oi
            });

          lastPushedDealId = dealId;
        }
      }
    }

    // **********************************************************************
  }
}
