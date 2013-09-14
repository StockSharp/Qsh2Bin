#region Copyright (c) 2011-2013 Николай Морошкин, http://www.moroshkin.com/
/*

  Настоящий исходный код является частью приложения «Торговый привод QScalp»
  (http://www.qscalp.ru)  и  предоставлен  исключительно  в  ознакомительных
  целях.  Какое-либо коммерческое использование данного кода без письменного
  разрешения автора запрещено.

*/
#endregion

using System;
using System.IO;

using QScalp.History.Internals;

namespace QScalp.History.Reader
{
  abstract class QshReader : IDisposable
  {
    // **********************************************************************

    public static QshReader Open(string path)
    {
      FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
      BinaryReader br = DataFile.GetReader(fs);

      int version = br.ReadByte();

      switch(version)
      {
        case 3: return new V3.QshReader3(fs, br);
        default: throw new FormatException("Неподдерживаемая версия файла (" + version + ")");
      }
    }

    // **********************************************************************

    readonly FileStream fs;
    protected BinaryReader br;

    // **********************************************************************

    public long FileSize { get { return fs.Length; } }
    public long FilePosition { get { return fs.Position; } }

    public string AppName { get; protected set; }
    public string Comment { get; protected set; }
    public DateTime RecDateTime { get; protected set; }

    public abstract int StreamsCount { get; }
    public abstract IQshStream this[int i] { get; }

    public DateTime CurrentDateTime { get; protected set; }
    public int CurrentStreamIndex { get; protected set; }

    // **********************************************************************

    protected QshReader(FileStream fs, BinaryReader br)
    {
      this.fs = fs;
      this.br = br;
    }

    // **********************************************************************

    public void Dispose()
    {
      if(br != null)
        br.Dispose();

      if(fs != null)
        fs.Dispose();
    }

    // **********************************************************************

    public abstract void Read(bool push);

    // **********************************************************************
  }
}
