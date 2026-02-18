import api from "../config/api";

export default class EmployeeService {
  // ========== SERVER-SIDE PAGINATION ==========
  static async getPaged(pageNumber = 1, pageSize = 10, includeArchived = false) {
    try {
      const res = await api.get(`/Employees/paged`, {
        params: {
          pageNumber,
          pageSize,
          sortBy: "Name",
          sortDescending: false,
          searchTerm: "",
          includeArchived,
        },
      });

      const paginationHeader = res.headers["x-pagination"];
      const pagination = paginationHeader ? JSON.parse(paginationHeader) : null;

      return {
        data: res.data.items || res.data,
        pagination: pagination || {
          totalCount: res.data.totalCount || 0,
          pageSize: res.data.pageSize || pageSize,
          pageNumber: res.data.pageNumber || pageNumber,
          totalPages: res.data.totalPages || 1,
        },
      };
    } catch (err) {
      throw err; // ✅ keep full Axios error
    }
  }

  // Get all employees
  static async getAll(includeArchived = false) {
    try {
      const res = await api.get(`/Employees?includeArchived=${includeArchived}`);
      return res.data;
    } catch (err) {
      throw err; // ✅
    }
  }

  // Get employee by ID
  static async getById(id) {
    try {
      const res = await api.get(`/Employees/${id}`);
      return res.data;
    } catch (err) {
      throw err; // ✅
    }
  }

  // Search employees
  static async search(keyword) {
    try {
      const res = await api.get(`/Employees/search?keyword=${keyword}`);
      return res.data;
    } catch (err) {
      throw err; // ✅
    }
  }

  // Create employee
  static async create(employeeData) {
    try {
      const res = await api.post("/Employees", employeeData);
      return res.data;
    } catch (err) {
      throw err; // ✅
    }
  }

  // Update employee
  static async update(id, employeeData) {
    try {
      const res = await api.put(`/Employees/${id}`, employeeData);
      return res.data;
    } catch (err) {
      throw err; // ✅ this is the key fix — keeps err.response intact
    }
  }

  // Delete employee
  static async delete(id) {
    try {
      await api.delete(`/Employees/${id}`);
      return true;
    } catch (err) {
      throw err; // ✅
    }
  }

  // Archive employee
  static async archive(id) {
    try {
      const res = await api.post(`/Employees/${id}/archive`);
      return res.data;
    } catch (err) {
      throw err; // ✅
    }
  }

  // Unarchive employee
  static async unarchive(id) {
    try {
      const res = await api.post(`/Employees/${id}/unarchive`);
      return res.data;
    } catch (err) {
      throw err; // ✅
    }
  }

  // Calculate salary
  static async calculateSalary(id, startDate, endDate) {
    try {
      const res = await api.post(
        `/Employees/${id}/calculate-salary?startDate=${startDate}&endDate=${endDate}`
      );
      return res.data;
    } catch (err) {
      throw err; // ✅
    }
  }
}