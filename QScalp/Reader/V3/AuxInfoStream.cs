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
  sealed class AuxInfoStream : QshStream, IAuxInfoStream
  {
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

    int lastPrice;

    // **********************************************************************

    public event Action<int, AuxInfo> Handler;

    // **********************************************************************

    public Security Security { get; private set; }

    // **********************************************************************

    public AuxInfoStream(DataReader dr)
      : base(StreamType.AuxInfo, dr)
    {
      Security = new Security(dr.ReadString());
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      AuxInfoFlagsV3 flags = (AuxInfoFlagsV3)dr.ReadByte();

      if((flags & AuxInfoFlagsV3.DateTime) != 0)
        lastDateTime = dr.ReadDateTime(ref baseDateTime);


      if((flags & AuxInfoFlagsV3.__DateTime2) != 0)
        dr.ReadDateTime(ref __baseDateTime2);


      if((flags & AuxInfoFlagsV3.AskSum) != 0)
        lastAskSum = dr.ReadRelative(ref baseAskSum);

      if((flags & AuxInfoFlagsV3.BidSum) != 0)
        lastBidSum = dr.ReadRelative(ref baseBidSum);

      if((flags & AuxInfoFlagsV3.OI) != 0)
        lastOI = dr.ReadRelative(ref baseOI);

      if((flags & AuxInfoFlagsV3.Price) != 0)
        lastPrice = dr.ReadPackInt();


      if((flags & AuxInfoFlagsV3.__PackInt2) != 0)
        dr.ReadPackInt();

      if((flags & AuxInfoFlagsV3.__PackInt3) != 0)
        dr.ReadPackInt();


      if(push && Handler != null)
        Handler(Security.Key, new AuxInfo(lastDateTime, lastPrice,
          lastAskSum, lastBidSum, lastOI, 0, 0, 0, 0, null));
    }

    // **********************************************************************
  }
}
