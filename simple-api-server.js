const express = require('express');
const cors = require('cors');
const app = express();
const port = 5003;

// Enable CORS for all origins
app.use(cors({
    origin: ['http://localhost:3000', 'http://localhost:8080', 'http://localhost:8081', 'http://localhost:8082', 'https://enterprise-toolkit-web.onrender.com'],
    credentials: true
}));

app.use(express.json());

// Root endpoint
app.get('/', (req, res) => {
    console.log('Root endpoint hit');
    res.json({
        message: 'Enterprise IT Toolkit API Server',
        version: '1.0.0',
        status: 'running',
        endpoints: [
            'GET /',
            'POST /api/auth/login',
            'GET /health',
            'GET /api/system/health',
            'GET /api/system/performance',
            'GET /api/network/adapters',
            'GET /api/phpadmin/status',
            'GET /api/database/status',
            'GET /api/cache/status',
            'GET /api/monitoring/status'
        ],
        testCredentials: {
            username: 'admin',
            password: 'admin123'
        }
    });
});

// Handle requests to port 8080 (PHP Admin Panel check)
app.get('/', (req, res) => {
    const host = req.get('host');
    console.log('Request to:', host);
    
    if (host && host.includes('8080')) {
        console.log('PHP Admin Panel status check');
        res.json({
            message: 'PHP Admin Panel',
            status: 'running',
            port: 8080,
            service: 'phpmyadmin'
        });
    } else {
        res.json({
            message: 'Enterprise IT Toolkit API Server',
            version: '1.0.0',
            status: 'running'
        });
    }
});

// Handle HEAD requests to port 8080 (for status checks)
app.head('/', (req, res) => {
    const host = req.get('host');
    console.log('HEAD request to:', host);
    
    if (host && host.includes('8080')) {
        console.log('PHP Admin Panel HEAD status check');
        res.status(200).end();
    } else {
        res.status(200).end();
    }
});

// Authentication endpoint
app.post('/api/auth/login', (req, res) => {
    console.log('Login attempt:', req.body);
    const { username, password } = req.body;
    
    if (username === 'admin' && password === 'admin123') {
        res.json({
            success: true,
            message: 'Login successful',
            token: 'demo-token-12345',
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

// Health check endpoint
app.get('/health', (req, res) => {
    res.json({
        status: 'healthy',
        timestamp: new Date().toISOString()
    });
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

// System performance endpoint
app.get('/api/system/performance', (req, res) => {
    res.json({
        cpuUsage: Math.floor(Math.random() * 30) + 20, // 20-50%
        memoryUsage: (Math.random() * 2 + 7).toFixed(1), // 7-9 GB
        diskUsage: Math.floor(Math.random() * 100) + 200, // 200-300 GB
        networkSpeed: Math.floor(Math.random() * 100) + 50 // 50-150 Mbps
    });
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
        ]
    });
});

// Service status endpoints
app.get('/api/phpadmin/status', (req, res) => {
    res.json({ status: 'running', service: 'phpmyadmin', port: 8080 });
});

app.get('/api/database/status', (req, res) => {
    res.json({ status: 'running', service: 'postgresql', port: 5432 });
});

app.get('/api/cache/status', (req, res) => {
    res.json({ status: 'running', service: 'redis', port: 6379 });
});

app.get('/api/monitoring/status', (req, res) => {
    res.json({ status: 'running', service: 'grafana', port: 3000 });
});

// Remote execution endpoint
app.post('/api/remote/execute', (req, res) => {
    const { command, target } = req.body;
    console.log('Remote execution request:', { command, target });
    
    res.json({
        success: true,
        command: command,
        target: target,
        output: 'Command executed successfully (demo mode)',
        timestamp: new Date().toISOString()
    });
});

// Remote connect endpoint
app.post('/api/remote/connect', (req, res) => {
    const { hostname, username } = req.body;
    console.log('Remote connect request:', { hostname, username });
    
    res.json({
        success: true,
        message: 'Connected to remote machine (demo mode)',
        hostname: hostname,
        username: username,
        timestamp: new Date().toISOString()
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
        ]
    });
});

// Network test endpoint
app.get('/api/network/test', (req, res) => {
    res.json({
        ping: '8ms',
        download: '95.2 Mbps',
        upload: '12.8 Mbps',
        latency: '8ms',
        status: 'Connected'
    });
});

// Security check endpoint
app.get('/api/security/check', (req, res) => {
    res.json({
        firewall: 'Enabled',
        antivirus: 'Active',
        updates: 'Current',
        vulnerabilities: 0,
        status: 'Secure'
    });
});

// Handle any missing endpoints to prevent 403 errors
app.use((req, res) => {
    console.log('Missing endpoint requested:', req.path);
    res.json({
        error: 'Endpoint not found',
        path: req.path,
        availableEndpoints: [
            'GET /',
            'POST /api/auth/login',
            'GET /health',
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
    console.log(`🚀 Simple API server running at http://localhost:${port}`);
    console.log('📋 Available endpoints:');
    console.log('  • GET /');
    console.log('  • POST /api/auth/login');
    console.log('  • GET /health');
    console.log('  • GET /api/system/health');
    console.log('  • GET /api/system/performance');
    console.log('  • GET /api/network/adapters');
    console.log('  • POST /api/remote/execute');
    console.log('  • POST /api/remote/connect');
    console.log('  • GET /api/software/inventory');
    console.log('  • GET /api/network/test');
    console.log('  • GET /api/security/check');
    console.log('🔐 Test credentials: admin / admin123');
});