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

namespace QScalp.History.Reader.V4
{
  sealed class DealsStream : QshStream, IDealsStream
  {
    // **********************************************************************

    long lastMilliseconds;
    long lastId;
    long lastOrderId;
    int lastPrice;
    int lastVolume;
    int lastOI;

    // **********************************************************************

    public Security Security { get; private set; }
    public event Action<Deal> Handler;

    // **********************************************************************

    public DealsStream(DataReader dr)
      : base(StreamType.Deals, dr)
    {
      Security = new Security(dr.ReadString());
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      DealFlags flags = (DealFlags)dr.ReadByte();

      // ------------------------------------------------------------

      if((flags & DealFlags.DateTime) != 0)
        lastMilliseconds = dr.ReadGrowing(lastMilliseconds);

      if((flags & DealFlags.Id) != 0)
        lastId = dr.ReadGrowing(lastId);

      if((flags & DealFlags.OrderId) != 0)
        lastOrderId += dr.ReadLeb128();

      if((flags & DealFlags.Price) != 0)
        lastPrice += (int)dr.ReadLeb128();

      if((flags & DealFlags.Volume) != 0)
        lastVolume = (int)dr.ReadLeb128();

      if((flags & DealFlags.OI) != 0)
        lastOI += (int)dr.ReadLeb128();

      // ------------------------------------------------------------

      if(push && Handler != null)
        Handler(new Deal()
        {
          DateTime = DateTimeHelper.FromMs(lastMilliseconds),
          Id = lastId,
          OrderId = lastOrderId,
          Type = (DealType)(flags & DealFlags.Type),
          Price = lastPrice,
          Volume = lastVolume,
          OI = lastOI
        });
    }

    // **********************************************************************
  }
}
