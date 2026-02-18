import React from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import Dashboard from "./pages/Dashboard/Dashboard";
import Login from "./pages/Login/Login";
import Employee from "./pages/Employee/Employee";
import Payroll from "./pages/Payroll/Payroll";
import ProtectedRoute from "./components/ProtectedRoute";
import { ROUTES, BASE_PATH } from "./routes";


function App() {
  return (
    <Router>
      <Routes>
        {/* Redirect root to login */}
        <Route path="/" element={<Navigate to={ROUTES.LOGIN} replace />} />
        <Route path={BASE_PATH} element={<Navigate to={ROUTES.LOGIN} replace />} />

        {/* Public Route */}
        <Route path={ROUTES.LOGIN} element={<Login />} />

        {/* Protected Routes */}
        <Route
          path={ROUTES.DASHBOARD}
          element={
            <ProtectedRoute>
              <Dashboard />
            </ProtectedRoute>
          }
        />
        <Route
          path={ROUTES.PAYROLL}
          element={
            <ProtectedRoute>
              <Payroll />
            </ProtectedRoute>
          }
        />
        <Route
          path={ROUTES.EMPLOYEE}
          element={
            <ProtectedRoute>
              <Employee />
            </ProtectedRoute>
          }
        />

        {/* Catch all - redirect to login */}
        <Route path="*" element={<Navigate to={ROUTES.LOGIN} replace />} />
      </Routes>
    </Router>
  );
}

export default App;