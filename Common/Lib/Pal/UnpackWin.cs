using SimpleUtility;

namespace Lib.Pal;

/// <summary>
/// 这部分代码是 SDLPAL yj1.c 的一部分
/// https://github.com/sdlpal/sdlpal
/// </summary>
public static unsafe partial class PalUtil
{
    struct TreeNodeWin
    {
        public  ushort              weight;
        public  ushort              value;
        public  TreeNodeWin*        parent;
        public  TreeNodeWin*        left;
        public  TreeNodeWin*        right;
    }

    struct TreeWin
    {
        public  TreeNodeWin*        node;
        public  TreeNodeWin**       list;
    }

    static ReadOnlySpan<byte> data1Win =>
    [
       0x3f, 0x0b, 0x17, 0x03, 0x2f, 0x0a, 0x16, 0x00, 0x2e, 0x09, 0x15, 0x02, 0x2d, 0x01, 0x08, 0x00,
      0x3e, 0x07, 0x14, 0x03, 0x2c, 0x06, 0x13, 0x00, 0x2b, 0x05, 0x12, 0x02, 0x2a, 0x01, 0x04, 0x00,
      0x3d, 0x0b, 0x11, 0x03, 0x29, 0x0a, 0x10, 0x00, 0x28, 0x09, 0x0f, 0x02, 0x27, 0x01, 0x08, 0x00,
      0x3c, 0x07, 0x0e, 0x03, 0x26, 0x06, 0x0d, 0x00, 0x25, 0x05, 0x0c, 0x02, 0x24, 0x01, 0x04, 0x00,
      0x3b, 0x0b, 0x17, 0x03, 0x23, 0x0a, 0x16, 0x00, 0x22, 0x09, 0x15, 0x02, 0x21, 0x01, 0x08, 0x00,
      0x3a, 0x07, 0x14, 0x03, 0x20, 0x06, 0x13, 0x00, 0x1f, 0x05, 0x12, 0x02, 0x1e, 0x01, 0x04, 0x00,
      0x39, 0x0b, 0x11, 0x03, 0x1d, 0x0a, 0x10, 0x00, 0x1c, 0x09, 0x0f, 0x02, 0x1b, 0x01, 0x08, 0x00,
      0x38, 0x07, 0x0e, 0x03, 0x1a, 0x06, 0x0d, 0x00, 0x19, 0x05, 0x0c, 0x02, 0x18, 0x01, 0x04, 0x00,
      0x37, 0x0b, 0x17, 0x03, 0x2f, 0x0a, 0x16, 0x00, 0x2e, 0x09, 0x15, 0x02, 0x2d, 0x01, 0x08, 0x00,
      0x36, 0x07, 0x14, 0x03, 0x2c, 0x06, 0x13, 0x00, 0x2b, 0x05, 0x12, 0x02, 0x2a, 0x01, 0x04, 0x00,
      0x35, 0x0b, 0x11, 0x03, 0x29, 0x0a, 0x10, 0x00, 0x28, 0x09, 0x0f, 0x02, 0x27, 0x01, 0x08, 0x00,
      0x34, 0x07, 0x0e, 0x03, 0x26, 0x06, 0x0d, 0x00, 0x25, 0x05, 0x0c, 0x02, 0x24, 0x01, 0x04, 0x00,
      0x33, 0x0b, 0x17, 0x03, 0x23, 0x0a, 0x16, 0x00, 0x22, 0x09, 0x15, 0x02, 0x21, 0x01, 0x08, 0x00,
      0x32, 0x07, 0x14, 0x03, 0x20, 0x06, 0x13, 0x00, 0x1f, 0x05, 0x12, 0x02, 0x1e, 0x01, 0x04, 0x00,
      0x31, 0x0b, 0x11, 0x03, 0x1d, 0x0a, 0x10, 0x00, 0x1c, 0x09, 0x0f, 0x02, 0x1b, 0x01, 0x08, 0x00,
      0x30, 0x07, 0x0e, 0x03, 0x1a, 0x06, 0x0d, 0x00, 0x19, 0x05, 0x0c, 0x02, 0x18, 0x01, 0x04, 0x00
    ];

    static ReadOnlySpan<byte> data2Win =>
    [
       0x08, 0x05, 0x06, 0x04, 0x07, 0x05, 0x06, 0x03, 0x07, 0x05, 0x06, 0x04, 0x07, 0x04, 0x05, 0x03
    ];

