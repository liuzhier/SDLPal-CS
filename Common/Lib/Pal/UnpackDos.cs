#region License
/*
 * Copyright (c) 2025, liuzhier <lichunxiao_lcx@qq.com>.
 * 
 * This file is part of SDLPAL-CS.
 * 
 * SDLPAL-CS is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License, version 3
 * as published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * Portions based on PalLibrary by Lou Yihua <louyihua@21cn.com>.
 * Copyright (c) 2006-2007, Lou Yihua.
 * 
 * Ported to C from C++ and modified for compatibility with Big-Endian
 * by Wei Mingzhi <whistler_wmz@users.sf.net>.
 * 
 * Ported to C# from C by Li Chunxiao <lichunxiao_lcx@qq.com>.
 * 
 */
#endregion License

using SimpleUtility;
using System.Runtime.InteropServices;

namespace Lib.Pal;

public static unsafe partial class PalUtil
{
    struct YJ1_TreeNode
    {
        public  byte        value;
        public  bool        leaf;
        public  YJ1_TreeNode        *parent = null;
        public  YJ1_TreeNode        *left   = null;
        public  YJ1_TreeNode        *right  = null;

        public YJ1_TreeNode() { }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct YJ_1_FILEHEADER
    {
        public  uint        Signature;              // 'YJ_1'
        public  int         UncompressedLength;     // size before compression
        public  int         CompressedLength;       // size after compression
        public  ushort      BlockCount;             // number of blocks
        public  byte        Unknown;
        public  byte        HuffmanTreeLength;      // length of huffman tree
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct YJ_1_BLOCKHEADER
    {
        public  ushort      UncompressedLength;     // maximum 0x4000
        public  ushort      CompressedLength;       // including the header
        public  fixed   ushort      LZSSRepeatTable[4];
        public  fixed   byte        LZSSOffsetCodeLengthTable[4];
        public  fixed   byte        LZSSRepeatCodeLengthTable[3];
        public  fixed   byte        CodeCountCodeLengthTable[3];
        public  fixed   byte        CodeCountTable[2];
    }

    static int Yj1_get_bits(void *src, int *bitptr, int count)
    {
        byte        *temp = ((byte*)src) + ((* bitptr >> 4) << 1);
        int         mask;
        byte        bptr = (byte)(*bitptr & 0xf);
        *bitptr += count;
        if (count > 16 - bptr)
        {
            count = count + bptr - 16;
            mask = 0xffff >> bptr;
            return (ushort)((((temp[0] | (temp[1] << 8)) & mask) << count) | ((temp[2] | (temp[3] << 8)) >> (16 - count)));
        }
        else
            return (ushort)(((ushort)((temp[0] | (temp[1] << 8)) << bptr)) >> (16 - count));
    }

    static ushort Yj1_get_loop(void* src, int* bitptr, YJ_1_BLOCKHEADER* header)
    {
        if (Yj1_get_bits(src, bitptr, 1) != 0)
            return header->CodeCountTable[0];
        else
        {
            int temp = Yj1_get_bits(src, bitptr, 2);
            if (temp != 0)
                return (ushort)Yj1_get_bits(src, bitptr, header->CodeCountCodeLengthTable[temp - 1]);
            else
                return header->CodeCountTable[1];
        }
    }

    static ushort Yj1_get_count(void* src, int* bitptr, YJ_1_BLOCKHEADER* header)
    {
        ushort temp;
        if ((temp = (ushort)Yj1_get_bits(src, bitptr, 2)) != 0)
        {
            if (Yj1_get_bits(src, bitptr, 1) != 0)
                return (ushort)Yj1_get_bits(src, bitptr, header->LZSSRepeatCodeLengthTable[temp - 1]);
            else
                return header->LZSSRepeatTable[temp];
        }
        else
            return header->LZSSRepeatTable[0];
    }

    /// <summary>
    /// 对二进制流进行解码
    /// </summary>
    /// <param name="source">源二进制流</param>
    /// <returns>解码后的二进制流和流长度</returns>
    static (nint, int) UnpackDos(nint source)
    {
        YJ_1_FILEHEADER*        hdr = (YJ_1_FILEHEADER*)source;
        nint                    destination;
        byte*                   src = (byte*)source;
        byte*                   dest;
        uint                    i;
        YJ1_TreeNode*           root, node;

        S.Failed(
            "Util.UnpackDos",
            "源数据缓冲区为空",
            source != 0
        );

        S.Failed(
            "Util.UnpackDos",
            "源数据缓冲区头部标识错误，期望为 \"YJ_1\"",
            hdr->Signature == 0x315f4a59
        );

        do
        {
            ushort tree_len = (ushort)(hdr->HuffmanTreeLength * 2);
            int bitptr = 0;
            byte *flag = src + 16 + tree_len;

            node = root = (YJ1_TreeNode*)C.malloc(sizeof(YJ1_TreeNode) * (tree_len + 1));

            root[0].leaf = false;
            root[0].value = 0;
            root[0].left = root + 1;
            root[0].right = root + 2;
            for (i = 1; i <= tree_len; i++)
            {
                root[i].leaf = Yj1_get_bits(flag, &bitptr, 1) == 0;
                root[i].value = src[15 + i];
                if (root[i].leaf)
                    root[i].left = root[i].right = null;
                else
                {
                    root[i].left = root + (root[i].value << 1) + 1;
                    root[i].right = root[i].left + 1;
                }
            }
            src += 16 + tree_len + ((((tree_len & 0xf) != 0) ? (tree_len >> 4) + 1 : (tree_len >> 4)) << 1);
        } while (false);

        dest = (byte*)(destination = C.malloc(hdr->UncompressedLength));

        for (i = 0; i < hdr->BlockCount; i++)
        {
            int bitptr;
            YJ_1_BLOCKHEADER* header;

            header = (YJ_1_BLOCKHEADER*)src;
            src += 4;
            if (header->CompressedLength == 0)
            {
                ushort hul = header->UncompressedLength;
                while (hul-- > 0)
                {
                    *dest++ = *src++;
                }
                continue;
            }
            src += 20;
            bitptr = 0;
            for (; ; )
            {
                ushort loop;
                if ((loop = Yj1_get_loop(src, &bitptr, header)) == 0)
                    break;

                while (loop-- > 0)
                {
                    node = root;
                    for (; !node->leaf;)
                    {
                        if (Yj1_get_bits(src, &bitptr, 1) != 0)
                            node = node->right;
                        else
                            node = node->left;
                    }
                    *dest++ = node->value;
                }

                if ((loop = Yj1_get_loop(src, &bitptr, header)) == 0)
                    break;

                while (loop-- > 0)
                {
                    uint pos, count;
                    count = Yj1_get_count(src, &bitptr, header);
                    pos = (uint)Yj1_get_bits(src, &bitptr, 2);
                    pos = (uint)Yj1_get_bits(src, &bitptr, header->LZSSOffsetCodeLengthTable[pos]);
                    while (count-- > 0)
                    {
                        *dest = *(dest - pos);
                        dest++;
                    }
                }
            }
            src = ((byte*)header) + header->CompressedLength;
        }

        C.free(root);

        return (destination, hdr->UncompressedLength);
    }
}
