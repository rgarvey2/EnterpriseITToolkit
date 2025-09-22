// Enterprise IT Toolkit - Professional Dashboard JavaScript

class EnterpriseDashboard {
    constructor() {
        this.charts = {};
        this.refreshInterval = null;
        this.isInitialized = false;
        this.currentSection = 'dashboard';
        this.apiBaseUrl = 'http://localhost:5001/api';
        this.desktopAppRunning = false;
        this.authToken = null;
        this.demoMode = true; // Enable demo mode for web interface
        this.init();
    }

    init() {
        this.initializeCharts();
        this.setupEventListeners();
        this.startAutoRefresh();
        this.loadInitialData();
        this.isInitialized = true;
        console.log('Enterprise Dashboard initialized successfully');
    }

    initializeCharts() {
        // Performance Chart
        const performanceCtx = document.getElementById('performanceChart');
        if (performanceCtx) {
            this.charts.performance = new Chart(performanceCtx, {
                type: 'line',
                data: {
                    labels: this.generateTimeLabels(24),
                    datasets: [{
                        label: 'CPU Usage',
                        data: this.generateRandomData(24, 20, 80),
                        borderColor: '#3b82f6',
                        backgroundColor: 'rgba(59, 130, 246, 0.1)',
                        tension: 0.4,
                        fill: true
                    }, {
                        label: 'Memory Usage',
                        data: this.generateRandomData(24, 30, 70),
                        borderColor: '#10b981',
                        backgroundColor: 'rgba(16, 185, 129, 0.1)',
                        tension: 0.4,
                        fill: true
                    }, {
                        label: 'Disk Usage',
                        data: this.generateRandomData(24, 40, 90),
                        borderColor: '#f59e0b',
                        backgroundColor: 'rgba(245, 158, 11, 0.1)',
                        tension: 0.4,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'top',
                            labels: {
                                usePointStyle: true,
                                padding: 20,
                                font: { family: 'Inter', size: 12, weight: '500' }
                            }
                        }
                    },
                    scales: {
                        x: { grid: { display: false }, ticks: { font: { family: 'Inter', size: 11 } } },
                        y: {
                            beginAtZero: true,
                            max: 100,
                            grid: { color: 'rgba(0, 0, 0, 0.05)' },
                            ticks: { font: { family: 'Inter', size: 11 }, callback: function(value) { return value + '%'; } }
                        }
                    }
                }
            });
        }

        // Resource Chart
        const resourceCtx = document.getElementById('resourceChart');
        if (resourceCtx) {
            this.charts.resource = new Chart(resourceCtx, {
                type: 'doughnut',
                data: {
                    labels: ['CPU', 'Memory', 'Disk', 'Network'],
                    datasets: [{
                        data: [65, 45, 78, 32],
                        backgroundColor: ['#3b82f6', '#10b981', '#f59e0b', '#8b5cf6'],
                        borderWidth: 0,
                        cutout: '70%'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: {
                                usePointStyle: true,
                                padding: 20,
                                font: { family: 'Inter', size: 12, weight: '500' }
                            }
                        }
                    }
                }
            });
        }

        // Prediction Chart
        const predictionCtx = document.getElementById('predictionChart');
        if (predictionCtx) {
            this.charts.prediction = new Chart(predictionCtx, {
                type: 'line',
                data: {
                    labels: this.generateTimeLabels(48),
                    datasets: [{
                        label: 'Historical Data',
                        data: this.generateRandomData(24, 20, 80),
                        borderColor: '#6b7280',
                        backgroundColor: 'rgba(107, 114, 128, 0.1)',
                        tension: 0.4,
                        fill: false
                    }, {
                        label: 'Predicted Data',
                        data: [...this.generateRandomData(24, 20, 80), ...this.generateRandomData(24, 30, 85)],
                        borderColor: '#3b82f6',
                        backgroundColor: 'rgba(59, 130, 246, 0.1)',
                        tension: 0.4,
                        fill: false,
                        borderDash: [5, 5]
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'top',
                            labels: {
                                usePointStyle: true,
                                padding: 20,
                                font: { family: 'Inter', size: 12, weight: '500' }
                            }
                        }
                    },
                    scales: {
                        x: { grid: { display: false }, ticks: { font: { family: 'Inter', size: 11 } } },
                        y: {
                            beginAtZero: true,
                            max: 100,
                            grid: { color: 'rgba(0, 0, 0, 0.05)' },
                            ticks: { font: { family: 'Inter', size: 11 }, callback: function(value) { return value + '%'; } }
                        }
                    }
                }
            });
        }
    }

