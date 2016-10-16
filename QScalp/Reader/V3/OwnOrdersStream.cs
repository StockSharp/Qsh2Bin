#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
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
  sealed class OwnOrdersStream : QshStream, IOwnOrdersStream
  {
    // **********************************************************************

    public Security Security { get; private set; }
    public event Action<int, OwnOrder> Handler;

    // **********************************************************************

    public OwnOrdersStream(DataReader dr)
      : base(StreamType.OwnOrders, dr)
    {
      Security = new Security(dr.ReadString());
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      long id = dr.ReadInt64();
      int p = dr.ReadPackInt();
      int q = dr.ReadPackInt();

      if(push && Handler != null)
      {
        OwnOrder order;

        if(p < 0)
          order = new OwnOrder(id, -p);
        else if(id < 0)
          order = new OwnOrder(OwnOrderType.Stop, id, p, q);
        else
          order = new OwnOrder(OwnOrderType.Regular, id, p, q);

        Handler(Security.Key, order);
      }
    }

    // **********************************************************************
  }
}
