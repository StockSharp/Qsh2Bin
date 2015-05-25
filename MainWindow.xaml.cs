namespace StockSharp.Qsh2Bin
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;

	using Ecng.Common;
	using Ecng.Collections;
	using Ecng.Xaml;

	using MoreLinq;

	using QScalp;
	using QScalp.History;
	using QScalp.History.Reader;

	using StockSharp.Algo;
	using StockSharp.Algo.Storages;
	using StockSharp.BusinessEntities;
	using StockSharp.Logging;
	using StockSharp.Messages;
	using StockSharp.Xaml;

	using Security = StockSharp.BusinessEntities.Security;

	public partial class MainWindow
	{
		private readonly SecurityIdGenerator _idGenerator = new SecurityIdGenerator();
		private readonly LogManager _logManager = new LogManager();

		public MainWindow()
		{
			InitializeComponent();

			_logManager.Listeners.Add(new GuiLogListener(LogControl));
			_logManager.Listeners.Add(new FileLogListener { LogDirectory = "Logs", SeparateByDates = SeparateByDateModes.FileName });
		}

		private void Convert_OnClick(object sender, RoutedEventArgs e)
		{
			BusyIndicator.BusyContent = "Запуск конвертации...";
			BusyIndicator.IsBusy = true;

			var qshPath = QshFolder.Folder;
			var binPath = BinFolder.Folder;

			Task.Factory.StartNew(() =>
			{
				var registry = new StorageRegistry();

				((LocalMarketDataDrive)registry.DefaultDrive).Path = binPath;
				ConvertDirectory(qshPath, registry, ExchangeBoard.Forts /* TODO надо сделать выбор в GUI */);
			})
			.ContinueWith(t =>
			{
				BusyIndicator.IsBusy = false;

				if (t.IsFaulted)
				{
					t.Exception.LogError();

					new MessageBoxBuilder()
						.Text("В процессе конвертации произошла ошибка. Ошибка записана в лог.")
						.Error()
						.Owner(this)
						.Show();

					return;
				}

				new MessageBoxBuilder()
					.Text("Конвертация выполнена.")
					.Owner(this)
					.Show();

			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		private void ConvertDirectory(string path, IStorageRegistry registry, ExchangeBoard board)
		{
			Directory.GetFiles(path, "*.qsh").ForEach(f => ConvertFile(f, registry, board));
			Directory.GetDirectories(path).ForEach(d => ConvertDirectory(d, registry, board));
		}

		private void ConvertFile(string fileName, IStorageRegistry registry, ExchangeBoard board)
		{
			this.GuiAsync(() => BusyIndicator.BusyContent = "Конвертация файла {0}...".Put(Path.GetFileName(fileName)));

			const int maxBufCount = 1000;

			var data = new Dictionary<Security, Tuple<List<QuoteChangeMessage>, List<ExecutionMessage>, List<Level1ChangeMessage>, List<ExecutionMessage>>>();

			using (var qr = QshReader.Open(fileName))
			{
				var currentDate = qr.CurrentDateTime;

				for (var i = 0; i < qr.StreamCount; i++)
				{
					var stream = (ISecurityStream)qr[i];
					var security = GetSecurity(stream.Security, board);
					var priceStep = security.PriceStep ?? 1;
					var securityId = security.ToSecurityId();

					var secData = data.SafeAdd(security, key => Tuple.Create(new List<QuoteChangeMessage>(), new List<ExecutionMessage>(), new List<Level1ChangeMessage>(), new List<ExecutionMessage>()));

					switch (stream.Type)
					{
						case StreamType.Stock:
						{
							((IStockStream)stream).Handler += (key, quotes, spread) =>
							{
								var quotes2 = quotes.Select(q =>
								{
									Sides side;

									switch (q.Type)
									{
										case QuoteType.Unknown:
										case QuoteType.Free:
										case QuoteType.Spread:
											throw new ArgumentException(q.Type.ToString());
										case QuoteType.Ask:
										case QuoteType.BestAsk:
											side = Sides.Sell;
											break;
										case QuoteType.Bid:
										case QuoteType.BestBid:
											side = Sides.Buy;
											break;
										default:
											throw new ArgumentOutOfRangeException();
									}

									return new QuoteChange(side, priceStep * q.Price, q.Volume);
								}).ToArray();

								var md = new QuoteChangeMessage
								{
									SecurityId = securityId,
									ServerTime = currentDate.ApplyTimeZone(TimeHelper.Moscow),
									Bids = quotes2.Where(q => q.Side == Sides.Buy),
									Asks = quotes2.Where(q => q.Side == Sides.Sell),
								};

								//if (md.Verify())
								//{
								secData.Item1.Add(md);

								if (secData.Item1.Count > maxBufCount)
								{
									registry.GetQuoteMessageStorage(security).Save(secData.Item1);
									secData.Item1.Clear();
								}
								//}
								//else
								//	_logManager.Application.AddErrorLog("Стакан для {0} в момент {1} не прошел валидацию. Лучший бид {2}, Лучший офер {3}.", security, qr.CurrentDateTime, md.BestBid, md.BestAsk);
							};
							break;
						}
						case StreamType.Deals:
						{
							((IDealsStream)stream).Handler += deal =>
							{
								secData.Item2.Add(new ExecutionMessage
								{
									ExecutionType = ExecutionTypes.Tick,
									SecurityId = securityId,
									OpenInterest = deal.OI == 0 ? (long?)null : deal.OI,
									ServerTime = deal.DateTime.ApplyTimeZone(TimeHelper.Moscow),
									Volume = deal.Volume,
									TradeId = deal.Id == 0 ? (long?)null : deal.Id,
									TradePrice = (decimal)deal.Price,
									OriginSide = 
										deal.Type == DealType.Buy
											? Sides.Buy
											: (deal.Type == DealType.Sell ? Sides.Sell : (Sides?)null)
								});

								if (secData.Item2.Count > maxBufCount)
								{
									registry.GetTickMessageStorage(security).Save(secData.Item2);
									secData.Item2.Clear();
								}
							};
							break;
						}
						case StreamType.OrdLog:
						{
							((IOrdLogStream)stream).Handler += (key, ol) =>
							{
								var msg = new ExecutionMessage
								{
									ExecutionType = ExecutionTypes.OrderLog,
									SecurityId = securityId,
									OpenInterest = ol.OI == 0 ? (long?)null : ol.OI,
									OrderId = ol.OrderId,
									Price = priceStep * ol.Price,
									ServerTime = ol.DateTime.ApplyTimeZone(TimeHelper.Moscow),
									Volume = ol.Amount,
									Balance = ol.AmountRest,
									TradeId = ol.DealId == 0 ? (long?)null : ol.DealId,
									TradePrice = ol.DealPrice == 0 ? (decimal?)null : priceStep * ol.DealPrice,
								};

								if (ol.Flags.Contains(OrdLogFlags.Add))
								{
									msg.OrderState = OrderStates.Active;
								}
								else if (ol.Flags.Contains(OrdLogFlags.Fill))
								{
									msg.OrderState = OrderStates.Done;
								}
								else if (ol.Flags.Contains(OrdLogFlags.Canceled))
								{
									msg.OrderState = OrderStates.Done;
								}
								else if (ol.Flags.Contains(OrdLogFlags.CanceledGroup))
								{
									msg.OrderState = OrderStates.Done;
								}

								if (ol.Flags.Contains(OrdLogFlags.Buy))
								{
									msg.OriginSide = Sides.Buy;
								}
								else if (ol.Flags.Contains(OrdLogFlags.Sell))
								{
									msg.OriginSide = Sides.Sell;
								}

								if (ol.Flags.Contains(OrdLogFlags.FillOrKill))
								{
									msg.TimeInForce = TimeInForce.MatchOrCancel;
								}

								if (ol.Flags.Contains(OrdLogFlags.Quote))
								{
									msg.TimeInForce = TimeInForce.PutInQueue;
								}

								if (ol.Flags.Contains(OrdLogFlags.Counter))
								{
								}

								if (ol.Flags.Contains(OrdLogFlags.CrossTrade))
								{
								}

								if (ol.Flags.Contains(OrdLogFlags.NonSystem))
								{
									msg.IsSystem = false;
								}

								secData.Item4.Add(msg);

								if (secData.Item4.Count > maxBufCount)
								{
									registry.GetOrderLogMessageStorage(security).Save(secData.Item4);
									secData.Item4.Clear();
								}
							};
							break;
						}
						case StreamType.AuxInfo:
						{
							((IAuxInfoStream)stream).Handler += (key, info) =>
							{
								secData.Item3.Add(new Level1ChangeMessage
								{
									SecurityId = securityId,
									ServerTime = info.DateTime.ApplyTimeZone(TimeHelper.Moscow),
								}
								.TryAdd(Level1Fields.LastTradePrice, priceStep * info.Price)
								.TryAdd(Level1Fields.BidsVolume, (decimal)info.BidTotal)
								.TryAdd(Level1Fields.AsksVolume, (decimal)info.AskTotal)
								.TryAdd(Level1Fields.HighPrice, priceStep * info.HiLimit)
								.TryAdd(Level1Fields.LowPrice, priceStep * info.LoLimit)
								.TryAdd(Level1Fields.OpenInterest, (decimal)info.OI));

								if (secData.Item3.Count > maxBufCount)
								{
									registry.GetLevel1MessageStorage(security).Save(secData.Item3);
									secData.Item3.Clear();
								}
							};
							break;
						}
						case StreamType.Orders:
						case StreamType.Trades:
						case StreamType.Messages:
						case StreamType.None:
						{
							continue;
						}
						default:
							throw new ArgumentOutOfRangeException("Неподдерживаемый тип потока {0}.".Put(stream.Type));
					}
				}

				while (qr.CurrentDateTime != DateTime.MaxValue)
					qr.Read(true);
			}

			foreach (var pair in data)
			{
				if (pair.Value.Item1.Any())
				{
					registry.GetQuoteMessageStorage(pair.Key).Save(pair.Value.Item1);
				}

				if (pair.Value.Item2.Any())
				{
					registry.GetTickMessageStorage(pair.Key).Save(pair.Value.Item2);
				}

				if (pair.Value.Item3.Any())
				{
					registry.GetLevel1MessageStorage(pair.Key).Save(pair.Value.Item3);
				}

				if (pair.Value.Item4.Any())
				{
					registry.GetOrderLogMessageStorage(pair.Key).Save(pair.Value.Item4);
				}
			}
		}

		private Security GetSecurity(QScalp.Security security, ExchangeBoard board)
		{
			return new Security
			{
				Id = _idGenerator.GenerateId(security.Ticker, board),
				Code = security.Ticker,
				Board = board,
				PriceStep = (decimal)security.Step,
				Decimals = security.Precision
			};
		}

		private void OnFolderChange(string folder)
		{
			Convert.IsEnabled = !QshFolder.Folder.IsEmpty() && !BinFolder.Folder.IsEmpty();
		}
	}
}