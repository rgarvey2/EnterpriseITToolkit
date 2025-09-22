const express = require('express');
const cors = require('cors');
const app = express();
const port = 5001;

// Enable CORS for all origins
app.use(cors({
    origin: ['http://localhost:3000', 'http://localhost:8080', 'https://enterprise-toolkit-web.onrender.com'],
    credentials: true
}));

app.use(express.json());

// Root endpoint
app.get('/', (req, res) => {
    res.json({
        message: 'Enterprise IT Toolkit API Server',
        version: '1.0.0',
        status: 'running',
        endpoints: [
            'POST /api/auth/login',
            'GET /health',
            'GET /api/system/health',
            'GET /api/system/performance',
            'GET /api/network/adapters'
        ],
        testCredentials: {
            username: 'admin',
            password: 'admin123'
        }
    });
});

// Simple authentication endpoint
app.post('/api/auth/login', (req, res) => {
    const { username, password } = req.body;
    
    console.log('Login attempt:', { username, password });
    
    // Simple authentication check
    if (username === 'admin' && password === 'admin123') {
        res.json({
            success: true,
            sessionToken: 'mock-session-token-' + Date.now(),
            username: 'admin',
            roles: ['admin'],
            expiresAt: new Date(Date.now() + 3600000).toISOString()
        });
    } else {
        res.status(401).json({
            error: 'Invalid credentials',
            requiresMfa: false
        });
    }
});

// Health check endpoint
app.get('/health', (req, res) => {
    res.json({ status: 'healthy', timestamp: new Date().toISOString() });
});

// System health endpoint
app.get('/api/system/health', (req, res) => {
    res.json({
        overallHealth: 98.5,
        checks: [
            { name: 'CPU', status: 'Healthy' },
            { name: 'Memory', status: 'Healthy' },
            { name: 'Disk', status: 'Healthy' },
            { name: 'Network', status: 'Healthy' }
        ]
    });
});

// Performance metrics endpoint
app.get('/api/system/performance', (req, res) => {
    res.json({
        overallScore: 94.2,
        cpuUsage: 45.2,
        memoryUsage: 67.8,
        diskUsage: 34.5,
        networkUsage: 32.1
    });
});

// Network adapters endpoint
app.get('/api/network/adapters', (req, res) => {
    res.json([
        {
            name: 'Ethernet',
            status: 'Up',
            ipAddresses: ['192.168.1.100'],
            description: 'Primary network adapter'
        }
    ]);
});

app.listen(port, () => {
    console.log(`🚀 Simple API server running at http://localhost:${port}`);
    console.log(`📋 Available endpoints:`);
    console.log(`  • POST /api/auth/login`);
    console.log(`  • GET /health`);
    console.log(`  • GET /api/system/health`);
    console.log(`  • GET /api/system/performance`);
    console.log(`  • GET /api/network/adapters`);
    console.log(`\n🔐 Test credentials: admin / admin123`);
});
