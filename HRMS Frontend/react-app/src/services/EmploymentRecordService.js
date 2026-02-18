import api from "../config/api";

export default class EmploymentRecordService {
  // Create employment record
  static async create(employmentRecordData) {
    try {
      const res = await api.post("/EmploymentRecords", employmentRecordData);
      return res.data;
    } catch (err) {
      console.error("EmploymentRecordService.create error:", err);
      throw new Error(err.response?.data?.message || "Failed to create employment record");
    }
  }

  // Get all employment records for an employee
  static async getByEmployeeId(employeeId) {
    try {
      const res = await api.get(`/EmploymentRecords/employee/${employeeId}`);
      return res.data;
    } catch (err) {
      console.error("EmploymentRecordService.getByEmployeeId error:", err);
      throw new Error(err.response?.data?.message || "Failed to fetch employment records");
    }
  }

  // Get employment record by ID
  static async getById(id) {
    try {
      const res = await api.get(`/EmploymentRecords/${id}`);
      return res.data;
    } catch (err) {
      console.error("EmploymentRecordService.getById error:", err);
      throw new Error(err.response?.data?.message || "Failed to fetch employment record");
    }
  }

  // Update employment record
  static async update(id, employmentRecordData) {
    try {
      const res = await api.put(`/EmploymentRecords/${id}`, employmentRecordData);
      return res.data;
    } catch (err) {
      console.error("EmploymentRecordService.update error:", err);
      throw new Error(err.response?.data?.message || "Failed to update employment record");
    }
  }

  // Delete employment record
  static async delete(id) {
    try {
      await api.delete(`/EmploymentRecords/${id}`);
      return true;
    } catch (err) {
      console.error("EmploymentRecordService.delete error:", err);
      throw new Error(err.response?.data?.message || "Failed to delete employment record");
    }
  }

  // Activate employment record (set as active)
  static async activate(id) {
    try {
      const res = await api.post(`/EmploymentRecords/${id}/activate`);
      return res.data;
    } catch (err) {
      console.error("EmploymentRecordService.activate error:", err);
      throw new Error(err.response?.data?.message || "Failed to activate employment record");
    }
  }

  // Deactivate employment record
  static async deactivate(id) {
    try {
      const res = await api.post(`/EmploymentRecords/${id}/deactivate`);
      return res.data;
    } catch (err) {
      console.error("EmploymentRecordService.deactivate error:", err);
      throw new Error(err.response?.data?.message || "Failed to deactivate employment record");
    }
  }

  // Get active employment record for an employee
  static async getActiveByEmployeeId(employeeId) {
    try {
      const res = await api.get(`/EmploymentRecords/employee/${employeeId}/active`);
      return res.data;
    } catch (err) {
      console.error("EmploymentRecordService.getActiveByEmployeeId error:", err);
      throw new Error(err.response?.data?.message || "Failed to fetch active employment record");
    }
  }
}