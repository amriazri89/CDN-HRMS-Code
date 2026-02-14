// src/components/Skeleton/Skeleton.jsx
import React from "react";
import "./Skeleton.css";

/**
 * <Skeleton variant="rect" width="100%" height="16px" />
 * variants: rect, circle, text
 */
export default function Skeleton({ variant = "rect", width = "100%", height = 12, className = "" }) {
  const cls = `skeleton skeleton--${variant} ${className}`;
  const style = { width, height };
  return <div className={cls} style={style} aria-hidden="true" />;
}
