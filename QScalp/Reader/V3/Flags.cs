#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;

namespace QScalp.History.Reader.V3
{
  // ************************************************************************

  [Flags]
  enum DealFlagsV3
  {
    Type = 0x03,

    DateTime = 0x04,
    Price = 0x08,
    Volume = 0x10,
    OI = 0x20,
    Id = 0x40,

    None = 0
  }

  // **********************************************************************

  [Flags]
  enum AuxInfoFlagsV3
  {
    DateTime = 0x01,
    __DateTime2 = 0x02, // reserved

    AskSum = 0x04,
    BidSum = 0x08,
    OI = 0x10,
    Price = 0x20,

    __PackInt2 = 0x40, // reserved
    __PackInt3 = 0x80, // reserved

    None = 0
  }

  // ************************************************************************
}
