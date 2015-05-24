#region Copyright (c) 2011-2015 Николай Морошкин, http://www.moroshkin.com/
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
  sealed class DealsStream : QshStream, IDealsStream
  {
    // **********************************************************************

    DateTime baseDateTime;
    DateTime lastDateTime;

    int basePrice;
    double lastPrice;

    int lastVolume;

    int baseOI;
    int lastOI;

    long lastId;

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
      Deal d = new Deal();

      DealFlagsV3 flags = (DealFlagsV3)dr.ReadByte();

      if((flags & DealFlagsV3.DateTime) != 0)
        d.DateTime = lastDateTime = dr.ReadDateTime(ref baseDateTime);
      else
        d.DateTime = lastDateTime;

      if((flags & DealFlagsV3.Price) != 0)
        d.Price = lastPrice = Security.GetPrice(dr.ReadRelative(ref basePrice));
      else
        d.Price = lastPrice;

      if((flags & DealFlagsV3.Volume) != 0)
        d.Volume = lastVolume = dr.ReadPackInt();
      else
        d.Volume = lastVolume;

      if((flags & DealFlagsV3.OI) != 0)
        d.OI = lastOI = dr.ReadRelative(ref baseOI);
      else
        d.OI = lastOI;

      if((flags & DealFlagsV3.Id) != 0)
        d.Id = dr.ReadRelative(ref lastId);

      d.Type = (DealType)(flags & DealFlagsV3.Type);

      if(push && Handler != null)
      {
        d.SecKey = Security.Key;
        Handler(d);
      }
    }

    // **********************************************************************
  }
}
