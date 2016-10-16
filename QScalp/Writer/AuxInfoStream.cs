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
  sealed class AuxInfoStream
  {
    // **********************************************************************

    readonly DataWriter dw;
    readonly int sid;

    // **********************************************************************

    long lastMilliseconds;
    int lastAskTotal;
    int lastBidTotal;
    int lastOI;
    int lastPrice;
    int lastHiLimit;
    int lastLoLimit;
    double lastDeposit;
    double lastRate;

    // **********************************************************************

    public AuxInfoStream(DataWriter dw, int sid, Security s)
    {
      this.dw = dw;
      this.sid = sid;

      dw.Write((byte)StreamType.AuxInfo);
      dw.Write(s.Entry);
    }

    // **********************************************************************

    public void Write(DateTime dateTime, AuxInfo auxInfo)
    {
      dw.WriteRecHeader(sid, dateTime);

      // --------------------------------------------------

      long milliseconds = DateTimeHelper.ToMs(auxInfo.DateTime);
      AuxInfoFlags flags = AuxInfoFlags.None;

      // --------------------------------------------------

      if(lastMilliseconds != milliseconds)
        flags |= AuxInfoFlags.DateTime;

      if(lastAskTotal != auxInfo.AskTotal)
        flags |= AuxInfoFlags.AskTotal;

      if(lastBidTotal != auxInfo.BidTotal)
        flags |= AuxInfoFlags.BidTotal;

      if(lastOI != auxInfo.OI)
        flags |= AuxInfoFlags.OI;

      if(lastPrice != auxInfo.Price)
        flags |= AuxInfoFlags.Price;

      if(lastHiLimit != auxInfo.HiLimit
        || lastLoLimit != auxInfo.LoLimit
        || lastDeposit != auxInfo.Deposit)
      {
        flags |= AuxInfoFlags.SessionInfo;
      }

      if(lastRate != auxInfo.Rate)
        flags |= AuxInfoFlags.Rate;

      if(auxInfo.Message != null)
        flags |= AuxInfoFlags.Message;

      // --------------------------------------------------

      dw.Write((byte)flags);

      // --------------------------------------------------

      if((flags & AuxInfoFlags.DateTime) != 0)
        dw.WriteGrowing(milliseconds, ref lastMilliseconds);

      if((flags & AuxInfoFlags.AskTotal) != 0)
      {
        dw.WriteLeb128(auxInfo.AskTotal - lastAskTotal);
        lastAskTotal = auxInfo.AskTotal;
      }

      if((flags & AuxInfoFlags.BidTotal) != 0)
      {
        dw.WriteLeb128(auxInfo.BidTotal - lastBidTotal);
        lastBidTotal = auxInfo.BidTotal;
      }

      if((flags & AuxInfoFlags.OI) != 0)
      {
        dw.WriteLeb128(auxInfo.OI - lastOI);
        lastOI = auxInfo.OI;
      }

      if((flags & AuxInfoFlags.Price) != 0)
      {
        dw.WriteLeb128(auxInfo.Price - lastPrice);
        lastPrice = auxInfo.Price;
      }

      if((flags & AuxInfoFlags.SessionInfo) != 0)
      {
        dw.WriteLeb128(auxInfo.HiLimit);
        dw.WriteLeb128(auxInfo.LoLimit);
        dw.Write(auxInfo.Deposit);

        lastHiLimit = auxInfo.HiLimit;
        lastLoLimit = auxInfo.LoLimit;
        lastDeposit = auxInfo.Deposit;
      }

      if((flags & AuxInfoFlags.Rate) != 0)
      {
        dw.Write(auxInfo.Rate);
        lastRate = auxInfo.Rate;
      }

      if((flags & AuxInfoFlags.Message) != 0)
        dw.Write(auxInfo.Message);

      // --------------------------------------------------
    }

    // **********************************************************************
  }
}
