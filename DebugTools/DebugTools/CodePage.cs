using Lib.Pal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Tmds.DBus.Protocol;
using Vanara.Extensions;

namespace DebugTools;

public static class CodePage
{
    public  const   int     CharHeight          = 16;
    public  const   int     UnicodeLowerTop     = 0xd800;
    public  const   int     UnicodeUpperBase    = 0xf900;
    public  const   int     UnicodeUpperTop     = 65534;

    static BinaryReader _fontData { get; set; } = new(Assembly.GetExecutingAssembly().GetManifestResourceStream(@"FontData")!);
    static BinaryReader _widthData { get; set; } = new(Assembly.GetExecutingAssembly().GetManifestResourceStream(@"FontWidth")!);

    public static void Free()
    {
        _fontData?.Dispose();
        _widthData?.Dispose();
    }

    public static int GetCharWidth(ushort charId)
    {
        //if ((charId >= UnicodeLowerTop && charId < UnicodeUpperBase) || charId >= UnicodeUpperBase)
        if (charId >= UnicodeLowerTop && charId < UnicodeUpperBase)
            //
            // 空字符
            //
            return 0;

        //
        // 在字体库中查找此字符
        //
        if (charId >= UnicodeUpperBase)
            charId -= (UnicodeUpperBase - UnicodeLowerTop);

        //
        // 获取宽度
        //
        _widthData.BaseStream.Seek(charId, SeekOrigin.Begin);
        return _widthData.ReadByte() >> 1;
    }

    public static ushort[] GetCharMask(ushort charId)
    {
        ushort[]        data;

        _fontData.BaseStream.Seek(charId * sizeof(ushort) * CharHeight, SeekOrigin.Begin);

        data = new ushort[CharHeight];
        for (int i = 0; i < CharHeight; i++)
            data[i] = _fontData.ReadUInt16();

        return data;
    }
}
