#region Copyright (c) 2011-2013 Николай Морошкин, http://www.moroshkin.com/
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

    DateTime baseDateTime;
    DateTime lastDateTime;

    int basePrice;
    double lastPrice;

    int lastVolume;

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

      DealFlags flags = (DealFlags)deal.Type & DealFlags.Type;

      if(lastDateTime != deal.DateTime)
      {
        lastDateTime = deal.DateTime;
        flags |= DealFlags.DateTime;
      }

      if(lastPrice != deal.Price)
      {
        lastPrice = deal.Price;
        flags |= DealFlags.Price;
      }

      if(lastVolume != deal.Volume)
      {
        lastVolume = deal.Volume;
        flags |= DealFlags.Volume;
      }

      dw.Write((byte)flags);

      if((flags & DealFlags.DateTime) > 0)
        dw.WriteDateTime(deal.DateTime, ref baseDateTime);

      if((flags & DealFlags.Price) > 0)
        dw.WriteRelative(s.GetTicks(deal.Price), ref basePrice);

      if((flags & DealFlags.Volume) > 0)
        dw.WritePackInt(deal.Volume);
    }

    // **********************************************************************
  }
}
