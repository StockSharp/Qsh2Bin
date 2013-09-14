#region Copyright (c) 2011-2013 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;

namespace QScalp.History.Reader
{
  // ************************************************************************

  interface IQshStream { StreamType Type { get; } }
  interface ISecurityStream : IQshStream { Security Security { get; } }

  interface IStockStream : ISecurityStream { event Action<int, Quote[], Spread> Handler;  }
  interface IDealsStream : ISecurityStream { event Action<Deal> Handler;  }
  interface IOrdersStream : ISecurityStream { event Action<int, OwnOrder> Handler;  }
  interface ITradesStream : ISecurityStream { event Action<int, TraderReply> Handler;  }
  interface IMessagesStream : IQshStream { event Action<Message> Handler;  }
  interface IAuxInfoStream : ISecurityStream { event Action<AuxInfo> Handler; }

  // ************************************************************************
}
