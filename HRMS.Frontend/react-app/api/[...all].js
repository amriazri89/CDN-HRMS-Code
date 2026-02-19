// api/[...all].js
// Vercel Serverless Function - Proxies all requests to EC2 backend
// This bypasses CORS and HTTPS certificate issues

export default async function handler(req, res) {
  // ========== CORS HEADERS ==========
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, PATCH, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization');
  res.setHeader('Access-Control-Allow-Credentials', 'true');
  
  // Handle OPTIONS preflight request
  if (req.method === 'OPTIONS') {
    return res.status(200).end();
  }

  // ========== EXTRACT PATH ==========
  // req.url = /api/Auth/login
  // We want: Auth/login
  const path = req.url.replace('/api/', '');
  
  // ========== YOUR EC2 BACKEND ==========
  // Using HTTP on port 5000 (no certificate issues)
  const backendUrl = `https://sri.sri-eras.xyz/api/Auth/login/api/${path}`;
  
  console.log(`🔄 Proxying: ${req.method} ${backendUrl}`);

  try {
    // ========== PREPARE HEADERS ==========
    const headers = {
      'Content-Type': 'application/json',
    };
    
    // Forward Authorization header (for JWT tokens)
    if (req.headers.authorization) {
      headers['Authorization'] = req.headers.authorization;
    }

    // ========== PREPARE REQUEST BODY ==========
    let body = undefined;
    if (req.method !== 'GET' && req.method !== 'HEAD') {
      // For POST, PUT, DELETE - forward the body
      body = JSON.stringify(req.body);
    }

    // ========== CALL BACKEND ==========
    const response = await fetch(backendUrl, {
      method: req.method,
      headers: headers,
      body: body,
    });

    // ========== PARSE RESPONSE ==========
    const contentType = response.headers.get('content-type');
    let data;
    
    if (contentType && contentType.includes('application/json')) {
      data = await response.json();
    } else {
      data = await response.text();
    }

    // ========== RETURN TO FRONTEND ==========
    console.log(`✅ Response: ${response.status}`);
    return res.status(response.status).json(data);
    
  } catch (error) {
    // ========== ERROR HANDLING ==========
    console.error('❌ Proxy error:', error.message);
    return res.status(500).json({ 
      error: 'Cannot connect to backend server',
      message: error.message,
      backend: backendUrl
    });
  }
}