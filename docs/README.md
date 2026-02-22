# SDLPal-CS
# 使用 C# .NET 10 实现的经典中国游戏 PAL 的同人版
## 功能概述：
1. Mod 解包工具使用 C# .NET 10 + Avalonia 开发

2. 大多数逻辑直接从 [sdlpal](https://github.com/sdlpal/sdlpal) 移植而来

4. 实现了所有数据全部可直接编辑，且数据结构明了（全部导出为 json 数据）

5. 脚本部分使用了阉割语法后的 typescript 语言，面向有些许编程基础的群体

6. 将所有图像导出为了 png-32。直接砍掉了画面调色板，目前仅剩地图在使用调色板，其他资源均直接使用 png-32 图像。

6. 将 PAL.MKF 导出为 Photoshop 可用的 ACT 颜色表（仅供地图使用）

7. 地图编辑器是将 [palresearch/MapEditor](https://github.com/palxex/palresearch/tree/master/MapEditor) Hack 过的版本

---
## 目前支持平台：
Windows，未来将支持 Mac、Linux、Android、IOS 等主流平台

