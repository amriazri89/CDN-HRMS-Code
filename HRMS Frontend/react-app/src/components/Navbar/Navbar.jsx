// src/components/Navbar/Navbar.jsx
import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { FaUserCircle, FaSignOutAlt } from "react-icons/fa";
import UserService from "../../services/AuthService";
import "./Navbar.css";

const Navbar = () => {
  const navigate = useNavigate();
  const [user, setUser] = useState(null);

  useEffect(() => {
    const currentUser = UserService.getCurrentUser();
    setUser(currentUser); // null if not logged in
    console.log("Fetched user info:", currentUser);
  }, []);

  const handleLogout = () => {
    UserService.logout();
    setUser(null);
    navigate("/cdn/hrms/login", {
      state: { message: "You have been logged out successfully." },
    });
  };

  return (
    <div className="navbar">
      <h2 className="navbar-title">Human Resource Management System [HRMS]</h2>

      <div className="nav-actions">
        <div className="profile-wrapper" title={user?.username || "Profile"}>
          <FaUserCircle className="nav-icon profile-icon" />
          Welcome [ <span className="nav-name">{user?.username || "Guest"}</span> ]
        </div>

        <FaSignOutAlt
          className="nav-icon logout-icon"
          title="Logout"
          onClick={handleLogout}
        />
      </div>
    </div>
  );
};

export default Navbar;
