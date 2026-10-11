from pathlib import Path
import re
p = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\qltbyt-frontend\src\pages\devices\DeviceFormPage.tsx")
t = p.read_text(encoding="utf-8")
t2, n = re.subn(
    r'<Form\.Item name="countryOfOrigin" label="Nước sản xuất">.*?</Form\.Item>',
    '<Form.Item name="countryOfOrigin" label="Nước sản xuất">\n                <Input placeholder="VD: Nhật Bản, Đức..." />\n              </Form.Item>',
    t,
    count=1,
    flags=re.S,
)
print("country replacements", n)
t2 = t2.replace(
    "parser={(v) => Number((v ?? '').replace(/,/g, '')) as unknown as 0}",
    "parser={(v) => Number((v ?? '').replace(/,/g, '') || '0')}",
)
# remove unused countryOptions if still referenced - keep countries query unused? remove countries query and countryOptions to avoid lint
if "const countryOptions" in t2:
    t2 = t2.replace(
        """  const countries = useQuery({
    queryKey: ['master', 'countries'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<MasterItem[]>>('/api/master/countries');
      return res.data.data ?? [];
    },
  });

""",
        "",
    )
    t2 = t2.replace(
        """  const countryOptions = (countries.data ?? []).map((c) => ({
    value: c.name ?? String(c.id),
    label: c.name ?? String(c.id),
  }));
""",
        "",
    )
p.write_text(t2, encoding="utf-8", newline="\n")
print("ok")
