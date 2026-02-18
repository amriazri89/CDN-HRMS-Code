import React, { useEffect, useState } from "react";
import EmployeeService from "../../services/EmployeeService";
import EmploymentRecordService from "../../services/EmploymentRecordService";
import MainLayout from "../../components/MainLayout/MainLayout";
import Pagination from "../../components/Pagination/Pagination";
import {
  FaEdit, FaTrash, FaArchive, FaUndo,
  FaMoneyBillWave, FaBriefcase, FaList, FaSearch,
} from "react-icons/fa";
import "./Employee.css";

const daysOfWeek = [
  { name: "Sunday", value: 0 },
  { name: "Monday", value: 1 },
  { name: "Tuesday", value: 2 },
  { name: "Wednesday", value: 3 },
  { name: "Thursday", value: 4 },
  { name: "Friday", value: 5 },
  { name: "Saturday", value: 6 },
];

// ========== HELPER: Parse API errors ==========
const parseApiError = (err) => {
  const data = err.response?.data;
  if (!data) return { message: err.message, fields: [] };
  return {
    message: data.message || data.Message || "Something went wrong",
    fields: Array.isArray(data.errors)
      ? data.errors.map((e) => ({
          property: e.propertyName || e.PropertyName || "",
          error: e.errorMessage || e.ErrorMessage || "",
        }))
      : [],
  };
};

