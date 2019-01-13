#region Copyright (c) 2011-2016 Николай Морошкин, http://www.moroshkin.com/
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
      FileStream fs = null;

      try
      {
        fs = new FileStream(path, FileMode.Open, FileAccess.Read);

        Stream ds = QshFile.GetDataStream(fs);
        int version = ds.ReadByte();

        switch(version)
        {
          case 4: return new V4.QshReaderImpl(fs, ds);

          default:
            throw new FormatException("Неподдерживаемая версия файла (" + version + ")");
        }
      }
      catch
      {
        if(fs != null)
          fs.Dispose();

        throw;
      }
    }

    // **********************************************************************

    readonly FileStream fs;

    // **********************************************************************

    public long FileSize { get { return fs.Length; } }
    public long FilePosition { get { return fs.Position; } }

    public string AppName { get; protected set; }
    public string Comment { get; protected set; }
    public DateTime RecDateTime { get; protected set; }

    public abstract int StreamCount { get; }
    public abstract IQshStream this[int i] { get; }

    public DateTime CurrentDateTime { get; protected set; }
    public int CurrentStreamIndex { get; protected set; }

    // **********************************************************************

    protected QshReader(FileStream fs)
    {
      this.fs = fs;
    }

    // **********************************************************************

    public virtual void Dispose()
    {
      if(fs != null)
        fs.Dispose();
    }

    // **********************************************************************

    public abstract void Read(bool push);

    // **********************************************************************
  }
}
