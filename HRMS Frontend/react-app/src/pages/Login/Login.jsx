import React, { useEffect, useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import "./Login.scss";
import aclogo from "../../assets/cdn-logo.jpg";
import AuthService from "../../services/AuthService.js";
import ROUTES from "../../routes";

const Login = () => {
    useEffect(() => {
      document.title = "CDN Login Portal"; // set tab name
    }, []);
  

  const [formData, setFormData] = useState({
    username: "",
    password: "",
  });

  // form-level message (for errors/success from submit)
  const [message, setMessage] = useState("");
  const [messageType, setMessageType] = useState(""); // "success" | "error"

  // portal-level message (for forwarded messages like logout)
  const [portalMessage, setPortalMessage] = useState("");
  const [portalMessageType, setPortalMessageType] = useState("");

  const navigate = useNavigate();
  const location = useLocation();

  // handle forwarded messages (e.g. logout -> redirect with message)
  useEffect(() => {
    if (!location.state?.message) return;

    setPortalMessage(location.state.message);
    setPortalMessageType(location.state.type || "success");

    navigate(location.pathname, { replace: true, state: null });

    const fadeTimer = setTimeout(() => {
      const el = document.querySelector(".portal-message");
      if (el) el.classList.add("fade-out");
    }, 5200);

    const clearTimer = setTimeout(() => {
      setPortalMessage("");
      setPortalMessageType("");
    }, 1500);

    return () => {
      clearTimeout(fadeTimer);
      clearTimeout(clearTimer);
    };
  }, [location, navigate]);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    setMessage("");
    setMessageType("");

    try {
      await AuthService.login(formData.username, formData.password);
      setMessageType("success");
      setMessage("Login successful! Redirecting...");
      setTimeout(() => navigate(ROUTES.DASHBOARD), 1500);
    } catch (err) {
      console.error("Login error:", err);
      setMessageType("error");
      setMessage(
        err?.response?.data?.message || err.message || "Login failed"
      );
    }
  };

  return (
    <div className="login-page">
      <div className="login-signup-container">
        <img src={aclogo} alt="Logo" className="login-icon" style={{height:"20vh"}}/>
        <h1 className="portal-title">
          <span style={{ color: "black" }}>CDN </span>
          <span style={{ color: "#1a4b58" }}>HRMS</span> Portal
        </h1>

        {/* portal-level message */}
        {portalMessage && (
          <div
            className={`portal-message ${
              portalMessageType === "error" ? "error" : "success"
            }`}
          >
            {portalMessage}
          </div>
        )}

        <div className="login-signup-form">
          <h2 className="form-heading">  Complete Developer Network</h2>
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <input
                type="username"
                name="username"
                value={formData.username}
                onChange={handleChange}
                required
                placeholder="Username"
              />
              <label>Username</label>
            </div>

            <div className="form-group">
              <input
                type="password"
                name="password"
                value={formData.password}
                onChange={handleChange}
                required
                placeholder="Password"
              />
              <label>Password</label>
            </div>

            <button type="submit">Login</button>
          </form>

          {message && (
            <span className={`form-message ${messageType}`}>{message}</span>
          )}
        </div>
      </div>
    </div>
  );
};

export default Login;