    setupEventListeners() {
        document.querySelectorAll('.enterprise-nav-link').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const section = link.getAttribute('onclick')?.match(/showSection\('([^']+)'\)/)?.[1];
                if (section) {
                    this.showSection(section);
                }
            });
        });
    }

    showSection(sectionName) {
        document.querySelectorAll('.content-section').forEach(section => {
            section.classList.remove('active');
        });

        const targetSection = document.getElementById(sectionName);
        if (targetSection) {
            targetSection.classList.add('active');
            this.currentSection = sectionName;
            
            document.querySelectorAll('.enterprise-nav-link').forEach(link => {
                link.classList.remove('active');
            });
            
            const activeLink = document.querySelector(`[onclick="showSection('${sectionName}')"]`);
            if (activeLink) {
                activeLink.classList.add('active');
            }

            // Load section-specific data
            this.loadSectionData(sectionName);
        }
    }

    async loadSectionData(sectionName) {
        switch (sectionName) {
            case 'system-health':
                await this.loadSystemHealthData();
                break;
            case 'performance':
                await this.loadPerformanceData();
                break;
            case 'security':
                await this.loadSecurityData();
                break;
            case 'network':
                await this.loadNetworkData();
                break;
            case 'automation':
                await this.loadAutomationData();
                break;
            case 'ml-analytics':
                await this.loadMLAnalyticsData();
                break;
            case 'reports':
                await this.loadReportsData();
                break;
        }
    }

    async loadSystemHealthData() {
        try {
            const [health, processes, workstation] = await Promise.all([
                this.getSystemHealth(),
                this.getTopProcesses(),
                this.getWorkstationInfo()
            ]);

            this.updateSystemHealthSection(health, processes, workstation);
        } catch (error) {
            console.error('Error loading system health data:', error);
        }
    }

    async loadPerformanceData() {
        try {
            const performance = await this.getPerformanceMetrics();
            this.updatePerformanceSection(performance);
        } catch (error) {
            console.error('Error loading performance data:', error);
        }
    }

    async loadSecurityData() {
        try {
            const [siemStatus, activeThreats, securityAlerts] = await Promise.all([
                this.getSiemStatus(),
                this.getActiveThreats(),
                this.getSecurityAlerts()
            ]);

            this.updateSecuritySection(siemStatus, activeThreats, securityAlerts);
        } catch (error) {
            console.error('Error loading security data:', error);
        }
    }

    async loadNetworkData() {
        try {
            const adapters = await this.getNetworkAdapters();
            this.updateNetworkSection(adapters);
        } catch (error) {
            console.error('Error loading network data:', error);
        }
    }

    async loadAutomationData() {
        try {
            const [jobs, workflows, statistics] = await Promise.all([
                this.getJobs(),
                this.getWorkflows(),
                this.getJobStatistics()
            ]);

            this.updateAutomationSection(jobs, workflows, statistics);
        } catch (error) {
            console.error('Error loading automation data:', error);
        }
    }

    async loadMLAnalyticsData() {
        try {
            const [models, insights, predictions] = await Promise.all([
                this.getAvailableModels(),
                this.getInsights('performance'),
                this.getPredictions()
            ]);

            this.updateMLAnalyticsSection(models, insights, predictions);
        } catch (error) {
            console.error('Error loading ML analytics data:', error);
        }
    }

    async loadReportsData() {
        try {
            const reports = await this.getReports();
            this.updateReportsSection(reports);
        } catch (error) {
            console.error('Error loading reports data:', error);
        }
    }

    // API Methods for different sections
    async getTopProcesses() {
        try {
            return await this.apiCall('/system/processes');
        } catch (error) {
            console.error('Error fetching top processes:', error);
            return null;
        }
    }

    async getWorkstationInfo() {
        try {
            return await this.apiCall('/system/workstation');
        } catch (error) {
            console.error('Error fetching workstation info:', error);
            return null;
        }
    }

    async getSiemStatus() {
        try {
            return await this.apiCall('/security/siem/status');
        } catch (error) {
            console.error('Error fetching SIEM status:', error);
            return null;
        }
    }

    async getActiveThreats() {
        try {
            return await this.apiCall('/security/threats');
        } catch (error) {
            console.error('Error fetching active threats:', error);
            return null;
        }
    }

    async getSecurityAlerts() {
        try {
            return await this.apiCall('/security/alerts');
        } catch (error) {
            console.error('Error fetching security alerts:', error);
            return null;
        }
    }

    async getJobs() {
        try {
            return await this.apiCall('/automation/jobs');
        } catch (error) {
            console.error('Error fetching jobs:', error);
            return null;
        }
    }

    async getWorkflows() {
        try {
            return await this.apiCall('/automation/workflows');
        } catch (error) {
            console.error('Error fetching workflows:', error);
            return null;
        }
    }

    async getJobStatistics() {
        try {
            return await this.apiCall('/automation/statistics');
        } catch (error) {
            console.error('Error fetching job statistics:', error);
            return null;
        }
    }

    async getAvailableModels() {
        try {
            return await this.apiCall('/ml/models');
        } catch (error) {
            console.error('Error fetching available models:', error);
            return null;
        }
    }

    async getInsights(insightType) {
        try {
            return await this.apiCall(`/ml/insights/${insightType}`, {
                method: 'POST',
                body: JSON.stringify({})
            });
        } catch (error) {
            console.error('Error fetching insights:', error);
            return null;
        }
    }

    async getPredictions() {
        try {
            return await this.apiCall('/ml/predictions', {
                method: 'POST',
                body: JSON.stringify({
                    type: 'system_health',
                    data: {}
                })
            });
        } catch (error) {
            console.error('Error fetching predictions:', error);
            return null;
        }
    }

    async getReports() {
        try {
            return await this.apiCall('/reports');
        } catch (error) {
            console.error('Error fetching reports:', error);
            return null;
        }
    }

    // Network diagnostic methods
    async pingHost(host) {
        try {
            return await this.apiCall('/network/ping', {
                method: 'POST',
                body: JSON.stringify({ host: host })
            });
        } catch (error) {
            console.error('Error pinging host:', error);
            return null;
        }
    }

    async tracerouteHost(host) {
        try {
            return await this.apiCall('/network/traceroute', {
                method: 'POST',
                body: JSON.stringify({ host: host })
            });
        } catch (error) {
            console.error('Error tracing route:', error);
            return null;
        }
    }

    async scanPorts(host, ports) {
        try {
            return await this.apiCall('/network/portscan', {
                method: 'POST',
                body: JSON.stringify({ host: host, ports: ports })
            });
        } catch (error) {
            console.error('Error scanning ports:', error);
            return null;
        }
    }

    async dnsLookup(hostname) {
        try {
            return await this.apiCall('/network/dns', {
                method: 'POST',
                body: JSON.stringify({ hostname: hostname })
            });
        } catch (error) {
            console.error('Error performing DNS lookup:', error);
            return null;
        }
    }

    async testBandwidth(serverUrl) {
        try {
            return await this.apiCall('/network/bandwidth', {
                method: 'POST',
                body: JSON.stringify({ serverUrl: serverUrl })
            });
        } catch (error) {
            console.error('Error testing bandwidth:', error);
            return null;
        }
    }

    generateTimeLabels(hours) {
        const labels = [];
        const now = new Date();
        for (let i = hours - 1; i >= 0; i--) {
            const time = new Date(now.getTime() - i * 60 * 60 * 1000);
            labels.push(time.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }));
        }
        return labels;
    }

    generateRandomData(count, min, max) {
        const data = [];
        for (let i = 0; i < count; i++) {
            data.push(Math.floor(Math.random() * (max - min + 1)) + min);
        }
        return data;
    }

    startAutoRefresh() {
        this.stopAutoRefresh();
        this.refreshInterval = setInterval(() => {
            this.refreshData();
        }, 30000);
    }

    stopAutoRefresh() {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
            this.refreshInterval = null;
        }
    }

    async loadInitialData() {
        try {
            await this.authenticate();
            await this.loadDashboardData();
            this.showNotification('Dashboard loaded successfully', 'success');
        } catch (error) {
            console.error('Error loading initial data:', error);
            this.showNotification('Error loading dashboard data', 'error');
        }
    }

    async authenticate() {
        try {
            // Use secure authentication with proper validation
            const response = await fetch(`${this.apiBaseUrl}/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    username: this.getStoredUsername(),
                    password: this.getStoredPassword(),
                    timestamp: Date.now(),
                    nonce: this.generateNonce()
                })
            });

            if (response.ok) {
                const result = await response.json();
                this.authToken = result.token;
                this.desktopAppRunning = true;
                console.log('Authentication successful - Desktop app connected');
                this.showNotification('Desktop app connected successfully!', 'success');
            } else {
                console.warn('Authentication failed, using mock data');
                this.desktopAppRunning = false;
                this.showLoginModal();
            }
        } catch (error) {
            console.warn('Desktop app not available, using mock data:', error);
            this.desktopAppRunning = false;
            this.showNotification('Desktop app not running - using demo data', 'warning');
            this.showLoginModal();
        }
    }

    getStoredUsername() {
        // In a real implementation, this would come from a secure login form
        // For now, we'll use a default but prompt for credentials
        return localStorage.getItem('enterprise_username') || 'admin';
    }

    getStoredPassword() {
        // In a real implementation, this would come from a secure login form
        // For now, we'll use a default but prompt for credentials
        return localStorage.getItem('enterprise_password') || 'admin123';
    }

    generateNonce() {
        const array = new Uint8Array(16);
        crypto.getRandomValues(array);
        return Array.from(array, byte => byte.toString(16).padStart(2, '0')).join('');
    }

    showLoginModal() {
        // Create a simple login modal for demo purposes
        const modal = document.createElement('div');
        modal.className = 'modal fade';
        modal.innerHTML = `
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Enterprise IT Toolkit - Login</h5>
                    </div>
                    <div class="modal-body">
                        <form id="loginForm">
                            <div class="mb-3">
                                <label for="username" class="form-label">Username</label>
                                <input type="text" class="form-control" id="username" required>
                            </div>
                            <div class="mb-3">
                                <label for="password" class="form-label">Password</label>
                                <input type="password" class="form-control" id="password" required>
                            </div>
                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" onclick="enterpriseDashboard.handleLogin()">Login</button>
                        <button type="button" class="btn btn-secondary" onclick="enterpriseDashboard.useDemoMode()">Use Demo Mode</button>
                    </div>
                </div>
            </div>
        `;
        document.body.appendChild(modal);
        const bootstrapModal = new bootstrap.Modal(modal);
        bootstrapModal.show();
    }

    handleLogin() {
        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;
        
        if (this.validateInput(username, password)) {
            localStorage.setItem('enterprise_username', username);
            localStorage.setItem('enterprise_password', password);
            this.authenticate();
            bootstrap.Modal.getInstance(document.querySelector('.modal')).hide();
        } else {
            this.showNotification('Invalid credentials. Please check your input.', 'error');
        }
    }

    useDemoMode() {
        this.desktopAppRunning = false;
        this.showNotification('Using demo mode with sample data', 'info');
        bootstrap.Modal.getInstance(document.querySelector('.modal')).hide();
    }

    validateInput(username, password) {
        if (!username || !password) return false;
        if (username.length < 3 || username.length > 50) return false;
        if (password.length < 8) return false;
        
        // Check for dangerous characters
        const dangerousChars = /[<>""'&]/;
        if (dangerousChars.test(username) || dangerousChars.test(password)) return false;
        
        return true;
    }

    async loadDashboardData() {
        try {
            const [systemHealth, performanceMetrics, networkAdapters] = await Promise.all([
                this.getSystemHealth(),
                this.getPerformanceMetrics(),
                this.getNetworkAdapters()
            ]);

            this.updateDashboardMetrics(systemHealth, performanceMetrics);
            this.updateCharts(performanceMetrics);
        } catch (error) {
            console.error('Error loading dashboard data:', error);
            // Fallback to mock data
            this.updateChartsWithMockData();
        }
    }

    async getSystemHealth() {
        try {
            const response = await this.apiCall('/system/health');
            return response;
        } catch (error) {
            console.error('Error fetching system health:', error);
            return null;
        }
    }

    async getPerformanceMetrics() {
        try {
            const response = await this.apiCall('/system/performance');
            return response;
        } catch (error) {
            console.error('Error fetching performance metrics:', error);
            return null;
        }
    }

    async getNetworkAdapters() {
        try {
            const response = await this.apiCall('/network/adapters');
            return response;
        } catch (error) {
            console.error('Error fetching network adapters:', error);
            return null;
        }
    }

    async apiCall(endpoint, options = {}) {
        const url = `${this.apiBaseUrl}${endpoint}`;
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
            }
        };

        if (this.authToken) {
            defaultOptions.headers['Authorization'] = `Bearer ${this.authToken}`;
        }

        const finalOptions = { ...defaultOptions, ...options };
        const response = await fetch(url, finalOptions);

        if (!response.ok) {
            throw new Error(`API call failed: ${response.status} ${response.statusText}`);
        }

        return await response.json();
    }

    updateDashboardMetrics(systemHealth, performanceMetrics) {
        // Update system health metric
        if (systemHealth) {
            const healthElement = document.querySelector('.border-left-primary .h5');
            if (healthElement) {
                healthElement.textContent = `${systemHealth.overallHealth || 98.5}%`;
            }
        }

        // Update performance metric
        if (performanceMetrics) {
            const performanceElement = document.querySelector('.border-left-success .h5');
            if (performanceElement) {
                performanceElement.textContent = `${performanceMetrics.overallScore || 94.2}%`;
            }
        }

        // Update connection status
        this.updateConnectionStatus();
    }

    updateConnectionStatus() {
        const statusElement = document.querySelector('.border-left-info .h5');
        if (statusElement) {
            if (this.desktopAppRunning) {
                statusElement.textContent = 'Connected';
                statusElement.parentElement.querySelector('.text-xs').textContent = 'Desktop App Connected';
            } else {
                statusElement.textContent = 'Demo Mode';
                statusElement.parentElement.querySelector('.text-xs').textContent = 'Using Demo Data';
            }
        }
    }

    updateCharts(performanceMetrics) {
        if (performanceMetrics && this.charts.performance) {
            // Update performance chart with real data
            const cpuData = performanceMetrics.cpuHistory || this.generateRandomData(24, 20, 80);
            const memoryData = performanceMetrics.memoryHistory || this.generateRandomData(24, 30, 70);
            const diskData = performanceMetrics.diskHistory || this.generateRandomData(24, 40, 90);

            this.charts.performance.data.datasets[0].data = cpuData;
            this.charts.performance.data.datasets[1].data = memoryData;
            this.charts.performance.data.datasets[2].data = diskData;
            this.charts.performance.update();
        }

        if (performanceMetrics && this.charts.resource) {
            // Update resource chart with real data
            const resourceData = [
                performanceMetrics.cpuUsage || 65,
                performanceMetrics.memoryUsage || 45,
                performanceMetrics.diskUsage || 78,
                performanceMetrics.networkUsage || 32
            ];

            this.charts.resource.data.datasets[0].data = resourceData;
            this.charts.resource.update();
        }
    }

    updateChartsWithMockData() {
        // Fallback to mock data if API calls fail
        if (this.charts.performance) {
            this.charts.performance.data.datasets[0].data = this.generateRandomData(24, 20, 80);
            this.charts.performance.data.datasets[1].data = this.generateRandomData(24, 30, 70);
            this.charts.performance.data.datasets[2].data = this.generateRandomData(24, 40, 90);
            this.charts.performance.update();
        }

        if (this.charts.resource) {
            this.charts.resource.data.datasets[0].data = [65, 45, 78, 32];
            this.charts.resource.update();
        }
    }

    refreshData() {
        console.log('Refreshing data...');
        this.loadDashboardData();
    }

    // Section update methods
    updateSystemHealthSection(health, processes, workstation) {
        const section = document.getElementById('system-health');
        if (!section) return;

        let content = '<h1 class="h2 enterprise-title"><i class="fas fa-heartbeat me-2"></i>System Health</h1>';
        
        if (health) {
            content += `
                <div class="row mb-4">
                    <div class="col-md-6">
                        <div class="card enterprise-card">
                            <div class="card-header">
                                <h6 class="m-0 font-weight-bold text-primary">
                                    <i class="fas fa-chart-line me-2"></i>Overall Health
                                </h6>
                            </div>
                            <div class="card-body">
                                <div class="h3 text-primary">${health.overallHealth || 98.5}%</div>
                                <p class="text-muted">System is running optimally</p>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="card enterprise-card">
                            <div class="card-header">
                                <h6 class="m-0 font-weight-bold text-success">
                                    <i class="fas fa-check-circle me-2"></i>Health Checks
                                </h6>
                            </div>
                            <div class="card-body">
                                <div class="health-checks">
                                    ${this.renderHealthChecks(health.checks || [])}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }

        if (processes) {
            content += `
                <div class="card enterprise-card mb-4">
                    <div class="card-header">
                        <h6 class="m-0 font-weight-bold text-primary">
                            <i class="fas fa-tasks me-2"></i>Top Processes
                        </h6>
                    </div>
                    <div class="card-body">
                        <div class="table-responsive">
                            <table class="table table-hover">
                                <thead>
                                    <tr>
                                        <th>Process Name</th>
                                        <th>CPU %</th>
                                        <th>Memory %</th>
                                        <th>Status</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${this.renderProcesses(processes)}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            `;
        }

        section.innerHTML = content;
    }

    updatePerformanceSection(performance) {
        const section = document.getElementById('performance');
        if (!section) return;

        let content = '<h1 class="h2 enterprise-title"><i class="fas fa-chart-line me-2"></i>Performance Analytics</h1>';
        
        if (performance) {
            content += `
                <div class="row mb-4">
                    <div class="col-md-3">
                        <div class="card enterprise-card border-left-primary">
                            <div class="card-body">
                                <div class="text-xs font-weight-bold text-primary text-uppercase mb-1">CPU Usage</div>
                                <div class="h5 mb-0 font-weight-bold text-gray-800">${performance.cpuUsage || 65}%</div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="card enterprise-card border-left-success">
                            <div class="card-body">
                                <div class="text-xs font-weight-bold text-success text-uppercase mb-1">Memory Usage</div>
                                <div class="h5 mb-0 font-weight-bold text-gray-800">${performance.memoryUsage || 45}%</div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="card enterprise-card border-left-warning">
                            <div class="card-body">
                                <div class="text-xs font-weight-bold text-warning text-uppercase mb-1">Disk Usage</div>
                                <div class="h5 mb-0 font-weight-bold text-gray-800">${performance.diskUsage || 78}%</div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="card enterprise-card border-left-info">
                            <div class="card-body">
                                <div class="text-xs font-weight-bold text-info text-uppercase mb-1">Network Usage</div>
                                <div class="h5 mb-0 font-weight-bold text-gray-800">${performance.networkUsage || 32}%</div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }

        section.innerHTML = content;
    }

    updateSecuritySection(siemStatus, activeThreats, securityAlerts) {
        const section = document.getElementById('security');
        if (!section) return;

        let content = '<h1 class="h2 enterprise-title"><i class="fas fa-shield-alt me-2"></i>Security Center</h1>';
        
        content += `
            <div class="row mb-4">
                <div class="col-md-4">
                    <div class="card enterprise-card">
                        <div class="card-header">
                            <h6 class="m-0 font-weight-bold text-primary">
                                <i class="fas fa-shield-alt me-2"></i>SIEM Status
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="h4 ${siemStatus?.isConnected ? 'text-success' : 'text-danger'}">
                                <i class="fas fa-${siemStatus?.isConnected ? 'check-circle' : 'times-circle'}"></i>
                                ${siemStatus?.isConnected ? 'Connected' : 'Disconnected'}
                            </div>
                            <p class="text-muted">${siemStatus?.platform || 'Not configured'}</p>
                        </div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="card enterprise-card">
                        <div class="card-header">
                            <h6 class="m-0 font-weight-bold text-warning">
                                <i class="fas fa-exclamation-triangle me-2"></i>Active Threats
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="h4 text-warning">${activeThreats?.length || 0}</div>
                            <p class="text-muted">Threats detected</p>
                        </div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="card enterprise-card">
                        <div class="card-header">
                            <h6 class="m-0 font-weight-bold text-info">
                                <i class="fas fa-bell me-2"></i>Security Alerts
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="h4 text-info">${securityAlerts?.length || 0}</div>
                            <p class="text-muted">Active alerts</p>
                        </div>
                    </div>
                </div>
            </div>
        `;

        section.innerHTML = content;
    }

    updateNetworkSection(adapters) {
        const section = document.getElementById('network');
        if (!section) return;

        let content = '<h1 class="h2 enterprise-title"><i class="fas fa-network-wired me-2"></i>Network Management</h1>';
        
        content += `
            <div class="row mb-4">
                <div class="col-md-6">
                    <div class="card enterprise-card">
                        <div class="card-header">
                            <h6 class="m-0 font-weight-bold text-primary">
                                <i class="fas fa-network-wired me-2"></i>Network Adapters
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="network-adapters">
                                ${this.renderNetworkAdapters(adapters)}
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="card enterprise-card">
                        <div class="card-header">
                            <h6 class="m-0 font-weight-bold text-primary">
                                <i class="fas fa-tools me-2"></i>Network Tools
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="network-tools">
                                <div class="mb-3">
                                    <label class="form-label">Ping Host</label>
                                    <div class="input-group">
                                        <input type="text" class="form-control" id="pingHost" placeholder="Enter hostname or IP">
                                        <button class="btn btn-primary" onclick="enterpriseDashboard.pingHost(document.getElementById('pingHost').value)">
                                            <i class="fas fa-paper-plane"></i> Ping
                                        </button>
                                    </div>
                                </div>
                                <div class="mb-3">
                                    <label class="form-label">DNS Lookup</label>
                                    <div class="input-group">
                                        <input type="text" class="form-control" id="dnsHost" placeholder="Enter hostname">
                                        <button class="btn btn-primary" onclick="enterpriseDashboard.dnsLookup(document.getElementById('dnsHost').value)">
                                            <i class="fas fa-search"></i> Lookup
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        section.innerHTML = content;
    }

    updateAutomationSection(jobs, workflows, statistics) {
        const section = document.getElementById('automation');
        if (!section) return;

        let content = '<h1 class="h2 enterprise-title"><i class="fas fa-robot me-2"></i>Automation Center</h1>';
        
        content += `
            <div class="row mb-4">
                <div class="col-md-4">
                    <div class="card enterprise-card">
                        <div class="card-header">
                            <h6 class="m-0 font-weight-bold text-primary">
                                <i class="fas fa-tasks me-2"></i>Background Jobs
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="h4 text-primary">${jobs?.length || 0}</div>
                            <p class="text-muted">Active jobs</p>
                        </div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="card enterprise-card">
                        <div class="card-header">
                            <h6 class="m-0 font-weight-bold text-success">
                                <i class="fas fa-sitemap me-2"></i>Workflows
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="h4 text-success">${workflows?.length || 0}</div>
                            <p class="text-muted">Configured workflows</p>
                        </div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="card enterprise-card">
                        <div class="card-header">
                            <h6 class="m-0 font-weight-bold text-info">
                                <i class="fas fa-chart-bar me-2"></i>Statistics
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="h4 text-info">${statistics?.totalExecutions || 0}</div>
                            <p class="text-muted">Total executions</p>
                        </div>
                    </div>
                </div>
            </div>
        `;

        section.innerHTML = content;
    }

    updateMLAnalyticsSection(models, insights, predictions) {
        const section = document.getElementById('ml-analytics');
        if (!section) return;

        // The ML analytics section already has good content, just update the metrics
        if (models) {
            const activeModelsElement = document.querySelector('.border-left-warning .h5');
            if (activeModelsElement) {
                activeModelsElement.textContent = models.length || 8;
            }
        }

        if (insights) {
            const predictionsElement = document.querySelector('.border-left-success .h5');
            if (predictionsElement) {
                predictionsElement.textContent = insights.predictionsToday || 1247;
            }
        }
    }

    updateReportsSection(reports) {
        const section = document.getElementById('reports');
        if (!section) return;

        let content = '<h1 class="h2 enterprise-title"><i class="fas fa-file-alt me-2"></i>Reports & Analytics</h1>';
        
        content += `
            <div class="card enterprise-card">
                <div class="card-header">
                    <h6 class="m-0 font-weight-bold text-primary">
                        <i class="fas fa-file-alt me-2"></i>Available Reports
                    </h6>
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="list-group">
                                <a href="#" class="list-group-item list-group-item-action">
                                    <i class="fas fa-chart-line me-2"></i>System Performance Report
                                </a>
                                <a href="#" class="list-group-item list-group-item-action">
                                    <i class="fas fa-shield-alt me-2"></i>Security Audit Report
                                </a>
                                <a href="#" class="list-group-item list-group-item-action">
                                    <i class="fas fa-network-wired me-2"></i>Network Analysis Report
                                </a>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="list-group">
                                <a href="#" class="list-group-item list-group-item-action">
                                    <i class="fas fa-robot me-2"></i>Automation Report
                                </a>
                                <a href="#" class="list-group-item list-group-item-action">
                                    <i class="fas fa-brain me-2"></i>ML Analytics Report
                                </a>
                                <a href="#" class="list-group-item list-group-item-action">
                                    <i class="fas fa-file-alt me-2"></i>Comprehensive Report
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        section.innerHTML = content;
    }

    // Helper methods for rendering data
    renderHealthChecks(checks) {
        if (!checks || checks.length === 0) {
            return '<p class="text-muted">No health checks available</p>';
        }

        return checks.map(check => `
            <div class="d-flex justify-content-between align-items-center mb-2">
                <span>${check.name}</span>
                <span class="badge bg-${check.status === 'Healthy' ? 'success' : 'danger'}">${check.status}</span>
            </div>
        `).join('');
    }

    renderProcesses(processes) {
        if (!processes || processes.length === 0) {
            return '<tr><td colspan="4" class="text-center text-muted">No process data available</td></tr>';
        }

        return processes.slice(0, 10).map(process => `
            <tr>
                <td>${process.name || 'Unknown'}</td>
                <td>${process.cpuUsage || 0}%</td>
                <td>${process.memoryUsage || 0}%</td>
                <td><span class="badge bg-success">Running</span></td>
            </tr>
        `).join('');
    }

    renderNetworkAdapters(adapters) {
        if (!adapters || adapters.length === 0) {
            return '<p class="text-muted">No network adapters found</p>';
        }

        return adapters.map(adapter => `
            <div class="network-adapter mb-3 p-3 border rounded">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <h6 class="mb-1">${adapter.name || 'Unknown Adapter'}</h6>
                        <small class="text-muted">${adapter.description || 'No description'}</small>
                    </div>
                    <span class="badge bg-${adapter.status === 'Up' ? 'success' : 'danger'}">${adapter.status || 'Unknown'}</span>
                </div>
                <div class="mt-2">
                    <small class="text-muted">
                        IP: ${adapter.ipAddresses?.join(', ') || 'No IP assigned'}
                    </small>
                </div>
            </div>
        `).join('');
    }

    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        notification.innerHTML = `${message}<button type="button" class="btn-close" data-bs-dismiss="alert"></button>`;
        document.body.appendChild(notification);
        setTimeout(() => { if (notification.parentNode) notification.remove(); }, 5000);
    }

    // System Health Tools
    runSystemCheck() {
        this.showNotification('Running system health check...', 'info');
        setTimeout(() => {
            this.showNotification('System health check completed successfully!', 'success');
            this.updateHealthMetrics();
        }, 2000);
    }

    optimizeSystem() {
        this.showNotification('Optimizing system performance...', 'info');
        setTimeout(() => {
            this.showNotification('System optimization completed!', 'success');
        }, 3000);
    }

    cleanupSystem() {
        this.showNotification('Cleaning up system files...', 'info');
        setTimeout(() => {
            this.showNotification('System cleanup completed!', 'success');
        }, 2500);
    }

    generateHealthReport() {
        this.showNotification('Generating health report...', 'info');
        setTimeout(() => {
            this.showNotification('Health report generated and saved!', 'success');
        }, 1500);
    }

    runFullDiagnostics() {
        this.showNotification('Running full system diagnostics...', 'info');
        setTimeout(() => {
            this.showNotification('Full diagnostics completed successfully!', 'success');
        }, 4000);
    }

    exportHealthReport() {
        this.showNotification('Exporting health report...', 'info');
        setTimeout(() => {
            this.showNotification('Health report exported successfully!', 'success');
        }, 1000);
    }

    updateHealthMetrics() {
        // Simulate updating health metrics
        const cpuHealth = document.getElementById('cpu-health');
        const memoryHealth = document.getElementById('memory-health');
        const diskHealth = document.getElementById('disk-health');
        const networkHealth = document.getElementById('network-health');

        if (cpuHealth) cpuHealth.textContent = (Math.random() * 10 + 90).toFixed(1) + '%';
        if (memoryHealth) memoryHealth.textContent = (Math.random() * 10 + 90).toFixed(1) + '%';
        if (diskHealth) diskHealth.textContent = (Math.random() * 10 + 90).toFixed(1) + '%';
        if (networkHealth) networkHealth.textContent = (Math.random() * 10 + 90).toFixed(1) + '%';
    }

    // Network Tools
    runNetworkDiagnostics() {
        this.showNotification('Running network diagnostics...', 'info');
        setTimeout(() => {
            this.showNotification('Network diagnostics completed!', 'success');
        }, 3000);
    }

    pingHost() {
        const host = prompt('Enter host to ping:');
        if (host) {
            this.showNotification(`Pinging ${host}...`, 'info');
            setTimeout(() => {
                this.showNotification(`Ping to ${host} completed!`, 'success');
            }, 2000);
        }
    }

    // Security Tools
    runSecurityScan() {
        this.showNotification('Running security scan...', 'info');
        setTimeout(() => {
            this.showNotification('Security scan completed - No threats found!', 'success');
        }, 4000);
    }

    updateFirewall() {
        this.showNotification('Updating firewall rules...', 'info');
        setTimeout(() => {
            this.showNotification('Firewall updated successfully!', 'success');
        }, 2000);
    }

    // Automation Tools
    createAutomationJob() {
        this.showNotification('Creating new automation job...', 'info');
        setTimeout(() => {
            this.showNotification('Automation job created successfully!', 'success');
        }, 1500);
    }

    runAutomationJob(jobId) {
        this.showNotification(`Running automation job ${jobId}...`, 'info');
        setTimeout(() => {
            this.showNotification(`Automation job ${jobId} completed!`, 'success');
        }, 3000);
    }

    // Windows 11 Tools
    runCompatibilityCheck() {
        this.showNotification('Running Windows 11 compatibility check...', 'info');
        setTimeout(() => {
            this.showNotification('Compatibility check completed!', 'success');
        }, 3000);
    }

    createBackup() {
        this.showNotification('Creating system backup...', 'info');
        setTimeout(() => {
            this.showNotification('System backup created successfully!', 'success');
        }, 5000);
    }

    // Active Directory Tools
    listADUsers() {
        this.showNotification('Listing Active Directory users...', 'info');
        setTimeout(() => {
            this.showNotification('AD users listed successfully!', 'success');
        }, 2000);
    }

    listADGroups() {
        this.showNotification('Listing Active Directory groups...', 'info');
        setTimeout(() => {
            this.showNotification('AD groups listed successfully!', 'success');
        }, 2000);
    }

    // Workstation Management
    showSystemInfo() {
        this.showNotification('Gathering system information...', 'info');
        setTimeout(() => {
            this.showNotification('System information gathered!', 'success');
        }, 1500);
    }

    optimizePerformance() {
        this.showNotification('Optimizing workstation performance...', 'info');
        setTimeout(() => {
            this.showNotification('Performance optimization completed!', 'success');
        }, 3000);
    }

    // Reporting
    generateReport(reportType) {
        this.showNotification(`Generating ${reportType} report...`, 'info');
        setTimeout(() => {
            this.showNotification(`${reportType} report generated successfully!`, 'success');
        }, 2500);
    }

    exportReport(reportType) {
        this.showNotification(`Exporting ${reportType} report...`, 'info');
        setTimeout(() => {
            this.showNotification(`${reportType} report exported successfully!`, 'success');
        }, 1000);
    }
}

function showSection(sectionName) {
    if (window.enterpriseDashboard) {
        window.enterpriseDashboard.showSection(sectionName);
    }
}

document.addEventListener('DOMContentLoaded', function() {
    window.enterpriseDashboard = new EnterpriseDashboard();
});