    static void AdjustTreeWin(TreeWin tree, ushort value)
    {
        TreeNodeWin         tmp;
        TreeNodeWin*        node, tmp1, temp;

        node = tree.list[value];

        while (node->value != 0x280)
        {
            temp = node + 1;

            while (node->weight == temp->weight)
                temp++;

            temp--;

            if (temp != node)
            {
                tmp1 = node->parent;
                node->parent = temp->parent;
                temp->parent = tmp1;

                if (node->value > 0x140)
                {
                    node->left->parent = temp;
                    node->right->parent = temp;
                }
                else
                {
                    tree.list[node->value] = temp;
                }

                if (temp->value > 0x140)
                {
                    temp->left->parent = node;
                    temp->right->parent = node;
                }
                else
                {
                    tree.list[temp->value] = node;
                }

                tmp = *node;
                *node = *temp;
                *temp = tmp;
                node = temp;
            }

            node->weight++;
            node = node->parent;
        }

        node->weight++;
    }

    static void BuildTreeWin(TreeWin* tree)
    {
        int                 i, ptr;
        TreeNodeWin**       list;
        TreeNodeWin*        node;

        if ((tree->list = list = (TreeNodeWin**)C.malloc(sizeof(TreeNodeWin*) * 321)) == null)
        {
            goto Failed;
        }

        if ((tree->node = node = (TreeNodeWin*)C.malloc(sizeof(TreeNodeWin) * 641)) == null)
        {
            C.free(list);
            goto Failed;
        }

        for (i = 0; i <= 0x140; i++)
        {
            list[i] = node + i;
        }

        for (i = 0; i <= 0x280; i++)
        {
            node[i].value = (ushort)i;
            node[i].weight = 1;
        }

        tree->node[0x280].parent = tree->node + 0x280;

        for (i = 0, ptr = 0x141; ptr <= 0x280; i += 2, ptr++)
        {
            node[ptr].left = node + i;
            node[ptr].right = node + i + 1;
            node[i].parent = node[i + 1].parent = node + ptr;
            node[ptr].weight = (ushort)(node[i].weight + node[i + 1].weight);
        }

        return;

    Failed:
        S.Failed(
           "Util.BuildTreeWin",
           "The Huffman tree construction failed!"
        );
    }

    /// <summary>
    /// 从字节流中获取指定位置上的 bit 值
    /// </summary>
    /// <param name="data">字节流</param>
    /// <param name="pos">要获取字节流中的第几位 bit</param>
    /// <returns>bit 值</returns>
    static int GetBitWin(byte* data, uint pos) => data[pos / 8] >> (int)(pos % 8) & 1;

    static int GetDecompressSizeWin(nint source) => *(int*)source;

    /// <summary>
    /// 对二进制流进行解码
    /// </summary>
    /// <param name="source">源二进制流</param>
    /// <returns>解码后的二进制流和流长度</returns>
    static (nint, int) UnpackWin(nint source)
    {
        int                 Length;
        uint                len = 0, ptr = 0;
        nint                destination;
        byte*               src = (byte*)source + 4;
        byte*               dest;
        TreeWin             tree;
        TreeNodeWin*        node;

        S.Failed(
            "Util.UnpakWin",
            "The source data 'Source' is a null pointer",
            source != 0
        );

        Length = GetDecompressSizeWin(source);
        destination = C.malloc(Length);
        dest = (byte*)destination;

        BuildTreeWin(&tree);

        while (true)
        {
            ushort val;
            node = tree.node + 0x280;
            while (node->value > 0x140)
            {
                if (GetBitWin(src, ptr) != 0)
                    node = node->right;
                else
                    node = node->left;
                ptr++;
            }
            val = node->value;
            if (tree.node[0x280].weight == 0x8000)
            {
                int i;
                for (i = 0; i < 0x141; i++)
                    if ((tree.list[i]->weight & 0x1) != 0)
                        AdjustTreeWin(tree, (ushort)i);
                for (i = 0; i <= 0x280; i++)
                    tree.node[i].weight >>= 1;
            }
            AdjustTreeWin(tree, val);
            if (val > 0xff)
            {
                int i;
                uint temp, tmp, pos;
                byte* pre;
                for (i = 0, temp = 0; i < 8; i++, ptr++)
                    temp |= (uint)GetBitWin(src, ptr) << i;
                tmp = temp & 0xff;
                for (; i < data2Win[(int)(tmp & 0xf)] + 6; i++, ptr++)
                    temp |= (uint)GetBitWin(src, ptr) << i;
                temp >>= data2Win[(int)(tmp & 0xf)];
                pos = (temp & 0x3f) | ((uint)data1Win[(int)tmp] << 6);
                if (pos == 0xfff)
                    break;
                pre = dest - pos - 1;
                for (i = 0; i < val - 0xfd; i++)
                    *dest++ = *pre++;
                len += (uint)(val - 0xfd);
            }
            else
            {
                *dest++ = (byte)val;
                len++;
            }
        }

        C.free(tree.list);
        C.free(tree.node);

        return (destination, Length);
    }
}
