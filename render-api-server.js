const express = require('express');
const cors = require('cors');
const app = express();
const port = process.env.PORT || 10000;

// Enable CORS for all origins
app.use(cors({
    origin: ['https://enterprise-toolkit-web.onrender.com', 'http://localhost:8082', 'http://localhost:3000'],
    credentials: true
}));

app.use(express.json());

// Root endpoint
app.get('/', (req, res) => {
    res.json({
        message: 'Enterprise IT Toolkit API Server (Render)',
        version: '1.0.0',
        status: 'running',
        environment: 'production'
    });
});

// Health check endpoint
app.get('/health', (req, res) => {
    res.json({
        status: 'healthy',
        timestamp: new Date().toISOString(),
        environment: 'render'
    });
});

// Authentication endpoint
app.post('/api/auth/login', (req, res) => {
    const { username, password } = req.body;
    
    if (username === 'admin' && password === 'admin123') {
        res.json({
            success: true,
            message: 'Login successful',
            token: 'render-demo-token-12345',
            user: {
                username: 'admin',
                role: 'administrator',
                permissions: ['all']
            }
        });
    } else {
        res.status(401).json({
            success: false,
            message: 'Invalid credentials'
        });
    }
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
        ],
        environment: 'render'
    });
});

// System performance endpoint
app.get('/api/system/performance', (req, res) => {
    res.json({
        cpuUsage: Math.floor(Math.random() * 30) + 20,
        memoryUsage: (Math.random() * 2 + 7).toFixed(1),
        diskUsage: Math.floor(Math.random() * 100) + 200,
        networkSpeed: Math.floor(Math.random() * 100) + 50,
        environment: 'render'
    });
});

// Service status endpoints
app.get('/api/phpadmin/status', (req, res) => {
    res.json({ status: 'running', service: 'phpmyadmin', port: 8080, environment: 'render' });
});

app.get('/api/database/status', (req, res) => {
    res.json({ status: 'running', service: 'postgresql', port: 5432, environment: 'render' });
});

app.get('/api/cache/status', (req, res) => {
    res.json({ status: 'running', service: 'redis', port: 6379, environment: 'render' });
});

app.get('/api/monitoring/status', (req, res) => {
    res.json({ status: 'running', service: 'grafana', port: 3000, environment: 'render' });
});

// Network adapters endpoint
app.get('/api/network/adapters', (req, res) => {
    res.json({
        adapters: [
            {
                name: 'Ethernet',
                status: 'Connected',
                speed: '1 Gbps',
                ipAddress: '192.168.1.100'
            },
            {
                name: 'Wi-Fi',
                status: 'Connected',
                speed: '866 Mbps',
                ipAddress: '192.168.1.101'
            }
        ],
        environment: 'render'
    });
});

// Remote execution endpoint
app.post('/api/remote/execute', (req, res) => {
    const { command, target } = req.body;
    
    res.json({
        success: true,
        command: command,
        target: target,
        output: 'Command executed successfully (Render demo mode)',
        timestamp: new Date().toISOString(),
        environment: 'render'
    });
});

// Remote connect endpoint
app.post('/api/remote/connect', (req, res) => {
    const { hostname, username } = req.body;
    
    res.json({
        success: true,
        message: 'Connected to remote machine (Render demo mode)',
        hostname: hostname,
        username: username,
        timestamp: new Date().toISOString(),
        environment: 'render'
    });
});

// Software inventory endpoint
app.get('/api/software/inventory', (req, res) => {
    res.json({
        totalApplications: 45,
        applications: [
            { name: 'Microsoft Office 365', version: '16.0.14326.20404', status: 'Active' },
            { name: 'Google Chrome', version: '117.0.5938.132', status: 'Active' },
            { name: 'Adobe Acrobat Reader', version: '23.006.20320', status: 'Active' },
            { name: 'Visual Studio Code', version: '1.82.2', status: 'Active' }
        ],
        environment: 'render'
    });
});

// Network test endpoint
app.get('/api/network/test', (req, res) => {
    res.json({
        ping: '8ms',
        download: '95.2 Mbps',
        upload: '12.8 Mbps',
        latency: '8ms',
        status: 'Connected',
        environment: 'render'
    });
});

// Security check endpoint
app.get('/api/security/check', (req, res) => {
    res.json({
        firewall: 'Enabled',
        antivirus: 'Active',
        updates: 'Current',
        vulnerabilities: 0,
        status: 'Secure',
        environment: 'render'
    });
});

// Handle any missing endpoints
app.use((req, res) => {
    res.json({
        error: 'Endpoint not found',
        path: req.path,
        environment: 'render',
        availableEndpoints: [
            'GET /',
            'GET /health',
            'POST /api/auth/login',
            'GET /api/system/health',
            'GET /api/system/performance',
            'GET /api/network/adapters',
            'GET /api/phpadmin/status',
            'GET /api/database/status',
            'GET /api/cache/status',
            'GET /api/monitoring/status',
            'POST /api/remote/execute',
            'POST /api/remote/connect',
            'GET /api/software/inventory',
            'GET /api/network/test',
            'GET /api/security/check'
        ]
    });
});

// Start server
app.listen(port, () => {
    console.log(`🚀 Render API server running on port ${port}`);
    console.log('📋 Available endpoints:');
    console.log('  • GET /');
    console.log('  • GET /health');
    console.log('  • POST /api/auth/login');
    console.log('  • GET /api/system/health');
    console.log('  • GET /api/system/performance');
    console.log('  • GET /api/network/adapters');
    console.log('  • GET /api/phpadmin/status');
    console.log('  • GET /api/database/status');
    console.log('  • GET /api/cache/status');
    console.log('  • GET /api/monitoring/status');
    console.log('  • POST /api/remote/execute');
    console.log('  • POST /api/remote/connect');
    console.log('  • GET /api/software/inventory');
    console.log('  • GET /api/network/test');
    console.log('  • GET /api/security/check');
    console.log('🔐 Test credentials: admin / admin123');
});
