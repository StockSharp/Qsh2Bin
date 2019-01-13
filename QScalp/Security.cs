#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;
using System.Globalization;

namespace QScalp
{
  sealed class Security
  {
    // **********************************************************************

    const int stepRoundDigits = 14;
    static readonly char[] sep = new char[] { ':' };

    double step, inverseStep;

    NumberFormatInfo priceFormat;

    // **********************************************************************

    public string Entry { get; private set; }

    public string CName { get; private set; }
    public string Ticker { get; private set; }
    public string AuxCode { get; private set; }
    public int Id { get; private set; }

    public double Step
    {
      get { return step; }

      private set
      {
        if(value > 0)
          step = Math.Round(value, stepRoundDigits);
        else
          step = 1;

        inverseStep = 1 / step;

        double d = step;
        int precision = 0;

        while(d != (int)(d) && precision <= stepRoundDigits)
        {
          d = Math.Round(d * 10, stepRoundDigits);
          precision++;
        }

        priceFormat.NumberDecimalDigits = precision;
      }
    }

    // **********************************************************************

    public Security(string entry)
    {
      priceFormat = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();

      if(string.IsNullOrEmpty(entry))
      {
        this.Entry = string.Empty;
        Reset(string.Empty);
        return;
      }

      this.Entry = entry;

      string[] s = entry.Split(sep);

      if(s.Length == 5)
      {
        CName = s[0];
        Ticker = s[1];
        AuxCode = s[2];

        int id = 0;

        bool error = s[3].Length > 0 && !int.TryParse(s[3],
          NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out id);

        Id = id;

        if(double.TryParse(s[4], NumberStyles.Float,
          NumberFormatInfo.InvariantInfo, out step))
        {
          Step = step;
        }
        else
        {
          Step = 1;
          error = true;
        }

        if(error)
          Ticker = "{" + Ticker + "}";
      }
      else
        Reset("{err}");
    }

    // **********************************************************************

    void Reset(string state)
    {
      CName = Ticker = state;
      AuxCode = string.Empty;
      Id = 0;
      Step = 1;
    }

    // **********************************************************************

    public override string ToString()
    {
      if(Entry.Length == 0)
        return string.Empty;

      return CName + " / " + Ticker
        + (AuxCode.Length == 0 ? string.Empty : " " + AuxCode)
        + (Id == 0 ? string.Empty : " " + Id) + ", "
        + GetPrice(1).ToString("N", priceFormat) + " пт";
    }

    // **********************************************************************

    public int GetTicks(double price)
    {
      return (int)Math.Round(price * inverseStep);
    }

    // **********************************************************************

    public double GetPrice(double ticks) { return ticks * Step; }

    // **********************************************************************
  }
}
