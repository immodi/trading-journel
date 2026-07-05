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


def print_response(response):
    print(f"Status: {response.status}")
    body = response.read().decode()

    if body:
        try:
            print(json.dumps(json.loads(body), indent=2))
        except json.JSONDecodeError:
            print(body)

    print("-" * 60)


def request(base_url, method, path="", payload=None):
    url = base_url + path

    data = None
    headers = {}

    if payload is not None:
        data = json.dumps(payload).encode()
        headers["Content-Type"] = "application/json"

    req = Request(url, data=data, headers=headers, method=method)

    with urlopen(req) as response:
        print(f"{method} {url}")
        print_response(response)


def create_user():
    print("CREATE USER")

    payload = {
        "username": "johndoe",
        "email": "john.doe@example.com",
        "password": "Password123!"
    }

    request(USER_URL, "POST", payload=payload)

    # assuming first user = 1 (simple dev assumption)
    return 1


def test_users():
    print("\nGET USERS")
    request(USER_URL, "GET")

    print("\nGET USER BY ID")
    request(USER_URL, "GET", "/1")


def test_trades():
    print("\nCREATE TRADE")

    request(TRADE_URL, "POST", payload={
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
    request(TRADE_URL, "GET")

    print("\nGET TRADE BY ID")
    request(TRADE_URL, "GET", "/1")

    print("\nUPDATE TRADE")
    request(TRADE_URL, "PUT", "/1", {
        "exitPrice": 23500.00,
        "commission": 5.0
    })

    print("\nGET TRADE AFTER UPDATE")
    request(TRADE_URL, "GET", "/1")

    print("\nDELETE TRADE")
    request(TRADE_URL, "DELETE", "/1")

    print("\nGET TRADES AFTER DELETE")
    request(TRADE_URL, "GET")


def main():
    user_id = create_user()

    test_users()
    test_trades()


if __name__ == "__main__":
    try:
        main()
    except HTTPError as e:
        print(f"HTTP {e.code}")
        print(e.read().decode())
    except URLError as e:
        print(f"Connection error: {e.reason}")
