// src/components/LoadingOverlay/LoadingOverlay.jsx
import React from "react";
import LoadingSpinner from "./LoadingSpinner";
import "./LoadingOverlay.css";

export default function LoadingOverlay({ message = "Loading...", size = 48 }) {
  return (
    <div className="overlay-root" role="status" aria-live="polite" aria-label={message}>
      <div className="overlay-card">
        <LoadingSpinner size={size} />
        {message && <div className="overlay-msg">{message}</div>}
      </div>
    </div>
  );
}
