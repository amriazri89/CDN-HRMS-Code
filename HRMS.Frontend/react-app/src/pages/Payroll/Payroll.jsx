import React, { useEffect, useState } from "react";
import EmployeeService from "../../services/EmployeeService";
import MainLayout from "../../components/MainLayout/MainLayout";
import { FaCalculator, FaCalendarAlt, FaMoneyBillWave, FaUser, FaBirthdayCake } from "react-icons/fa";
import "./Payroll.scss";

const Payroll = () => {
  const [employees, setEmployees] = useState([]);
  const [selectedEmployee, setSelectedEmployee] = useState("");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [loading, setLoading] = useState(false);
  const [calculationResult, setCalculationResult] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    document.title = "HRMS - Payroll";
    fetchEmployees();
    
    // Set default dates (current month)
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);
    
    setStartDate(firstDay.toISOString().split('T')[0]);
    setEndDate(lastDay.toISOString().split('T')[0]);
  }, []);

  const fetchEmployees = async () => {
    try {
      setLoading(true);
      const data = await EmployeeService.getAll(false); // Only active employees
      setEmployees(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error("❌ Fetch failed:", err);
      setError("Failed to load employees: " + err.message);
      setEmployees([]);
    } finally {
      setLoading(false);
    }
  };

  const handleCalculate = async (e) => {
    e.preventDefault();
    
    // Validation
    if (!selectedEmployee) {
      setError("Please select an employee");
      return;
    }

    if (!startDate || !endDate) {
      setError("Please select both start and end dates");
      return;
    }

    if (new Date(startDate) > new Date(endDate)) {
      setError("Start date must be before end date");
      return;
    }

    try {
      setLoading(true);
      setError("");
      setCalculationResult(null);

      const result = await EmployeeService.calculateSalary(
        selectedEmployee,
        startDate,
        endDate
      );

      setCalculationResult(result);
    } catch (err) {
      console.error("❌ Calculation failed:", err);
      setError(err.message || "Failed to calculate salary");
      setCalculationResult(null);
    } finally {
      setLoading(false);
    }
  };

  const handleReset = () => {
    setSelectedEmployee("");
    setCalculationResult(null);
    setError("");
    
    // Reset to current month
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);
    
    setStartDate(firstDay.toISOString().split('T')[0]);
    setEndDate(lastDay.toISOString().split('T')[0]);
  };

  const getSelectedEmployeeDetails = () => {
    return employees.find(emp => emp.employeeId === selectedEmployee);
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('en-MY', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('en-MY', {
      style: 'currency',
      currency: 'MYR',
      minimumFractionDigits: 2
    }).format(amount);
  };

  const calculateDaysDifference = () => {
    if (!startDate || !endDate) return 0;
    const start = new Date(startDate);
    const end = new Date(endDate);
    const diffTime = Math.abs(end - start);
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
    return diffDays;
  };

  return (
    <MainLayout>
      <div className="payroll-page">
        <div className="payroll-header">
          <div className="payroll-header-content">
            <FaMoneyBillWave className="payroll-header-icon" />
            <div>
              <h1>Payroll Calculation</h1>
              <p>Calculate employee salary for a specific period</p>
            </div>
          </div>
        </div>

        <div className="payroll-container">
          {/* Calculation Form */}
          <div className="payroll-form-card">
            <div className="card-header">
              <FaCalculator className="card-icon" />
              <h2>Salary Calculator</h2>
            </div>

            <form onSubmit={handleCalculate} className="payroll-form">
              {/* Employee Selection */}
              <div className="form-group">
                <label>
                  <FaUser className="label-icon" />
                  Select Employee *
                </label>
                <select
                  value={selectedEmployee}
                  onChange={(e) => setSelectedEmployee(e.target.value)}
                  required
                  disabled={loading}
                  className="select-input"
                >
                  <option value="">-- Choose Employee --</option>
                  {employees.map((emp) => (
                    <option key={emp.employeeId} value={emp.employeeId}>
                      {emp.employeeNumber} - {emp.name} ({emp.position || "N/A"})
                    </option>
                  ))}
                </select>
              </div>

              {/* Selected Employee Info */}
              {selectedEmployee && getSelectedEmployeeDetails() && (
                <div className="employee-info-box">
                  <div className="employee-info-row">
                    <span className="info-label">Employee Number:</span>
                    <span className="info-value">{getSelectedEmployeeDetails().employeeNumber}</span>
                  </div>
                  <div className="employee-info-row">
                    <span className="info-label">Name:</span>
                    <span className="info-value">{getSelectedEmployeeDetails().name}</span>
                  </div>
                  <div className="employee-info-row">
                    <span className="info-label">Position:</span>
                    <span className="info-value">{getSelectedEmployeeDetails().position || "N/A"}</span>
                  </div>
                  {getSelectedEmployeeDetails().dateOfBirth && (
                    <div className="employee-info-row">
                      <span className="info-label">
                        <FaBirthdayCake className="birthday-icon" />
                        Date of Birth:
                      </span>
                      <span className="info-value">
                        {formatDate(getSelectedEmployeeDetails().dateOfBirth)}
                      </span>
                    </div>
                  )}
                </div>
              )}

              {/* Date Range */}
              <div className="date-range-group">
                <div className="form-group">
                  <label>
                    <FaCalendarAlt className="label-icon" />
                    Start Date *
                  </label>
                  <input
                    type="date"
                    value={startDate}
                    onChange={(e) => setStartDate(e.target.value)}
                    required
                    disabled={loading}
                    className="date-input"
                  />
                </div>

                <div className="form-group">
                  <label>
                    <FaCalendarAlt className="label-icon" />
                    End Date *
                  </label>
                  <input
                    type="date"
                    value={endDate}
                    onChange={(e) => setEndDate(e.target.value)}
                    required
                    disabled={loading}
                    className="date-input"
                  />
                </div>
              </div>

              {/* Period Info */}
              {startDate && endDate && (
                <div className="period-info">
                  <FaCalendarAlt className="period-icon" />
                  <span>
                    Period: {calculateDaysDifference()} day(s) 
                    ({formatDate(startDate)} - {formatDate(endDate)})
                  </span>
                </div>
              )}

              {/* Error Message */}
              {error && (
                <div className="error-message">
                  <span>⚠️ {error}</span>
                </div>
              )}

              {/* Action Buttons */}
              <div className="form-actions">
                <button
                  type="submit"
                  className="btn-calculate"
                  disabled={loading}
                >
                  {loading ? (
                    <>⏳ Calculating...</>
                  ) : (
                    <>
                      <FaCalculator /> Calculate Salary
                    </>
                  )}
                </button>
                <button
                  type="button"
                  className="btn-reset"
                  onClick={handleReset}
                  disabled={loading}
                >
                  Reset
                </button>
              </div>
            </form>
          </div>

          {/* Calculation Result */}
          {calculationResult && (
            <div className="payroll-result-card">
              <div className="result-header">
                <FaMoneyBillWave className="result-icon" />
                <h2>Salary Calculation Result</h2>
              </div>

              <div className="result-content">
                {/* Employee Details */}
                <div className="result-section">
                  <h3>Employee Information</h3>
                  <div className="result-detail-row">
                    <span className="detail-label">Employee ID:</span>
                    <span className="detail-value">{getSelectedEmployeeDetails()?.employeeNumber}</span>
                  </div>
                  <div className="result-detail-row">
                    <span className="detail-label">Name:</span>
                    <span className="detail-value">{getSelectedEmployeeDetails()?.name}</span>
                  </div>
                  <div className="result-detail-row">
                    <span className="detail-label">Position:</span>
                    <span className="detail-value">{getSelectedEmployeeDetails()?.position || "N/A"}</span>
                  </div>
                </div>

                {/* Period Details */}
                <div className="result-section">
                  <h3>Calculation Period</h3>
                  <div className="result-detail-row">
                    <span className="detail-label">Start Date:</span>
                    <span className="detail-value">{formatDate(calculationResult.startDate)}</span>
                  </div>
                  <div className="result-detail-row">
                    <span className="detail-label">End Date:</span>
                    <span className="detail-value">{formatDate(calculationResult.endDate)}</span>
                  </div>
                  <div className="result-detail-row">
                    <span className="detail-label">Total Days:</span>
                    <span className="detail-value">{calculateDaysDifference()} day(s)</span>
                  </div>
                </div>

                {/* Salary Breakdown */}
                <div className="result-section salary-breakdown">
                  <h3>Salary Breakdown</h3>
                  <div className="breakdown-info">
                    <p>✓ Working days are paid at <strong>2× daily rate</strong></p>
                    <p>✓ Birthday bonus: <strong>1× daily rate</strong> (if birthday falls in period)</p>
                  </div>
                </div>

                {/* Total Amount */}
                <div className="result-total">
                  <div className="total-label">Total Take Home Pay</div>
                  <div className="total-amount">
                    {formatCurrency(calculationResult.takeHomePay)}
                  </div>
                  <div className="total-currency">({calculationResult.currency})</div>
                </div>

                {/* Print/Download Actions */}
                <div className="result-actions">
                  <button
                    className="btn-print"
                    onClick={() => window.print()}
                  >
                    🖨️ Print
                  </button>
                  <button
                    className="btn-calculate-another"
                    onClick={handleReset}
                  >
                    Calculate Another
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Info Card */}
          {!calculationResult && (
            <div className="info-card">
              <div className="info-header">
                <FaCalculator className="info-icon" />
                <h3>How Salary Calculation Works</h3>
              </div>
              <div className="info-content">
                <div className="info-item">
                  <div className="info-number">1</div>
                  <div className="info-text">
                    <h4>Working Days Payment</h4>
                    <p>Each working day is paid at <strong>2× the daily rate</strong></p>
                  </div>
                </div>
                <div className="info-item">
                  <div className="info-number">2</div>
                  <div className="info-text">
                    <h4>Birthday Bonus</h4>
                    <p>If the employee's birthday falls within the period, they receive an additional <strong>1× daily rate</strong> bonus</p>
                  </div>
                </div>
                <div className="info-item">
                  <div className="info-number">3</div>
                  <div className="info-text">
                    <h4>Example Calculation</h4>
                    <p>
                      Daily Rate: RM 150<br />
                      Working Days: Monday, Wednesday, Friday<br />
                      Period: May 13-16, 2025<br />
                      <br />
                      May 13 (Mon): RM 150 × 2 = RM 300<br />
                      May 14 (Wed): RM 150 × 2 = RM 300<br />
                      May 16 (Fri): RM 150 × 2 = RM 300<br />
                      <br />
                      <strong>Total: RM 900</strong>
                    </p>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </MainLayout>
  );
};

export default Payroll;