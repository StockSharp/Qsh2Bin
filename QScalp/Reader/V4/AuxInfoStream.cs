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
  sealed class AuxInfoStream : QshStream, IAuxInfoStream
  {
    // **********************************************************************

    long lastMilliseconds;
    int lastAskTotal;
    int lastBidTotal;
    int lastOI;
    int lastPrice;
    int lastHiLimit;
    int lastLoLimit;
    double lastDeposit;
    double lastRate;

    // **********************************************************************

    public Security Security { get; private set; }
    public event Action<AuxInfo> Handler;

    // **********************************************************************

    public AuxInfoStream(DataReader dr)
      : base(StreamType.AuxInfo, dr)
    {
      Security = new Security(dr.ReadString());
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      AuxInfoFlags flags = (AuxInfoFlags)dr.ReadByte();
      string message;

      // ------------------------------------------------------------

      if((flags & AuxInfoFlags.DateTime) != 0)
        lastMilliseconds = dr.ReadGrowing(lastMilliseconds);

      if((flags & AuxInfoFlags.AskTotal) != 0)
        lastAskTotal += (int)dr.ReadLeb128();

      if((flags & AuxInfoFlags.BidTotal) != 0)
        lastBidTotal += (int)dr.ReadLeb128();

      if((flags & AuxInfoFlags.OI) != 0)
        lastOI += (int)dr.ReadLeb128();

      if((flags & AuxInfoFlags.Price) != 0)
        lastPrice += (int)dr.ReadLeb128();

      if((flags & AuxInfoFlags.SessionInfo) != 0)
      {
        lastHiLimit = (int)dr.ReadLeb128();
        lastLoLimit = (int)dr.ReadLeb128();
        lastDeposit = dr.ReadDouble();
      }

      if((flags & AuxInfoFlags.Rate) != 0)
        lastRate = dr.ReadDouble();

      if((flags & AuxInfoFlags.Message) != 0)
        message = dr.ReadString();
      else
        message = null;

      // ------------------------------------------------------------

      if(push && Handler != null)
        Handler(new AuxInfo(
          DateTimeHelper.FromMs(lastMilliseconds),
          lastPrice, lastAskTotal, lastBidTotal,
          lastOI, lastHiLimit, lastLoLimit,
          lastDeposit, lastRate, message));
    }

    // **********************************************************************
  }
}
