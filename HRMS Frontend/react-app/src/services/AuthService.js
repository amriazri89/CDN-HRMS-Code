// src/services/AuthService.js
import axios from 'axios';

// ✅ Use relative path - proxied by Vercel
const API_BASE = "/api";

// Create axios instance
const api = axios.create({
  baseURL: API_BASE,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add JWT token to requests automatically
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ========== AUTH SERVICE ==========
const AuthService = {
  // Login
  login: async (username, password) => {
    console.log('🔐 Attempting login...', { username });
    
    try {
      const response = await api.post('/Auth/login', {
        username,
        password,
      });
      
      console.log('✅ Login successful:', response.data);
      
      if (response.data.token) {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('username', username);
      }
      
      return response.data;
    } catch (error) {
      console.error('❌ Login failed:', error);
      throw error;
    }
  },

  // Register
  register: async (username, password) => {
    const response = await api.post('/Auth/register', {
      username,
      password,
    });
    return response.data;
  },

  // Logout
  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
  },

  // Get current user token
  getToken: () => {
    return localStorage.getItem('token');
  },

  // Check if user is authenticated
  isAuthenticated: () => {
    return !!localStorage.getItem('token');
  },

  // Get current username
  getCurrentUser: () => {
    return localStorage.getItem('username');
  },
};

// ✅ Export AuthService as default (not api)
export default AuthService;

// Also export api if you need it elsewhere
export { api };