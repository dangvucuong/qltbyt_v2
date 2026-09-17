from pathlib import Path
ctrl = Path("d:/DVC/PROJECT/QLTBYT_NEW/QLTBYT.API/Controllers")
repl = {
    "BiddingController.cs": [
        ("ActionResult<ApiResponse<IReadOnlyList<BiddingPackageDto>>> GetAll", "ActionResult<ApiResponse<IReadOnlyList<BiddingPackageDto>>>> GetAll"),
        ("ApiResponse<IReadOnlyList<BiddingPackageDto>>>.CreateSuccess", "ApiResponse<IReadOnlyList<BiddingPackageDto>>.CreateSuccess"),
        ("ActionResult<ApiResponse<IReadOnlyList<DeviceListDto>>> GetDevices", "ActionResult<ApiResponse<IReadOnlyList<DeviceListDto>>>> GetDevices"),
        ("ApiResponse<IReadOnlyList<DeviceListDto>>>.CreateSuccess", "ApiResponse<IReadOnlyList<DeviceListDto>>.CreateSuccess"),
    ],
    "DevicesController.cs": [
        ("ApiResponse<IReadOnlyList<DeviceHistoryItemDto>>>.CreateSuccess", "ApiResponse<IReadOnlyList<DeviceHistoryItemDto>>.CreateSuccess"),
        ("ApiResponse<IReadOnlyList<DeviceSupplyDto>>>.CreateSuccess", "ApiResponse<IReadOnlyList<DeviceSupplyDto>>.CreateSuccess"),
    ],
}
for fn, pairs in repl.items():
    p = ctrl / fn
    t = p.read_text(encoding="utf-8")
    for old, new in pairs:
        t = t.replace(old, new)
    p.write_text(t, encoding="utf-8", newline="\n")
    print("ok", fn)
