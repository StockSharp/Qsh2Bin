#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;
using System.Collections.Generic;

namespace QScalp.History.Writer
{
  sealed class QuotesStream
  {
    // **********************************************************************

    Quote[] lastQuotes = new Quote[0];
    readonly List<Quote> diffQuotes = new List<Quote>(100);

    int lastPrice;

    readonly DataWriter dw;
    readonly int sid;

    // **********************************************************************

    public QuotesStream(DataWriter dw, int sid, Security s)
    {
      this.dw = dw;
      this.sid = sid;

      dw.Write((byte)StreamType.Quotes);
      dw.Write(s.Entry);
    }

    // **********************************************************************

    static bool QuoteTypeEquals(QuoteType qt1, QuoteType qt2)
    {
      switch(qt1)
      {
        case QuoteType.Ask:
        case QuoteType.BestAsk:
          return qt2 == QuoteType.Ask || qt2 == QuoteType.BestAsk;

        case QuoteType.Bid:
        case QuoteType.BestBid:
          return qt2 == QuoteType.Bid || qt2 == QuoteType.BestBid;

        default:
          return qt1 == qt2;
      }
    }

    // **********************************************************************

    public void Write(DateTime dateTime, Quote[] quotes)
    {
      // ------------------------------------------------------------

      diffQuotes.Clear();

      int i = 0;

      foreach(Quote lq in lastQuotes)
      {
        while(i < quotes.Length && lq.Price < quotes[i].Price)
          diffQuotes.Add(quotes[i++]);

        if(i < quotes.Length && quotes[i].Price == lq.Price)
        {
          if(!QuoteTypeEquals(lq.Type, quotes[i].Type) || lq.Volume != quotes[i].Volume)
            diffQuotes.Add(quotes[i]);

          i++;
        }
        else
          diffQuotes.Add(new Quote(lq.Price, 0, QuoteType.Unknown));
      }

      while(i < quotes.Length)
        diffQuotes.Add(quotes[i++]);

      lastQuotes = quotes;

      // ------------------------------------------------------------

      if(diffQuotes.Count > 0)
      {
        dw.WriteRecHeader(sid, dateTime);
        dw.WriteLeb128(diffQuotes.Count);

        foreach(Quote q in diffQuotes)
        {
          dw.WriteLeb128(q.Price - lastPrice);
          lastPrice = q.Price;

          switch(q.Type)
          {
            case QuoteType.Ask:
            case QuoteType.BestAsk:
              dw.WriteLeb128(q.Volume);
              break;

            case QuoteType.Bid:
            case QuoteType.BestBid:
              dw.WriteLeb128(-q.Volume);
              break;

            default:
              dw.WriteLeb128(0);
              break;
          }
        }
      }

      // ------------------------------------------------------------
    }

    // **********************************************************************
  }
}
