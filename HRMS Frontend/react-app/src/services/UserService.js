import api from "./api";

export default class UserService {
 static async login(username, password) {
  try {
    console.log("🔐 Attempting login...", { username });
    
    const res = await api.post("/Auth/login", { username, password });
    console.log("✅ Login successful!", res.data);
    
    const data = res.data || {};

    if (data.token) localStorage.setItem("token", data.token);
    if (data.username) localStorage.setItem("username", data.username);

    return data;
  } catch (err) {
    console.error("❌ Login failed:", err);
    console.error("Error details:", {
      status: err.response?.status,
      message: err.response?.data?.message,
      fullError: err.response
    });

    if (!err.response) {
      throw new Error("Cannot connect to server. Is API running?");
    }

    const status = err.response?.status;
    const message = err.response?.data?.message;

    if (status === 401) {
      throw new Error("Invalid username or password");
    }
    
    throw new Error(message || "Login failed");
  }
}

  static async register(username, password) {
    try {
      const res = await api.post("/Auth/register", { username, password });
      return res.data;
    } catch (err) {
      throw new Error(err.response?.data?.message || "Registration failed");
    }
  }

  static async verifyToken() {
    try {
      const res = await api.get("/Auth/verify");
      return res.data;
    } catch (err) {
      this.logout();
      throw new Error("Session expired");
    }
  }

  static getCurrentUser() {
    return {
      username: localStorage.getItem("username"),
      token: localStorage.getItem("token"),
      isAuthenticated: !!localStorage.getItem("token")
    };
  }

  static isAuthenticated() {
    return !!localStorage.getItem("token");
  }

  static getToken() {
    return localStorage.getItem("token");
  }

  static logout() {
    localStorage.clear();
  }
}