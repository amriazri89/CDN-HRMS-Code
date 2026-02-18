import api from "../config/api";

export default class EmploymentRecordService {
  static async create(employmentRecordData) {
    try {
      const res = await api.post("/EmploymentRecords", employmentRecordData);
      return res.data;
    } catch (err) {
      throw err;
    }
  }

  static async getByEmployeeId(employeeId) {
    try {
      const res = await api.get(`/EmploymentRecords/employee/${employeeId}`);
      return res.data;
    } catch (err) {
      throw err;
    }
  }

  static async getById(id) {
    try {
      const res = await api.get(`/EmploymentRecords/${id}`);
      return res.data;
    } catch (err) {
      throw err;
    }
  }

  static async update(id, employmentRecordData) {
    try {
      const res = await api.put(`/EmploymentRecords/${id}`, employmentRecordData);
      return res.data;
    } catch (err) {
      throw err;
    }
  }

  static async delete(id) {
    try {
      await api.delete(`/EmploymentRecords/${id}`);
      return true;
    } catch (err) {
      throw err;
    }
  }

  static async activate(id) {
    try {
      const res = await api.post(`/EmploymentRecords/${id}/activate`);
      return res.data;
    } catch (err) {
      throw err;
    }
  }

  static async deactivate(id) {
    try {
      const res = await api.post(`/EmploymentRecords/${id}/deactivate`);
      return res.data;
    } catch (err) {
      throw err;
    }
  }

  static async getActiveByEmployeeId(employeeId) {
    try {
      const res = await api.get(`/EmploymentRecords/employee/${employeeId}/active`);
      return res.data;
    } catch (err) {
      throw err;
    }
  }
}