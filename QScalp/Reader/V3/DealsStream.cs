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

using QScalp.History.Internals;

namespace QScalp.History.Reader.V3
{
  sealed class DealsStream : QshStream3, IDealsStream
  {
    // **********************************************************************

    DateTime baseDateTime;
    DateTime lastDateTime;

    int basePrice;
    double lastPrice;

    int lastVolume;

    // **********************************************************************

    public Security Security { get; private set; }
    public event Action<Deal> Handler;

    // **********************************************************************

    public DealsStream(BinaryReader br)
      : base(StreamType.Deals, br)
    {
      Security = new Security(br.ReadString());
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      Deal d = new Deal();

      DealFlags flags = (DealFlags)br.ReadByte();

      if((flags & DealFlags.DateTime) > 0)
        d.DateTime = lastDateTime = DataReader.ReadDateTime(br, ref baseDateTime);
      else
        d.DateTime = lastDateTime;

      if((flags & DealFlags.Price) > 0)
        d.Price = lastPrice = Security.GetPrice(DataReader.ReadRelative(br, ref basePrice));
      else
        d.Price = lastPrice;

      if((flags & DealFlags.Volume) > 0)
        d.Volume = lastVolume = DataReader.ReadPackInt(br);
      else
        d.Volume = lastVolume;

      d.Type = (DealType)(flags & DealFlags.Type);

      if(push && Handler != null)
      {
        d.SecKey = Security.Key;
        Handler(d);
      }
    }

    // **********************************************************************
  }
}
