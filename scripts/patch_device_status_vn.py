# -*- coding: utf-8 -*-
"""Patch device status labels to Vietnamese with diacritics."""
from pathlib import Path

SEEDER = Path(__file__).resolve().parents[1] / "QLTBYT.API" / "Infrastructure" / "DevelopmentDataSeeder.cs"
ENSURER = Path(__file__).resolve().parents[1] / "QLTBYT.API.Tests" / "Infrastructure" / "DemoSampleDataEnsurer.cs"

OLD_BLOCK = """        if (!await context.DeviceStatuses.AnyAsync(cancellationToken))
        {
            context.DeviceStatuses.AddRange(
                new DeviceStatus { Id = 1, Name = "Dang hoat dong" },
                new DeviceStatus { Id = 2, Name = "Khong hoat dong / Hong" },
                new DeviceStatus { Id = 3, Name = "Chua su dung" });
        }"""

NEW_BLOCK = """        if (!await context.DeviceStatuses.AnyAsync(cancellationToken))
        {
            context.DeviceStatuses.AddRange(
                new DeviceStatus { Id = 1, Name = "Đang hoạt động" },
                new DeviceStatus { Id = 2, Name = "Không hoạt động / Hỏng" },
                new DeviceStatus { Id = 3, Name = "Chưa sử dụng" });
        }

        await EnsureDeviceStatusLabelsAsync(context, cancellationToken);"""

INSERT_BEFORE_HIERARCHY = """        await EnsureDemoDeviceHierarchyAsync(context, cancellationToken);"""

ENSURE_METHOD = '''
    private static async Task EnsureDeviceStatusLabelsAsync(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var canonicalById = new Dictionary<int, string>
        {
            { 1, "Đang hoạt động" },
            { 2, "Không hoạt động / Hỏng" },
            { 3, "Chưa sử dụng" },
        };

        var legacyToCanonical = new Dictionary<string, string>
        {
            { "Dang hoat dong", "Đang hoạt động" },
            { "Khong hoat dong / Hong", "Không hoạt động / Hỏng" },
            { "Chua su dung", "Chưa sử dụng" },
        };

        var statuses = await context.DeviceStatuses.ToListAsync(cancellationToken);
        foreach (var status in statuses)
        {
            if (canonicalById.TryGetValue(status.Id, out var byId))
            {
                status.Name = byId;
                continue;
            }

            if (status.Name != null && legacyToCanonical.TryGetValue(status.Name, out var byLegacy))
            {
                status.Name = byLegacy;
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

'''

ENSURER_METHOD = '''
    public static async Task EnsureDeviceStatusLabelsAsync(AppDbContext context)
    {
        var canonicalById = new Dictionary<int, string>
        {
            { 1, "Đang hoạt động" },
            { 2, "Không hoạt động / Hỏng" },
            { 3, "Chưa sử dụng" },
        };

        var legacyToCanonical = new Dictionary<string, string>
        {
            { "Dang hoat dong", "Đang hoạt động" },
            { "Khong hoat dong / Hong", "Không hoạt động / Hỏng" },
            { "Chua su dung", "Chưa sử dụng" },
        };

        var statuses = await context.DeviceStatuses.ToListAsync();
        foreach (var status in statuses)
        {
            if (canonicalById.TryGetValue(status.Id, out var byId))
            {
                status.Name = byId;
                continue;
            }

            if (status.Name != null && legacyToCanonical.TryGetValue(status.Name, out var byLegacy))
            {
                status.Name = byLegacy;
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

'''


def read_text(path: Path) -> str:
    b = path.read_bytes()
    if len(b) > 2 and b[1] == 0:
        return b.decode("utf-16-le")
    return b.decode("utf-8")


def write_text(path: Path, text: str) -> None:
    path.write_text(text, encoding="utf-8", newline="\n")


def main() -> None:
    seeder_text = read_text(SEEDER)
    if OLD_BLOCK not in seeder_text:
        if "Đang hoạt động" in seeder_text and "EnsureDeviceStatusLabelsAsync" in seeder_text:
            print("Seeder already patched")
        else:
            raise SystemExit("Could not find expected seeder block")
    else:
        seeder_text = seeder_text.replace(OLD_BLOCK, NEW_BLOCK, 1)

    if "EnsureDeviceStatusLabelsAsync" not in seeder_text.split("EnsureDemoDeviceHierarchyAsync")[0]:
        seeder_text = seeder_text.replace(
            "    private static async Task EnsureDemoDeviceHierarchyAsync",
            ENSURE_METHOD + "    private static async Task EnsureDemoDeviceHierarchyAsync",
            1,
        )

    write_text(SEEDER, seeder_text)
    print("Patched DevelopmentDataSeeder.cs")

    ensurer_text = read_text(ENSURER)
    if "EnsureDeviceStatusLabelsAsync" not in ensurer_text:
        ensurer_text = ensurer_text.replace(
            "    public static async Task EnsureSampleUserPasswordsAsync",
            ENSURER_METHOD + "    public static async Task EnsureSampleUserPasswordsAsync",
            1,
        )
        write_text(ENSURER, ensurer_text)
        print("Patched DemoSampleDataEnsurer.cs")
    else:
        print("Ensurer already patched")


if __name__ == "__main__":
    main()
