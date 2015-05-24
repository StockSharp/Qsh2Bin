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
  sealed class DealsStream
  {
    // **********************************************************************

    readonly DataWriter dw;
    readonly int sid;
    readonly Security s;

    long lastMilliseconds;
    long lastId;

    int lastPrice;
    double lastRawPrice;

    int lastVolume;
    int lastOI;

    // **********************************************************************

    public DealsStream(DataWriter dw, int sid, Security s)
    {
      this.dw = dw;
      this.sid = sid;
      this.s = s;

      dw.Write((byte)StreamType.Deals);
      dw.Write(s.Entry);
    }

    // **********************************************************************

    public void Write(DateTime dateTime, Deal deal)
    {
      dw.WriteRecHeader(sid, dateTime);

      // ------------------------------------------------------------

      long milliseconds = DateTimeHelper.ToMs(deal.DateTime);
      DealFlags flags = (DealFlags)deal.Type & DealFlags.Type;

      if(lastMilliseconds != milliseconds)
        flags |= DealFlags.DateTime;

      if(deal.Id != 0)
        flags |= DealFlags.Id;

      //if(deal.OrderId != 0)
      //  flags |= DealFlags.OrderId;

      if(lastRawPrice != deal.Price)
      {
        lastRawPrice = deal.Price;
        flags |= DealFlags.Price;
      }

      if(lastVolume != deal.Volume)
        flags |= DealFlags.Volume;

      if(lastOI != deal.OI)
        flags |= DealFlags.OI;

      // ------------------------------------------------------------

      dw.Write((byte)flags);

      // ------------------------------------------------------------

      if((flags & DealFlags.DateTime) != 0)
        dw.WriteGrowing(milliseconds, ref lastMilliseconds);

      if((flags & DealFlags.Id) != 0)
        dw.WriteGrowing(deal.Id, ref lastId);

      if((flags & DealFlags.Price) != 0)
      {
        int p = s.GetTicks(deal.Price);

        dw.WriteLeb128(p - lastPrice);
        lastPrice = p;
      }

      if((flags & DealFlags.Volume) != 0)
      {
        dw.WriteLeb128(deal.Volume);
        lastVolume = deal.Volume;
      }

      if((flags & DealFlags.OI) != 0)
      {
        dw.WriteLeb128(deal.OI - lastOI);
        lastOI = deal.OI;
      }

      // ------------------------------------------------------------
    }

    // **********************************************************************
  }
}
