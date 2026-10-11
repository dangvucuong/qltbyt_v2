# -*- coding: utf-8 -*-
import json
import urllib.error
import urllib.request
from datetime import date
from pathlib import Path

BASE = "http://localhost:5149"
OUT = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\artifacts\usage-log-smoke.txt")


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
        raw = e.read().decode("utf-8")
        try:
            return e.code, json.loads(raw)
        except Exception:
            return e.code, {"raw": raw}


def main():
    lines = []
    _, login = req("POST", "/api/auth/login", body={"username": "admin", "password": "Sample@123"})
    token = login["data"]["token"]
    _, devices = req("GET", "/api/devices?page=1&pageSize=1", token)
    device_id = devices["data"]["items"][0]["id"]
    lines.append(f"deviceId={device_id}")

    today = date.today().isoformat()
    # Create
    code, created = req(
        "POST",
        f"/api/devices/{device_id}/usage-logs",
        token,
        {"hours": 3, "sessions": 2, "inputDate": today},
    )
    lines.append(f"CREATE {code} id={created.get('data',{}).get('id')} hours={created.get('data',{}).get('hours')}")
    log_id = created.get("data", {}).get("id")

    # Upsert same day
    code, updated = req(
        "POST",
        f"/api/devices/{device_id}/usage-logs",
        token,
        {"hours": 5, "sessions": 4, "inputDate": today},
    )
    same_id = updated.get("data", {}).get("id") == log_id
    hours_ok = updated.get("data", {}).get("hours") == 5
    lines.append(f"UPSERT {code} sameId={same_id} hours5={hours_ok}")

    # List
    code, listed = req("GET", f"/api/devices/{device_id}/usage-logs", token)
    count = len(listed.get("data") or [])
    lines.append(f"GET {code} count={count}")

    OUT.write_text("\n".join(lines), encoding="utf-8")


if __name__ == "__main__":
    main()
