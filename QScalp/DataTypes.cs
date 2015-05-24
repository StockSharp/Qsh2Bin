#region Copyright (c) 2011-2015 Николай Морошкин, http://www.moroshkin.com/
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

  enum QuoteType { Unknown, Free, Spread, Ask, Bid, BestAsk, BestBid }
  enum DealType { Unknown, Buy, Sell }
  enum OwnOrderType { None, Regular, Stop }
  enum OwnTradeSource { Unknown, Real, Emulator, Manual, History }
  enum MessageType { None, Info, Warning, Error }
  enum TraderReplyType { Acceptance, Rejection, OrderUpdate, OwnTrade, StopOrder }

  // ************************************************************************

  struct Quote
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

  sealed class Deal
  {
    public int SecKey;
    public long Id;
    public double Price;
    public int Volume;
    public DealType Type;
    public int OI;
    public DateTime DateTime;
  }

  // ************************************************************************

  struct OwnOrder
  {
    public readonly OwnOrderType Type;

    public readonly long Id;
    public readonly int Price;
    public readonly int Quantity;

    public readonly object Transaction;

    public OwnOrder(long id, int price)
    {
      this.Type = OwnOrderType.None;
      this.Id = id;
      this.Price = price;
      this.Quantity = 0;
      this.Transaction = null;
    }

    public OwnOrder(OwnOrderType type, long id, int price, int quantity, object t)
    {
      this.Type = type;
      this.Id = id;
      this.Price = price;
      this.Quantity = quantity;
      this.Transaction = t;
    }
  }

  // ************************************************************************

  sealed class OwnTradeReply
  {
    public readonly TraderReplyType Type;
    public readonly OwnTradeSource Source;
    public readonly DateTime DateTime;
    public readonly long TradeId;
    public readonly long OrderId;
    public readonly int Price;
    public readonly int Quantity;

    public OwnTradeReply(OwnTradeSource source, DateTime dateTime,
      long tradeId, long orderId, int price, int quantity)
    {
      this.Type = TraderReplyType.OwnTrade;

      this.Source = source;
      this.DateTime = dateTime;
      this.TradeId = tradeId;
      this.OrderId = orderId;
      this.Price = price;
      this.Quantity = quantity;
    }
  }

  // ************************************************************************

  struct Message
  {
    public readonly DateTime DateTime;
    public readonly MessageType Type;
    public readonly string Text;

    public Message(MessageType type, string text)
    {
      this.DateTime = DateTime.Now;
      this.Type = type;
      this.Text = text;
    }

    public Message(DateTime dateTime, MessageType type, string text)
    {
      this.DateTime = dateTime;
      this.Type = type;
      this.Text = text;
    }
  }

  // ************************************************************************

  class AuxInfo
  {
    public readonly DateTime DateTime;
    public readonly int Price;

    public readonly int AskTotal;
    public readonly int BidTotal;
    public readonly int OI;

    public readonly int HiLimit;
    public readonly int LoLimit;
    public readonly double Deposit;

    public readonly double Rate;
    public readonly string Message;

    public AuxInfo() { }

    public AuxInfo(DateTime dateTime, int price, int askTotal, int bidTotal,
      int oi, int hiLimit, int loLimit, double deposit, double rate, string message)
    {
      this.DateTime = dateTime;
      this.Price = price;

      this.AskTotal = askTotal;
      this.BidTotal = bidTotal;
      this.OI = oi;

      this.HiLimit = hiLimit;
      this.LoLimit = loLimit;
      this.Deposit = deposit;

      this.Rate = rate;
      this.Message = message;
    }
  }

  // ************************************************************************

  [Flags]
  enum OrdLogFlags
  {
    NonZeroReplAct = 1 << 0,
    SessIdChanged = 1 << 1,

    Add = 1 << 2,
    Fill = 1 << 3,

    Buy = 1 << 4,
    Sell = 1 << 5,

    Quote = 1 << 7, // Котировочная
    Counter = 1 << 8, // Встречная
    NonSystem = 1 << 9, // Внесистемная
    EndOfTransaction = 1 << 10, // Запись является последней в транзакции
    FillOrKill = 1 << 11, // Заявка Fill-or-kill
    Moved = 1 << 12, // Запись является результатом операции перемещения заявки
    Canceled = 1 << 13, // Запись является результатом операции удаления заявки
    CanceledGroup = 1 << 14, // Запись является результатом группового удаления
    CrossTrade = 1 << 15, // Признак удаления остатка заявки по причине кросс-сделки

    None = 0
  }

  // ************************************************************************

  class OrdLogEntry
  {
    public readonly OrdLogFlags Flags;

    public readonly DateTime DateTime;
    public readonly long OrderId;

    public readonly int Price;

    public readonly int Amount;
    public readonly int AmountRest;

    public readonly long DealId;
    public readonly int DealPrice;
    public readonly int OI;

    public OrdLogEntry(OrdLogFlags flags, DateTime dateTime, long orderId,
      int price, int amount, int amountRest, long dealId, int dealPrice, int oi)
    {
      this.Flags = flags;
      this.DateTime = dateTime;
      this.OrderId = orderId;
      this.Price = price;
      this.Amount = amount;
      this.AmountRest = amountRest;
      this.DealId = dealId;
      this.DealPrice = dealPrice;
      this.OI = oi;
    }
  }

  // ************************************************************************
}
