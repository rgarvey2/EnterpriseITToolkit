// Secure Authentication System
class SecureAuth {
    constructor() {
        this.apiBaseUrl = '/api';
        this.tokenKey = 'auth_token';
        this.expiryKey = 'token_expires';
    }
    
    async login(username, password) {
        // Validate input
        if (!this.validateInput(username, password)) {
            throw new Error('Invalid input provided');
        }
        
        try {
            const response = await fetch(`${this.apiBaseUrl}/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    username: username,
                    password: password,
                    timestamp: Date.now(),
                    nonce: this.generateNonce()
                })
            });
            
            if (!response.ok) {
                throw new Error('Authentication failed');
            }
            
            const result = await response.json();
            this.storeTokenSecurely(result.token);
            return result;
        } catch (error) {
            console.error('Login error:', error);
            throw error;
        }
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
    
    generateNonce() {
        const array = new Uint8Array(16);
        crypto.getRandomValues(array);
        return Array.from(array, byte => byte.toString(16).padStart(2, '0')).join('');
    }
    
    storeTokenSecurely(token) {
        // Store in session storage (not localStorage for security)
        sessionStorage.setItem(this.tokenKey, token);
        
        // Set expiration (15 minutes)
        const expiration = new Date(Date.now() + 15 * 60 * 1000);
        sessionStorage.setItem(this.expiryKey, expiration.toISOString());
    }
    
    getToken() {
        const token = sessionStorage.getItem(this.tokenKey);
        const expiry = sessionStorage.getItem(this.expiryKey);
        
        if (!token || !expiry) return null;
        
        // Check if token is expired
        if (new Date() > new Date(expiry)) {
            this.logout();
            return null;
        }
        
        return token;
    }
    
    logout() {
        sessionStorage.removeItem(this.tokenKey);
        sessionStorage.removeItem(this.expiryKey);
        window.location.href = '/';
    }
    
    isAuthenticated() {
        return this.getToken() !== null;
    }
    
    async makeAuthenticatedRequest(url, options = {}) {
        const token = this.getToken();
        if (!token) {
            throw new Error('Not authenticated');
        }
        
        const defaultOptions = {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        };
        
        const finalOptions = { ...defaultOptions, ...options };
        if (finalOptions.headers) {
            finalOptions.headers = { ...defaultOptions.headers, ...finalOptions.headers };
        }
        
        const response = await fetch(url, finalOptions);
        
        if (response.status === 401) {
            this.logout();
            throw new Error('Session expired');
        }
        
        return response;
    }
}

// Initialize secure auth
window.secureAuth = new SecureAuth();
