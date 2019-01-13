#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;
using QScalp.Shared;

namespace QScalp.History.Reader.V4
{
  sealed class QuotesStream : QshStream, IQuotesStream
  {
    // **********************************************************************

    readonly RawQuotes rawQuotes;
    int lastPrice;

    // **********************************************************************

    public Security Security { get; private set; }
    public event Action<Quote[]> Handler;

    // **********************************************************************

    public QuotesStream(DataReader dr)
      : base(StreamType.Quotes, dr)
    {
      Security = new Security(dr.ReadString());
      rawQuotes = new RawQuotes();
    }

    // **********************************************************************

    public override void Read(bool push)
    {
      int n = (int)dr.ReadLeb128();

      for(int i = 0; i < n; i++)
      {
        lastPrice += (int)dr.ReadLeb128();
        int v = (int)dr.ReadLeb128();

        if(v == 0)
          rawQuotes.Remove(lastPrice);
        else
          rawQuotes[lastPrice] = v;
      }

      if(push && Handler != null)
        Handler(rawQuotes.GetQuotes());
    }

    // **********************************************************************
  }
}
