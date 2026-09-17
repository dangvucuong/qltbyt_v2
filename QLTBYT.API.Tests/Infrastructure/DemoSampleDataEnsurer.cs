using QLTBYT.API.Data;
using QLTBYT.API.Infrastructure;

namespace QLTBYT.API.Tests.Infrastructure;

public static class DemoSampleDataEnsurer
{
    public static Task EnsureVietnameseSampleDataAsync(AppDbContext context) =>
        SampleDataVietnameseRefresher.EnsureAllAsync(context);

    public static Task EnsureDemoDeviceAttachmentsAsync(AppDbContext context) =>
        EnsureVietnameseSampleDataAsync(context);

    public static Task EnsureDemoDeviceHierarchyAsync(AppDbContext context) =>
        EnsureVietnameseSampleDataAsync(context);

    public static Task EnsureDeviceStatusLabelsAsync(AppDbContext context) =>
        DeviceStatusLabelEnsurer.EnsureAsync(context);

    public static Task EnsureMasterDataAsync(AppDbContext context) =>
        EnsureVietnameseSampleDataAsync(context);

    public static Task EnsureSampleUserPasswordsAsync(AppDbContext context) =>
        EnsureVietnameseSampleDataAsync(context);
}
