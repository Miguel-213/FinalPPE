import argparse
import base64
import hashlib
import hmac
import json
import os
from pathlib import Path
import secrets
import sqlite3
import subprocess
import tempfile
import time
import urllib.error
import urllib.request


parser = argparse.ArgumentParser(description="Pruebas HTTP con una base de datos temporal.")
parser.add_argument("--dotnet", default="dotnet")
parser.add_argument("--port", type=int, default=5014)
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]
base_url = f"http://localhost:{args.port}"
key = secrets.token_hex(32)
checks = 0


def request(method, path, data=None, token=None, headers=None):
    headers = dict(headers or {})
    if token:
        headers["Authorization"] = f"Bearer {token}"
    body = None
    if data is not None:
        headers["Content-Type"] = "application/json"
        body = json.dumps(data).encode()
    req = urllib.request.Request(base_url + path, body, headers, method=method)
    try:
        response = urllib.request.urlopen(req, timeout=5)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        raw = response.read()
        content_type = response.headers.get("Content-Type", "")
        result = json.loads(raw) if raw and "json" in content_type else raw.decode()
        return response.status, result, response.headers


def expect(method, path, status, data=None, token=None, headers=None):
    global checks
    actual, result, response_headers = request(method, path, data, token, headers)
    assert actual == status, f"{method} {path}: esperado {status}, recibido {actual}: {result}"
    checks += 1
    print(f"OK {method} {path}: {status}")
    return result, response_headers


def encode(data):
    return base64.urlsafe_b64encode(json.dumps(data).encode()).rstrip(b"=")


def signed_token(**changes):
    payload = {"sub": "1", "email": "prueba@example.com", "iss": "FinalPPE",
               "aud": "FinalPPE", "exp": int(time.time()) + 3600}
    payload.update(changes)
    message = encode({"alg": "HS256", "typ": "JWT"}) + b"." + encode(payload)
    signature = hmac.new(key.encode(), message, hashlib.sha256).digest()
    return (message + b"." + base64.urlsafe_b64encode(signature).rstrip(b"=")).decode()


