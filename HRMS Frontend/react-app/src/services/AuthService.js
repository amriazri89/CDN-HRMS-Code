// src/services/AuthService.js
import api from "../config/api";

const AuthService = {
  login: async (username, password) => {
    const response = await api.post("/Auth/login", {
      username,
      password,
    });

    if (response.data.token) {
      // store token
      localStorage.setItem("token", response.data.token);

      // store full user object for later
      const user = {
        username: response.data.username || username,
        email: response.data.email || null,
        id: response.data.id || null,
      };
      localStorage.setItem("user", JSON.stringify(user));
    }

    return response.data;
  },

  logout: () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
  },

  isAuthenticated: () => {
    return !!localStorage.getItem("token");
  },

  getCurrentUser: () => {
    const user = localStorage.getItem("user");
    return user ? JSON.parse(user) : null;
  },
};

export default AuthService;
