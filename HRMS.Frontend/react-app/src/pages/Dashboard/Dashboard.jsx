// src/pages/Dashboard/Dashboard.jsx
import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import EmployeeService from "../../services/EmployeeService";
import MainLayout from "../../components/MainLayout/MainLayout";
import ROUTES from "../../routes";
import {
  FaUsers, FaUserTie, FaMoneyBillWave, FaBriefcase,
  FaCalendarAlt, FaChartLine, FaArrowUp, FaArrowDown,
  FaClock, FaBirthdayCake,
} from "react-icons/fa";
import "./Dashboard.css";

const Dashboard = () => {
  const navigate = useNavigate();
  const [stats, setStats] = useState({
    total: 0, active: 0, archived: 0, newThisMonth: 0,
  });
  const [recent, setRecent]       = useState([]);
  const [birthdays, setBirthdays] = useState([]);
  const [loading, setLoading]     = useState(true);
  const [time, setTime]           = useState(new Date());

  useEffect(() => {
    document.title = "HRMS - Dashboard";
    load();
    const t = setInterval(() => setTime(new Date()), 1000);
    return () => clearInterval(t);
  }, []);

  const load = async () => {
    try {
      const active   = await EmployeeService.getAll(false);
      const archived = (await EmployeeService.getAll(true)).filter(e => e.isArchived);
      const first    = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
      const newMonth = active.filter(e => new Date(e.dateCreated) >= first);

      setStats({ total: active.length + archived.length, active: active.length, archived: archived.length, newThisMonth: newMonth.length });

      const sorted = [...active].sort((a, b) => new Date(b.dateCreated) - new Date(a.dateCreated));
      setRecent(sorted.slice(0, 5));
      setBirthdays(upcomingBdays(active, 30));
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  const upcomingBdays = (employees, days) => {
    const today = new Date();
    return employees
      .filter(e => e.dateOfBirth)
      .map(e => {
        const dob  = new Date(e.dateOfBirth);
        const next = new Date(today.getFullYear(), dob.getMonth(), dob.getDate());
        if (next < today) next.setFullYear(today.getFullYear() + 1);
        const daysUntil = Math.ceil((next - today) / 86400000);
        return { ...e, next, daysUntil, turnsAge: today.getFullYear() - dob.getFullYear() + 1 };
      })
      .filter(e => e.daysUntil <= days)
      .sort((a, b) => a.daysUntil - b.daysUntil)
      .slice(0, 5);
  };

  const fmtDate = d => new Date(d).toLocaleDateString("en-MY", { day: "numeric", month: "short", year: "numeric" });
  const fmtTime = d => d.toLocaleTimeString("en-MY", { hour: "2-digit", minute: "2-digit", second: "2-digit" });

  const greeting = () => {
    const h = time.getHours();
    return h < 12 ? "Good Morning" : h < 18 ? "Good Afternoon" : "Good Evening";
  };

  const user = localStorage.getItem("username") || "User";

  return (
    <MainLayout>
      <div className="db-page">

        {/* HEADER */}
        <div className="db-header">
          <div className="db-header-left">
            <h1>{greeting()}, {user}! 👋</h1>
            <p>Welcome back to HRMS Payroll System</p>
          </div>
          <div className="db-clock">
            <FaCalendarAlt className="db-clock-icon" />
            <div>
              <div className="db-clock-date">
                {time.toLocaleDateString("en-MY", { weekday: "long", year: "numeric", month: "long", day: "numeric" })}
              </div>
              <div className="db-clock-time">{fmtTime(time)}</div>
            </div>
          </div>
        </div>

        {/* STATS */}
        <div className="db-stats">
          <div className="db-stat db-stat-blue" onClick={() => navigate(ROUTES.EMPLOYEE)}>
            <div className="db-stat-icon db-stat-icon-blue"><FaUsers /></div>
            <div className="db-stat-body">
              <div className="db-stat-label">Total Employees</div>
              <div className="db-stat-value">{loading ? "…" : stats.total}</div>
              <div className="db-stat-note db-stat-note-pos"><FaArrowUp /> {stats.newThisMonth} new this month</div>
            </div>
          </div>

          <div className="db-stat db-stat-green" onClick={() => navigate(ROUTES.EMPLOYEE)}>
            <div className="db-stat-icon db-stat-icon-green"><FaUserTie /></div>
            <div className="db-stat-body">
              <div className="db-stat-label">Active Employees</div>
              <div className="db-stat-value">{loading ? "…" : stats.active}</div>
              <div className="db-stat-note db-stat-note-neu"><FaClock /> Currently working</div>
            </div>
          </div>

          <div className="db-stat db-stat-purple" onClick={() => navigate(ROUTES.PAYROLL)}>
            <div className="db-stat-icon db-stat-icon-purple"><FaMoneyBillWave /></div>
            <div className="db-stat-body">
              <div className="db-stat-label">Payroll</div>
              <div className="db-stat-value">Ready</div>
              <div className="db-stat-note db-stat-note-neu"><FaChartLine /> Calculate salary</div>
            </div>
          </div>

          <div className="db-stat db-stat-orange" onClick={() => navigate(ROUTES.EMPLOYEE)}>
            <div className="db-stat-icon db-stat-icon-orange"><FaBriefcase /></div>
            <div className="db-stat-body">
              <div className="db-stat-label">Archived</div>
              <div className="db-stat-value">{loading ? "…" : stats.archived}</div>
              <div className="db-stat-note db-stat-note-neg"><FaArrowDown /> Inactive employees</div>
            </div>
          </div>
        </div>

        {/* GRID */}
        <div className="db-grid">

          {/* Recent Employees */}
          <div className="db-card">
            <div className="db-card-head">
              <h2 className="db-card-title">
                <FaClock className="db-card-icon" /> Recent Employees
              </h2>
              <button className="db-view-all" onClick={() => navigate(ROUTES.EMPLOYEE)}>View All →</button>
            </div>

            {loading ? (
              <div className="db-empty"><FaUsers className="db-empty-icon" /><span>Loading…</span></div>
            ) : recent.length === 0 ? (
              <div className="db-empty"><FaUsers className="db-empty-icon" /><span>No employees yet</span></div>
            ) : (
              <div className="db-emp-list">
                {recent.map(emp => (
                  <div key={emp.employeeId} className="db-emp-row" onClick={() => navigate(ROUTES.EMPLOYEE)}>
                    <div className="db-emp-avatar">{emp.name.charAt(0).toUpperCase()}</div>
                    <div className="db-emp-body">
                      <div className="db-emp-name">{emp.name}</div>
                      <div className="db-emp-meta">{emp.employeeNumber} · {emp.position || "N/A"}</div>
                    </div>
                    <div className="db-emp-date">{fmtDate(emp.dateCreated)}</div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Upcoming Birthdays */}
          <div className="db-card">
            <div className="db-card-head">
              <h2 className="db-card-title">
                <FaBirthdayCake className="db-card-icon-bday" /> Upcoming Birthdays
              </h2>
              <span className="db-badge">Next 30 Days</span>
            </div>

            {loading ? (
              <div className="db-empty"><FaBirthdayCake className="db-empty-icon" /><span>Loading…</span></div>
            ) : birthdays.length === 0 ? (
              <div className="db-empty"><FaBirthdayCake className="db-empty-icon" /><span>No birthdays in the next 30 days</span></div>
            ) : (
              <div className="db-bday-list">
                {birthdays.map(emp => (
                  <div key={emp.employeeId} className="db-bday-row">
                    <div className="db-bday-icon"><FaBirthdayCake /></div>
                    <div className="db-bday-body">
                      <div className="db-bday-name">{emp.name}</div>
                      <div className="db-bday-meta">{fmtDate(emp.next)} · Turning {emp.turnsAge}</div>
                    </div>
                    <div>
                      {emp.daysUntil === 0
                        ? <span className="db-bday-tag-today">Today! 🎉</span>
                        : emp.daysUntil === 1
                          ? <span className="db-bday-tag-tomorrow">Tomorrow</span>
                          : <span className="db-bday-tag-days">{emp.daysUntil}d</span>}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Quick Actions */}
          <div className="db-card">
            <div className="db-card-head">
              <h2 className="db-card-title"><FaChartLine className="db-card-icon" /> Quick Actions</h2>
            </div>
            <div className="db-actions">
              <button className="db-action-btn db-action-blue"   onClick={() => navigate(ROUTES.EMPLOYEE)}>
                <FaUsers className="db-action-icon" /> Add Employee
              </button>
              <button className="db-action-btn db-action-green"  onClick={() => navigate(ROUTES.PAYROLL)}>
                <FaMoneyBillWave className="db-action-icon" /> Calculate Salary
              </button>
              <button className="db-action-btn db-action-purple" onClick={() => navigate(ROUTES.EMPLOYEE)}>
                <FaBriefcase className="db-action-icon" /> Manage Records
              </button>
              <button className="db-action-btn db-action-orange" onClick={() => navigate(ROUTES.EMPLOYEE)}>
                <FaUserTie className="db-action-icon" /> View All
              </button>
            </div>
          </div>

          {/* System Info */}
          <div className="db-card">
            <div className="db-card-head">
              <h2 className="db-card-title"><FaChartLine className="db-card-icon" /> System Information</h2>
            </div>
            <div className="db-sysinfo">
              <div className="db-sysinfo-row">
                <span className="db-sysinfo-label">System</span>
                <span className="db-sysinfo-value">HRMS Payroll System</span>
              </div>
              <div className="db-sysinfo-row">
                <span className="db-sysinfo-label">Version</span>
                <span className="db-sysinfo-value">1.0.0</span>
              </div>
              <div className="db-sysinfo-row">
                <span className="db-sysinfo-label">Status</span>
                <span className="db-sysinfo-active">Active</span>
              </div>
              <div className="db-sysinfo-row">
                <span className="db-sysinfo-label">Last Updated</span>
                <span className="db-sysinfo-value">{fmtDate(new Date())}</span>
              </div>
            </div>
          </div>

        </div>
      </div>
    </MainLayout>
  );
};

export default Dashboard;