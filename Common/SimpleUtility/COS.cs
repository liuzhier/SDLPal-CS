using System;
using System.IO;

namespace SimpleUtility;

/// <summary>
/// COS 类名称是 Create objects safely 的缩写，
/// 其旨在为程序“安全地创建对象”。
/// </summary>
public static class COS
{
    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <param name="fDelExists">当该值为 true 时，若文件存在，则自动删除后再创建它</param>
    public static void Dir(string path, bool fDelExists = true)
    {
        try
        {
            if (fDelExists)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                Directory.CreateDirectory(path);
            }
            else
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }
        catch (Exception e)
        {
            S.Failed(
               "COS.Dir",
               e.Message
            );
        }
    }
}
