using SDL3;
using System;

namespace SDLPal;

public static class Common
{
    public static readonly SDL.Color
        ColorNone   = COS.Color(0x2BF666FF),
        ColorBlack  = COS.Color(0x000001FF),
        ColorWhite  = COS.Color(0xFFFFFFFF),
        ColorGold   = COS.Color(0xFFD700FF),
        ColorYellow = COS.Color(0xEF7D31FF),
        ColorRed    = COS.Color(0xFF0000FF),
        ColorGreen  = COS.Color(0x00FF00FF),
        ColorBlue   = COS.Color(0x0000FFFF),
        ColorCyan   = COS.Color(0x00FFFFFF),
        ColorPurple = COS.Color(0xAE00FFFF),
        ColorPink   = COS.Color(0xFF6969FF);

    public static readonly int
        W32 = S.Ratio(32),
        W16 = S.Ratio(16),
        H16 = S.Ratio(16),
        H15 = S.Ratio(15),
        H8  = S.Ratio(8);

    public static readonly int
        NBX = (int)Math.Ceiling((double)PalViewport.Rect.W / W32),
        NBY = (int)Math.Ceiling((double)PalViewport.Rect.H / H16);

    public static readonly Random PalRandom = new();
    public static readonly SDL.FColor DefaultColorMask = new(1, 1, 1, 1);

    public enum PalFilter
    {
        Morning  = 0,
        Noon,
        Dusk,
        Night,
    }

    public enum PalKey
    {
        None        = 0,
        Menu        = (1 << 0),
        Search      = (1 << 1),
        Down        = (1 << 2),
        Left        = (1 << 3),
        Up          = (1 << 4),
        Right       = (1 << 5),
        PgUp        = (1 << 6),
        PgDn        = (1 << 7),
        Repeat      = (1 << 8),
        Auto        = (1 << 9),
        Defend      = (1 << 10),
        UseItem     = (1 << 11),
        EquipItem   = (1 << 12),
        ThrowItem   = (1 << 13),
        Flee        = (1 << 14),
        Status      = (1 << 15),
        Force       = (1 << 16),
        Home        = (1 << 17),
        End         = (1 << 18),
    };

    public enum PalDirection : short
    {
        Current = -1,       // 未知
        South   = 0,        // 西南（左下）
        West,               // 西北（左上）
        North,              // 东北（右上）
        East,               // 东南（右下）
    }

    public enum PalHorizontalAlign
    {
        Left,           // 左对齐
        Middle,         // 居中显示（对齐垂直中轴线）
        Right,          // 右对齐
        Stretch,        // 水平拉伸
    }

    public enum PalVerticalAlign
    {
        Top,            // 顶端对齐
        Middle,         // 居中显示（对齐水平中轴线）
        Bottom,         // 底端对齐
        Stretch,        // 垂直拉伸
    }

    public enum PalDialogPosition
    {
        Top,        // 顶部
        Bottom,     // 下部
        Middle,     // 中间
        Custom,     // 自定义位置
    }

    public enum PalEquipmentPart
    {
        None,           // 不可装备于任何部位
        Head,           // 头戴
        Cloak,          // 披挂
        Body,           // 身穿
        Hand,           // 手持
        Foot,           // 脚穿
        Ornament,       // 佩戴
        Temp,           // 战斗临时（不属于装备，专用于战斗时加的临时属性，如大蒜加的毒抗）
    }

    public enum PalStatus
    {
        AttackFriends       = 0,        // 封魔（乱）
        CannotAction        = 1,        // 定身（定）
        Sleep               = 2,        // 昏眠（眠）
        CannotUseMagic      = 3,        // 咒封（封）
        DeceasedCanAttack   = 4,        // 傀儡（暂时使死者继续普攻）
        MorePhysicalAttacks = 5,        // 天罡（暂时提升物理攻击）
        MoreDefense         = 6,        // 金刚（暂时提升防御）
        ActionsFaster       = 7,        // 仙风（暂时提升身法）
        DualAttack          = 8,        // 醉仙（两次普攻）
    }
}
