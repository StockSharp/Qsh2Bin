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
using System.IO.Compression;
using System.Text;

namespace QScalp.History.Internals
{
  static class QshFile
  {
    // **********************************************************************

    static readonly byte[] prefix = Encoding.ASCII.GetBytes("QScalp History Data");

    // **********************************************************************

    public static Stream Create(string path, bool compress, byte version)
    {
      Stream stm = new FileStream(path, FileMode.Create, FileAccess.Write);

      if(compress)
        stm = new GZipStream(stm, CompressionMode.Compress);

      stm.Write(prefix, 0, prefix.Length);
      stm.WriteByte(version);

      return stm;
    }

    // **********************************************************************

    static bool CheckPrefix(Stream stream, byte[] buffer)
    {
      if(stream.Read(buffer, 0, buffer.Length) != prefix.Length)
        return false;

      for(int i = 0; i < buffer.Length; i++)
        if(buffer[i] != prefix[i])
          return false;

      return true;
    }

    // **********************************************************************

    public static Stream GetDataStream(FileStream fs)
    {
      byte[] buffer = new byte[prefix.Length];

      if(CheckPrefix(fs, buffer))
        return fs;

      Stream stream = null;

      try
      {
        fs.Position = 0;
        stream = new GZipStream(fs, CompressionMode.Decompress, true);

        if(CheckPrefix(stream, buffer))
          return stream;
      }
      catch { }

      if(stream != null)
      {
        stream.Dispose();
        stream = null;
      }

      try
      {
        fs.Position = 0;
        stream = new DeflateStream(fs, CompressionMode.Decompress, true);

        if(CheckPrefix(stream, buffer))
          return stream;
      }
      catch { }

      if(stream != null)
      {
        stream.Dispose();
        stream = null;
      }

      throw new FormatException("Неверный формат файла");
    }

    // **********************************************************************
  }
}
