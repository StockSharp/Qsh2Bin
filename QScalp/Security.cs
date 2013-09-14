#region Copyright (c) 2011-2013 Николай Морошкин, http://www.moroshkin.com/
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
  public class Security
  {
    // **********************************************************************

    const int StepRoundDigits = 14;

    static readonly char[] sep = new char[] { ':' };

    string entry;

    string auxcode;
    double step, inverseStep;

    NumberFormatInfo priceFormat;

    // **********************************************************************

    public string CName { get; private set; }
    public string Ticker { get; private set; }

    public string AuxCode
    {
      get { return auxcode; }

      private set
      {
        if(value == null || value.Length == 0)
          auxcode = null;
        else
          auxcode = value;
      }
    }

    public int Id { get; private set; }

    public double Step
    {
      get { return step; }

      private set
      {
        if(value > 0)
          step = Math.Round(value, StepRoundDigits);
        else
          step = 1;

        inverseStep = 1 / step;

        double d = step;
        int precision = 0;

        while(d != (int)(d) && precision <= StepRoundDigits)
        {
          d = Math.Round(d * 10, StepRoundDigits);
          precision++;
        }

        priceFormat.NumberDecimalDigits = precision;
      }
    }

    public int Precision { get { return priceFormat.NumberDecimalDigits; } }
    public int Key { get; private set; }

    // **********************************************************************

    public string Entry
    {
      get { return entry; }

      set
      {
        if(value == null)
          entry = string.Empty;
        else
          entry = value;

        if(entry.Length == 0)
        {
          Reset(string.Empty);
          return;
        }

        string[] s = entry.Split(sep);

        if(s.Length == 5)
        {
          CName = s[0];
          Ticker = s[1];
          AuxCode = s[2];

          int id = 0;

          bool error = s[3].Length > 0 && !int.TryParse(s[3], NumberStyles.Integer,
            NumberFormatInfo.InvariantInfo, out id);

          Id = id;

          if(double.TryParse(s[4], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out step))
            Step = step;
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
    }

    // **********************************************************************

    public Security(string entry)
    {
      priceFormat = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();
      this.Entry = entry;
    }

    // **********************************************************************

    void Reset(string state)
    {
      CName = Ticker = state;
      AuxCode = null;
      Id = 0;
      Step = 1;
      Key = 0;
    }

    // **********************************************************************

    public override string ToString()
    {
      if(entry.Length == 0)
        return string.Empty;

      return CName + " / " + Ticker
        + (AuxCode == null ? string.Empty : " " + AuxCode)
        + (Id == 0 ? string.Empty : " " + Id)
        + ", " + GetString(1) + " пт";
    }

    // **********************************************************************

    public int GetTicks(double price)
    {
      return (int)Math.Round(price * inverseStep);
    }

    // **********************************************************************

    public double GetPrice(double ticks) { return ticks * Step; }

    // **********************************************************************

    public string GetString(double ticks, string fmt = "N")
    {
      return GetPrice(ticks).ToString(fmt, priceFormat);
    }

    // **********************************************************************
  }
}
