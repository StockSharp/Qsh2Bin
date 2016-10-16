#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;

namespace QScalp.History.Reader.V4
{
  sealed class MessagesStream : QshStream, IMessagesStream
  {
    // **********************************************************************

    public event Action<Message> Handler;

    // **********************************************************************

    public MessagesStream(DataReader dr) : base(StreamType.Messages, dr) { }

    // **********************************************************************

    public override void Read(bool push)
    {
      Message msg = new Message(new DateTime(dr.ReadInt64()),
        (MessageType)dr.ReadByte(), dr.ReadString());

      if(push && Handler != null)
        Handler(msg);
    }

    // **********************************************************************
  }
}
