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
  static class DateTimeHelper
  {
    // **********************************************************************

    public static long ToMs(DateTime dateTime)
    {
      return dateTime.Ticks / TimeSpan.TicksPerMillisecond;
    }

    // **********************************************************************

    public static DateTime FromMs(long milliseconds)
    {
      return new DateTime(milliseconds * TimeSpan.TicksPerMillisecond);
    }

    // **********************************************************************
  }
}
