import json
import sys
from urllib.request import Request, urlopen
from urllib.error import HTTPError, URLError

if len(sys.argv) != 2:
    print("Usage: python test_api.py <port>")
    sys.exit(1)

port = sys.argv[1]

TRADE_URL = f"http://localhost:{port}/api/trade"
USER_URL = f"http://localhost:{port}/api/user"
AUTH_URL = f"http://localhost:{port}/api/auth"


# -----------------------------
# HTTP helper
# -----------------------------
def print_response(response):
    print(f"Status: {response.status}")
    body = response.read().decode()

    if body:
        try:
            print(json.dumps(json.loads(body), indent=2))
        except json.JSONDecodeError:
            print(body)

    print("-" * 60)


def request(base_url, method, path="", payload=None, token=None):
    url = base_url + path

    data = None
    headers = {}

    if payload is not None:
        data = json.dumps(payload).encode()
        headers["Content-Type"] = "application/json"

    if token:
        headers["Authorization"] = f"Bearer {token}"

    req = Request(url, data=data, headers=headers, method=method)

    with urlopen(req) as response:
        print(f"{method} {url}")
        print_response(response)


# -----------------------------
# AUTH
# -----------------------------
def register_user():
    print("\nREGISTER USER")

    payload = {
        "username": "johndoe",
        "email": "john.doe@example.com",
        "password": "Password123!"
    }

    req = Request(
        f"{AUTH_URL}/register",
        data=json.dumps(payload).encode(),
        headers={"Content-Type": "application/json"},
        method="POST"
    )

    with urlopen(req) as response:
        body = json.loads(response.read().decode())
        print_response(response)
        return body["token"]


def test_auth():
    print("\nLOGIN")

    request(AUTH_URL, "POST", "/login", {
        "email": "john.doe@example.com",
        "password": "Password123!"
    })

    print("\nLOGIN WITH INVALID PASSWORD")
    try:
        request(AUTH_URL, "POST", "/login", {
            "email": "john.doe@example.com",
            "password": "WrongPassword"
        })
    except HTTPError as e:
        print(f"Status: {e.code}")
        print(e.read().decode())
        print("-" * 60)

    print("\nLOGIN WITH UNKNOWN EMAIL")
    try:
        request(AUTH_URL, "POST", "/login", {
            "email": "doesnotexist@example.com",
            "password": "Password123!"
        })
    except HTTPError as e:
        print(f"Status: {e.code}")
        print(e.read().decode())
        print("-" * 60)


# -----------------------------
# USERS (PROTECTED)
# -----------------------------
def test_users(token):
    print("\nGET USERS")
    request(USER_URL, "GET", token=token)

    print("\nGET USER BY ID")
    request(USER_URL, "GET", "/1", token=token)


# -----------------------------
# TRADES (PROTECTED)
# -----------------------------
def test_trades(token):
    print("\nCREATE TRADE")

    request(TRADE_URL, "POST", token=token, payload={
        "userId": 1,
        "instrument": "MNQ SEP26",
        "direction": 0,
        "entryPrice": 23456.25,
        "exitPrice": 23468.75,
        "quantity": 2,
        "entryTime": "2026-07-05T10:00:00Z",
        "exitTime": "2026-07-05T10:15:00Z",
        "commission": 4.2
    })

    print("\nGET TRADES")
    request(TRADE_URL, "GET", token=token)

    print("\nGET TRADE BY ID")
    request(TRADE_URL, "GET", "/1", token=token)

    print("\nUPDATE TRADE")
    request(TRADE_URL, "PUT", "/1", token=token, payload={
        "exitPrice": 23500.00,
        "commission": 5.0
    })

    print("\nDELETE TRADE")
    request(TRADE_URL, "DELETE", "/1", token=token)


# -----------------------------
# MAIN
# -----------------------------
def main():
    token = register_user()

    test_auth()
    test_users(token)
    test_trades(token)


if __name__ == "__main__":
    try:
        main()
    except HTTPError as e:
        print(f"HTTP {e.code}")
        print(e.read().decode())
    except URLError as e:
        print(f"Connection error: {e.reason}")