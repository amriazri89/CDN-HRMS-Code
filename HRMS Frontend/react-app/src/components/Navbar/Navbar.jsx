// src/components/Navbar/Navbar.jsx
import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { FaUserCircle, FaSignOutAlt } from "react-icons/fa";
import UserService from "../../services/UserService"; // use logout helper
import "./Navbar.css";

const Navbar = () => {
  const navigate = useNavigate();
  const [userName, setUserName] = useState("");

  useEffect(() => {
    // Prefer the simple stored keys that your UserService actually sets
    const name = localStorage.getItem("name");
    const email = localStorage.getItem("userEmail");
    const id = localStorage.getItem("userId");
    console.log("Fetched user info:", { name, email, id });
    if (name) setUserName(name);
    else if (email) setUserName(email);
    else if (id) setUserName(id);
    else setUserName(""); // guest
  }, []);

const handleLogout = () => {
  UserService.logout();
  setUserName("");
  navigate("/etiqa/hrms/login", {
    state: { message: "You have been logged out successfully." },
  });
};


  return (
    <div className="navbar">
      <h2 className="navbar-title">Human Resource Management System [HRMS]</h2>

      <div className="nav-actions">
        <div className="profile-wrapper" title={UserService.getCurrentUser(). username || "Profile"}>
          <FaUserCircle className="nav-icon profile-icon" />
          Welcome [ <span className="nav-name">{UserService.getCurrentUser(). username || "Guest"}</span> ]
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
