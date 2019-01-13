namespace StockSharp.Qsh2StockSharp
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;

	using Ecng.Common;
	using Ecng.Collections;
	using Ecng.Serialization;
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
	using StockSharp.Plaza;
	using StockSharp.Xaml;

	public partial class MainWindow
	{
		private class Settings
		{
			public string QshFolder { get; set; }
			public string StockSharpFolder { get; set; }
			public StorageFormats Format { get; set; }
			public string Board { get; set; }
			public string SecurityLike { get; set; }
			public bool MultiThread { get; set; }
			public bool OrderLog2OrderBook { get; set; }
			public int OrderBookMaxDepth { get; set; } = 5;
		}

		private readonly SecurityIdGenerator _idGenerator = new SecurityIdGenerator();
		private readonly LogManager _logManager = new LogManager();

		private bool _isStarted;

		private DateTimeOffset _startConvertTime;

		private const string _settingsDir = "Settings";

		private static readonly string _settingsFile = Path.Combine(_settingsDir, "settings.xml");
		private static readonly string _convertedFilesFile = Path.Combine(_settingsDir, "converted_files.txt");

		private readonly HashSet<string> _convertedFiles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
		private readonly HashSet<string> _convertedPerTaskPoolFiles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		public MainWindow()
		{
			InitializeComponent();

			Directory.CreateDirectory(_settingsDir);

			_logManager.Application.LogLevel = LogLevels.Verbose;

			_logManager.Listeners.Add(new GuiLogListener(LogControl));
			_logManager.Listeners.Add(new FileLogListener { LogDirectory = "Logs", SeparateByDates = SeparateByDateModes.FileName });

			Format.SetDataSource<StorageFormats>();
			Format.SetSelectedValue<StorageFormats>(StorageFormats.Binary);

			Board.Boards.AddRange(ExchangeBoard.EnumerateExchangeBoards().Where(b => b.Exchange == Exchange.Moex));
            Board.SelectedBoard = ExchangeBoard.Forts;

			try
			{
				if (File.Exists(_settingsFile))
				{
					var settings = new XmlSerializer<Settings>().Deserialize(_settingsFile);

					QshFolder.Folder = settings.QshFolder;
					StockSharpFolder.Folder = settings.StockSharpFolder;
					Format.SetSelectedValue<StorageFormats>(settings.Format);
					SecurityLike.Text = settings.SecurityLike;
					MultiThread.IsChecked = settings.MultiThread;
					OrderLog2OrderBook.IsChecked = settings.OrderLog2OrderBook;

					Board.SelectedBoard =
						settings.Board.IsEmpty()
							? ExchangeBoard.Forts
							: Board.Boards.FirstOrDefault(b => b.Code.CompareIgnoreCase(settings.Board)) ?? ExchangeBoard.Forts;
				}

				if (File.Exists(_convertedFilesFile))
				{
					_convertedFiles.AddRange(File.ReadAllLines(_convertedFilesFile));
				}
			}
			catch (Exception ex)
			{
				ex.LogError();
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			SaveSettings();
			base.OnClosed(e);
		}

		private Settings SaveSettings()
		{
			var settings = new Settings
			{
				QshFolder = QshFolder.Folder,
				StockSharpFolder = StockSharpFolder.Folder,
				Format = Format.GetSelectedValue<StorageFormats>() ?? StorageFormats.Binary,
				SecurityLike = SecurityLike.Text,
				Board = Board.SelectedBoard?.Code,
				MultiThread = MultiThread.IsChecked == true,
				OrderLog2OrderBook = OrderLog2OrderBook.IsEnabled && OrderLog2OrderBook.IsChecked == true,
			};

			try
			{
				new XmlSerializer<Settings>().Serialize(settings, _settingsFile);
			}
			catch (Exception ex)
			{
				ex.LogError();
			}

			return settings;
		}

		private void LockControls(bool isEnabled)
		{
			FoldersGrid.IsEnabled = StorageSettingsBox.IsEnabled = SecSettingsBox.IsEnabled = isEnabled;
		}

		private void Convert_OnClick(object sender, RoutedEventArgs e)
		{
			Convert.IsEnabled = false;

			if (_isStarted)
			{
				_logManager.Application.AddInfoLog("Остановка конвертации.");
				_isStarted = false;
				return;
			}

			LockControls(false);

			_logManager.Application.AddInfoLog("Запуск конвертации.");
			_isStarted = true;

			var settings = SaveSettings();
			var board = Board.SelectedBoard;

			var orderLog2OrderBookBuilders = settings.OrderLog2OrderBook ? new Dictionary<SecurityId, IOrderLogMarketDepthBuilder>() : null;

			Task.Factory.StartNew(() =>
			{
				var registry = new StorageRegistry();
				((LocalMarketDataDrive)registry.DefaultDrive).Path = settings.StockSharpFolder;

				this.GuiAsync(() =>
				{
					Convert.Content = "Остановить";
					Convert.IsEnabled = true;
				});

				_startConvertTime = DateTimeOffset.Now;

				ConvertDirectory(settings.QshFolder, registry, settings.Format, board, settings.SecurityLike, settings.MultiThread, orderLog2OrderBookBuilders, settings.OrderBookMaxDepth);
			})
			.ContinueWith(t =>
			{
				Convert.Content = "Запустить";
				Convert.IsEnabled = true;

				LockControls(true);

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

				var text = "Конвертация {0} {1}.".Put(_isStarted ? "выполнена за" : "остановлена через", 
					(DateTimeOffset.Now - _startConvertTime).ToString("g"));

				_logManager.Application.AddInfoLog(text);

				new MessageBoxBuilder()
					.Text(text)
					.Owner(this)
					.Show();

				_isStarted = false;

			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		private void ConvertDirectory(string path, IStorageRegistry registry, StorageFormats format, ExchangeBoard board, string securityLike, bool multithread, Dictionary<SecurityId, IOrderLogMarketDepthBuilder> orderLog2OrderBookBuilders, int orderBookMaxDepth)
		{
			if (!_isStarted)
				return;

			var files = Directory.GetFiles(path, "*.qsh");

			if (!multithread)
				files.ForEach(f => ConvertFile(f, registry, format, board, securityLike, orderLog2OrderBookBuilders, orderBookMaxDepth));
			else
			{
				Parallel.ForEach(files, file => ConvertFile(file, registry, format, board, securityLike, orderLog2OrderBookBuilders, orderBookMaxDepth));
			}

			//пишем имена сконвертированных в деректории файлов qsh, в файл 
			File.AppendAllLines(_convertedFilesFile, _convertedPerTaskPoolFiles);
			_convertedPerTaskPoolFiles.Clear();

			Directory.GetDirectories(path).ForEach(d => ConvertDirectory(d, registry, format, board, securityLike, multithread, orderLog2OrderBookBuilders, orderBookMaxDepth));
		}

		private void TryFlushData<TMessage>(IStorageRegistry registry, BusinessEntities.Security security, StorageFormats format, object arg, List<TMessage> messages, QshReader reader)
			where TMessage : Messages.Message
		{
			const int maxBufCount = 1000;

			if (messages.Count <= maxBufCount)
				return;

			_logManager.Application.AddDebugLog("Файл прочитан на {1}%.", messages.Count, (reader.FilePosition * 100) / reader.FileSize);

			registry.GetStorage(security, typeof(TMessage), arg, null, format).Save(messages);
			messages.Clear();
		}

		private void ConvertFile(string fileName, IStorageRegistry registry, StorageFormats format, ExchangeBoard board, string securityLike, Dictionary<SecurityId, IOrderLogMarketDepthBuilder> orderLog2OrderBookBuilders, int orderBookMaxDepth)
		{
			if (!_isStarted)
				return;

			var fileNameKey = format + "_" + fileName;

			if (_convertedFiles.Contains(fileNameKey))
				return;

			_logManager.Application.AddInfoLog("Начата конвертация файла {0}.", fileName);

			var securitiesStrings = securityLike.Split(",");

			var data = new Dictionary<BusinessEntities.Security, Tuple<List<QuoteChangeMessage>, List<ExecutionMessage>, List<Level1ChangeMessage>, List<ExecutionMessage>>>();

			using (var qr = QshReader.Open(fileName))
			{
				var reader = qr;
				var currentDate = qr.CurrentDateTime;

				for (var i = 0; i < qr.StreamCount; i++)
				{
					var stream = (ISecurityStream)qr[i];
					var security = GetSecurity(stream.Security, board);
					var priceStep = security.PriceStep ?? 1;
					var securityId = security.ToSecurityId();
					var lastTransactionId = 0L;
					var builder = orderLog2OrderBookBuilders?.SafeAdd(securityId, key => new PlazaOrderLogMarketDepthBuilder(key));

					if (securitiesStrings.Length > 0)
					{
						var secCode = securityId.SecurityCode;

						var streamContainsSecurityFromMask = securitiesStrings.Any(mask =>
						{
							var isEndMulti = mask.EndsWith("*");
							var isStartMulti = mask.StartsWith("*");

							if (isStartMulti)
								mask = mask.Substring(1);

							if (isEndMulti)
								mask = mask.Substring(0, mask.Length - 1);

							if (isEndMulti)
							{
								if (isStartMulti)
									return secCode.ContainsIgnoreCase(mask);
								else
									return secCode.StartsWith(mask, StringComparison.InvariantCultureIgnoreCase);
							}
							else if (isStartMulti)
								return secCode.EndsWith(mask, StringComparison.InvariantCultureIgnoreCase);
							else
								return secCode.CompareIgnoreCase(mask);
						});

						if (!streamContainsSecurityFromMask)
							continue;
					}

					var secData = data.SafeAdd(security, key => Tuple.Create(new List<QuoteChangeMessage>(), new List<ExecutionMessage>(), new List<Level1ChangeMessage>(), new List<ExecutionMessage>()));

					switch (stream.Type)
					{
						case StreamType.Quotes:
						{
							((IQuotesStream)stream).Handler += quotes =>
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
									LocalTime = reader.CurrentDateTime.ApplyTimeZone(TimeHelper.Moscow),
									SecurityId = securityId,
									ServerTime = currentDate.ApplyTimeZone(TimeHelper.Moscow),
									Bids = quotes2.Where(q => q.Side == Sides.Buy),
									Asks = quotes2.Where(q => q.Side == Sides.Sell),
								};

								//if (md.Verify())
								//{
								secData.Item1.Add(md);

								TryFlushData(registry, security, format, null, secData.Item1, reader);

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
									LocalTime = reader.CurrentDateTime.ApplyTimeZone(TimeHelper.Moscow),
									HasTradeInfo = true,
									ExecutionType = ExecutionTypes.Tick,
									SecurityId = securityId,
									OpenInterest = deal.OI == 0 ? (long?)null : deal.OI,
									ServerTime = deal.DateTime.ApplyTimeZone(TimeHelper.Moscow),
									TradeVolume = deal.Volume,
									TradeId = deal.Id == 0 ? (long?)null : deal.Id,
									TradePrice = deal.Price,
									OriginSide = 
										deal.Type == DealType.Buy
											? Sides.Buy
											: (deal.Type == DealType.Sell ? Sides.Sell : (Sides?)null)
								});

								TryFlushData(registry, security, format, ExecutionTypes.Tick, secData.Item2, reader);
							};
							break;
						}
						case StreamType.OrdLog:
						{
							((IOrdLogStream)stream).Handler += ol =>
							{
								var currTransactionId = ol.DateTime.Ticks;

								if (lastTransactionId < currTransactionId)
									lastTransactionId = currTransactionId;
								else if (lastTransactionId >= currTransactionId)
									lastTransactionId++;

								var msg = new ExecutionMessage
								{
									LocalTime = reader.CurrentDateTime.ApplyTimeZone(TimeHelper.Moscow),
									ExecutionType = ExecutionTypes.OrderLog,
									SecurityId = securityId,
									OpenInterest = ol.OI == 0 ? (long?)null : ol.OI,
									OrderId = ol.OrderId,
									OrderPrice = priceStep * ol.Price,
									ServerTime = ol.DateTime.ApplyTimeZone(TimeHelper.Moscow),
									OrderVolume = ol.Amount,
									Balance = ol.AmountRest,
									TradeId = ol.DealId == 0 ? (long?)null : ol.DealId,
									TradePrice = ol.DealPrice == 0 ? (decimal?)null : priceStep * ol.DealPrice,
									TransactionId = lastTransactionId
								};

								var status = 0;

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
									status |= 0x200000;
								}
								else if (ol.Flags.Contains(OrdLogFlags.CanceledGroup))
								{
									msg.OrderState = OrderStates.Done;
									status |= 0x400000;
								}
								else if (ol.Flags.Contains(OrdLogFlags.Moved))
								{
									status |= 0x100000;
								}

								if (ol.Flags.Contains(OrdLogFlags.Buy))
								{
									msg.Side = Sides.Buy;
								}
								else if (ol.Flags.Contains(OrdLogFlags.Sell))
								{
									msg.Side = Sides.Sell;
								}

								if (ol.Flags.Contains(OrdLogFlags.FillOrKill))
								{
									msg.TimeInForce = TimeInForce.MatchOrCancel;
									status |= 0x00080000;
								}

								if (ol.Flags.Contains(OrdLogFlags.Quote))
								{
									msg.TimeInForce = TimeInForce.PutInQueue;
									status |= 0x01;
								}

								if (ol.Flags.Contains(OrdLogFlags.Counter))
								{
									status |= 0x02;
								}

								if (ol.Flags.Contains(OrdLogFlags.CrossTrade))
								{
									status |= 0x20000000;
								}

								if (ol.Flags.Contains(OrdLogFlags.NonSystem))
								{
									msg.IsSystem = false;
									status |= 0x04;
								}

								if (ol.Flags.Contains(OrdLogFlags.EndOfTransaction))
								{
									status |= 0x1000;
								}

								msg.OrderStatus = status;

								if (builder == null)
								{
									secData.Item4.Add(msg);

									TryFlushData(registry, security, format, ExecutionTypes.OrderLog, secData.Item4, reader);
								}
								else
								{
									//if (builder.Depth.Bids.Any() || builder.Depth.Asks.Any() || msg.ServerTime.TimeOfDay >= new TimeSpan(0, 18, 45, 00, 1))
									{
										bool updated;

										try
										{
											updated = builder.Update(msg);
										}
										catch
										{
											updated = false;
										}

										if (updated)
										{
											secData.Item1.Add(new QuoteChangeMessage
											{
												SecurityId = securityId,
												ServerTime = builder.Depth.ServerTime,
												Bids = builder.Depth.Bids.Take(orderBookMaxDepth).ToArray(),
												Asks = builder.Depth.Asks.Take(orderBookMaxDepth).ToArray(),
												IsSorted = builder.Depth.IsSorted,
												LocalTime = builder.Depth.LocalTime,
											});

											TryFlushData(registry, security, format, null, secData.Item1, reader);
										}
									}
								}
							};
							break;
						}
						case StreamType.AuxInfo:
						{
							((IAuxInfoStream)stream).Handler += info =>
							{
								var time = info.DateTime;

								if (time.IsDefault())
									time = qr.CurrentDateTime;

								var l1Msg = new Level1ChangeMessage
					            {
						            LocalTime = reader.CurrentDateTime.ApplyTimeZone(TimeHelper.Moscow),
						            SecurityId = securityId,
						            ServerTime = time.ApplyTimeZone(TimeHelper.Moscow),
					            }
					            .TryAdd(Level1Fields.LastTradePrice, priceStep * info.Price)
					            .TryAdd(Level1Fields.BidsVolume, (decimal)info.BidTotal)
					            .TryAdd(Level1Fields.AsksVolume, (decimal)info.AskTotal)
					            .TryAdd(Level1Fields.HighPrice, priceStep * info.HiLimit)
					            .TryAdd(Level1Fields.LowPrice, priceStep * info.LoLimit)
					            .TryAdd(Level1Fields.OpenInterest, (decimal)info.OI);

								if (l1Msg.Changes.Count == 0)
									return;

								secData.Item3.Add(l1Msg);

								TryFlushData(registry, security, format, null, secData.Item3, reader);
							};
							break;
						}
						case StreamType.OwnOrders:
						case StreamType.OwnTrades:
						case StreamType.Messages:
						case StreamType.None:
						{
							continue;
						}
						default:
							throw new ArgumentOutOfRangeException("Неподдерживаемый тип потока {0}.".Put(stream.Type));
					}
				}

				if (data.Count > 0)
				{
					while (qr.CurrentDateTime != DateTime.MaxValue && _isStarted)
						qr.Read(true);
				}
			}

			if (!_isStarted)
				return;

			foreach (var pair in data)
			{
				if (pair.Value.Item1.Any())
				{
					registry.GetQuoteMessageStorage(pair.Key, format: format).Save(pair.Value.Item1);
				}

				if (pair.Value.Item2.Any())
				{
					registry.GetTickMessageStorage(pair.Key, format: format).Save(pair.Value.Item2);
				}

				if (pair.Value.Item3.Any())
				{
					registry.GetLevel1MessageStorage(pair.Key, format: format).Save(pair.Value.Item3);
				}

				if (pair.Value.Item4.Any())
				{
					registry.GetOrderLogMessageStorage(pair.Key, format: format).Save(pair.Value.Item4);
				}
			}

			if (data.Count > 0)
			{
				//File.AppendAllLines(_convertedFilesFile, new[] { fileNameKey });
				_convertedFiles.Add(fileNameKey);
				_convertedPerTaskPoolFiles.Add(fileNameKey);
			}

			_logManager.Application.AddInfoLog("Завершена конвертация файла {0}.", fileName);
		}

		private BusinessEntities.Security GetSecurity(QScalp.Security security, ExchangeBoard board)
		{
			return new BusinessEntities.Security
			{
				Id = _idGenerator.GenerateId(security.Ticker, board),
				Code = security.Ticker,
				Board = board,
				PriceStep = (decimal)security.Step,
				//Decimals = security.Precision
			};
		}

		private void TryEnable()
		{
			Convert.IsEnabled = !QshFolder.Folder.IsEmpty() && !StockSharpFolder.Folder.IsEmpty() && Board.SelectedBoard != null;
		}

		private void Board_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			TryEnable();
		}

		private void OnFolderChanged(string folder)
		{
			TryEnable();
		}
	}
}