with tempfile.TemporaryDirectory(prefix="finalppe-tests-") as temporary:
    database = Path(temporary) / "pruebas.db"
    env = dict(os.environ, Jwt__Key=key, ASPNETCORE_ENVIRONMENT="Development",
               ConnectionStrings__DefaultConnection=f"Data Source={database}")
    env.pop("ASPNETCORE_HTTPS_PORT", None)
    subprocess.run([args.dotnet, "build", "--nologo"], cwd=root, env=env, check=True)
    subprocess.run([args.dotnet, "tool", "restore"], cwd=root, env=env, check=True)
    subprocess.run([args.dotnet, "tool", "run", "dotnet-ef", "database", "update"],
                   cwd=root, env=env, check=True)

    with open(Path(temporary) / "api.log", "w+", encoding="utf-8") as log:
        server = subprocess.Popen(
            [args.dotnet, str(root / "bin/Debug/net10.0/FinalPPE.dll"), "--urls", base_url],
            cwd=root, env=env, stdout=log, stderr=subprocess.STDOUT)
        try:
            for attempt in range(100):
                if server.poll() is not None:
                    log.seek(0)
                    raise RuntimeError(log.read())
                try:
                    request("GET", "/trofeos")
                    break
                except urllib.error.URLError:
                    time.sleep(0.2)
            else:
                raise RuntimeError("La API no inició a tiempo.")

            document, _ = expect("GET", "/openapi/v1.json", 200)
            scheme = document["components"]["securitySchemes"]["Bearer"]
            assert scheme["type"] == "http" and scheme["scheme"] == "bearer"
            for path, operations in document["paths"].items():
                for method, operation in operations.items():
                    protected = path.startswith("/trofeos") and method in ("post", "patch", "delete")
                    assert bool(operation.get("security")) == protected
                    expected_status = "201" if method == "post" else "200"
                    assert expected_status in operation["responses"]
                    if protected:
                        assert operation["security"] == [{"Bearer": []}]
                        assert "401" in operation["responses"]
            scalar, _ = expect("GET", "/scalar/v1", 200)
            assert "scalar" in scalar.lower()
            empty, _ = expect("GET", "/trofeos", 200)
            assert empty == []
            expect("GET", "/trofeos/999999", 404)
            preflight = {"Origin": "http://localhost:5173",
                         "Access-Control-Request-Method": "POST",
                         "Access-Control-Request-Headers": "authorization,content-type"}
            _, cors = expect("OPTIONS", "/trofeos", 204, headers=preflight)
            assert cors["Access-Control-Allow-Origin"] == "http://localhost:5173"

            credentials = {"email": "prueba@example.com", "password": "Prueba123456"}
            user, _ = expect("POST", "/auth/register", 201, credentials)
            assert set(user) == {"message", "id", "email"}
            expect("POST", "/auth/register", 409, credentials)
            expect("POST", "/auth/login", 401, dict(credentials, password="incorrecta"))
            expect("POST", "/auth/login", 401, dict(credentials, email="no@example.com"))
            login, _ = expect("POST", "/auth/login", 201, credentials)
            assert set(login) == {"access_token"}
            token = login["access_token"]
            claims = json.loads(base64.urlsafe_b64decode(token.split(".")[1] + "=="))
            assert claims["sub"] == str(user["id"]) and claims["email"] == user["email"]
            assert 7190 <= claims["exp"] - time.time() <= 7200
            connection = sqlite3.connect(database)
            try:
                stored = connection.execute("SELECT Password FROM Usuarios").fetchone()[0]
                assert stored and stored != credentials["password"]
            finally:
                connection.close()

            trophy = {"nombre": "Minecraft 1", "juego": "Minecraft", "dificultad": "Facil",
                      "requisito": "Completar una tarea", "estado": False, "imagen": ""}
            bad_tokens = [None, "no-es-jwt", signed_token(iss="Otro"),
                          signed_token(aud="Otro"), signed_token(exp=int(time.time()) - 1),
                          signed_token(exp=None), token[:-10] + "AAAAAAAAAA"]
            for invalid in bad_tokens:
                _, challenge = expect("POST", "/trofeos", 401, trophy, invalid)
                assert challenge.get("WWW-Authenticate", "").startswith("Bearer")
                expect("PATCH", "/trofeos/1", 401, {"estado": True}, invalid)
                expect("DELETE", "/trofeos/1", 401, token=invalid)

            created = []
            for number in range(1, 6):
                item, location = expect("POST", "/trofeos", 201,
                                        dict(trophy, nombre=f"Minecraft {number}"), token)
                assert set(item) == {"id", "nombre", "juego", "dificultad", "requisito", "estado", "imagen"}
                assert location["Location"].endswith(f"/trofeos/{item['id']}")
                created.append(item)
            all_items, _ = expect("GET", "/trofeos", 200)
            assert len(all_items) == 5
            search, _ = expect("GET", "/trofeos?q=MINECRAFT", 200)
            assert len(search) == 5
            page, _ = expect("GET", "/trofeos?q=minecraft&page=2&pageSize=4", 200)
            assert [item["id"] for item in page] == [created[4]["id"]]
            page, _ = expect("GET", "/trofeos?page=2", 200)
            assert len(page) == 1
            page, _ = expect("GET", "/trofeos?page=1&pageSize=0", 200)
            assert len(page) == 4
            no_results, _ = expect("GET", "/trofeos?q=sin-coincidencias", 200)
            assert no_results == []

            path = f"/trofeos/{created[0]['id']}"
            individual, _ = expect("GET", path, 200)
            assert individual == created[0]
            updated, _ = expect("PATCH", path, 200, {"estado": True}, token)
            assert updated["estado"] is True and updated["nombre"] == trophy["nombre"]
            updated, _ = expect("PATCH", path, 200, {"estado": False}, token)
            assert updated["estado"] is False
            expect("PATCH", "/trofeos/999999", 404, {"estado": True}, token)
            expect("DELETE", "/trofeos/999999", 404, token=token)
            deleted, _ = expect("DELETE", path, 200, token=token)
            assert deleted["id"] == created[0]["id"]
            expect("GET", path, 404)
            expect("POST", "/trofeos", 401, trophy)
            print(f"\n{checks} comprobaciones HTTP correctas. Base temporal; no se modifican datos locales.")
        finally:
            server.terminate()
            server.wait(timeout=15)
