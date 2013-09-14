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
  sealed class MessagesStream : QshStream3, IMessagesStream
  {
    // **********************************************************************

    public event Action<Message> Handler;

    // **********************************************************************

    public MessagesStream(BinaryReader br) : base(StreamType.Messages, br) { }

    // **********************************************************************

    public override void Read(bool push)
    {
      Message msg = new Message(new DateTime(br.ReadInt64()),
        (MessageType)br.ReadByte(), br.ReadString());

      if(push && Handler != null)
        Handler(msg);
    }

    // **********************************************************************
  }
}
