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
  sealed class QshReader3 : QshReader
  {
    // **********************************************************************

    QshStream3[] streams;
    QshStream3 cStream;

    DateTime baseDateTime;

    // **********************************************************************

    public override int StreamsCount { get { return streams.Length; } }
    public override IQshStream this[int i] { get { return streams[i]; } }

    // **********************************************************************

    public QshReader3(FileStream fs, BinaryReader br)
      : base(fs, br)
    {
      AppName = br.ReadString();
      Comment = br.ReadString();
      RecDateTime = baseDateTime = new DateTime(br.ReadInt64(), DateTimeKind.Utc);

      int streamsCount = br.ReadByte();

      if(streamsCount == 0)
        throw new Exception("Нет потоков данных");

      streams = new QshStream3[streamsCount];

      for(int i = 0; i < streams.Length; i++)
      {
        StreamType st = (StreamType)br.ReadByte();

        switch(st)
        {
          case StreamType.Stock:
            streams[i] = new StockStream(br);
            break;

          case StreamType.Deals:
            streams[i] = new DealsStream(br);
            break;

          case StreamType.Orders:
            streams[i] = new OrdersStream(br);
            break;

          case StreamType.Trades:
            streams[i] = new TradesStream(br);
            break;

          case StreamType.Messages:
            streams[i] = new MessagesStream(br);
            break;

          case StreamType.AuxInfo:
            streams[i] = new AuxInfoStream(br);
            break;

          default:
            throw new FormatException("Неизвестный тип данных: " + st);
        }
      }

      if(streams.Length == 1)
        cStream = streams[0];

      ReadNextRecordHeader();
    }

    // **********************************************************************

    void ReadNextRecordHeader()
    {
      try
      {
        CurrentDateTime = DataReader.ReadDateTime(br, ref baseDateTime);

        if(streams.Length > 1)
        {
          CurrentStreamIndex = br.ReadByte();
          cStream = streams[CurrentStreamIndex];
        }
      }
      catch(EndOfStreamException)
      {
        CurrentDateTime = DateTime.MaxValue;
      }
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      cStream.Read(push);
      ReadNextRecordHeader();
    }

    // **********************************************************************
  }
}
