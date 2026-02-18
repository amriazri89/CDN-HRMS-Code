// src/services/AuthService.js
import api from "../config/api";

const AuthService = {
  login: async (username, password) => {
    const response = await api.post("/Auth/login", {
      username,
      password,
    });

    if (response.data.token) {
      localStorage.setItem("token", response.data.token);
      localStorage.setItem("username", username);
    }

    return response.data;
  },

  logout: () => {
    localStorage.removeItem("token");
    localStorage.removeItem("username");
  },

  isAuthenticated: () => {
    return !!localStorage.getItem("token");
  },

  getCurrentUser: () => {
    return localStorage.getItem("username");
  },
};

export default AuthService;
