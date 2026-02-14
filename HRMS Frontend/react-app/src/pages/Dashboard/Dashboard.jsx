import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import EmployeeService from "../../services/EmployeeService";
import MainLayout from "../../components/MainLayout/MainLayout";
import ROUTES from "../../routes";
import {
  FaUsers,
  FaUserTie,
  FaMoneyBillWave,
  FaBriefcase,
  FaCalendarAlt,
  FaChartLine,
  FaArrowUp,
  FaArrowDown,
  FaClock,
  FaBirthdayCake,
} from "react-icons/fa";
import "./Dashboard.css";

const Dashboard = () => {
  const navigate = useNavigate();
  const [stats, setStats] = useState({
    totalEmployees: 0,
    activeEmployees: 0,
    archivedEmployees: 0,
    newThisMonth: 0,
  });
  const [recentEmployees, setRecentEmployees] = useState([]);
  const [upcomingBirthdays, setUpcomingBirthdays] = useState([]);
  const [loading, setLoading] = useState(true);
  const [currentTime, setCurrentTime] = useState(new Date());

  useEffect(() => {
    document.title = "HRMS - Dashboard";
    fetchDashboardData();

    const timer = setInterval(() => {
      setCurrentTime(new Date());
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      
      const activeEmployees = await EmployeeService.getAll(false);
      const archivedEmployees = await EmployeeService.getAll(true);
      const archived = archivedEmployees.filter(emp => emp.isArchived);

      const now = new Date();
      const firstDayOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
      
      const newThisMonth = activeEmployees.filter(emp => {
        const createdDate = new Date(emp.dateCreated);
        return createdDate >= firstDayOfMonth;
      });

      setStats({
        totalEmployees: activeEmployees.length + archived.length,
        activeEmployees: activeEmployees.length,
        archivedEmployees: archived.length,
        newThisMonth: newThisMonth.length,
      });

      const sortedByDate = [...activeEmployees].sort((a, b) => 
        new Date(b.dateCreated) - new Date(a.dateCreated)
      );
      setRecentEmployees(sortedByDate.slice(0, 5));

      const upcomingBdays = getUpcomingBirthdays(activeEmployees, 30);
      setUpcomingBirthdays(upcomingBdays);

    } catch (err) {
      console.error("❌ Failed to fetch dashboard data:", err);
    } finally {
      setLoading(false);
    }
  };

  const getUpcomingBirthdays = (employees, daysAhead) => {
    const today = new Date();
    const futureDate = new Date();
    futureDate.setDate(today.getDate() + daysAhead);

    return employees
      .filter(emp => emp.dateOfBirth)
      .map(emp => {
        const dob = new Date(emp.dateOfBirth);
        const thisYearBirthday = new Date(today.getFullYear(), dob.getMonth(), dob.getDate());
        
        if (thisYearBirthday < today) {
          thisYearBirthday.setFullYear(today.getFullYear() + 1);
        }

        const daysUntil = Math.ceil((thisYearBirthday - today) / (1000 * 60 * 60 * 24));

        return {
          ...emp,
          nextBirthday: thisYearBirthday,
          daysUntil: daysUntil,
          age: today.getFullYear() - dob.getFullYear(),
        };
      })
      .filter(emp => emp.daysUntil <= daysAhead)
      .sort((a, b) => a.daysUntil - b.daysUntil)
      .slice(0, 5);
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('en-MY', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  const formatTime = (date) => {
    return date.toLocaleTimeString('en-MY', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  };

  const getGreeting = () => {
    const hour = currentTime.getHours();
    if (hour < 12) return "Good Morning";
    if (hour < 18) return "Good Afternoon";
    return "Good Evening";
  };

  const username = localStorage.getItem("username") || "User";

  return (
    <MainLayout>
      <div className="dashboard-page">
        <div className="dashboard-header">
          <div className="header-left">
            <h1>{getGreeting()}, {username}! 👋</h1>
            <p>Welcome back to HRMS Payroll System</p>
          </div>
          <div className="header-right">
            <div className="current-datetime">
              <FaCalendarAlt className="datetime-icon" />
              <div className="datetime-content">
                <div className="current-date">{currentTime.toLocaleDateString('en-MY', { 
                  weekday: 'long', 
                  year: 'numeric', 
                  month: 'long', 
                  day: 'numeric' 
                })}</div>
                <div className="current-time">{formatTime(currentTime)}</div>
              </div>
            </div>
          </div>
        </div>

        {/* Statistics Cards */}
        <div className="stats-grid">
          <div className="stat-card blue" onClick={() => navigate(ROUTES.EMPLOYEE)}>
            <div className="stat-icon-wrapper blue-bg">
              <FaUsers className="stat-icon" />
            </div>
            <div className="stat-content">
              <div className="stat-label">Total Employees</div>
              <div className="stat-value">{loading ? "..." : stats.totalEmployees}</div>
              <div className="stat-change positive">
                <FaArrowUp /> {stats.newThisMonth} new this month
              </div>
            </div>
          </div>

          <div className="stat-card green" onClick={() => navigate(ROUTES.EMPLOYEE)}>
            <div className="stat-icon-wrapper green-bg">
              <FaUserTie className="stat-icon" />
            </div>
            <div className="stat-content">
              <div className="stat-label">Active Employees</div>
              <div className="stat-value">{loading ? "..." : stats.activeEmployees}</div>
              <div className="stat-change neutral">
                <FaClock /> Currently working
              </div>
            </div>
          </div>

          <div className="stat-card purple" onClick={() => navigate(ROUTES.PAYROLL)}>
            <div className="stat-icon-wrapper purple-bg">
              <FaMoneyBillWave className="stat-icon" />
            </div>
            <div className="stat-content">
              <div className="stat-label">Payroll</div>
              <div className="stat-value">Ready</div>
              <div className="stat-change neutral">
                <FaChartLine /> Calculate salary
              </div>
            </div>
          </div>

          <div className="stat-card orange" onClick={() => navigate(ROUTES.EMPLOYEE)}>
            <div className="stat-icon-wrapper orange-bg">
              <FaBriefcase className="stat-icon" />
            </div>
            <div className="stat-content">
              <div className="stat-label">Archived</div>
              <div className="stat-value">{loading ? "..." : stats.archivedEmployees}</div>
              <div className="stat-change negative">
                <FaArrowDown /> Inactive employees
              </div>
            </div>
          </div>
        </div>

        {/* Main Content Grid */}
        <div className="dashboard-content">
          {/* Recent Employees */}
          <div className="content-card">
            <div className="card-header">
              <div className="card-title">
                <FaClock className="card-title-icon" />
                <h2>Recent Employees</h2>
              </div>
              <button 
                className="view-all-btn"
                onClick={() => navigate(ROUTES.EMPLOYEE)}
              >
                View All →
              </button>
            </div>
            <div className="card-body">
              {loading ? (
                <div className="loading-state">Loading...</div>
              ) : recentEmployees.length === 0 ? (
                <div className="empty-state">
                  <FaUsers className="empty-icon" />
                  <p>No employees yet</p>
                </div>
              ) : (
                <div className="employee-list">
                  {recentEmployees.map((emp) => (
                    <div 
                      key={emp.employeeId} 
                      className="employee-item"
                      onClick={() => navigate(ROUTES.EMPLOYEE)}
                    >
                      <div className="employee-avatar">
                        {emp.name.charAt(0).toUpperCase()}
                      </div>
                      <div className="employee-info">
                        <div className="employee-name">{emp.name}</div>
                        <div className="employee-meta">
                          {emp.employeeNumber} • {emp.position || "N/A"}
                        </div>
                      </div>
                      <div className="employee-date">
                        {formatDate(emp.dateCreated)}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Upcoming Birthdays */}
          <div className="content-card">
            <div className="card-header">
              <div className="card-title">
                <FaBirthdayCake className="card-title-icon birthday" />
                <h2>Upcoming Birthdays</h2>
              </div>
              <span className="badge">Next 30 Days</span>
            </div>
            <div className="card-body">
              {loading ? (
                <div className="loading-state">Loading...</div>
              ) : upcomingBirthdays.length === 0 ? (
                <div className="empty-state">
                  <FaBirthdayCake className="empty-icon" />
                  <p>No birthdays in the next 30 days</p>
                </div>
              ) : (
                <div className="birthday-list">
                  {upcomingBirthdays.map((emp) => (
                    <div key={emp.employeeId} className="birthday-item">
                      <div className="birthday-icon-wrapper">
                        <FaBirthdayCake className="birthday-icon" />
                      </div>
                      <div className="birthday-info">
                        <div className="birthday-name">{emp.name}</div>
                        <div className="birthday-meta">
                          {formatDate(emp.nextBirthday)} • Turning {emp.age + 1}
                        </div>
                      </div>
                      <div className="birthday-countdown">
                        {emp.daysUntil === 0 ? (
                          <span className="today-badge">Today! 🎉</span>
                        ) : emp.daysUntil === 1 ? (
                          <span className="tomorrow-badge">Tomorrow</span>
                        ) : (
                          <span className="days-badge">{emp.daysUntil} days</span>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Quick Actions */}
          <div className="content-card quick-actions-card">
            <div className="card-header">
              <div className="card-title">
                <FaChartLine className="card-title-icon" />
                <h2>Quick Actions</h2>
              </div>
            </div>
            <div className="card-body">
              <div className="quick-actions-grid">
                <button 
                  className="quick-action-btn blue"
                  onClick={() => navigate(ROUTES.EMPLOYEE)}
                >
                  <FaUsers className="quick-action-icon" />
                  <span>Add Employee</span>
                </button>
                <button 
                  className="quick-action-btn green"
                  onClick={() => navigate(ROUTES.PAYROLL)}
                >
                  <FaMoneyBillWave className="quick-action-icon" />
                  <span>Calculate Salary</span>
                </button>
                <button 
                  className="quick-action-btn purple"
                  onClick={() => navigate(ROUTES.EMPLOYEE)}
                >
                  <FaBriefcase className="quick-action-icon" />
                  <span>Manage Records</span>
                </button>
                <button 
                  className="quick-action-btn orange"
                  onClick={() => navigate(ROUTES.EMPLOYEE)}
                >
                  <FaUserTie className="quick-action-icon" />
                  <span>View All</span>
                </button>
              </div>
            </div>
          </div>

          {/* System Info */}
          <div className="content-card system-info-card">
            <div className="card-header">
              <div className="card-title">
                <FaChartLine className="card-title-icon" />
                <h2>System Information</h2>
              </div>
            </div>
            <div className="card-body">
              <div className="system-info-list">
                <div className="system-info-item">
                  <span className="info-label">System:</span>
                  <span className="info-value">HRMS Payroll System</span>
                </div>
                <div className="system-info-item">
                  <span className="info-label">Version:</span>
                  <span className="info-value">1.0.0</span>
                </div>
                <div className="system-info-item">
                  <span className="info-label">Status:</span>
                  <span className="status-badge active">Active</span>
                </div>
                <div className="system-info-item">
                  <span className="info-label">Last Updated:</span>
                  <span className="info-value">{formatDate(new Date())}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default Dashboard;