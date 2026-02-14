import React, { useEffect, useState } from "react";
import EmployeeService from "../../services/EmployeeService";
import EmploymentRecordService from "../../services/EmploymentRecordService";
import MainLayout from "../../components/MainLayout/MainLayout";
import {
  FaEdit,
  FaTrash,
  FaArchive,
  FaUndo,
  FaMoneyBillWave,
  FaBriefcase,
  FaList,
  FaSearch,
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

const Employee = () => {
  const [employees, setEmployees] = useState([]);
  const [filteredEmployees, setFilteredEmployees] = useState([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isEmploymentModalOpen, setIsEmploymentModalOpen] = useState(false);
  const [isViewEmploymentModalOpen, setIsViewEmploymentModalOpen] = useState(false);
  const [editingEmployee, setEditingEmployee] = useState(null);
  const [selectedEmployeeForEmployment, setSelectedEmployeeForEmployment] = useState(null);
  const [viewingEmployeeRecords, setViewingEmployeeRecords] = useState(null);

  const [addForm, setAddForm] = useState({
    name: "",
    nationalNumber: "",
    contactNumber: "",
    position: "",
    address: "",
    dateOfBirth: "",
    employmentType: "Permanent",
    dailyRate: "",
    startDate: "",
    endDate: "",
    workingDays: [],
    skillSets: [],
  });

  const [editForm, setEditForm] = useState({
    name: "",
    nationalNumber: "",
    contactNumber: "",
    position: "",
    address: "",
    dateOfBirth: "",
  });

  const [employmentForm, setEmploymentForm] = useState({
    employmentType: "Permanent",
    position: "",
    dailyRate: "",
    startDate: "",
    endDate: "",
    workingDays: [],
    skillSets: [],
  });

  const [skillInput, setSkillInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [showArchived, setShowArchived] = useState(false);

  useEffect(() => {
    document.title = "HRMS - Employees";
    fetchEmployees();
  }, [showArchived]);

  // Auto-search with debouncing
  useEffect(() => {
    const delayDebounce = setTimeout(() => {
      handleSearch(searchQuery);
    }, 300); // 300ms delay after user stops typing

    return () => clearTimeout(delayDebounce);
  }, [searchQuery, employees]);

  const fetchEmployees = async () => {
    try {
      setLoading(true);
      const data = await EmployeeService.getAll(showArchived);
      setEmployees(Array.isArray(data) ? data : []);
      setFilteredEmployees(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error("❌ Fetch failed:", err);
      alert("Failed to load employees: " + err.message);
      setEmployees([]);
      setFilteredEmployees([]);
    } finally {
      setLoading(false);
    }
  };

  // Auto-search function (wildcard search)
  const handleSearch = (query) => {
    if (!query.trim()) {
      // If search is empty, show all employees
      setFilteredEmployees(employees);
      return;
    }

    const searchTerm = query.toLowerCase().trim();
    
    const filtered = employees.filter((emp) => {
      const employeeNumber = (emp.employeeNumber || "").toLowerCase();
      const name = (emp.name || "").toLowerCase();
      
      // Wildcard search: matches anywhere in the string
      return employeeNumber.includes(searchTerm) || name.includes(searchTerm);
    });

    setFilteredEmployees(filtered);
  };

  const handleSearchChange = (e) => {
    setSearchQuery(e.target.value);
  };

  const clearSearch = () => {
    setSearchQuery("");
    setFilteredEmployees(employees);
  };

  // ========== VIEW EMPLOYMENT RECORDS ==========
  const openViewEmploymentModal = async (employee) => {
    try {
      setLoading(true);
      const records = await EmploymentRecordService.getByEmployeeId(employee.employeeId);
      setViewingEmployeeRecords({ ...employee, employmentRecords: records });
      setIsViewEmploymentModalOpen(true);
    } catch (err) {
      console.error("❌ Failed to load employment records:", err);
      alert("Failed to load employment records: " + err.message);
    } finally {
      setLoading(false);
    }
  };

  const closeViewEmploymentModal = () => {
    setIsViewEmploymentModalOpen(false);
    setViewingEmployeeRecords(null);
  };

  const handleActivateRecord = async (recordId) => {
    if (!window.confirm("Activate this employment record? This will deactivate all other records.")) return;
    
    try {
      setLoading(true);
      await EmploymentRecordService.activate(recordId);
      alert("✅ Employment record activated successfully!");
      
      const records = await EmploymentRecordService.getByEmployeeId(viewingEmployeeRecords.employeeId);
      setViewingEmployeeRecords({ ...viewingEmployeeRecords, employmentRecords: records });
    } catch (err) {
      alert("❌ " + err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteRecord = async (recordId) => {
    if (!window.confirm("Delete this employment record? This action cannot be undone.")) return;
    
    try {
      setLoading(true);
      await EmploymentRecordService.delete(recordId);
      alert("✅ Employment record deleted successfully!");
      
      const records = await EmploymentRecordService.getByEmployeeId(viewingEmployeeRecords.employeeId);
      setViewingEmployeeRecords({ ...viewingEmployeeRecords, employmentRecords: records });
    } catch (err) {
      alert("❌ " + err.message);
    } finally {
      setLoading(false);
    }
  };

  // ========== ADD NEW EMPLOYEE ==========
  const openAddModal = () => {
    setAddForm({
      name: "",
      nationalNumber: "",
      contactNumber: "",
      position: "",
      address: "",
      dateOfBirth: "",
      employmentType: "Permanent",
      dailyRate: "",
      startDate: new Date().toISOString().split("T")[0],
      endDate: "",
      workingDays: [],
      skillSets: [],
    });
    setSkillInput("");
    setIsAddModalOpen(true);
  };

  const closeAddModal = () => {
    setIsAddModalOpen(false);
  };

  const handleAddChange = (e) => {
    setAddForm({ ...addForm, [e.target.name]: e.target.value });
  };

  const toggleAddWorkingDay = (dayValue) => {
    setAddForm((prev) => ({
      ...prev,
      workingDays: prev.workingDays.includes(dayValue)
        ? prev.workingDays.filter((d) => d !== dayValue)
        : [...prev.workingDays, dayValue],
    }));
  };

  const addSkillToAdd = () => {
    const skill = skillInput.trim();
    if (!skill) return;
    if (addForm.skillSets.includes(skill)) {
      alert("This skill is already added");
      return;
    }
    setAddForm((prev) => ({
      ...prev,
      skillSets: [...prev.skillSets, skill],
    }));
    setSkillInput("");
  };

  const removeSkillFromAdd = (skill) => {
    setAddForm((prev) => ({
      ...prev,
      skillSets: prev.skillSets.filter((s) => s !== skill),
    }));
  };

  const handleAddSubmit = async (e) => {
    e.preventDefault();

    if (!addForm.name || !addForm.nationalNumber || !addForm.dateOfBirth) {
      alert("Please fill in all required employee fields");
      return;
    }

    if (!addForm.dailyRate || addForm.workingDays.length === 0) {
      alert("Please enter daily rate and select at least one working day");
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
        position: addForm.position || "",
        startDate: addForm.startDate,
        endDate: addForm.endDate || null,
        dailyRate: parseFloat(addForm.dailyRate),
        workingDays: addForm.workingDays,
        skillSets: addForm.skillSets,
      };

      await EmploymentRecordService.create(employmentPayload);

      alert(`✅ Employee created successfully!\nEmployee Number: ${createdEmployee.employeeNumber}`);
      closeAddModal();
      await fetchEmployees();
    } catch (err) {
      console.error("❌ Creation failed:", err);
      alert("❌ Failed to create employee:\n" + err.message);
    } finally {
      setLoading(false);
    }
  };

  // ========== EDIT EMPLOYEE ==========
  const openEditModal = (employee) => {
    setEditingEmployee(employee);
    setEditForm({
      name: employee.name || "",
      nationalNumber: employee.nationalNumber || "",
      contactNumber: employee.contactNumber || "",
      position: employee.position || "",
      address: employee.address || "",
      dateOfBirth: employee.dateOfBirth?.split("T")[0] || "",
    });
    setIsEditModalOpen(true);
  };

  const closeEditModal = () => {
    setIsEditModalOpen(false);
    setEditingEmployee(null);
  };

  const handleEditChange = (e) => {
    setEditForm({ ...editForm, [e.target.name]: e.target.value });
  };

  const handleEditSubmit = async (e) => {
    e.preventDefault();

    const payload = {
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
      alert("✅ Employee updated successfully!");
      closeEditModal();
      await fetchEmployees();
    } catch (err) {
      console.error("❌ Update failed:", err);
      alert("❌ Failed to update employee:\n" + err.message);
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
    setSkillInput("");
    setIsEmploymentModalOpen(true);
  };

  const closeEmploymentModal = () => {
    setIsEmploymentModalOpen(false);
    setSelectedEmployeeForEmployment(null);
  };

  const handleEmploymentChange = (e) => {
    setEmploymentForm({ ...employmentForm, [e.target.name]: e.target.value });
  };

  const toggleEmploymentWorkingDay = (dayValue) => {
    setEmploymentForm((prev) => ({
      ...prev,
      workingDays: prev.workingDays.includes(dayValue)
        ? prev.workingDays.filter((d) => d !== dayValue)
        : [...prev.workingDays, dayValue],
    }));
  };

  const addSkillToEmployment = () => {
    const skill = skillInput.trim();
    if (!skill) return;
    if (employmentForm.skillSets.includes(skill)) {
      alert("This skill is already added");
      return;
    }
    setEmploymentForm((prev) => ({
      ...prev,
      skillSets: [...prev.skillSets, skill],
    }));
    setSkillInput("");
  };

  const removeSkillFromEmployment = (skill) => {
    setEmploymentForm((prev) => ({
      ...prev,
      skillSets: prev.skillSets.filter((s) => s !== skill),
    }));
  };

  const handleEmploymentSubmit = async (e) => {
    e.preventDefault();

    if (!employmentForm.dailyRate || employmentForm.workingDays.length === 0) {
      alert("Please enter daily rate and select at least one working day");
      return;
    }

    const payload = {
      employeeId: selectedEmployeeForEmployment.employeeId,
      employmentType: employmentForm.employmentType,
      position: employmentForm.position || selectedEmployeeForEmployment.position || "",
      startDate: employmentForm.startDate,
      endDate: employmentForm.endDate || null,
      dailyRate: parseFloat(employmentForm.dailyRate),
      workingDays: employmentForm.workingDays,
      skillSets: employmentForm.skillSets,
    };

    try {
      setLoading(true);
      await EmploymentRecordService.create(payload);
      alert("✅ Employment record created successfully!");
      closeEmploymentModal();
      await fetchEmployees();
    } catch (err) {
      console.error("❌ Employment record creation failed:", err);
      alert("❌ Failed to create employment record:\n" + err.message);
    } finally {
      setLoading(false);
    }
  };

  // ========== OTHER ACTIONS ==========
  const handleDelete = async (id) => {
    if (!window.confirm("Are you sure you want to delete this employee?")) return;
    try {
      setLoading(true);
      await EmployeeService.delete(id);
      alert("✅ Employee deleted successfully!");
      await fetchEmployees();
    } catch (err) {
      alert("❌ " + err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleArchive = async (id) => {
    if (!window.confirm("Archive this employee?")) return;
    try {
      setLoading(true);
      await EmployeeService.archive(id);
      alert("✅ Employee archived successfully!");
      if (!showArchived) {
        await fetchEmployees();
      }
    } catch (err) {
      alert("❌ " + err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleUnarchive = async (id) => {
    if (!window.confirm("Unarchive this employee?")) return;
    try {
      setLoading(true);
      await EmployeeService.unarchive(id);
      alert("✅ Employee unarchived successfully!");
      if (showArchived) {
        await fetchEmployees();
      }
    } catch (err) {
      alert("❌ " + err.message);
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
      alert(
        `💰 Salary Calculation\n\n` +
          `Employee: ${employee.name}\n` +
          `Period: ${start} to ${end}\n\n` +
          `Take Home Pay: ${result.currency} ${result.takeHomePay.toFixed(2)}`
      );
    } catch (err) {
      alert("❌ " + err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <MainLayout>
      <div className="employee-page">
        <div className="employee-header">
          <div>
            <h1>Employee Management</h1>
            <p>Manage employee records and employment details</p>
          </div>
          <div className="employee-header-actions">
            <button className="btn-primary" onClick={openAddModal} disabled={loading}>
              + Add Employee
            </button>
            <button
              className="btn-secondary"
              onClick={() => setShowArchived(!showArchived)}
              disabled={loading}
            >
              {showArchived ? "Active Only" : "Show Archived"}
            </button>
          </div>
        </div>

        {/* ========== SEARCH BAR ========== */}
        <div className="search-container">
          <div className="search-input-wrapper">
            <FaSearch className="search-icon" />
            <input
              type="text"
              className="search-input"
              placeholder="Search by employee number or name..."
              value={searchQuery}
              onChange={handleSearchChange}
              disabled={loading}
            />
            {searchQuery && (
              <button className="clear-search-btn" onClick={clearSearch}>
                ×
              </button>
            )}
          </div>
          {searchQuery && (
            <div className="search-results-info">
              Found {filteredEmployees.length} employee{filteredEmployees.length !== 1 ? 's' : ''} 
              {searchQuery && ` matching "${searchQuery}"`}
            </div>
          )}
        </div>

        {loading && <p className="loading-text">⏳ Loading...</p>}

        <table className="employee-table">
          <thead>
            <tr>
              <th>Employee No</th>
              <th>Name</th>
              <th>National ID</th>
              <th>Contact</th>
              <th>Position</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredEmployees.length === 0 ? (
              <tr>
                <td colSpan="7" style={{ textAlign: "center", padding: "20px" }}>
                  {loading 
                    ? "Loading..." 
                    : searchQuery 
                      ? `No employees found matching "${searchQuery}"`
                      : "No employees found"
                  }
                </td>
              </tr>
            ) : (
              filteredEmployees.map((emp) => (
                <tr key={emp.employeeId}>
                  <td>{emp.employeeNumber}</td>
                  <td>{emp.name}</td>
                  <td>{emp.nationalNumber}</td>
                  <td>{emp.contactNumber || "-"}</td>
                  <td>{emp.position || "-"}</td>
                  <td>
                    <span className={emp.isArchived ? "badge-archived" : "badge-active"}>
                      {emp.isArchived ? "Archived" : "Active"}
                    </span>
                  </td>
                  <td className="action-buttons">
                    {!emp.isArchived ? (
                      <>
                        <FaEdit
                          className="action-icon edit-icon"
                          onClick={() => openEditModal(emp)}
                          title="Edit Basic Info"
                        />
                        <FaList
                          className="action-icon list-icon"
                          onClick={() => openViewEmploymentModal(emp)}
                          title="View Employment Records"
                        />
                        <FaBriefcase
                          className="action-icon employment-icon"
                          onClick={() => openEmploymentModal(emp)}
                          title="Add Employment Record"
                        />
                        <FaMoneyBillWave
                          className="action-icon salary-icon"
                          onClick={() => handleCalculateSalary(emp)}
                          title="Calculate Salary"
                        />
                        <FaArchive
                          className="action-icon archive-icon"
                          onClick={() => handleArchive(emp.employeeId)}
                          title="Archive"
                        />
                        <FaTrash
                          className="action-icon delete-icon"
                          onClick={() => handleDelete(emp.employeeId)}
                          title="Delete"
                        />
                      </>
                    ) : (
                      <FaUndo
                        className="action-icon unarchive-icon"
                        onClick={() => handleUnarchive(emp.employeeId)}
                        title="Unarchive"
                      />
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>

        {/* ALL MODALS - Same as before, keeping them unchanged */}
        {/* ... (keeping all your existing modal code) ... */}

        {/* ========== ADD MODAL ========== */}
        {isAddModalOpen && (
          <div className="modal-overlay" onClick={closeAddModal}>
            <div className="modal large-modal" onClick={(e) => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Add New Employee</h2>
                <button className="x-btn" onClick={closeAddModal} disabled={loading}>×</button>
              </div>

              <form onSubmit={handleAddSubmit} className="modal-form">
                <div className="form-section">
                  <h3>Employee Information</h3>
                  <div className="form-row">
                    <input type="text" name="name" value={addForm.name} onChange={handleAddChange} placeholder="Full Name *" required />
                    <input type="text" name="nationalNumber" value={addForm.nationalNumber} onChange={handleAddChange} placeholder="National ID (e.g., 940110-01-5678) *" required />
                  </div>
                  <div className="form-row">
                    <input type="text" name="contactNumber" value={addForm.contactNumber} onChange={handleAddChange} placeholder="Contact (+60123456789)" />
                    <input type="text" name="position" value={addForm.position} onChange={handleAddChange} placeholder="Position (e.g., Developer)" />
                  </div>
                  <div className="form-row">
                    <input type="text" name="address" value={addForm.address} onChange={handleAddChange} placeholder="Residential Address" />
                    <input type="date" name="dateOfBirth" value={addForm.dateOfBirth} onChange={handleAddChange} placeholder="Date of Birth *" required />
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
                    <input type="date" name="endDate" value={addForm.endDate} onChange={handleAddChange} placeholder="End Date (Optional)" />
                  </div>

                  <div className="working-days-section">
                    <h4>Working Days * (Select at least one)</h4>
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
                      <input type="text" value={skillInput} onChange={(e) => setSkillInput(e.target.value)} placeholder="Enter skill (e.g., C#, ReactJs)" onKeyPress={(e) => e.key === "Enter" && (e.preventDefault(), addSkillToAdd())} />
                      <button type="button" onClick={addSkillToAdd} className="add-skill-btn">Add</button>
                    </div>
                    {addForm.skillSets.length > 0 && (
                      <div className="skills-list">
                        {addForm.skillSets.map((skill, idx) => (
                          <span key={idx} className="skill-tag">
                            {skill}
                            <button type="button" onClick={() => removeSkillFromAdd(skill)} className="remove-skill-btn">×</button>
                          </span>
                        ))}
                      </div>
                    )}
                  </div>
                </div>

                <div className="modal-actions">
                  <button type="submit" className="btn-primary" disabled={loading}>
                    {loading ? "Creating..." : "Create Employee"}
                  </button>
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
                <input type="text" name="name" value={editForm.name} onChange={handleEditChange} placeholder="Full Name *" required />
                <input type="text" name="nationalNumber" value={editForm.nationalNumber} onChange={handleEditChange} placeholder="National ID *" required />
                <input type="text" name="contactNumber" value={editForm.contactNumber} onChange={handleEditChange} placeholder="Contact Number" />
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
                  <input type="date" name="endDate" value={employmentForm.endDate} onChange={handleEmploymentChange} placeholder="End Date (Optional)" />
                </div>
                <div className="working-days-section">
                  <h4>Working Days * (Select at least one)</h4>
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
                        <span key={idx} className="skill-tag">
                          {skill}
                          <button type="button" onClick={() => removeSkillFromEmployment(skill)} className="remove-skill-btn">×</button>
                        </span>
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
                {viewingEmployeeRecords.employmentRecords && viewingEmployeeRecords.employmentRecords.length > 0 ? (
                  viewingEmployeeRecords.employmentRecords.map((record, index) => (
                    <div key={index} className="employment-record-card">
                      <div className="record-header">
                        <h4>{record.employmentType} - {record.position}</h4>
                        <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                          <span className={record.isActive ? "badge-active" : "badge-archived"}>
                            {record.isActive ? "Active" : "Inactive"}
                          </span>
                          {!record.isActive && (
                            <button onClick={() => handleActivateRecord(record.employmentRecordId)} className="btn-primary" style={{ fontSize: '12px', padding: '4px 8px' }}>
                              Activate
                            </button>
                          )}
                          <button onClick={() => handleDeleteRecord(record.employmentRecordId)} className="cancel-btn" style={{ fontSize: '12px', padding: '4px 8px' }}>
                            Delete
                          </button>
                        </div>
                      </div>
                      <div className="record-details">
                        <p><strong>Daily Rate:</strong> MYR {record.dailyRate?.toFixed(2)}</p>
                        <p><strong>Start Date:</strong> {new Date(record.startDate).toLocaleDateString()}</p>
                        {record.endDate && <p><strong>End Date:</strong> {new Date(record.endDate).toLocaleDateString()}</p>}
                        <p><strong>Working Days:</strong> {record.workingDays?.map(wd => daysOfWeek[wd.dayOfWeek]?.name || wd.dayName).join(", ") || "None"}</p>
                        {record.skillSets && record.skillSets.length > 0 && (
                          <div className="record-skills">
                            <strong>Skills:</strong>
                            <div className="skills-list">
                              {record.skillSets.map((skill, idx) => (
                                <span key={idx} className="skill-tag">{skill.skillName}</span>
                              ))}
                            </div>
                          </div>
                        )}
                      </div>
                    </div>
                  ))
                ) : (
                  <p className="no-records">No employment records found for this employee.</p>
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