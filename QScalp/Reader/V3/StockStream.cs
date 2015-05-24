#region Copyright (c) 2011-2015 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;
using QScalp.Shared;

namespace QScalp.History.Reader.V3
{
  sealed class StockStream : QshStream, IStockStream
  {
    // **********************************************************************

    public Security Security { get; private set; }
    public event Action<int, Quote[], Spread> Handler;

    readonly RawQuotes rawQuotes;

    int basePrice;

    // **********************************************************************

    public StockStream(DataReader dr)
      : base(StreamType.Stock, dr)
    {
      Security = new Security(dr.ReadString());
      rawQuotes = new RawQuotes();
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      int n = dr.ReadPackInt();

      for(int i = 0; i < n; i++)
      {
        int p = dr.ReadRelative(ref basePrice);
        int v = dr.ReadPackInt();

        if(v == 0)
          rawQuotes.Remove(p);
        else
          rawQuotes[p] = v;
      }

      if(push && Handler != null)
      {
        Quote[] quotes;
        Spread spread;

        rawQuotes.GetQuotes(out quotes, out spread);

        Handler(Security.Key, quotes, spread);
      }
    }

    // **********************************************************************
  }
}
