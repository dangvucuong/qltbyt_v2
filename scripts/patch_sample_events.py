from pathlib import Path
p = Path(r"d:/DVC/PROJECT/QLTBYT_NEW/QLTBYT.API/Infrastructure/SampleDataVietnameseRefresher.cs")
t = p.read_text(encoding="utf-8")
if "EnsureDemoEventsAsync" in t:
    print("already")
    raise SystemExit(0)
t = t.replace(
    "        await RecreateDemoDevicesAsync(context, cancellationToken);\n        if (context.ChangeTracker.HasChanges())",
    "        await RecreateDemoDevicesAsync(context, cancellationToken);\n        await EnsureDemoEventsAsync(context, cancellationToken);\n        if (context.ChangeTracker.HasChanges())",
    1,
)
method = r'''
    private static async Task EnsureDemoEventsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var demoIds = await context.Devices
            .Where(d => d.DeviceCode != null && d.DeviceCode.StartsWith("TB-DEMO-"))
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);
        if (demoIds.Count == 0)
            return;

        var old = await context.DeviceEvents
            .Where(e => e.DeviceId != null && demoIds.Contains(e.DeviceId.Value))
            .ToListAsync(cancellationToken);
        context.DeviceEvents.RemoveRange(old);

        var device002 = await context.Devices
            .FirstOrDefaultAsync(d => d.DeviceCode == "TB-DEMO-002", cancellationToken);
        if (device002 is null)
        {
            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        context.DeviceEvents.Add(new DeviceEvent
        {
            DeviceId = device002.Id,
            TypeId = 1,
            Content = "May do huyet ap khong hoat dong - yeu cau kiem tra demo",
            EventDateTime = DateTime.Now.AddHours(-2),
            StatusId = 1,
            AccountUsername = "user",
            Keyword = "Bao hong"
        });
        await context.SaveChangesAsync(cancellationToken);
    }

'''
t = t.replace(
    "    private static async Task EnsureDemoDeviceHierarchyAsync",
    method + "    private static async Task EnsureDemoDeviceHierarchyAsync",
    1,
)
p.write_text(t, encoding="utf-8", newline="\n")
print("ok")
