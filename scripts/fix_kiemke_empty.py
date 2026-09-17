from pathlib import Path

# Backend helper + kiemke fix
kiemke = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\QLTBYT.API\Services\ReportService.Kiemke.cs")
text = kiemke.read_text(encoding="utf-8")
if "NormalizeOptionalFilter" not in text:
    text = text.replace(
        "        if (!string.IsNullOrWhiteSpace(query.ProcurementSource))\n            queryable = queryable.Where(d => d.ProcurementSource == query.ProcurementSource.Trim());\n",
        "        var procurement = NormalizeOptionalFilter(query.ProcurementSource);\n        if (!string.IsNullOrEmpty(procurement))\n            queryable = queryable.Where(d => d.ProcurementSource == procurement);\n",
    )
    text = text.replace(
        "        queryable = ApplyLocationFilter(queryable, query.LocationCode);\n",
        "        queryable = ApplyLocationFilter(queryable, NormalizeOptionalFilter(query.LocationCode));\n",
    )
    text = text.replace(
        "    private static KiemkeAssetRowDto MapKiemkeRow(Device device)\n",
        "    private static string? NormalizeOptionalFilter(string? value)\n    {\n        if (string.IsNullOrWhiteSpace(value)) return null;\n        var trimmed = value.Trim();\n        if (string.Equals(trimmed, \"null\", StringComparison.OrdinalIgnoreCase)) return null;\n        return trimmed;\n    }\n\n    private static KiemkeAssetRowDto MapKiemkeRow(Device device)\n",
    )
    kiemke.write_text(text, encoding="utf-8", newline="\n")

# Frontend fix via full buildQuery and handlers
page = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\qltbyt-frontend\src\pages\reports\KiemkeAssetPage.tsx")
pt = page.read_text(encoding="utf-8-sig")

old_bq = """function buildQuery(params: Record<string, string | number | boolean | undefined>) {
  const q = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v === undefined || v === '') return;
    q.set(k, String(v));
  });
  return q.toString();
}"""

new_bq = """function buildQuery(params: Record<string, string | number | boolean | undefined | null>) {
  const q = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v === undefined || v === null || v === '') return;
    if (typeof v === 'number' && Number.isNaN(v)) return;
    if (k !== 'excludeZeroAmount' && v === false) return;
    q.set(k, String(v));
  });
  return q.toString();
}

function normalizePaged<T>(payload: PagedResult<T> | undefined | null): PagedResult<T> {
  if (!payload) return { items: [], totalCount: 0, page: 1, pageSize: 10, totalPages: 0 };
  const raw = payload as PagedResult<T> & { Items?: T[]; TotalCount?: number };
  return {
    items: raw.items ?? raw.Items ?? [],
    totalCount: raw.totalCount ?? raw.TotalCount ?? 0,
    page: raw.page ?? 1,
    pageSize: raw.pageSize ?? 10,
    totalPages: raw.totalPages ?? 0,
  };
}"""

pt = pt.replace(old_bq, new_bq)

pt = pt.replace("onChange={setOrgId}", "onChange={(v) => setOrgId(v ?? undefined)}")
pt = pt.replace("onChange={setLocation}", "onChange={(v) => setLocation(v ?? undefined)}")
pt = pt.replace("onChange={setProcurement}", "onChange={(v) => setProcurement(v ?? undefined)}")
pt = pt.replace("onChange={setReceivedYear}", "onChange={(v) => setReceivedYear(v ?? null)}")
pt = pt.replace("onChange={(v) => setMinAmount(v ?? undefined)}", "onChange={(v) => setMinAmount(v == null || Number.isNaN(v) ? undefined : v)}")
pt = pt.replace("onChange={(v) => setMaxAmount(v ?? undefined)}", "onChange={(v) => setMaxAmount(v == null || Number.isNaN(v) ? undefined : v)}")

old_qf = """      return res.data.data!;
    },
  });

  const baseColumns = ["""

new_qf = """      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.message || 'Không tải được dữ liệu kiểm kê');
      }
      return normalizePaged(res.data.data);
    },
  });

  const baseColumns = ["""

pt = pt.replace(
    """      return res.data.data!;
    },
  });

  const inventoryParams""",
    """      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.message || 'Không tải được dữ liệu kiểm kê');
      }
      return normalizePaged(res.data.data);
    },
  });

  const inventoryParams""",
)

# dept query too
pt = pt.replace(
    """      const res = await axiosClient.get<ApiResponse<PagedResult<KiemkeAssetRow>>>(
        `/api/asset-inventory?${qs}`,
      );
      return res.data.data!;
    },
  });

  const inventoryParams""",
    """      const res = await axiosClient.get<ApiResponse<PagedResult<KiemkeAssetRow>>>(
        `/api/asset-inventory?${qs}`,
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.message || 'Không tải được dữ liệu kiểm kê');
      }
      return normalizePaged(res.data.data);
    },
  });

  const inventoryParams""",
)

# add useEffect for query errors - use onError in useQuery instead
if "message.error(inventoryQuery.error" not in pt:
    pt = pt.replace(
        "import { useMemo, useState } from 'react';",
        "import { useEffect, useMemo, useState } from 'react';",
    )
    pt = pt.replace(
        "  const inventoryQuery = useQuery({",
        """  const inventoryQuery = useQuery({""",
    )
    insert_after = """  });

  const baseColumns = ["""
    # find inventory query closing and add useEffect
    marker = "  const inventoryQuery = useQuery({"
    idx = pt.find(marker)
    if idx >= 0 and "inventoryQuery.error" not in pt:
        pt = pt.replace(
            """  });

  const baseColumns = [""",
            """  });

  useEffect(() => {
    if (inventoryQuery.error) {
      message.error(inventoryQuery.error instanceof Error ? inventoryQuery.error.message : 'Tải dữ liệu thất bại');
    }
  }, [inventoryQuery.error]);

  useEffect(() => {
    if (deptQuery.error) {
      message.error(deptQuery.error instanceof Error ? deptQuery.error.message : 'Tải dữ liệu thất bại');
    }
  }, [deptQuery.error]);

  const baseColumns = [""",
            1,
        )

page.write_text(pt, encoding="utf-8", newline="\n")
print("patched")
