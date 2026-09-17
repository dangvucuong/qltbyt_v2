from pathlib import Path
p = Path("d:/DVC/PROJECT/QLTBYT_NEW/QLTBYT.API/Models/DTOs/Organization/OrganizationListDto.cs")
text = p.read_text(encoding="utf-8")
if "Address" not in text:
    text = text.replace(
        "    public string? Email { get; set; }\n}",
        "    public string? Email { get; set; }\n    public string? Address { get; set; }\n    public string? TaxCode { get; set; }\n}",
    )
    p.write_text(text, encoding="utf-8", newline="\n")
    print("dto ok")
crud = Path("d:/DVC/PROJECT/QLTBYT_NEW/QLTBYT.API/Services/OrganizationService.Crud.cs")
t = crud.read_text(encoding="utf-8")
if "Address = o.Address" not in t:
    t = t.replace(
        "        Email = o.Email\n    };",
        "        Email = o.Email,\n        Address = o.Address,\n        TaxCode = o.TaxCode\n    };",
    )
    crud.write_text(t, encoding="utf-8", newline="\n")
    print("map ok")
ref = Path("d:/DVC/PROJECT/QLTBYT_NEW/QLTBYT.API/Infrastructure/SampleDataVietnameseRefresher.cs")
rt = ref.read_text(encoding="utf-8")
if "EnsureDemoBiddingAsync" not in rt:
    rt = rt.replace(
        "        await EnsureDemoEventsAsync(context, cancellationToken);",
        "        await EnsureDemoEventsAsync(context, cancellationToken);\n        await EnsureDemoBiddingAsync(context, cancellationToken);",
    )
    insert = '''
    private static async Task EnsureDemoBiddingAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        const string code = "GT-DEMO-2024";
        var pkg = await context.BiddingPackages.FirstOrDefaultAsync(b => b.PackageCode == code, cancellationToken);
        if (pkg is null)
        {
            pkg = new BiddingPackage
            {
                PackageCode = code,
                Name = "G\u00f3i th\u1ea7u mua m\u00e1y th\u1edf oxy demo",
                Status = "\u0110ang th\u1ef1c hi\u1ec7n",
                WinningVendor = "C\u00f4ng ty Thi\u1ebft b\u1ecb Y t\u1ebf Demo",
                TaxCode = "0123456789",
                ContractNumber = "HD-DEMO-2024-01",
                Value = 1500000000
            };
            context.BiddingPackages.Add(pkg);
            await context.SaveChangesAsync(cancellationToken);
        }

        var devices = await context.Devices
            .Where(d => d.DeviceCode != null && d.DeviceCode.StartsWith("TB-DEMO-"))
            .ToListAsync(cancellationToken);
        foreach (var d in devices)
        {
            d.BiddingPackageCode = code;
        }
        if (devices.Count > 0)
            await context.SaveChangesAsync(cancellationToken);
    }

'''
    rt = rt.replace("\n    private static async Task EnsureDemoEventsAsync", insert + "\n    private static async Task EnsureDemoEventsAsync")
    ref.write_text(rt, encoding="utf-8", newline="\n")
    print("bidding seed ok")
