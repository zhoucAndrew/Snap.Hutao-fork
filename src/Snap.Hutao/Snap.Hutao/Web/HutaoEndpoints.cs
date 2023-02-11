﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Web;

/// <summary>
/// 胡桃 API 端点
/// </summary>
[SuppressMessage("", "SA1201")]
[SuppressMessage("", "SA1124")]
internal static class HutaoEndpoints
{
    /// <summary>
    /// 胡桃资源主机名
    /// </summary>
    public const string StaticHutao = "static.hut.ao";

    #region HutaoAPI

    /// <summary>
    /// 上传日志
    /// </summary>
    public const string HutaoLogUpload = $"{HomaSnapGenshinApi}/HutaoLog/Upload";

    /// <summary>
    /// 检查 uid 是否上传记录
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>路径</returns>
    public static string RecordCheck(string uid)
    {
        return $"{HomaSnapGenshinApi}/Record/Check?uid={uid}";
    }

    /// <summary>
    /// uid 排行
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>路径</returns>
    public static string RecordRank(string uid)
    {
        return $"{HomaSnapGenshinApi}/Record/Rank?uid={uid}";
    }

    /// <summary>
    /// 上传记录
    /// </summary>
    public const string RecordUpload = $"{HomaSnapGenshinApi}/Record/Upload";

    /// <summary>
    /// 统计信息
    /// </summary>
    public const string StatisticsOverview = $"{HomaSnapGenshinApi}/Statistics/Overview";

    /// <summary>
    /// 出场率
    /// </summary>
    public const string StatisticsAvatarAttendanceRate = $"{HomaSnapGenshinApi}/Statistics/Avatar/AttendanceRate";

    /// <summary>
    /// 使用率
    /// </summary>
    public const string StatisticsAvatarUtilizationRate = $"{HomaSnapGenshinApi}/Statistics/Avatar/UtilizationRate";

    /// <summary>
    /// 角色搭配
    /// </summary>
    public const string StatisticsAvatarAvatarCollocation = $"{HomaSnapGenshinApi}/Statistics/Avatar/AvatarCollocation";

    /// <summary>
    /// 角色持有率
    /// </summary>
    public const string StatisticsAvatarHoldingRate = $"{HomaSnapGenshinApi}/Statistics/Avatar/HoldingRate";

    /// <summary>
    /// 武器搭配
    /// </summary>
    public const string StatisticsWeaponWeaponCollocation = $"{HomaSnapGenshinApi}/Statistics/Weapon/WeaponCollocation";

    /// <summary>
    /// 持有率
    /// </summary>
    public const string StatisticsTeamCombination = $"{HomaSnapGenshinApi}/Statistics/Team/Combination";
    #endregion

    #region Metadata

    /// <summary>
    /// 胡桃元数据文件
    /// </summary>
    /// <param name="fileName">文件名称</param>
    /// <returns>路径</returns>
    public static string HutaoMetadataFile(string fileName)
    {
        return $"{HutaoMetadataSnapGenshinApi}/{fileName}";
    }
    #endregion

    #region Patcher

    /// <summary>
    /// 胡桃检查更新
    /// </summary>
    public const string PatcherHutaoStable = $"{PatcherDGPStudioApi}/hutao/stable";
    #endregion

    #region Static & Zip

    /// <summary>
    /// UI_Icon_None
    /// </summary>
    public static readonly Uri UIIconNone = new(StaticFile("Bg", "UI_Icon_None.png"));

    /// <summary>
    /// UI_ItemIcon_None
    /// </summary>
    public static readonly Uri UIItemIconNone = new(StaticFile("Bg", "UI_ItemIcon_None.png"));

    /// <summary>
    /// UI_AvatarIcon_Side_None
    /// </summary>
    public static readonly Uri UIAvatarIconSideNone = new(StaticFile("AvatarIcon", "UI_AvatarIcon_Side_None.png"));

    /// <summary>
    /// 压缩包资源
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="fileName">文件名称 包括后缀</param>
    /// <returns>路径</returns>
    public static string StaticFile(string category, string fileName)
    {
        return $"{StaticSnapGenshinApi}/{category}/{fileName}";
    }

    /// <summary>
    /// 压缩包资源
    /// </summary>
    /// <param name="fileName">文件名称 不包括后缀</param>
    /// <returns>路径</returns>
    public static string StaticZip(string fileName)
    {
        return $"{StaticZipSnapGenshinApi}/{fileName}.zip";
    }
    #endregion

    private const string HutaoMetadataSnapGenshinApi = "http://hutao-metadata.snapgenshin.com";
    private const string HomaSnapGenshinApi = "https://homa.snapgenshin.com";
    private const string PatcherDGPStudioApi = "https://patcher.dgp-studio.cn";
    private const string StaticSnapGenshinApi = "https://static.snapgenshin.com";
    private const string StaticZipSnapGenshinApi = "https://static-zip.snapgenshin.com";
}