// ========== ERROR BOX ==========
const ErrorBox = ({ error, onClose }) => {
  if (!error) return null;
  return (
    <div style={{
      background: "#fef2f2", border: "1px solid #fca5a5",
      borderRadius: "8px", padding: "12px 16px",
      marginBottom: "16px", position: "relative",
    }}>
      <button onClick={onClose} style={{
        position: "absolute", top: "8px", right: "12px",
        background: "none", border: "none", fontSize: "16px",
        cursor: "pointer", color: "#ef4444",
      }}>×</button>
      <p style={{ color: "#dc2626", fontWeight: "600", margin: "0 0 6px 0" }}>
        ❌ {error.message}
      </p>
      {error.fields?.length > 0 && (
        <ul style={{ margin: 0, paddingLeft: "18px" }}>
          {error.fields.map((f, i) => (
            <li key={i} style={{ color: "#b91c1c", fontSize: "14px" }}>
              <strong>{f.property}:</strong> {f.error}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

// ========== TOAST ==========
const Toast = ({ message, type }) => {
  if (!message) return null;
  const bg = type === "success" ? "#22c55e" : "#ef4444";
  return (
    <div style={{
      position: "fixed", top: "20px", right: "20px", zIndex: 9999,
      background: bg, color: "white", padding: "12px 20px",
      borderRadius: "8px", fontSize: "14px", fontWeight: "600",
      boxShadow: "0 4px 12px rgba(0,0,0,0.2)",
      maxWidth: "350px",
    }}>
      {message}
    </div>
  );
};

const Employee = () => {
  const [employees, setEmployees] = useState([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState([]);
  const [isSearchMode, setIsSearchMode] = useState(false);

  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isEmploymentModalOpen, setIsEmploymentModalOpen] = useState(false);
  const [isViewEmploymentModalOpen, setIsViewEmploymentModalOpen] = useState(false);
  const [editingEmployee, setEditingEmployee] = useState(null);
  const [selectedEmployeeForEmployment, setSelectedEmployeeForEmployment] = useState(null);
  const [viewingEmployeeRecords, setViewingEmployeeRecords] = useState(null);

  const [addError, setAddError] = useState(null);
  const [editError, setEditError] = useState(null);
  const [employmentError, setEmploymentError] = useState(null);

  // ========== TOAST STATE ==========
  const [toast, setToast] = useState({ message: "", type: "success" });

  const showToast = (message, type = "success") => {
    setToast({ message, type });
    setTimeout(() => setToast({ message: "", type: "success" }), 3000);
  };

  const [addForm, setAddForm] = useState({
    name: "", nationalNumber: "", contactNumber: "", position: "",
    address: "", dateOfBirth: "", employmentType: "Permanent",
    dailyRate: "", startDate: "", endDate: "", workingDays: [], skillSets: [],
  });

  const [editForm, setEditForm] = useState({
    employeeId: "", name: "", nationalNumber: "", contactNumber: "",
    position: "", address: "", dateOfBirth: "",
  });

  const [employmentForm, setEmploymentForm] = useState({
    employmentType: "Permanent", position: "", dailyRate: "",
    startDate: "", endDate: "", workingDays: [], skillSets: [],
  });

  const [skillInput, setSkillInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [showArchived, setShowArchived] = useState(false);

  useEffect(() => {
    document.title = "HRMS - Employees";
    fetchEmployees();
  }, [showArchived, pageNumber, pageSize]);

  useEffect(() => {
    const delayDebounce = setTimeout(() => {
      if (searchQuery.trim()) handleSearch(searchQuery);
      else { setIsSearchMode(false); setSearchResults([]); }
    }, 300);
    return () => clearTimeout(delayDebounce);
  }, [searchQuery]);

  const fetchEmployees = async () => {
    try {
      setLoading(true);
      const result = await EmployeeService.getPaged(pageNumber, pageSize, showArchived);
      setEmployees(Array.isArray(result.data) ? result.data : []);
      setTotalCount(result.pagination.TotalCount);
      setTotalPages(result.pagination.TotalPages);
    } catch (err) {
      console.error("Fetch failed:", err);
      setEmployees([]);
      setTotalCount(0);
      setTotalPages(1);
    } finally {
      setLoading(false);
    }
  };

  const handlePageChange = (n) => { setPageNumber(n); setSearchQuery(""); setIsSearchMode(false); };
  const handlePageSizeChange = (s) => { setPageSize(s); setPageNumber(1); setSearchQuery(""); setIsSearchMode(false); };

  const handleSearch = async (query) => {
    if (!query.trim()) { setIsSearchMode(false); setSearchResults([]); return; }
    try {
      setLoading(true);
      setIsSearchMode(true);
      const results = await EmployeeService.search(query);
      setSearchResults(Array.isArray(results) ? results : []);
    } catch (err) {
      setSearchResults([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSearchChange = (e) => setSearchQuery(e.target.value);
  const clearSearch = () => { setSearchQuery(""); setIsSearchMode(false); setSearchResults([]); };
  const displayedEmployees = isSearchMode ? searchResults : employees;

  // ========== VIEW EMPLOYMENT RECORDS ==========
  const openViewEmploymentModal = async (employee) => {
    try {
      setLoading(true);
      const records = await EmploymentRecordService.getByEmployeeId(employee.employeeId);
      setViewingEmployeeRecords({ ...employee, employmentRecords: records });
      setIsViewEmploymentModalOpen(true);
    } catch (err) {
      showToast("Failed to load employment records", "error");
    } finally {
      setLoading(false);
    }
  };

  const closeViewEmploymentModal = () => { setIsViewEmploymentModalOpen(false); setViewingEmployeeRecords(null); };

  const handleActivateRecord = async (recordId) => {
    if (!window.confirm("Activate this record? This will deactivate all others.")) return;
    try {
      setLoading(true);
      await EmploymentRecordService.activate(recordId);
      const records = await EmploymentRecordService.getByEmployeeId(viewingEmployeeRecords.employeeId);
      setViewingEmployeeRecords({ ...viewingEmployeeRecords, employmentRecords: records });
      showToast("Employment record activated!");
    } catch (err) {
      showToast(parseApiError(err).message, "error");
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteRecord = async (recordId) => {
    if (!window.confirm("Delete this record? Cannot be undone.")) return;
    try {
      setLoading(true);
      await EmploymentRecordService.delete(recordId);
      const records = await EmploymentRecordService.getByEmployeeId(viewingEmployeeRecords.employeeId);
      setViewingEmployeeRecords({ ...viewingEmployeeRecords, employmentRecords: records });
      showToast("Employment record deleted!");
    } catch (err) {
      showToast(parseApiError(err).message, "error");
    } finally {
      setLoading(false);
    }
  };

  // ========== ADD EMPLOYEE ==========
  const openAddModal = () => {
    setAddForm({
      name: "", nationalNumber: "", contactNumber: "", position: "",
      address: "", dateOfBirth: "", employmentType: "Permanent",
      dailyRate: "", startDate: new Date().toISOString().split("T")[0],
      endDate: "", workingDays: [], skillSets: [],
    });
    setAddError(null);
    setSkillInput("");
    setIsAddModalOpen(true);
  };

  const closeAddModal = () => { setIsAddModalOpen(false); setAddError(null); };
  const handleAddChange = (e) => setAddForm({ ...addForm, [e.target.name]: e.target.value });

  const toggleAddWorkingDay = (v) => setAddForm((p) => ({
    ...p,
    workingDays: p.workingDays.includes(v) ? p.workingDays.filter((d) => d !== v) : [...p.workingDays, v],
  }));

  const addSkillToAdd = () => {
    const skill = skillInput.trim();
    if (!skill) return;
    if (addForm.skillSets.includes(skill)) return;
    setAddForm((p) => ({ ...p, skillSets: [...p.skillSets, skill] }));
    setSkillInput("");
  };

  const removeSkillFromAdd = (s) => setAddForm((p) => ({ ...p, skillSets: p.skillSets.filter((x) => x !== s) }));

  const handleAddSubmit = async (e) => {
    e.preventDefault();
    setAddError(null);
    if (!addForm.name || !addForm.nationalNumber || !addForm.dateOfBirth) {
      setAddError({ message: "Please fill in all required fields", fields: [] });
      return;
    }
    if (!addForm.dailyRate || addForm.workingDays.length === 0) {
      setAddError({ message: "Please enter daily rate and select at least one working day", fields: [] });
      return;
    }
    try {
      setLoading(true);
      const employeePayload = {
        name: addForm.name.trim(),
        nationalNumber: addForm.nationalNumber.trim(),
        contactNumber: addForm.contactNumber.trim(),
        position: addForm.position.trim(),
        address: addForm.address.trim(),
        dateOfBirth: addForm.dateOfBirth,
      };
      const createdEmployee = await EmployeeService.create(employeePayload);
      const employmentPayload = {
        employeeId: createdEmployee.employeeId,
        employmentType: addForm.employmentType,
        position: addForm.position.trim() || "",
        startDate: addForm.startDate,
        endDate: addForm.endDate || null,
        dailyRate: parseFloat(addForm.dailyRate),
        isActive: true,
        workingDays: addForm.workingDays,
        skillSets: addForm.skillSets,
      };
      await EmploymentRecordService.create(employmentPayload);
      closeAddModal();
      await fetchEmployees(); // ← refresh list immediately
      showToast(`Employee ${createdEmployee.employeeNumber} created!`);
    } catch (err) {
      setAddError(parseApiError(err));
    } finally {
      setLoading(false);
    }
  };

  // ========== EDIT EMPLOYEE ==========
  const openEditModal = (employee) => {
    setEditingEmployee(employee);
    setEditForm({
      employeeId: employee.employeeId,
      name: employee.name || "",
      nationalNumber: employee.nationalNumber || "",
      contactNumber: employee.contactNumber || "",
      position: employee.position || "",
      address: employee.address || "",
      dateOfBirth: employee.dateOfBirth?.split("T")[0] || "",
    });
    setEditError(null);
    setIsEditModalOpen(true);
  };

  const closeEditModal = () => { setIsEditModalOpen(false); setEditingEmployee(null); setEditError(null); };
  const handleEditChange = (e) => setEditForm({ ...editForm, [e.target.name]: e.target.value });

  const handleEditSubmit = async (e) => {
    e.preventDefault();
    setEditError(null);
    const payload = {
      employeeId: editForm.employeeId,
      name: editForm.name.trim(),
      nationalNumber: editForm.nationalNumber.trim(),
      contactNumber: editForm.contactNumber.trim(),
      position: editForm.position.trim(),
      address: editForm.address.trim(),
      dateOfBirth: editForm.dateOfBirth,
    };
    try {
      setLoading(true);
      await EmployeeService.update(editingEmployee.employeeId, payload);
      closeEditModal();
      await fetchEmployees(); // ← refresh list immediately
      showToast("Employee updated!");
    } catch (err) {
      setEditError(parseApiError(err));
    } finally {
      setLoading(false);
    }
  };

  // ========== ADD EMPLOYMENT RECORD ==========
  const openEmploymentModal = (employee) => {
    setSelectedEmployeeForEmployment(employee);
    setEmploymentForm({
      employmentType: "Permanent",
      position: employee.position || "",
      dailyRate: "",
      startDate: new Date().toISOString().split("T")[0],
      endDate: "",
      workingDays: [],
      skillSets: [],
    });
    setEmploymentError(null);
    setSkillInput("");
    setIsEmploymentModalOpen(true);
  };

  const closeEmploymentModal = () => { setIsEmploymentModalOpen(false); setSelectedEmployeeForEmployment(null); setEmploymentError(null); };
  const handleEmploymentChange = (e) => setEmploymentForm({ ...employmentForm, [e.target.name]: e.target.value });

  const toggleEmploymentWorkingDay = (v) => setEmploymentForm((p) => ({
    ...p,
    workingDays: p.workingDays.includes(v) ? p.workingDays.filter((d) => d !== v) : [...p.workingDays, v],
  }));

  const addSkillToEmployment = () => {
    const skill = skillInput.trim();
    if (!skill) return;
    if (employmentForm.skillSets.includes(skill)) return;
    setEmploymentForm((p) => ({ ...p, skillSets: [...p.skillSets, skill] }));
    setSkillInput("");
  };

  const removeSkillFromEmployment = (s) => setEmploymentForm((p) => ({ ...p, skillSets: p.skillSets.filter((x) => x !== s) }));

  const handleEmploymentSubmit = async (e) => {
    e.preventDefault();
    setEmploymentError(null);
    if (!employmentForm.dailyRate || employmentForm.workingDays.length === 0) {
      setEmploymentError({ message: "Please enter daily rate and select at least one working day", fields: [] });
      return;
    }
    const payload = {
      employeeId: selectedEmployeeForEmployment.employeeId,
      employmentType: employmentForm.employmentType,
      position: employmentForm.position || selectedEmployeeForEmployment.position || "",
      startDate: employmentForm.startDate,
      endDate: employmentForm.endDate || null,
      dailyRate: parseFloat(employmentForm.dailyRate),
      isActive: true,
      workingDays: employmentForm.workingDays,
      skillSets: employmentForm.skillSets,
    };
    try {
      setLoading(true);
      await EmploymentRecordService.create(payload);
      closeEmploymentModal();
      await fetchEmployees(); // ← refresh list immediately
      showToast("Employment record created!");
    } catch (err) {
      setEmploymentError(parseApiError(err));
    } finally {
      setLoading(false);
    }
  };

  // ========== DELETE / ARCHIVE / UNARCHIVE ==========
  const handleDelete = async (id) => {
    if (!window.confirm("Delete this employee?")) return;
    try {
      setLoading(true);
      await EmployeeService.delete(id);
      // Immediately remove from local state — no need to wait for refetch
      setEmployees((prev) => prev.filter((emp) => emp.employeeId !== id));
      setSearchResults((prev) => prev.filter((emp) => emp.employeeId !== id));
      showToast("Employee deleted!");
    } catch (err) {
      showToast(parseApiError(err).message, "error");
    } finally {
      setLoading(false);
    }
  };

  const handleArchive = async (id) => {
    if (!window.confirm("Archive this employee?")) return;
    try {
      setLoading(true);
      await EmployeeService.archive(id);
      await fetchEmployees(); // ← refresh so archived employee disappears from active list
      showToast("Employee archived!");
    } catch (err) {
      showToast(parseApiError(err).message, "error");
    } finally {
      setLoading(false);
    }
  };

  const handleUnarchive = async (id) => {
    if (!window.confirm("Unarchive this employee?")) return;
    try {
      setLoading(true);
      await EmployeeService.unarchive(id);
      await fetchEmployees(); // ← refresh list immediately
      showToast("Employee unarchived!");
    } catch (err) {
      showToast(parseApiError(err).message, "error");
    } finally {
      setLoading(false);
    }
  };

  const handleCalculateSalary = async (employee) => {
    const start = prompt("Start Date (YYYY-MM-DD):", "2025-02-01");
    const end = prompt("End Date (YYYY-MM-DD):", "2025-02-14");
    if (!start || !end) return;
    try {
      setLoading(true);
      const result = await EmployeeService.calculateSalary(employee.employeeId, start, end);
      showToast(`${employee.name}: MYR ${result.takeHomePay.toFixed(2)}`);
    } catch (err) {
      showToast(parseApiError(err).message, "error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <MainLayout>
      {/* ========== TOAST ========== */}
      <Toast message={toast.message} type={toast.type} />

      <div className="employee-page">
        <div className="employee-header">
          <div>
            <h1>Employee Management</h1>
            <p>Manage employee records and employment details</p>
          </div>
          <div className="employee-header-actions">
            <button className="btn-primary" onClick={openAddModal} disabled={loading}>+ Add Employee</button>
            <button className="btn-secondary" onClick={() => { setShowArchived(!showArchived); setPageNumber(1); }} disabled={loading}>
              {showArchived ? "Active Only" : "Show Archived"}
            </button>
          </div>
        </div>

        <div className="search-container">
          <div className="search-input-wrapper">
            <FaSearch className="search-icon" />
            <input type="text" className="search-input" placeholder="Search by employee number or name..." value={searchQuery} onChange={handleSearchChange} disabled={loading} />
            {searchQuery && <button className="clear-search-btn" onClick={clearSearch}>×</button>}
          </div>
        </div>

        {loading && <p className="loading-text">⏳ Loading...</p>}

        <table className="employee-table">
          <thead>
            <tr>
              <th>Employee No</th><th>Name</th><th>National ID</th>
              <th>Contact</th><th>Position</th><th>Status</th><th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {displayedEmployees.length === 0 ? (
              <tr><td colSpan="7" style={{ textAlign: "center", padding: "20px" }}>
                {loading ? "Loading..." : isSearchMode ? `No results for "${searchQuery}"` : "No employees found"}
              </td></tr>
            ) : displayedEmployees.map((emp) => (
              <tr key={emp.employeeId}>
                <td>{emp.employeeNumber}</td>
                <td>{emp.name}</td>
                <td>{emp.nationalNumber}</td>
                <td>{emp.contactNumber || "-"}</td>
                <td>{emp.position || "-"}</td>
                <td><span className={emp.isArchived ? "badge-archived" : "badge-active"}>{emp.isArchived ? "Archived" : "Active"}</span></td>
                <td className="action-buttons">
                  {!emp.isArchived ? (
                    <>
                      <FaEdit className="action-icon edit-icon" onClick={() => openEditModal(emp)} title="Edit" />
                      <FaList className="action-icon list-icon" onClick={() => openViewEmploymentModal(emp)} title="View Records" />
                      <FaBriefcase className="action-icon employment-icon" onClick={() => openEmploymentModal(emp)} title="Add Record" />
                      <FaMoneyBillWave className="action-icon salary-icon" onClick={() => handleCalculateSalary(emp)} title="Salary" />
                      <FaArchive className="action-icon archive-icon" onClick={() => handleArchive(emp.employeeId)} title="Archive" />
                      <FaTrash className="action-icon delete-icon" onClick={() => handleDelete(emp.employeeId)} title="Delete" />
                    </>
                  ) : (
                    <FaUndo className="action-icon unarchive-icon" onClick={() => handleUnarchive(emp.employeeId)} title="Unarchive" />
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {!isSearchMode && !loading && displayedEmployees.length > 0 && (
          <Pagination pageNumber={pageNumber} totalPages={totalPages} totalCount={totalCount} pageSize={pageSize} onPageChange={handlePageChange} onPageSizeChange={handlePageSizeChange} />
        )}

        {isSearchMode && (
          <div style={{ textAlign: "center", padding: "20px", color: "#6b7280", fontStyle: "italic", background: "white", borderRadius: "8px", marginTop: "20px" }}>
            Found {displayedEmployees.length} result{displayedEmployees.length !== 1 ? "s" : ""}{searchQuery && ` for "${searchQuery}"`}
          </div>
        )}

        {/* ========== ADD MODAL ========== */}
        {isAddModalOpen && (
          <div className="modal-overlay" onClick={closeAddModal}>
            <div className="modal large-modal" onClick={(e) => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Add New Employee</h2>
                <button className="x-btn" onClick={closeAddModal} disabled={loading}>×</button>
              </div>
              <form onSubmit={handleAddSubmit} className="modal-form">
                <ErrorBox error={addError} onClose={() => setAddError(null)} />
                <div className="form-section">
                  <h3>Employee Information</h3>
                  <div className="form-row">
                    <input type="text" name="name" value={addForm.name} onChange={handleAddChange} placeholder="Full Name *" required />
                    <input type="text" name="nationalNumber" value={addForm.nationalNumber} onChange={handleAddChange} placeholder="National ID (e.g., 940110-01-5678) *" required />
                  </div>
                  <div className="form-row">
                    <input type="text" name="contactNumber" value={addForm.contactNumber} onChange={handleAddChange} placeholder="Contact (+60123456789)" />
                    <input type="text" name="position" value={addForm.position} onChange={handleAddChange} placeholder="Position" />
                  </div>
                  <div className="form-row">
                    <input type="text" name="address" value={addForm.address} onChange={handleAddChange} placeholder="Address" />
                    <input type="date" name="dateOfBirth" value={addForm.dateOfBirth} onChange={handleAddChange} required />
                  </div>
                </div>
                <div className="form-section">
                  <h3>Employment Details</h3>
                  <div className="form-row">
                    <select name="employmentType" value={addForm.employmentType} onChange={handleAddChange}>
                      <option value="Permanent">Permanent</option>
                      <option value="Contract">Contract</option>
                    </select>
                    <input type="number" name="dailyRate" value={addForm.dailyRate} onChange={handleAddChange} placeholder="Daily Rate (MYR) *" step="0.01" min="0" required />
                  </div>
                  <div className="form-row">
                    <input type="date" name="startDate" value={addForm.startDate} onChange={handleAddChange} required />
                    <input type="date" name="endDate" value={addForm.endDate} onChange={handleAddChange} />
                  </div>
                  <div className="working-days-section">
                    <h4>Working Days *</h4>
                    <div className="working-days-grid-clean">
                      {daysOfWeek.map((day) => (
                        <label key={day.value} className="checkbox-label">
                          <input type="checkbox" checked={addForm.workingDays.includes(day.value)} onChange={() => toggleAddWorkingDay(day.value)} />
                          {day.name}
                        </label>
                      ))}
                    </div>
                  </div>
                  <div className="skills-section">
                    <h4>Skills</h4>
                    <div className="skill-input-group">
                      <input type="text" value={skillInput} onChange={(e) => setSkillInput(e.target.value)} placeholder="e.g., C#, ReactJs" onKeyPress={(e) => e.key === "Enter" && (e.preventDefault(), addSkillToAdd())} />
                      <button type="button" onClick={addSkillToAdd} className="add-skill-btn">Add</button>
                    </div>
                    {addForm.skillSets.length > 0 && (
                      <div className="skills-list">
                        {addForm.skillSets.map((skill, idx) => (
                          <span key={idx} className="skill-tag">{skill}<button type="button" onClick={() => removeSkillFromAdd(skill)} className="remove-skill-btn">×</button></span>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
                <div className="modal-actions">
                  <button type="submit" className="btn-primary" disabled={loading}>{loading ? "Creating..." : "Create Employee"}</button>
                  <button type="button" className="cancel-btn" onClick={closeAddModal}>Cancel</button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* ========== EDIT MODAL ========== */}
        {isEditModalOpen && (
          <div className="modal-overlay" onClick={closeEditModal}>
            <div className="modal" onClick={(e) => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Edit Employee</h2>
                <button className="x-btn" onClick={closeEditModal}>×</button>
              </div>
              <form onSubmit={handleEditSubmit} className="modal-form">
                <ErrorBox error={editError} onClose={() => setEditError(null)} />
                <input type="hidden" name="employeeId" value={editForm.employeeId} />
                <input type="text" name="name" value={editForm.name} onChange={handleEditChange} placeholder="Full Name *" required />
                <input type="text" name="nationalNumber" value={editForm.nationalNumber} onChange={handleEditChange} placeholder="National ID (YYMMDD-XX-XXXX) *" required />
                <input type="text" name="contactNumber" value={editForm.contactNumber} onChange={handleEditChange} placeholder="Contact (+60123456789)" />
                <input type="text" name="position" value={editForm.position} onChange={handleEditChange} placeholder="Position" />
                <input type="text" name="address" value={editForm.address} onChange={handleEditChange} placeholder="Address" />
                <input type="date" name="dateOfBirth" value={editForm.dateOfBirth} onChange={handleEditChange} required />
                <div className="modal-actions">
                  <button type="submit" className="btn-primary" disabled={loading}>{loading ? "Updating..." : "Update"}</button>
                  <button type="button" className="cancel-btn" onClick={closeEditModal}>Cancel</button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* ========== EMPLOYMENT RECORD MODAL ========== */}
        {isEmploymentModalOpen && (
          <div className="modal-overlay" onClick={closeEmploymentModal}>
            <div className="modal large-modal" onClick={(e) => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Add Employment Record</h2>
                <button className="x-btn" onClick={closeEmploymentModal}>×</button>
              </div>
              <div className="employee-info">
                <p><strong>Employee:</strong> {selectedEmployeeForEmployment?.name}</p>
                <p><strong>Employee No:</strong> {selectedEmployeeForEmployment?.employeeNumber}</p>
              </div>
              <form onSubmit={handleEmploymentSubmit} className="modal-form">
                <ErrorBox error={employmentError} onClose={() => setEmploymentError(null)} />
                <div className="form-row">
                  <select name="employmentType" value={employmentForm.employmentType} onChange={handleEmploymentChange}>
                    <option value="Permanent">Permanent</option>
                    <option value="Contract">Contract</option>
                  </select>
                  <input type="text" name="position" value={employmentForm.position} onChange={handleEmploymentChange} placeholder="Position" />
                </div>
                <div className="form-row">
                  <input type="number" name="dailyRate" value={employmentForm.dailyRate} onChange={handleEmploymentChange} placeholder="Daily Rate (MYR) *" step="0.01" min="0" required />
                  <input type="date" name="startDate" value={employmentForm.startDate} onChange={handleEmploymentChange} required />
                </div>
                <input type="date" name="endDate" value={employmentForm.endDate} onChange={handleEmploymentChange} />
                <div className="working-days-section">
                  <h4>Working Days *</h4>
                  <div className="working-days-grid-clean">
                    {daysOfWeek.map((day) => (
                      <label key={day.value} className="checkbox-label">
                        <input type="checkbox" checked={employmentForm.workingDays.includes(day.value)} onChange={() => toggleEmploymentWorkingDay(day.value)} />
                        {day.name}
                      </label>
                    ))}
                  </div>
                </div>
                <div className="skills-section">
                  <h4>Skills</h4>
                  <div className="skill-input-group">
                    <input type="text" value={skillInput} onChange={(e) => setSkillInput(e.target.value)} placeholder="Enter skill" onKeyPress={(e) => e.key === "Enter" && (e.preventDefault(), addSkillToEmployment())} />
                    <button type="button" onClick={addSkillToEmployment} className="add-skill-btn">Add</button>
                  </div>
                  {employmentForm.skillSets.length > 0 && (
                    <div className="skills-list">
                      {employmentForm.skillSets.map((skill, idx) => (
                        <span key={idx} className="skill-tag">{skill}<button type="button" onClick={() => removeSkillFromEmployment(skill)} className="remove-skill-btn">×</button></span>
                      ))}
                    </div>
                  )}
                </div>
                <div className="modal-actions">
                  <button type="submit" className="btn-primary" disabled={loading}>{loading ? "Creating..." : "Create Employment Record"}</button>
                  <button type="button" className="cancel-btn" onClick={closeEmploymentModal}>Cancel</button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* ========== VIEW EMPLOYMENT RECORDS MODAL ========== */}
        {isViewEmploymentModalOpen && viewingEmployeeRecords && (
          <div className="modal-overlay" onClick={closeViewEmploymentModal}>
            <div className="modal large-modal" onClick={(e) => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Employment Records</h2>
                <button className="x-btn" onClick={closeViewEmploymentModal}>×</button>
              </div>
              <div className="employee-info">
                <p><strong>Employee:</strong> {viewingEmployeeRecords.name}</p>
                <p><strong>Employee No:</strong> {viewingEmployeeRecords.employeeNumber}</p>
              </div>
              <div className="employment-records-list">
                {viewingEmployeeRecords.employmentRecords?.length > 0 ? (
                  viewingEmployeeRecords.employmentRecords.map((record, index) => (
                    <div key={index} className="employment-record-card">
                      <div className="record-header">
                        <h4>{record.employmentType} - {record.position}</h4>
                        <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
                          <span className={record.isActive ? "badge-active" : "badge-archived"}>{record.isActive ? "Active" : "Inactive"}</span>
                          {!record.isActive && (
                            <button onClick={() => handleActivateRecord(record.employmentRecordId)} className="btn-primary" style={{ fontSize: "12px", padding: "4px 8px" }}>Activate</button>
                          )}
                          <button onClick={() => handleDeleteRecord(record.employmentRecordId)} className="cancel-btn" style={{ fontSize: "12px", padding: "4px 8px" }}>Delete</button>
                        </div>
                      </div>
                      <div className="record-details">
                        <p><strong>Daily Rate:</strong> MYR {record.dailyRate?.toFixed(2)}</p>
                        <p><strong>Start Date:</strong> {new Date(record.startDate).toLocaleDateString()}</p>
                        {record.endDate && <p><strong>End Date:</strong> {new Date(record.endDate).toLocaleDateString()}</p>}
                        <p><strong>Working Days:</strong> {record.workingDays?.map((wd) => daysOfWeek[wd.dayOfWeek]?.name || wd.dayName).join(", ") || "None"}</p>
                        {record.skillSets?.length > 0 && (
                          <div className="record-skills">
                            <strong>Skills:</strong>
                            <div className="skills-list">
                              {record.skillSets.map((skill, idx) => <span key={idx} className="skill-tag">{skill.skillName}</span>)}
                            </div>
                          </div>
                        )}
                      </div>
                    </div>
                  ))
                ) : (
                  <p className="no-records">No employment records found.</p>
                )}
              </div>
              <div className="modal-actions">
                <button type="button" className="cancel-btn" onClick={closeViewEmploymentModal}>Close</button>
              </div>
            </div>
          </div>
        )}
      </div>
    </MainLayout>
  );
};

export default Employee;