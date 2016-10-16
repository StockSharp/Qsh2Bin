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
  public class Security
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

    public int Precision { get { return priceFormat.NumberDecimalDigits; } }
    public int Key { get; private set; }

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
        {
          Ticker = "{" + Ticker + "}";
          Key = 0;
        }
        else
          InitKey();
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
      Key = 0;
    }

    // **********************************************************************

    void InitKey()
    {
      if(Id != 0)
        Key = GetKey(GetKey(CName), Id);
      else if(AuxCode.Length == 0)
        Key = GetKey(GetKey(CName), Ticker);
      else
        Key = GetKey(GetKey(CName), Ticker, AuxCode);
    }

    // **********************************************************************

    public static int GetKey(string cname)
    {
      return cname.GetHashCode();
    }

    public static int GetKey(int ckey, int id)
    {
      return ckey ^ id;
    }

    public static int GetKey(int ckey, string ticker)
    {
      return ckey ^ ticker.GetHashCode();
    }

    public static int GetKey(int ckey, string ticker, string auxcode)
    {
      return ckey ^ ticker.GetHashCode() ^ ~auxcode.GetHashCode();
    }

    // **********************************************************************

    public override string ToString()
    {
      if(Entry.Length == 0)
        return string.Empty;

      return CName + " / " + Ticker
        + (AuxCode.Length == 0 ? string.Empty : " " + AuxCode)
        + (Id == 0 ? string.Empty : " " + Id)
        + ", " + GetString(1, "N") + " пт";
    }

    // **********************************************************************

    public int GetTicks(double price)
    {
      return (int)Math.Round(price * inverseStep);
    }

    // **********************************************************************

    public double GetPrice(double ticks) { return ticks * Step; }

    // **********************************************************************

    public string GetString(double ticks, string fmt)
    {
      return GetPrice(ticks).ToString(fmt, priceFormat);
    }

    // **********************************************************************
  }
}
