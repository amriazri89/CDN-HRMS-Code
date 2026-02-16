import api from "./api";

export default class EmployeeService {
  // ========== SERVER-SIDE PAGINATION (FIXED) ==========
  static async getPaged(
    pageNumber = 1,
    pageSize = 10,
    includeArchived = false,
  ) {
    try {
      console.log("🔍 getPaged called:", {
        pageNumber,
        pageSize,
        includeArchived,
      }); // DEBUG

      const res = await api.get(`/Employees/paged`, {
        params: {
          pageNumber: pageNumber,
          pageSize: pageSize,
          sortBy: "Name",
          sortDescending: false,
          searchTerm: "",
          includeArchived: includeArchived, // ✅ Send to backend
        },
      });

      console.log("✅ Response:", res.data); // DEBUG

      const paginationHeader = res.headers["x-pagination"];
      const pagination = paginationHeader ? JSON.parse(paginationHeader) : null;

      return {
        data: res.data.items || res.data, // Backend returns PagedResult with .items
        pagination: pagination || {
          totalCount: res.data.totalCount || 0,
          pageSize: res.data.pageSize || pageSize,
          pageNumber: res.data.pageNumber || pageNumber,
          totalPages: res.data.totalPages || 1,
        },
      };
    } catch (err) {
      console.error("❌ getPaged error:", err.response?.data);
      throw new Error(
        err.response?.data?.message || "Failed to fetch employees",
      );
    }
  }

  // Get all employees (NO PAGINATION - for dropdown/select)
  static async getAll(includeArchived = false) {
    try {
      const res = await api.get(
        `/Employees?includeArchived=${includeArchived}`,
      );
      return res.data;
    } catch (err) {
      throw new Error(
        err.response?.data?.message || "Failed to fetch employees",
      );
    }
  }

  // Get employee by ID
  static async getById(id) {
    try {
      const res = await api.get(`/Employees/${id}`);
      return res.data;
    } catch (err) {
      throw new Error(
        err.response?.data?.message || "Failed to fetch employee",
      );
    }
  }

  // Search employees
  static async search(keyword) {
    try {
      const res = await api.get(`/Employees/search?keyword=${keyword}`);
      return res.data;
    } catch (err) {
      throw new Error(err.response?.data?.message || "Search failed");
    }
  }

  // Create employee
  static async create(employeeData) {
    try {
      const res = await api.post("/Employees", employeeData);
      return res.data;
    } catch (err) {
      throw new Error(
        err.response?.data?.message || "Failed to create employee",
      );
    }
  }

  // Update employee
  static async update(id, employeeData) {
    try {
      console.log("🔹 EmployeeService.update called");
      console.log("🔹 Employee ID:", id);
      console.log("🔹 Data:", employeeData);

      const res = await api.put(`/Employees/${id}`, employeeData);

      console.log("✅ Update successful:", res.data);
      return res.data;
    } catch (err) {
      console.error("❌ Update failed:", err);
      console.error("❌ Status:", err.response?.status);
      console.error("❌ Response data:", err.response?.data);

      throw new Error(
        err.response?.data?.message || "Failed to update employee",
      );
    }
  }

  // Delete employee
  static async delete(id) {
    try {
      await api.delete(`/Employees/${id}`);
      return true;
    } catch (err) {
      throw new Error(
        err.response?.data?.message || "Failed to delete employee",
      );
    }
  }

  // Archive employee
  static async archive(id) {
    try {
      const res = await api.post(`/Employees/${id}/archive`);
      return res.data;
    } catch (err) {
      throw new Error(
        err.response?.data?.message || "Failed to archive employee",
      );
    }
  }

  // Unarchive employee
  static async unarchive(id) {
    try {
      const res = await api.post(`/Employees/${id}/unarchive`);
      return res.data;
    } catch (err) {
      throw new Error(
        err.response?.data?.message || "Failed to unarchive employee",
      );
    }
  }

  // Calculate salary
  static async calculateSalary(id, startDate, endDate) {
    try {
      const res = await api.post(
        `/Employees/${id}/calculate-salary?startDate=${startDate}&endDate=${endDate}`,
      );
      return res.data;
    } catch (err) {
      throw new Error(
        err.response?.data?.message || "Failed to calculate salary",
      );
    }
  }
}
