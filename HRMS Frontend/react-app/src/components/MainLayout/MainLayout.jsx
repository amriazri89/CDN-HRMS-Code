// src/components/MainLayout/MainLayout.jsx
import React, { useEffect, useState } from "react";
import Sidebar from "../Sidebar/Sidebar.jsx";
import Navbar from "../Navbar/Navbar.jsx";
import LoadingOverlay from "../Loading/LoadingOverlay";
import Skeleton from "../Loading/Skeleton";
import "./MainLayout.css";
import "../../styles/style.css";
/**
 * props:
 *  - children
 *  - loading (optional) -> overlay
 *  - initialDelayMs (optional) -> how long to show skeleton hero
 */
const MainLayout = ({ children, loading = false, initialDelayMs = 500 }) => {
  const [initialLoading, setInitialLoading] = useState(true);

  useEffect(() => {
    const t = setTimeout(() => setInitialLoading(false), initialDelayMs);
    return () => clearTimeout(t);
  }, [initialDelayMs]);

  return (
    <div className="main-layout">
      <Sidebar />
      <div className="main-content">
        <Navbar />

        <div className="content">
          {initialLoading ? (
            // Skeleton hero + stat placeholders mimic your Dashboard layout
            <div style={{ padding: "20px 24px" }}>
              <div className="hero-skeleton">
                <Skeleton variant="rect" height={72} />
                <div style={{ display: "flex", gap: 16, marginTop: 18 }}>
                  <Skeleton style={{flex:1}} width="20%" height={64} />
                  <Skeleton width="20%" height={64} />
                  <Skeleton width="20%" height={64} />
                  <Skeleton width="20%" height={64} />
                  <Skeleton width="20%" height={64} />
                </div>
              </div>
              {/* a few card skeleton rows */}
              <div style={{ marginTop: 20 }}>
                <div style={{ display: "grid", gridTemplateColumns: "repeat(3,1fr)", gap: 16 }}>
                  <Skeleton height={160} />
                  <Skeleton height={160} />
                  <Skeleton height={160} />
                </div>
              </div>
            </div>
          ) : (
            <div style={{ position: "relative" }}>
              {children}
              {loading && <LoadingOverlay message="Please wait..." />}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default MainLayout;
