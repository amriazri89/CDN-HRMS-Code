import api from "./api";

const DashboardService = {
  getSummary: () => api.get("/dashboard/summary"),
  getRecentActivities: () => api.get("/dashboard/recent"),
  getChartsData: () => api.get("/dashboard/charts"),
};

export default DashboardService;
