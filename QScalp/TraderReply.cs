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

  public abstract class TraderReply
  {
    public enum Types { Acceptance, Rejection, OrderUpdate, OwnTrade }

    public readonly Types Type;

    public virtual int TId { get { throw new NotSupportedException(); } }
    public virtual long OId { get { throw new NotSupportedException(); } }
    public virtual string Error { get { throw new NotSupportedException(); } }

    public virtual int Active { get { throw new NotSupportedException(); } }
    public virtual int Filled { get { throw new NotSupportedException(); } }
    public virtual bool Incremental { get { throw new NotSupportedException(); } }

    public virtual DateTime DateTime { get { throw new NotSupportedException(); } }
    public virtual int PTicks { get { throw new NotSupportedException(); } }
    public virtual int Quantity { get { throw new NotSupportedException(); } }

    public TraderReply(Types type) { this.Type = type; }
  }

  // ************************************************************************

  public sealed class OrderUpdateReply : TraderReply
  {
    readonly int tid;
    readonly long oid;
    readonly int active;
    readonly int filled;
    readonly bool incremental;

    public override int TId { get { return tid; } }
    public override long OId { get { return oid; } }
    public override int Active { get { return active; } }
    public override int Filled { get { return filled; } }
    public override bool Incremental { get { return incremental; } }

    public OrderUpdateReply(int tid, long oid, int active, int filled, bool incremental)
      : base(Types.OrderUpdate)
    {
      this.tid = tid;
      this.oid = oid;
      this.active = active;
      this.filled = filled;
      this.incremental = incremental;
    }
  }

  // ************************************************************************

  public sealed class OwnTradeReply : TraderReply
  {
    readonly long oid;
    readonly DateTime dateTime;
    readonly int pticks;
    readonly int quantity;

    public override long OId { get { return oid; } }
    public override DateTime DateTime { get { return dateTime; } }
    public override int PTicks { get { return pticks; } }
    public override int Quantity { get { return quantity; } }

    public OwnTradeReply(long oid, DateTime dateTime, int pticks, int quantity)
      : base(Types.OwnTrade)
    {
      this.oid = oid;
      this.dateTime = dateTime;
      this.pticks = pticks;
      this.quantity = quantity;
    }
  }

  // ************************************************************************
}
