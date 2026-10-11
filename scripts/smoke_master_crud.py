# -*- coding: utf-8 -*-
import json
import urllib.error
import urllib.request
from pathlib import Path

BASE = "http://localhost:5149"
OUT = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\artifacts\master-crud-smoke.txt")


def req(method: str, path: str, token: str | None = None, body: dict | None = None):
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
    lines: list[str] = []
    code, login = req("POST", "/api/auth/login", body={"username": "admin", "password": "Sample@123"})
    assert code == 200 and login.get("success"), login
    token = login["data"]["token"]
    lines.append("LOGIN OK")

    catalogs = [
        ("org-types", "Loại ĐV smoke"),
        ("countries", "NuocSmoke"),
        ("task-types", "NV smoke"),
        ("event-keywords", "KW smoke"),
        ("personnel-types", "NS smoke"),
        ("document-types", "HS smoke"),
    ]

    get_map = {
        "org-types": "/api/master/org-types",
        "countries": "/api/master/countries",
        "task-types": "/api/master/task-types",
        "event-keywords": "/api/master/event-keywords",
        "personnel-types": "/api/master/personnel-types",
        "document-types": "/api/master/document-types",
    }

    for key, name in catalogs:
        admin = f"/api/admin/master/{key}"
        # CREATE
        code, created = req("POST", admin, token, {"name": name})
        ok = code == 200 and created.get("success")
        lines.append(f"CREATE {key}: {code} ok={ok} msg={created.get('message')} id={created.get('data',{}).get('id')}")
        if not ok:
            continue
        item_id = created["data"]["id"]
        # UPDATE
        new_name = name + " (sua)"
        code, updated = req("PUT", f"{admin}/{item_id}", token, {"name": new_name})
        ok = code == 200 and updated.get("success") and updated.get("data", {}).get("name") == new_name
        lines.append(f"UPDATE {key}: {code} ok={ok}")
        # GET list contains
        code, listed = req("GET", get_map[key], token)
        names = [i.get("name") for i in (listed.get("data") or [])]
        lines.append(f"GET {key}: contains={new_name in names} count={len(names)}")
        # DELETE
        code, deleted = req("DELETE", f"{admin}/{item_id}", token)
        ok = code == 200 and deleted.get("success")
        lines.append(f"DELETE {key}: {code} ok={ok}")

    # Non-admin should be forbidden
    code, user_login = req("POST", "/api/auth/login", body={"username": "user", "password": "Sample@123"})
    if code == 200 and user_login.get("success"):
        utoken = user_login["data"]["token"]
        code, denied = req("POST", "/api/admin/master/org-types", utoken, {"name": "forbidden"})
        lines.append(f"USER FORBIDDEN: {code} (expect 403)")
    else:
        lines.append("USER LOGIN skipped")

    OUT.write_text("\n".join(lines), encoding="utf-8")
    print(OUT.read_text(encoding="utf-8"))


if __name__ == "__main__":
    main()
