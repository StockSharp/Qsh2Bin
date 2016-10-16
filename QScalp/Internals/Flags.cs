#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;

namespace QScalp.History.Internals
{
  // ************************************************************************

  [Flags]
  enum DealFlags
  {
    Type = 0x03,

    DateTime = 0x04,
    Id = 0x08,
    OrderId = 0x10,
    Price = 0x20,
    Volume = 0x40,
    OI = 0x80,

    None = 0
  }

  // **********************************************************************

  [Flags]
  enum OrderFlags
  {
    DropAll = 0x01,
    Active = 0x02,
    External = 0x04,
    Stop = 0x08,

    None = 0
  }

  // **********************************************************************

  [Flags]
  enum AuxInfoFlags
  {
    DateTime = 0x01,

    AskTotal = 0x02,
    BidTotal = 0x04,
    OI = 0x08,
    Price = 0x10,

    SessionInfo = 0x20,
    Rate = 0x40,
    Message = 0x80,

    None = 0
  }

  // ************************************************************************

  [Flags]
  enum OrdLogEntryFlags
  {
    DateTime = 0x01,
    OrderId = 0x02,
    Price = 0x04,

    Amount = 0x08,
    AmountRest = 0x10,

    DealId = 0x20,
    DealPrice = 0x40,
    OI = 0x80,

    None = 0
  }

  // ************************************************************************
}
