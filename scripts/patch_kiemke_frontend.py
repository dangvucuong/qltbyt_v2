from pathlib import Path

ROOT = Path(r"d:\DVC\PROJECT\QLTBYT_NEW")
FE = ROOT / "qltbyt-frontend"

p = FE / "src/types/phase3.ts"
text = p.read_text(encoding="utf-8")
if "KiemkeAssetRow" not in text:
    insert = """
export interface KiemkeAssetRow {
  id: number;
  name?: string;
  identityCode?: string;
  manufacturer?: string;
  modelCode?: string;
  serial?: string;
  quantity?: number;
  unit?: string;
  organizationName?: string;
  locationCode?: string;
  procurementSourceLabel?: string;
  totalAmount?: number;
  receivedDate?: string;
  depreciationAmount?: number;
  note?: string;
}

"""
    text = text.replace("export interface BaocaoAmountOption", insert + "export interface BaocaoAmountOption")
    p.write_text(text, encoding="utf-8", newline="\n")

app = FE / "src/App.tsx"
at = app.read_text(encoding="utf-8")
if "KiemkeAssetPage" not in at:
    at = at.replace(
        "import { ReportsPage } from '@/pages/reports/ReportsPage';",
        "import { ReportsPage } from '@/pages/reports/ReportsPage';\nimport { KiemkeAssetPage } from '@/pages/reports/KiemkeAssetPage';",
    )
    at = at.replace(
        '<Route path="/scan-qr" element={<QrScanPage />} />',
        '<Route path="/scan-qr" element={<QrScanPage />} />\n            <Route path="/asset-inventory" element={<KiemkeAssetPage />} />',
    )
    app.write_text(at, encoding="utf-8", newline="\n")

ml = FE / "src/layouts/MainLayout.tsx"
mt = ml.read_text(encoding="utf-8")
if "/asset-inventory" not in mt:
    if "AuditOutlined" not in mt:
        mt = mt.replace("  QrcodeOutlined,", "  QrcodeOutlined,\n  AuditOutlined,")
    mt = mt.replace(
        ": location.pathname.startsWith('/reports')",
        ": location.pathname.startsWith('/asset-inventory')\n          ? '/asset-inventory'\n          : location.pathname.startsWith('/reports')",
    )
    mt = mt.replace(
        "            {\n              key: '/usage-logs',",
        "            {\n              key: '/asset-inventory',\n              icon: <AuditOutlined />,\n              label: <Link to=\"/asset-inventory\">Kiểm kê tài sản</Link>,\n            },\n            {\n              key: '/usage-logs',",
    )
    ml.write_text(mt, encoding="utf-8", newline="\n")

print("patched")
