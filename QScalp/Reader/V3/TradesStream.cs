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
  sealed class TradesStream : QshStream3, ITradesStream
  {
    // **********************************************************************

    DateTime baseDateTime;
    int basePrice;

    // **********************************************************************

    public Security Security { get; private set; }
    public event Action<int, TraderReply> Handler;

    // **********************************************************************

    public TradesStream(BinaryReader br)
      : base(StreamType.Trades, br)
    {
      Security = new Security(br.ReadString());
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      OwnTradeReply reply = new OwnTradeReply(
        br.ReadInt64(),
        DataReader.ReadDateTime(br, ref baseDateTime),
        DataReader.ReadRelative(br, ref basePrice),
        DataReader.ReadPackInt(br));

      if(push && Handler != null)
        Handler(Security.Key, reply);
    }

    // **********************************************************************
  }
}
