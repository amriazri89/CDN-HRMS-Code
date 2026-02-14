// src/components/LoadingSpinner/LoadingSpinner.jsx
import React from "react";
import "./LoadingSpinner.css";

/**
 * LoadingSpinner
 * - size: px
 * - color: stroke color
 * - thickness: stroke width
 */
export default function LoadingSpinner({ size = 44, color = "#2563eb", thickness = 4 }) {
  const style = { width: size, height: size };
  return (
    <div className="ls-container" style={style} aria-hidden="true">
      <svg className="ls-svg" viewBox="0 0 50 50" style={{ width: "100%", height: "100%" }}>
        <defs>
          <linearGradient id="lsGrad" x1="0%" x2="100%">
            <stop offset="0%" stopColor={color} stopOpacity="1" />
            <stop offset="100%" stopColor="#7dd3fc" stopOpacity="1" />
          </linearGradient>
        </defs>

        {/* background ring */}
        <circle className="ls-ring-bg" cx="25" cy="25" r="20" fill="none" stroke="#e6eefb" strokeWidth={thickness} />

        {/* animated arc */}
        <g transform="rotate(-90 25 25)">
          <circle
            className="ls-arc"
            cx="25"
            cy="25"
            r="20"
            fill="none"
            stroke="url(#lsGrad)"
            strokeWidth={thickness}
            strokeLinecap="round"
          />
        </g>
      </svg>
    </div>
  );
}
