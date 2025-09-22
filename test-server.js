const express = require('express');
const cors = require('cors');
const app = express();
const port = 5001;

// Enable CORS for all origins
app.use(cors());

app.use(express.json());

// Root endpoint
app.get('/', (req, res) => {
    console.log('Root endpoint hit');
    res.json({
        message: 'Test API Server',
        status: 'running',
        timestamp: new Date().toISOString()
    });
});

// Health endpoint
app.get('/health', (req, res) => {
    res.json({ status: 'healthy' });
});

// Login endpoint
app.post('/api/auth/login', (req, res) => {
    const { username, password } = req.body;
    console.log('Login attempt:', { username, password });
    
    if (username === 'admin' && password === 'admin123') {
        res.json({
            success: true,
            token: 'test-token-' + Date.now()
        });
    } else {
        res.status(401).json({ error: 'Invalid credentials' });
    }
});

app.listen(port, () => {
    console.log(`🚀 Test API server running at http://localhost:${port}`);
    console.log(`📋 Available endpoints:`);
    console.log(`  • GET /`);
    console.log(`  • GET /health`);
    console.log(`  • POST /api/auth/login`);
});
