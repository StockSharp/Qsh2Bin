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
  sealed class OrdLogStream
  {
    // **********************************************************************

    readonly DataWriter dw;
    readonly int sid;
    readonly Security s;

    long lastMilliseconds;
    long lastOrderId;

    int lastPrice;

    int lastAmount;
    int lastAmountRest;

    long lastDealId;
    int lastDealPrice;
    int lastOI;

    // **********************************************************************

    public OrdLogStream(DataWriter dw, int sid, Security s)
    {
      this.dw = dw;
      this.sid = sid;
      this.s = s;

      dw.Write((byte)StreamType.OrdLog);
      dw.Write(s.Entry);
    }

    // **********************************************************************

    public void Write(DateTime dateTime, OrdLogEntry entry)
    {
      dw.WriteRecHeader(sid, dateTime);

      // ------------------------------------------------------------

      long milliseconds = DateTimeHelper.ToMs(entry.DateTime);
      OrdLogEntryFlags flags = OrdLogEntryFlags.None;

      // ------------------------------------------------------------

      if(lastMilliseconds != milliseconds)
        flags |= OrdLogEntryFlags.DateTime;

      if(lastOrderId != entry.OrderId)
        flags |= OrdLogEntryFlags.OrderId;

      if(lastPrice != entry.Price)
        flags |= OrdLogEntryFlags.Price;

      if(lastAmount != entry.Amount)
        flags |= OrdLogEntryFlags.Amount;

      if((entry.Flags & OrdLogFlags.Fill) != 0)
      {
        if(lastAmountRest != entry.AmountRest)
          flags |= OrdLogEntryFlags.AmountRest;

        if(lastDealId != entry.DealId)
          flags |= OrdLogEntryFlags.DealId;

        if(lastDealPrice != entry.DealPrice)
          flags |= OrdLogEntryFlags.DealPrice;

        if(lastOI != entry.OI)
          flags |= OrdLogEntryFlags.OI;
      }

      // ------------------------------------------------------------

      dw.Write((byte)flags);

      // ------------------------------------------------------------

      dw.Write((ushort)entry.Flags);

      if((flags & OrdLogEntryFlags.DateTime) != 0)
        dw.WriteGrowing(milliseconds, ref lastMilliseconds);

      if((flags & OrdLogEntryFlags.OrderId) != 0)
      {
        if((entry.Flags & OrdLogFlags.Add) != 0)
          dw.WriteGrowing(entry.OrderId, ref lastOrderId);
        else
          dw.WriteLeb128(entry.OrderId - lastOrderId);
      }

      if((flags & OrdLogEntryFlags.Price) != 0)
      {
        dw.WriteLeb128(entry.Price - lastPrice);
        lastPrice = entry.Price;
      }

      if((flags & OrdLogEntryFlags.Amount) != 0)
      {
        dw.WriteLeb128(entry.Amount);
        lastAmount = entry.Amount;
      }

      if((flags & OrdLogEntryFlags.AmountRest) != 0)
      {
        dw.WriteLeb128(entry.AmountRest);
        lastAmountRest = entry.AmountRest;
      }

      if((flags & OrdLogEntryFlags.DealId) != 0)
        dw.WriteGrowing(entry.DealId, ref lastDealId);

      if((flags & OrdLogEntryFlags.DealPrice) != 0)
      {
        dw.WriteLeb128(entry.DealPrice - lastDealPrice);
        lastDealPrice = entry.DealPrice;
      }

      if((flags & OrdLogEntryFlags.OI) != 0)
      {
        dw.WriteLeb128(entry.OI - lastOI);
        lastOI = entry.OI;
      }

      // ------------------------------------------------------------
    }

    // **********************************************************************
  }
}
