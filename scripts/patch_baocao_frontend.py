from pathlib import Path
p = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\qltbyt-frontend\src\pages\reports\BaocaoStatisticsTab.tsx")
text = p.read_text(encoding="utf-8")
if "ColumnsType" not in text:
    text = text.replace(
        "import type { BaocaoAmountOption, BaocaoAssetRow, BaocaoOperationalRow } from '@/types/phase3';",
        "import type { ColumnsType } from 'antd/es/table';\nimport type { BaocaoAmountOption, BaocaoAssetRow, BaocaoOperationalRow } from '@/types/phase3';",
    )
text = text.replace(
    "  columns: Parameters<typeof Table>[0]['columns'];",
    "  columns: ColumnsType<BaocaoOperationalRow>;",
)
p.write_text(text, encoding="utf-8", newline="\n")
print("patched")
