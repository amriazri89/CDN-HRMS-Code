// src/config/api.js
// OR wherever you define your API URL

// ❌ OLD - Direct connection with certificate issues
// const API_URL = "https://ec2-35-172-146-76.compute-1.amazonaws.com:5001/api";

// ✅ NEW - Proxied through Vercel serverless function
const API_URL = "/api";

export default API_URL;