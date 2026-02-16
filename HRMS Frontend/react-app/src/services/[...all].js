import axios from "axios";

export default async function handler(req, res) {
  try {
    // Forward request to your EC2 backend
    const url = `http://ec2-35-172-146-76.compute-1.amazonaws.com:5000/api${req.url.replace(/^\/?api/, '')}`;

    const response = await axios({
      method: req.method,
      url,
      data: req.body,
      headers: req.headers,
    });

    res.status(response.status).json(response.data);
  } catch (err) {
    res.status(err.response?.status || 500).json({ error: err.message });
  }
}
