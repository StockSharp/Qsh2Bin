#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
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

namespace QScalp.History.Reader.V4
{
  sealed class QshReaderImpl : QshReader
  {
    // **********************************************************************

    readonly DataReader dr;
    readonly QshStream[] streams;

    QshStream currentStream;
    long lastMilliseconds;

    // **********************************************************************

    public override int StreamCount { get { return streams.Length; } }
    public override IQshStream this[int i] { get { return streams[i]; } }

    // **********************************************************************

    public QshReaderImpl(FileStream fs, Stream ds)
      : base(fs)
    {
      dr = new DataReader(ds);

      AppName = dr.ReadString();
      Comment = dr.ReadString();
      RecDateTime = new DateTime(dr.ReadInt64(), DateTimeKind.Utc);

      lastMilliseconds = DateTimeHelper.ToMs(RecDateTime);

      int streamCount = dr.ReadByte();

      if(streamCount == 0)
        throw new Exception("Нет потоков данных");

      streams = new QshStream[streamCount];

      for(int i = 0; i < streams.Length; i++)
      {
        StreamType st = (StreamType)dr.ReadByte();

        switch(st)
        {
          case StreamType.Quotes:
            streams[i] = new QuotesStream(dr);
            break;

          case StreamType.Deals:
            streams[i] = new DealsStream(dr);
            break;

          case StreamType.OwnOrders:
            streams[i] = new OwnOrdersStream(dr);
            break;

          case StreamType.OwnTrades:
            streams[i] = new OwnTradesStream(dr);
            break;

          case StreamType.Messages:
            streams[i] = new MessagesStream(dr);
            break;

          case StreamType.AuxInfo:
            streams[i] = new AuxInfoStream(dr);
            break;

          case StreamType.OrdLog:
            streams[i] = new OrdLogStream(dr);
            break;

          default:
            throw new FormatException("Неизвестный тип данных: " + st);
        }
      }

      if(streams.Length == 1)
        currentStream = streams[0];

      ReadNextRecordHeader();
    }

    // **********************************************************************

    void ReadNextRecordHeader()
    {
      try
      {
        lastMilliseconds = dr.ReadGrowing(lastMilliseconds);
        CurrentDateTime = DateTimeHelper.FromMs(lastMilliseconds);

        if(streams.Length > 1)
        {
          CurrentStreamIndex = dr.ReadByte();
          currentStream = streams[CurrentStreamIndex];
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
      currentStream.Read(push);
      ReadNextRecordHeader();
    }

    // **********************************************************************

    public override void Dispose()
    {
      if(dr != null)
        dr.Dispose();

      base.Dispose();
    }

    // **********************************************************************
  }
}
