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
  sealed class OwnOrdersStream
  {
    // **********************************************************************

    readonly DataWriter dw;
    readonly int sid;

    // **********************************************************************

    public OwnOrdersStream(DataWriter dw, int sid, Security s)
    {
      this.dw = dw;
      this.sid = sid;

      dw.Write((byte)StreamType.OwnOrders);
      dw.Write(s.Entry);
    }

    // **********************************************************************

    public void Write(DateTime dateTime, OwnOrder order)
    {
      dw.WriteRecHeader(sid, dateTime);

      if(order.Id == 0 && order.Price == 0)
        dw.Write((byte)OrderFlags.DropAll);
      else
      {
        OrderFlags flags;

        switch(order.Type)
        {
          case OwnOrderType.Regular:
            flags = OrderFlags.Active;
            break;

          case OwnOrderType.Stop:
            flags = OrderFlags.Active | OrderFlags.Stop;
            break;

          default:
            flags = OrderFlags.None;
            break;
        }

        if(order.Id > 0)
          flags |= OrderFlags.External;

        dw.Write((byte)flags);
        dw.WriteLeb128(order.Id);
        dw.WriteLeb128(order.Price);
        dw.WriteLeb128(order.Quantity);
      }
    }

    // **********************************************************************
  }
}
