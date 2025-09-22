# 🔧 **MANUAL CONFIGURATION STEPS**

## 📋 **VIRTUAL MEMORY CONFIGURATION**

### **System Information:**
- **Total Physical Memory**: 32 GB
- **Recommended Initial Size**: 48 MB (1.5x physical memory)
- **Recommended Maximum Size**: 96 MB (3x physical memory)

### **Step-by-Step Instructions:**

1. **Press Windows + R**, type `sysdm.cpl` and press Enter
2. **Click 'Advanced' tab**
3. **Under 'Performance', click 'Settings'**
4. **Click 'Advanced' tab**
5. **Under 'Virtual memory', click 'Change'**
6. **Uncheck 'Automatically manage paging file size for all drives'**
7. **Select your system drive (usually C:)**
8. **Select 'Custom size'**
9. **Set Initial size: 48 MB**
10. **Set Maximum size: 96 MB**
11. **Click 'Set', then 'OK'**
12. **Restart your computer when prompted**

---

## 🐳 **DOCKER DESKTOP CONFIGURATION**

### **System Information:**
- **Total Memory**: 32 GB
- **CPU Cores**: Available cores
- **Recommended Memory Limit**: 6-8 GB (25% of RAM)
- **Recommended CPU Limit**: 4-6 cores (50% of cores)

### **Step-by-Step Instructions:**

#### **Method 1: Docker Desktop GUI**
1. **Right-click on Docker Desktop icon** in system tray
2. **Select 'Settings' or 'Preferences'**
3. **Go to 'Resources' section**
4. **Configure the following settings:**
   - **Memory**: Set to **6-8 GB**
   - **CPUs**: Set to **4-6 cores**
   - **Disk image size**: Set to **64GB** (or as needed)
5. **Go to 'General' section**
6. **Enable 'Use WSL 2 based engine'** (if available)
7. **Go to 'Resources' → 'WSL Integration'**
8. **Disable WSL integration** for unused distributions
9. **Go to 'Resources' → 'Advanced'**
10. **Enable 'Enable file sharing'** for necessary drives only
11. **Click 'Apply & Restart'**

#### **Method 2: Settings File (Advanced)**
1. **Close Docker Desktop**
2. **Navigate to**: `%APPDATA%\Docker\settings.json`
3. **Create backup** of current settings
4. **Replace with optimized configuration**:

```json
{
  "memoryMiB": 8192,
  "cpus": 6,
  "diskSizeMiB": 65536,
  "vmType": "wsl2",
  "wslEngineEnabled": true,
  "fileSharingDirectories": [
    "C:\\Users\\ryan.garvey\\Desktop",
    "C:\\Users\\ryan.garvey\\Documents"
  ],
  "experimental": false,
  "autoStart": true,
  "startOnLogin": true,
  "analytics": false,
  "crashReporting": false,
  "updates": {
    "enabled": true,
    "channel": "stable"
  }
}
```

5. **Save the file**
6. **Start Docker Desktop**

---

## ✅ **VERIFICATION STEPS**

### **Virtual Memory Verification:**
1. **After restart**, open **Task Manager**
2. **Go to 'Performance' tab**
3. **Click 'Memory'**
4. **Check 'Committed'** section for virtual memory usage

### **Docker Desktop Verification:**
1. **Open Command Prompt or PowerShell**
2. **Run**: `docker system info`
3. **Check memory and CPU limits** in the output
4. **Run**: `docker system df`
5. **Check disk usage** and cleanup if needed

---

## 🚀 **OPTIMIZATION TIPS**

### **Virtual Memory:**
- **Initial size**: 1.5x physical memory (48 MB for 32GB system)
- **Maximum size**: 3x physical memory (96 MB for 32GB system)
- **Location**: Keep on fastest drive (usually C:)

### **Docker Desktop:**
- **Memory**: 25% of total RAM (6-8GB for 32GB system)
- **CPU**: 50% of available cores (4-6 cores)
- **WSL 2**: Enable for better performance
- **File sharing**: Only enable necessary drives
- **Updates**: Enable automatic updates

---

## ⚠️ **IMPORTANT NOTES**

1. **Restart Required**: Both configurations require system restart
2. **Backup Settings**: Always backup current settings before changes
3. **Monitor Performance**: Check system performance after changes
4. **Adjust as Needed**: Fine-tune settings based on usage patterns

---

## 🎯 **EXPECTED RESULTS**

### **After Virtual Memory Configuration:**
- **Better memory management**
- **Reduced disk swapping**
- **Improved system stability**

### **After Docker Desktop Configuration:**
- **Reduced resource consumption**
- **Better container performance**
- **Improved system responsiveness**
- **Prevented resource exhaustion**

---

## 📞 **TROUBLESHOOTING**

### **If Virtual Memory Issues:**
- **Check disk space** on system drive
- **Verify settings** in System Properties
- **Restart** if changes don't take effect

### **If Docker Desktop Issues:**
- **Restart Docker Desktop** after configuration
- **Check Docker logs** for errors
- **Verify WSL 2** is properly installed
- **Reset Docker Desktop** if needed

---

**Configuration completed! Your system should now be optimized for better performance.** 🚀
