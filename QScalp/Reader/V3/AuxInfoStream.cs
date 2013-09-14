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
  sealed class AuxInfoStream : QshStream3, IAuxInfoStream
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

    // **********************************************************************

    public event Action<AuxInfo> Handler;

    // **********************************************************************

    public Security Security { get; private set; }

    // **********************************************************************

    public AuxInfoStream(BinaryReader br)
      : base(StreamType.AuxInfo, br)
    {
      Security = new Security(br.ReadString());
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      AuxInfo auxInfo = new AuxInfo();

      AuxInfoFlags flags = (AuxInfoFlags)br.ReadByte();

      if((flags & AuxInfoFlags.DateTime) > 0)
        auxInfo.DateTime = lastDateTime = DataReader.ReadDateTime(br, ref baseDateTime);
      else
        auxInfo.DateTime = lastDateTime;


      if((flags & AuxInfoFlags.__DateTime2) > 0)
        auxInfo.__DateTime2 = DataReader.ReadDateTime(br, ref __baseDateTime2);


      if((flags & AuxInfoFlags.AskSum) > 0)
        auxInfo.AskSum = lastAskSum = DataReader.ReadRelative(br, ref baseAskSum);
      else
        auxInfo.AskSum = lastAskSum;

      if((flags & AuxInfoFlags.BidSum) > 0)
        auxInfo.BidSum = lastBidSum = DataReader.ReadRelative(br, ref baseBidSum);
      else
        auxInfo.BidSum = lastBidSum;

      if((flags & AuxInfoFlags.OI) > 0)
        auxInfo.OI = lastOI = DataReader.ReadRelative(br, ref baseOI);
      else
        auxInfo.OI = lastOI;


      if((flags & AuxInfoFlags.__PackInt1) > 0)
        auxInfo.__PackInt1 = DataReader.ReadPackInt(br);

      if((flags & AuxInfoFlags.__PackInt2) > 0)
        auxInfo.__PackInt2 = DataReader.ReadPackInt(br);

      if((flags & AuxInfoFlags.__PackInt3) > 0)
        auxInfo.__PackInt3 = DataReader.ReadPackInt(br);


      if(push && Handler != null)
        Handler(auxInfo);
    }

    // **********************************************************************
  }
}
