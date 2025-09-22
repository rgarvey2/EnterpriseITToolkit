const express = require('express');
const path = require('path');
const app = express();
const port = 8081;

// Serve static files from Web/wwwroot directory
app.use(express.static('./Web/wwwroot'));

// Serve the desktop-style HTML file as default
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'Web/wwwroot/desktop-style.html'));
});

// Serve the test HTML file
app.get('/test', (req, res) => {
    res.sendFile(path.join(__dirname, 'test-api-simple.html'));
});

app.listen(port, () => {
    console.log(`🚀 Test server running at http://localhost:${port}`);
    console.log(`📋 Open http://localhost:${port} to test the API`);
});
