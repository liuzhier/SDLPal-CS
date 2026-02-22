namespace SDLPal;

public static class PalSprites
{
    /// <summary>
    /// 获取道具贴图
    /// </summary>
    /// <param name="spriteId">贴图编号</param>
    /// <param name="surface">是否加载了新版贴图</param>
    /// <returns>是否加载了新版贴图</returns>
    public static bool GetItem(int spriteId, out nint surface)
    {
        var path = S.ModPath.Assets.Sprite.Item;
        var pathNew = $@"{path}New";

        //
        // 检查是否有可将其代替的新版贴图
        //
        pathNew = $@"{pathNew}\{spriteId:D5}.png";
        var isLoadNew = S.FileExist(pathNew, false);
        path = isLoadNew ? pathNew : $@"{path}\{spriteId:D5}.png";

        //
        // 将贴图加载为表面
        //
        surface = COS.Surface(path);
        return isLoadNew;
    }

    /// <summary>
    /// 获取道具贴图
    /// </summary>
    /// <param name="spriteId">贴图编号</param>
    /// <param name="surface">是否加载了新版贴图</param>
    /// <returns>是否加载了新版贴图</returns>
    public static bool GetAvatar(int spriteId, out nint surface)
    {
        var path = S.ModPath.Assets.Sprite.Avatar;
        var pathNew = $@"{path}New";

        //
        // 检查是否有可将其代替的新版贴图
        //
        pathNew = $@"{pathNew}\{spriteId:D5}.png";
        var isLoadNew = S.FileExist(pathNew, false);
        path = isLoadNew ? pathNew : $@"{path}\{spriteId:D5}.png";

        //
        // 将贴图加载为表面
        //
        surface = COS.Surface(path);
        return isLoadNew;
    }
}
