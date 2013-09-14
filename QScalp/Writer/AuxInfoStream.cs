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
  sealed class AuxInfoStream
  {
    // **********************************************************************

    readonly DataWriter dw;
    readonly int sid;

    // **********************************************************************

    DateTime baseDateTime;
    DateTime lastDateTime;

    DateTime __baseDateTime2;

    int baseAskSum;
    int lastAskSum;

    int baseBidSum;
    int lastBidSum;

    int baseOI;
    int lastOI;

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
      AuxInfoFlags flags = AuxInfoFlags.None;

      // --------------------------------------------------

      if(lastDateTime != auxInfo.DateTime)
      {
        lastDateTime = auxInfo.DateTime;
        flags |= AuxInfoFlags.DateTime;
      }


      if(auxInfo.__DateTime2 != null)
        flags |= AuxInfoFlags.__DateTime2;


      if(lastAskSum != auxInfo.AskSum)
      {
        lastAskSum = auxInfo.AskSum;
        flags |= AuxInfoFlags.AskSum;
      }

      if(lastBidSum != auxInfo.BidSum)
      {
        lastBidSum = auxInfo.BidSum;
        flags |= AuxInfoFlags.BidSum;
      }

      if(lastOI != auxInfo.OI)
      {
        lastOI = auxInfo.OI;
        flags |= AuxInfoFlags.OI;
      }


      if(auxInfo.__PackInt1 != null)
        flags |= AuxInfoFlags.__PackInt1;

      if(auxInfo.__PackInt2 != null)
        flags |= AuxInfoFlags.__PackInt2;

      if(auxInfo.__PackInt3 != null)
        flags |= AuxInfoFlags.__PackInt3;

      // --------------------------------------------------

      dw.WriteRecHeader(sid, dateTime);
      dw.Write((byte)flags);

      if((flags & AuxInfoFlags.DateTime) > 0)
        dw.WriteDateTime(auxInfo.DateTime, ref baseDateTime);


      if((flags & AuxInfoFlags.__DateTime2) > 0)
        dw.WriteDateTime(auxInfo.__DateTime2.Value, ref __baseDateTime2);


      if((flags & AuxInfoFlags.AskSum) > 0)
        dw.WriteRelative(auxInfo.AskSum, ref baseAskSum);

      if((flags & AuxInfoFlags.BidSum) > 0)
        dw.WriteRelative(auxInfo.BidSum, ref baseBidSum);

      if((flags & AuxInfoFlags.OI) > 0)
        dw.WriteRelative(auxInfo.OI, ref baseOI);


      if((flags & AuxInfoFlags.__PackInt1) > 0)
        dw.WritePackInt(auxInfo.__PackInt1.Value);

      if((flags & AuxInfoFlags.__PackInt2) > 0)
        dw.WritePackInt(auxInfo.__PackInt2.Value);

      if((flags & AuxInfoFlags.__PackInt3) > 0)
        dw.WritePackInt(auxInfo.__PackInt3.Value);

      // --------------------------------------------------
    }

    // **********************************************************************
  }
}
