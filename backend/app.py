import os
import sqlite3
from flask import Flask, request, jsonify
from flask_cors import CORS
import bcrypt
import jwt
from datetime import datetime, timedelta

app = Flask(__name__)
CORS(app)

JWT_SECRET = "JwtSecretoAsiQueNoHayNaDa"

DATA_DIR = os.path.join(os.getcwd(), "data")
DB_PATH = os.path.join(DATA_DIR, "pokemon.db")
os.makedirs(DATA_DIR, exist_ok=True)

def init_db():
    conn = sqlite3.connect(DB_PATH)
    cur = conn.cursor()
    cur.execute("""
    CREATE TABLE IF NOT EXISTS users (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      username TEXT UNIQUE NOT NULL,
      password_hash TEXT NOT NULL,
      email TEXT,
      created_at DATETIME DEFAULT CURRENT_TIMESTAMP
    );
    """)
    conn.commit()
    conn.close()

init_db()

def get_conn():
    return sqlite3.connect(DB_PATH)

@app.route("/api/auth/register", methods=["POST"])
def register():
    data = request.json or {}
    username = data.get("username")
    password = data.get("password")
    email = data.get("email")

    if not username or not password:
        return jsonify({"message":"username/password required"}), 400

    pw_hash = bcrypt.hashpw(password.encode(), bcrypt.gensalt()).decode()

    try:
        conn = get_conn()
        cur = conn.cursor()
        cur.execute("INSERT INTO users (username, password_hash, email) VALUES (?,?,?)",
                    (username, pw_hash, email))
        conn.commit()
        return jsonify({"message":"ok"})
    except sqlite3.IntegrityError:
        return jsonify({"message":"username exists"}), 400
    finally:
        conn.close()

@app.route("/api/auth/login", methods=["POST"])
def login():
    data = request.json or {}
    username = data.get("username")
    password = data.get("password")

    conn = get_conn()
    cur = conn.cursor()
    cur.execute("SELECT id, username, password_hash FROM users WHERE username = ? LIMIT 1", (username,))
    row = cur.fetchone()
    conn.close()

    if not row:
        return jsonify({"message":"invalid credentials"}), 401

    user_id, user_name, pass_hash = row

    if not bcrypt.checkpw(password.encode(), pass_hash.encode()):
        return jsonify({"message":"invalid credentials"}), 401

    payload = {
        "sub": user_name,
        "id": user_id,
        "exp": datetime.utcnow() + timedelta(hours=6)
    }

    token = jwt.encode(payload, JWT_SECRET, algorithm="HS256")
    return jsonify({"token": token})

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)
