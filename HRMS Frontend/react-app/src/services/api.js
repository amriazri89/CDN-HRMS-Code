import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:5001/api", 
  headers: {
    "Content-Type": "application/json",
  },
});

// 🔹 Add JWT token to every request
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// 🔹 Handle 401 errors (expired token)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.clear();
      if (window.location.pathname !== "/etiqa/hrms/login") {
        window.location.href = "/etiqa/hrms/login";
      }
    }
    return Promise.reject(error);
  }
);

export default api; 