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
  sealed class OrdersStream
  {
    // **********************************************************************

    readonly DataWriter dw;
    readonly int sid;

    // **********************************************************************

    public OrdersStream(DataWriter dw, int sid, Security s)
    {
      this.dw = dw;
      this.sid = sid;

      dw.Write((byte)StreamType.Orders);
      dw.Write(s.Entry);
    }

    // **********************************************************************

    public void Write(DateTime dateTime, OwnOrder order)
    {
      dw.WriteRecHeader(sid, dateTime);

      dw.Write(order.Id);
      dw.WritePackInt(order.IsActive ? order.Price : -order.Price);
      dw.WritePackInt(order.Quantity);
    }

    // **********************************************************************
  }
}
