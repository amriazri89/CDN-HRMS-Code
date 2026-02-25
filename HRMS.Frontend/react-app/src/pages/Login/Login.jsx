// src/pages/Login/Login.jsx
import React, { useEffect, useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import "./Login.scss";
import aclogo from "../../assets/cdn-logo.jpg";
import AuthService from "../../services/AuthService.js";
import ROUTES from "../../routes";

const Login = () => {
  useEffect(() => {
    document.title = "CDN HRMS – Login";
  }, []);

  const [formData, setFormData]     = useState({ username: "", password: "" });
  const [message, setMessage]       = useState("");
  const [messageType, setMessageType] = useState("");
  const [portalMessage, setPortalMessage]     = useState("");
  const [portalMessageType, setPortalMessageType] = useState("");
  const [loading, setLoading]       = useState(false);

  const navigate  = useNavigate();
  const location  = useLocation();

  // Handle forwarded messages (e.g. after logout)
  useEffect(() => {
    if (!location.state?.message) return;
    setPortalMessage(location.state.message);
    setPortalMessageType(location.state.type || "success");
    navigate(location.pathname, { replace: true, state: null });

    const fadeTimer  = setTimeout(() => {
      document.querySelector(".portal-message")?.classList.add("fade-out");
    }, 4500);
    const clearTimer = setTimeout(() => {
      setPortalMessage("");
      setPortalMessageType("");
    }, 5000);

    return () => { clearTimeout(fadeTimer); clearTimeout(clearTimer); };
  }, [location, navigate]);

  const handleChange = (e) =>
    setFormData({ ...formData, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage("");
    setMessageType("");
    setLoading(true);

    try {
      await AuthService.login(formData.username, formData.password);
      setMessageType("success");
      setMessage("Login successful! Redirecting…");
      setTimeout(() => navigate(ROUTES.DASHBOARD), 1200);
    } catch (err) {
      setMessageType("error");
      setMessage(
        err?.response?.data?.message || err.message || "Invalid credentials"
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-signup-container">

        {/* ── LEFT BRAND PANEL (desktop / tablet) ── */}
        <div className="login-brand-panel">
          <img src={aclogo} alt="CDN Logo" className="brand-logo" />
          <h1 className="brand-title">
            CDN <span>HRMS</span> Portal
          </h1>
          <div className="brand-divider" />
          <p className="brand-subtitle">
            Complete Developer Network<br />Human Resource Management System
          </p>
          <ul className="brand-features">
            <li>Employee records &amp; management</li>
            <li>Automated payroll calculation</li>
            <li>Birthday &amp; activity tracking</li>
            <li>Role-based secure access</li>
          </ul>
        </div>

        {/* ── RIGHT FORM PANEL ── */}
        <div className="login-form-panel">

          {/* Mobile-only logo + title */}
          <img src={aclogo} alt="CDN Logo" className="login-icon" />
          <h1 className="portal-title">
            CDN <span style={{ color: "#1e3a4a" }}>HRMS</span> Portal
          </h1>

          {/* Portal-level message (e.g. logout notice) */}
          {portalMessage && (
            <div className={`portal-message ${portalMessageType}`}>
              {portalMessage}
            </div>
          )}

          <div className="login-signup-form">
            <h2 className="form-heading">Welcome back</h2>
            <p className="form-subheading">Sign in to your account to continue</p>

            <form onSubmit={handleSubmit} noValidate>
              <div className="form-group">
                <input
                  id="username"
                  type="text"
                  name="username"
                  value={formData.username}
                  onChange={handleChange}
                  placeholder=" "
                  required
                  autoComplete="username"
                  disabled={loading}
                />
                <label htmlFor="username">Username</label>
              </div>

              <div className="form-group">
                <input
                  id="password"
                  type="password"
                  name="password"
                  value={formData.password}
                  onChange={handleChange}
                  placeholder=" "
                  required
                  autoComplete="current-password"
                  disabled={loading}
                />
                <label htmlFor="password">Password</label>
              </div>

              <button
                type="submit"
                className="login-submit-btn"
                disabled={loading}
              >
                {loading ? "Signing in…" : "Sign In"}
              </button>
            </form>

            {message && (
              <span className={`form-message ${messageType}`}>{message}</span>
            )}
          </div>
        </div>

      </div>
    </div>
  );
};

export default Login;