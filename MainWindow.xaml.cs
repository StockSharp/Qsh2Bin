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

	using QScalp;
	using QScalp.History.Reader;

	using StockSharp.Algo;
	using StockSharp.Algo.Storages;
	using StockSharp.BusinessEntities;
	using StockSharp.Logging;
	using StockSharp.Xaml;

	using Quote = StockSharp.BusinessEntities.Quote;
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
				ConvertDirectory(qshPath, registry, ExchangeBoard.MicexEqbr /* надо сделать выбор в GUI */);
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

			var data = new Dictionary<Security, Tuple<List<MarketDepth>, List<Trade>, List<SecurityChange>>>();

			using (var qr = QshReader.Open(fileName))
			{
				for (var i = 0; i < qr.StreamsCount; i++)
				{
					dynamic stream = qr[i];
					Security security = GetSecurity(stream.Security, board);

					var secData = data.SafeAdd(security, key => Tuple.Create(new List<MarketDepth>(), new List<Trade>(), new List<SecurityChange>()));

					switch ((StreamType)stream.Type)
					{
						case StreamType.Stock:
						{
							((IStockStream)stream).Handler += (key, quotes, spread) =>
							{
								var md = new MarketDepth(security).Update(quotes.Select(q =>
								{
									OrderDirections dir;

									switch (q.Type)
									{
										case QuoteType.Unknown:
										case QuoteType.Free:
										case QuoteType.Spread:
											throw new ArgumentException(q.Type.ToString());
										case QuoteType.Ask:
										case QuoteType.BestAsk:
											dir = OrderDirections.Sell;
											break;
										case QuoteType.Bid:
										case QuoteType.BestBid:
											dir = OrderDirections.Buy;
											break;
										default:
											throw new ArgumentOutOfRangeException();
									}

									return new Quote(security, security.MinStepSize * q.Price, q.Volume, dir);
								}), qr.CurrentDateTime);

								if (md.Verify())
								{
									secData.Item1.Add(md);

									if (secData.Item1.Count > maxBufCount)
									{
										registry.GetMarketDepthStorage(security).Save(secData.Item1);
										secData.Item1.Clear();
									}
								}
								else
									_logManager.Application.AddErrorLog("Стакан для {0} в момент {1} не прошел валидацию. Лучший бид {2}, Лучший офер {3}.", security, qr.CurrentDateTime, md.BestBid, md.BestAsk);
							};
							break;
						}
						case StreamType.Deals:
						{
							((IDealsStream)stream).Handler += deal =>
							{
								secData.Item2.Add(new Trade
								{
									Security = security,
									Time = deal.DateTime,
									Volume = deal.Volume,
									Price = (decimal)deal.Price,
									OrderDirection =
										deal.Type == DealType.Buy
											? OrderDirections.Buy
											: (deal.Type == DealType.Sell ? OrderDirections.Sell : (OrderDirections?)null)
								});

								if (secData.Item2.Count > maxBufCount)
								{
									registry.GetTradeStorage(security).Save(secData.Item2);
									secData.Item2.Clear();
								}
							};
							break;
						}
						case StreamType.AuxInfo:
						{
							var prevOI = new SecurityChange(security, DateTime.MinValue, SecurityChangeTypes.OpenInterest, 0m);
							var prevBidSum = new SecurityChange(security, DateTime.MinValue, SecurityChangeTypes.BidsVolume, 0m);
							var prevAskSum = new SecurityChange(security, DateTime.MinValue, SecurityChangeTypes.AsksVolume, 0m);

							((IAuxInfoStream)stream).Handler += info =>
							{
								var currOI = new SecurityChange(security, info.DateTime, SecurityChangeTypes.OpenInterest, (decimal)info.OI);
								var currBidSum = new SecurityChange(security, info.DateTime, SecurityChangeTypes.BidsVolume, (decimal)info.BidSum);
								var currAskSum = new SecurityChange(security, info.DateTime, SecurityChangeTypes.AsksVolume, (decimal)info.AskSum);

								if (!currOI.Value.Equals(prevOI.Value))
								{
									secData.Item3.Add(currOI);
									prevOI = currOI;
								}
								else if (!currBidSum.Value.Equals(prevBidSum.Value))
								{
									secData.Item3.Add(currBidSum);
									prevBidSum = currBidSum;
								}
								else if (!currAskSum.Value.Equals(prevAskSum.Value))
								{
									secData.Item3.Add(currAskSum);
									prevAskSum = currAskSum;
								}

								if (secData.Item3.Count > maxBufCount)
								{
									registry.GetSecurityChangeStorage(security).Save(secData.Item3);
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
							throw new ArgumentOutOfRangeException("Неподдерживаемый тип потока {0}.".Put((StreamType)stream.Type));
					}
				}

				while(qr.CurrentDateTime != DateTime.MaxValue)
					qr.Read(true);
			}

			foreach (var pair in data)
			{
				if (pair.Value.Item1.Any())
				{
					registry.GetMarketDepthStorage(pair.Key).Save(pair.Value.Item1);
				}

				if (pair.Value.Item2.Any())
				{
					registry.GetTradeStorage(pair.Key).Save(pair.Value.Item2);
				}

				if (pair.Value.Item3.Any())
				{
					registry.GetSecurityChangeStorage(pair.Key).Save(pair.Value.Item3);
				}
			}
		}

		private Security GetSecurity(QScalp.Security security, ExchangeBoard board)
		{
			return new Security
			{
				Id = _idGenerator.GenerateId(security.Ticker, board),
				Code = security.Ticker,
				ExchangeBoard = board,
				MinStepSize = (decimal)security.Step,
			};
		}

		private void OnFolderChange(string folder)
		{
			Convert.IsEnabled = !QshFolder.Folder.IsEmpty() && !BinFolder.Folder.IsEmpty();
		}
	}
}