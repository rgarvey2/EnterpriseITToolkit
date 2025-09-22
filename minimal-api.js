const express = require('express');
const app = express();
const port = 5002; // Use different port to avoid conflicts

app.use(express.json());

// Root endpoint
app.get('/', (req, res) => {
    console.log('Root endpoint accessed');
    res.json({
        message: 'Minimal API Server',
        status: 'working',
        timestamp: new Date().toISOString()
    });
});

// Health endpoint
app.get('/health', (req, res) => {
    res.json({ status: 'healthy' });
});

app.listen(port, () => {
    console.log(`🚀 Minimal API server running at http://localhost:${port}`);
});
