#region Copyright (c) 2011-2013 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;

namespace QScalp
{
  // ************************************************************************

  enum StreamType
  {
    Stock = 0x10,
    Deals = 0x20,
    Orders = 0x30,
    Trades = 0x40,
    Messages = 0x50,
    AuxInfo = 0x60,
    None = 0
  }

  // ************************************************************************

  public enum QuoteType { Unknown, Free, Spread, Ask, Bid, BestAsk, BestBid }

  public enum MessageType { None, Info, Warning, Error }
  public enum DealType { Unknown, Buy, Sell }

  // ************************************************************************

  struct Message
  {
    public readonly DateTime DateTime;
    public readonly MessageType Type;
    public readonly string Text;

    public Message(DateTime dateTime, MessageType type, string text)
    {
      this.DateTime = dateTime;
      this.Type = type;
      this.Text = text;
    }
  }

  // ************************************************************************

  public struct Quote
  {
    public int Price;
    public int Volume;
    public QuoteType Type;

    public Quote(int price, int volume, QuoteType type)
    {
      this.Price = price;
      this.Volume = volume;
      this.Type = type;
    }
  }

  // ************************************************************************

  public struct Spread
  {
    public readonly int Ask;
    public readonly int Bid;

    public Spread(int ask, int bid)
    {
      this.Ask = ask;
      this.Bid = bid;
    }
  }

  // ************************************************************************

  public struct Deal
  {
    public int SecKey;
    public double Price;
    public int Volume;
    public DealType Type;
    public DateTime DateTime;
  }

  // ************************************************************************

  struct OwnOrder
  {
    public readonly long Id;
    public readonly int Price;

    public readonly bool IsActive;
    public readonly int Quantity;

    public readonly object Tag;

    public OwnOrder(long id, int price)
    {
      this.Id = id;
      this.Price = price;
      this.IsActive = false;
      this.Quantity = 0;
      this.Tag = null;
    }

    public OwnOrder(long id, int price, int quantity, object tag)
    {
      this.Id = id;
      this.Price = price;
      this.IsActive = true;
      this.Quantity = quantity;
      this.Tag = tag;
    }
  }

  // ************************************************************************

  struct AuxInfo
  {
    public DateTime DateTime;

    public int AskSum;
    public int BidSum;
    public int OI;

    public DateTime? __DateTime2;
    public int? __PackInt1;
    public int? __PackInt2;
    public int? __PackInt3;

    public AuxInfo(DateTime dateTime, int askSum, int bidSum, int oi)
    {
      this.DateTime = dateTime;
      this.AskSum = askSum;
      this.BidSum = bidSum;
      this.OI = oi;

      __DateTime2 = null;
      __PackInt1 = null;
      __PackInt2 = null;
      __PackInt3 = null;
    }
  }

  // ************************************************************************
}
