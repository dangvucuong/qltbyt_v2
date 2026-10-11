# -*- coding: utf-8 -*-
import json
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

BASE = "http://localhost:5149"
OUT = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\artifacts\system-logs-smoke.txt")


def req(method, path, token=None, body=None):
    data = None if body is None else json.dumps(body).encode("utf-8")
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    r = urllib.request.Request(BASE + path, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(r) as resp:
            return resp.status, json.loads(resp.read().decode("utf-8"))
    except urllib.error.HTTPError as e:
        return e.code, json.loads(e.read().decode("utf-8"))


def main():
    lines = []
    _, login = req("POST", "/api/auth/login", body={"username": "admin", "password": "Sample@123"})
    token = login["data"]["token"]
    _, devices = req("GET", "/api/devices?page=1&pageSize=1", token)
    did = devices["data"]["items"][0]["id"]
    req(
        "POST",
        f"/api/devices/{did}/usage-logs",
        token,
        {"hours": 1, "sessions": 1, "inputDate": "2026-10-11"},
    )

    code, logs = req("GET", "/api/system-logs?page=1&pageSize=5", token)
    data = logs.get("data") or {}
    items = data.get("items") or []
    lines.append(f"GET {code} total={data.get('totalCount')} items={len(items)}")
    if items:
        first = items[0]
        lines.append(
            f"first content={first.get('content')} user={first.get('username')} device={first.get('deviceCode')}"
        )

    q = urllib.parse.urlencode({"page": 1, "pageSize": 5, "search": "Nhật ký"})
    code2, s = req("GET", f"/api/system-logs?{q}", token)
    lines.append(f"SEARCH {code2} total={(s.get('data') or {}).get('totalCount')}")
    OUT.write_text("\n".join(lines), encoding="utf-8")


if __name__ == "__main__":
    main()
