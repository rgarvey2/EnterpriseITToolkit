# Enhanced Security Configuration
# Windows Defender Advanced Settings

# Enable Controlled Folder Access
Set-MpPreference -EnableControlledFolderAccess Enabled

# Enable Network Protection
Set-MpPreference -EnableNetworkProtection Enabled

# Enable Cloud Protection
Set-MpPreference -EnableCloudProtection Enabled

# Set Real-time Protection
Set-MpPreference -DisableRealtimeMonitoring False

# Enable Behavior Monitoring
Set-MpPreference -DisableBehaviorMonitoring False

# Enable IOAV Protection
Set-MpPreference -DisableIOAVProtection False

# Enable Script Scanning
Set-MpPreference -DisableScriptScanning False
