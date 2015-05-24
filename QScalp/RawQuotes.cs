#region Copyright (c) 2011-2015 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System.Collections.Generic;

namespace QScalp.Shared
{
  sealed class RawQuotes : SortedDictionary<int, int>
  {
    // **********************************************************************

    sealed class PriceComparer : IComparer<int>
    {
      int IComparer<int>.Compare(int x, int y)
      {
        if(x > y)
          return -1;

        if(x < y)
          return 1;

        return 0;
      }
    }

    // **********************************************************************

    public RawQuotes() : base(new PriceComparer()) { }

    // **********************************************************************

    public void GetQuotes(out Quote[] quotes, out Spread spread)
    {
      quotes = new Quote[this.Count];

      SortedDictionary<int, int>.Enumerator enumerator = this.GetEnumerator();
      int index = -1;

      while(enumerator.MoveNext())
      {
        KeyValuePair<int, int> kvp = enumerator.Current;

        if(kvp.Value > 0)
        {
          index++;
          quotes[index] = new Quote(kvp.Key, kvp.Value, QuoteType.Ask);
        }
        else if(kvp.Value < 0)
        {
          if(index >= 0)
          {
            quotes[index].Type = QuoteType.BestAsk;
            spread = new Spread(quotes[index].Price, kvp.Key);
          }
          else
            spread = new Spread(0, kvp.Key);

          index++;
          quotes[index] = new Quote(kvp.Key, -kvp.Value, QuoteType.BestBid);

          while(enumerator.MoveNext())
          {
            kvp = enumerator.Current;

            if(kvp.Value < 0)
            {
              index++;
              quotes[index] = new Quote(kvp.Key, -kvp.Value, QuoteType.Bid);
            }
          }

          return;
        }
      }

      if(index >= 0)
      {
        quotes[index].Type = QuoteType.BestAsk;
        spread = new Spread(quotes[index].Price, 0);
      }
      else
        spread = new Spread();
    }

    // **********************************************************************
  }
}
