#region Copyright (c) 2011-2013 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;
using System.IO;

namespace QScalp.History.Reader.V3
{
  sealed class OrdersStream : QshStream3, IOrdersStream
  {
    // **********************************************************************

    public Security Security { get; private set; }
    public event Action<int, OwnOrder> Handler;

    // **********************************************************************

    public OrdersStream(BinaryReader br)
      : base(StreamType.Orders, br)
    {
      Security = new Security(br.ReadString());
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      long id = br.ReadInt64();
      int p = DataReader.ReadPackInt(br);
      int q = DataReader.ReadPackInt(br);

      if(push && Handler != null)
        Handler(Security.Key, p < 0 ? new OwnOrder(id, -p) : new OwnOrder(id, p, q, null));
    }

    // **********************************************************************
  }
}
