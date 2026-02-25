// src/pages/Login/Login.jsx
import React, { useEffect, useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import "./Login.scss";
import aclogo from "../../assets/cdn-logo-2.jpg";
import AuthService from "../../services/AuthService.js";
import ROUTES from "../../routes";

const Login = () => {
  useEffect(() => { document.title = "CDN HRMS – Login"; }, []);

  const [form, setForm]                   = useState({ username: "", password: "" });
  const [msg, setMsg]                     = useState({ text: "", type: "" });
  const [portalMsg, setPortalMsg]         = useState({ text: "", type: "" });
  const [loading, setLoading]             = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    if (!location.state?.message) return;
    setPortalMsg({ text: location.state.message, type: location.state.type || "success" });
    navigate(location.pathname, { replace: true, state: null });
    const t1 = setTimeout(() => document.querySelector(".portal-msg")?.classList.add("fade-out"), 4000);
    const t2 = setTimeout(() => setPortalMsg({ text: "", type: "" }), 4600);
    return () => { clearTimeout(t1); clearTimeout(t2); };
  }, [location, navigate]);

  const onChange = (e) => setForm((p) => ({ ...p, [e.target.name]: e.target.value }));

  const onSubmit = async (e) => {
    e.preventDefault();
    setMsg({ text: "", type: "" });
    setLoading(true);
    try {
      await AuthService.login(form.username, form.password);
      setMsg({ text: "Login successful! Redirecting…", type: "success" });
      setTimeout(() => navigate(ROUTES.DASHBOARD), 1200);
    } catch (err) {
      setMsg({ text: err?.response?.data?.message || err.message || "Invalid credentials", type: "error" });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="lp-root">
      <div className="lp-inner">

        {/* ── BRAND SIDE ── */}
        <div className="lp-brand">
          <img src={aclogo} alt="CDN" className="lp-logo" />
          <h1 className="lp-brand-title">CDN <span>HRMS</span> Portal</h1>
          <div className="lp-rule" />
          <p className="lp-brand-sub">Complete Developer Network<br />Human Resource Management System</p>
          <ul className="lp-features">
            <li>Employee records &amp; management</li>
            <li>Automated payroll calculation</li>
            <li>Birthday &amp; activity tracking</li>
            {/* <li>Role-based secure access</li> */}
          </ul>
        </div>

        {/* ── FORM SIDE ── */}
        <div className="lp-form-side">
          {portalMsg.text && (
            <div className={`portal-msg ${portalMsg.type}`}>{portalMsg.text}</div>
          )}

          <div className="lp-card">
            <h2 className="lp-card-title">Welcome Back</h2>
            <p className="lp-card-sub">Sign in to your account to continue</p>

            <div className="lp-creds">
              <span className="lp-creds-label">Demo Access Credentials</span>
              <div className="lp-creds-row">
                <span className="lp-creds-key">Username</span>
                <code className="lp-creds-val">Admin</code>
              </div>
              <div className="lp-creds-row">
                <span className="lp-creds-key">Password</span>
                <code className="lp-creds-val">Admin@123</code>
              </div>
            </div>

            <form onSubmit={onSubmit} noValidate>
              <input
                className="lp-input"
                type="text"
                name="username"
                value={form.username}
                onChange={onChange}
                placeholder="Username"
                required
                autoComplete="username"
                disabled={loading}
              />
              <input
                className="lp-input"
                type="password"
                name="password"
                value={form.password}
                onChange={onChange}
                placeholder="Password"
                required
                autoComplete="current-password"
                disabled={loading}
              />
              <button className="lp-btn" type="submit" disabled={loading}>
                {loading ? "Signing in…" : "Sign In"}
              </button>
            </form>

            {msg.text && <div className={`lp-msg ${msg.type}`}>{msg.text}</div>}
          </div>
        </div>

      </div>
    </div>
  );
};

export default Login;