#region Copyright (c) 2011-2013 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;

namespace QScalp.History.Reader.V3
{
  sealed class StockStream : QshStream3, IStockStream
  {
    // **********************************************************************

    public Security Security { get; private set; }
    public event Action<int, Quote[], Spread> Handler;

    readonly SortedDictionary<int, int> rawQuotes;

    int basePrice;

    // **********************************************************************

    public StockStream(BinaryReader br)
      : base(StreamType.Stock, br)
    {
      Security = new Security(br.ReadString());
      rawQuotes = new SortedDictionary<int, int>();
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      int n = DataReader.ReadPackInt(br);

      for(int i = 0; i < n; i++)
      {
        int p = DataReader.ReadRelative(br, ref basePrice);
        int v = DataReader.ReadPackInt(br);

        if(v == 0)
          rawQuotes.Remove(p);
        else
          rawQuotes[p] = v;
      }

      if(push && Handler != null)
      {
        Quote[] quotes = new Quote[rawQuotes.Count];
        Spread spread = new Spread();

        int i = rawQuotes.Count - 1;

        foreach(KeyValuePair<int, int> kvp in rawQuotes)
        {
          quotes[i].Price = kvp.Key;

          if(kvp.Value > 0)
          {
            quotes[i].Volume = kvp.Value;

            if(spread.Ask > 0)
              quotes[i].Type = QuoteType.Ask;
            else
            {
              quotes[i].Type = QuoteType.BestAsk;

              int j = i + 1;

              if(j < quotes.Length)
              {
                quotes[j].Type = QuoteType.BestBid;
                spread = new Spread(quotes[i].Price, quotes[j].Price);
              }
            }
          }
          else
          {
            quotes[i].Volume = -kvp.Value;
            quotes[i].Type = QuoteType.Bid;
          }

          i--;
        }

        Handler(Security.Key, quotes, spread);
      }
    }

    // **********************************************************************
  }
}
