// src/components/Sidebar/Sidebar.jsx
import React, { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import "./Sidebar.css";
import aclogo from "../../assets/cdn-logo-2.jpg";
import ROUTES from "../../routes";

const Sidebar = () => {
  const location = useLocation();
  const [isOpen, setIsOpen] = useState(false);

  const items = [
    { to: ROUTES.DASHBOARD, label: "Dashboard" },
    { to: ROUTES.PAYROLL,   label: "Payroll"   },
    { to: ROUTES.EMPLOYEE,  label: "Employee"  },
  ];

  const toggleSidebar = () => setIsOpen((prev) => !prev);
  const closeSidebar  = () => setIsOpen(false);

  return (
    <>
      {/* Hamburger button – only visible on mobile (CSS controls display) */}
      <button
        className={`sidebar-toggle${isOpen ? " is-open" : ""}`}
        onClick={toggleSidebar}
        aria-label={isOpen ? "Close menu" : "Open menu"}
        aria-expanded={isOpen}
      >
        <span />
        <span />
        <span />
      </button>

      {/* Dim overlay – click to close */}
      {isOpen && (
        <div className="sidebar-overlay" onClick={closeSidebar} aria-hidden="true" />
      )}

      {/* Sidebar drawer */}
      <aside className={`sidebar${isOpen ? " sidebar-open" : ""}`}>
        <img
          src={aclogo}
          alt="CDN Logo"
          className="sidebar-logo-img"
        />

        <nav className="sidebar-nav" aria-label="Main navigation">
          <ul>
            {items.map((it) => (
              <li
                key={it.to}
                className={location.pathname === it.to ? "active" : ""}
              >
                <Link to={it.to} onClick={closeSidebar}>
                  {it.label}
                </Link>
              </li>
            ))}
          </ul>
        </nav>
      </aside>
    </>
  );
};

export default Sidebar;