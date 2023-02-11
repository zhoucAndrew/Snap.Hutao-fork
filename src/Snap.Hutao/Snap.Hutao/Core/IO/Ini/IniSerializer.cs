﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace Snap.Hutao.Core.IO.Ini;

/// <summary>
/// Ini 序列化器
/// </summary>
internal static class IniSerializer
{
    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="fileStream">文件流</param>
    /// <returns>Ini 元素集合</returns>
    public static IEnumerable<IniElement> Deserialize(FileStream fileStream)
    {
        using (TextReader reader = new StreamReader(fileStream))
        {
            while (reader.ReadLine() is string line)
            {
                if (line.Length > 0)
                {
                    if (line[0] == '[')
                    {
                        yield return new IniSection(line[1..^1]);
                    }

                    if (line[0] == ';')
                    {
                        yield return new IniComment(line[1..]);
                    }

                    if (line.IndexOf('=') > 0)
                    {
                        string[] parameters = line.Split('=', 2);
                        yield return new IniParameter(parameters[0], parameters[1]);
                    }
                }

                continue;
            }
        }
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="fileStream">写入的流</param>
    /// <param name="elements">元素</param>
    public static void Serialize(FileStream fileStream, IEnumerable<IniElement> elements)
    {
        using (TextWriter writer = new StreamWriter(fileStream))
        {
            foreach (IniElement element in elements)
            {
                writer.WriteLine(element.ToString());
            }
        }
    }
}