#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;

namespace QScalp.History.Writer
{
  sealed class MessagesStream
  {
    // **********************************************************************

    readonly DataWriter dw;
    readonly int sid;

    // **********************************************************************

    public MessagesStream(DataWriter dw, int sid)
    {
      this.dw = dw;
      this.sid = sid;

      dw.Write((byte)StreamType.Messages);
    }

    // **********************************************************************

    public void Write(DateTime dateTime, Message msg)
    {
      dw.WriteRecHeader(sid, dateTime);

      dw.Write(msg.DateTime.Ticks);
      dw.Write((byte)msg.Type);
      dw.Write(msg.Text);
    }

    // **********************************************************************
  }
}
