#region Copyright (c) 2011-2013 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;

namespace QScalp.History.Writer
{
  sealed class TradesStream
  {
    // **********************************************************************

    readonly DataWriter dw;
    readonly int sid;

    DateTime baseDateTime;
    int basePrice;

    // **********************************************************************

    public TradesStream(DataWriter dw, int sid, Security s)
    {
      this.dw = dw;
      this.sid = sid;

      dw.Write((byte)StreamType.Trades);
      dw.Write(s.Entry);
    }

    // **********************************************************************

    public void Write(DateTime dateTime, TraderReply reply)
    {
      dw.WriteRecHeader(sid, dateTime);

      dw.Write(reply.OId);
      dw.WriteDateTime(reply.DateTime, ref baseDateTime);
      dw.WriteRelative(reply.PTicks, ref basePrice);
      dw.WritePackInt(reply.Quantity);
    }

    // **********************************************************************
  }
}
