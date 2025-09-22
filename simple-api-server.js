const express = require('express');
const cors = require('cors');
const app = express();
const port = 5003;

// Enable CORS for all origins
app.use(cors({
    origin: ['http://localhost:3000', 'http://localhost:8080', 'http://localhost:8081', 'https://enterprise-toolkit-web.onrender.com'],
    credentials: true
}));

app.use(express.json());

// Root endpoint - MUST be first
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

// Remote execution endpoint
app.post('/api/remote/execute', (req, res) => {
    const { target, function: functionName } = req.body;
    
    console.log(`Remote execution request: ${functionName} on ${target}`);
    
    // Simulate remote execution
    const results = {
        success: true,
        target: target,
        function: functionName,
        timestamp: new Date().toISOString(),
        results: simulateRemoteExecution(functionName, target)
    };
    
    res.json(results);
});

// Remote connection endpoint
app.post('/api/remote/connect', (req, res) => {
    const { target } = req.body;
    
    console.log(`Remote connection request to: ${target}`);
    
    // Simulate connection
    const connection = {
        success: true,
        target: target,
        connected: true,
        timestamp: new Date().toISOString(),
        systemInfo: {
            hostname: target,
            os: 'Windows 11 Pro',
            architecture: 'x64',
            uptime: '2 days, 14 hours',
            users: ['Administrator', 'Ryan Gurary']
        }
    };
    
    res.json(connection);
});

// System health endpoint with more details
app.get('/api/system/health', (req, res) => {
    res.json({
        overallHealth: 98.5,
        cpuUsage: Math.floor(Math.random() * 30) + 20, // 20-50%
        memoryUsage: (Math.random() * 2 + 7).toFixed(1), // 7-9 GB
        diskUsage: Math.floor(Math.random() * 100) + 200, // 200-300 GB
        networkStatus: 'Connected',
        checks: [
            { name: 'CPU', status: 'Healthy', value: '25%' },
            { name: 'Memory', status: 'Healthy', value: '8.2 GB / 16 GB' },
            { name: 'Disk', status: 'Healthy', value: '250 GB / 500 GB' },
            { name: 'Network', status: 'Healthy', value: 'Connected' }
        ]
    });
});

// Software inventory endpoint
app.get('/api/software/inventory', (req, res) => {
    res.json({
        totalApplications: 45,
        applications: [
            { name: 'Microsoft Office 365', version: '16.0.14326.20404', publisher: 'Microsoft Corporation' },
            { name: 'Google Chrome', version: '117.0.5938.132', publisher: 'Google LLC' },
            { name: 'Visual Studio Code', version: '1.82.2', publisher: 'Microsoft Corporation' },
            { name: 'Adobe Acrobat Reader', version: '23.006.20320', publisher: 'Adobe Inc.' },
            { name: 'Windows Security', version: '4.18.23080.1004', publisher: 'Microsoft Corporation' }
        ]
    });
});

// Network test endpoint
app.get('/api/network/test', (req, res) => {
    res.json({
        ping: {
            google: { host: '8.8.8.8', time: '12ms', status: 'Success' },
            cloudflare: { host: '1.1.1.1', time: '8ms', status: 'Success' }
        },
        dns: {
            primary: '8.8.8.8',
            secondary: '8.8.4.4',
            status: 'Resolving'
        },
        connectivity: 'All tests passed'
    });
});

// Security check endpoint
app.get('/api/security/check', (req, res) => {
    res.json({
        antivirus: { status: 'Active', definition: 'Up to date', lastScan: '2 hours ago' },
        firewall: { status: 'Enabled', rules: 15, blocked: 0 },
        windowsUpdate: { status: 'Up to date', lastCheck: '1 day ago' },
        vulnerabilities: { critical: 0, high: 0, medium: 2, low: 5 },
        overall: 'Secure'
    });
});

// Function to simulate remote execution
function simulateRemoteExecution(functionName, target) {
    const simulations = {
        'system-health': {
            cpu: '25%',
            memory: '8.2 GB / 16 GB',
            disk: '250 GB / 500 GB',
            network: 'Connected',
            status: 'All systems operational'
        },
        'software-inventory': {
            totalApplications: 45,
            lastUpdated: new Date().toISOString(),
            status: 'Inventory complete'
        },
        'network-test': {
            ping: '12ms',
            dns: 'Resolving',
            connectivity: 'All tests passed'
        },
        'optimize-system': {
            tempFilesCleared: '2.3 GB',
            startupPrograms: 'Optimized',
            services: 'Optimized',
            status: 'System optimized'
        },
        'security-check': {
            antivirus: 'Active',
            firewall: 'Enabled',
            vulnerabilities: '2 medium, 5 low',
            status: 'Secure'
        },
        'backup-registry': {
            backupLocation: 'C:\\Backups\\Registry',
            size: '45 MB',
            status: 'Backup completed successfully'
        }
    };
    
    return simulations[functionName] || { status: 'Function executed successfully' };
}

app.listen(port, () => {
    console.log(`🚀 Simple API server running at http://localhost:${port}`);
    console.log(`📋 Available endpoints:`);
    console.log(`  • GET /`);
    console.log(`  • POST /api/auth/login`);
    console.log(`  • GET /health`);
    console.log(`  • GET /api/system/health`);
    console.log(`  • GET /api/system/performance`);
    console.log(`  • GET /api/network/adapters`);
    console.log(`  • POST /api/remote/execute`);
    console.log(`  • POST /api/remote/connect`);
    console.log(`  • GET /api/software/inventory`);
    console.log(`  • GET /api/network/test`);
    console.log(`  • GET /api/security/check`);
    console.log(`\n🔐 Test credentials: admin / admin123`);
});

// Handle specific endpoints that might be requested
app.get('/api/netif', (req, res) => {
    console.log('Network interface endpoint requested');
    res.json({
        interfaces: [
            { name: 'Ethernet', status: 'Connected', ip: '192.168.1.100' },
            { name: 'Wi-Fi', status: 'Connected', ip: '192.168.1.101' },
            { name: 'Bluetooth', status: 'Available', ip: 'N/A' }
        ]
    });
});

// Handle any missing endpoints to prevent 403 errors
app.get('/api/*', (req, res) => {
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
            'GET /api/netif',
            'POST /api/remote/execute',
            'POST /api/remote/connect',
            'GET /api/software/inventory',
            'GET /api/network/test',
            'GET /api/security/check'
        ]
    });
});
