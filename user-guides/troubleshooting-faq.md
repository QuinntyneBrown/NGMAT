# Troubleshooting and FAQ

Find solutions to common issues and answers to frequently asked questions about NGMAT.

## Table of Contents

- [Installation Issues](#installation-issues)
- [Login and Authentication](#login-and-authentication)
- [Mission Management](#mission-management)
- [Propagation Issues](#propagation-issues)
- [Visualization Problems](#visualization-problems)
- [Performance Issues](#performance-issues)
- [Data Import/Export](#data-importexport)
- [Frequently Asked Questions](#frequently-asked-questions)

## Installation Issues

### Cannot Install Desktop Application

**Issue:** Installer fails or won't run

**Solutions:**
1. **Check system requirements:**
   - Windows 10/11 64-bit
   - .NET 8.0 Runtime installed
   - Admin privileges

2. **Install .NET Runtime:**
   - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/)
   - Install .NET 8.0 Runtime (not SDK)
   - Restart computer

3. **Run as Administrator:**
   - Right-click installer
   - Select "Run as administrator"

4. **Check antivirus:**
   - Temporarily disable antivirus
   - Try installation again
   - Re-enable antivirus after install

5. **Check disk space:**
   - Ensure at least 2 GB free space
   - Clear temporary files if needed

### Web Application Won't Load

**Issue:** Blank page or errors in browser

**Solutions:**
1. **Clear browser cache:**
   - Ctrl+Shift+Delete (Chrome/Edge)
   - Clear cached images and files
   - Restart browser

2. **Check browser compatibility:**
   - Use Chrome, Firefox, Edge, or Safari
   - Update to latest version
   - Enable JavaScript

3. **Disable extensions:**
   - Try incognito/private mode
   - Disable ad blockers temporarily
   - Check for conflicting extensions

4. **Check internet connection:**
   - Verify connection is active
   - Try different network
   - Check firewall settings

## Login and Authentication

### Cannot Log In

**Issue:** Login fails with correct credentials

**Solutions:**
1. **Verify email and password:**
   - Check for typos
   - Ensure Caps Lock is off
   - Copy-paste carefully

2. **Reset password:**
   - Click "Forgot Password?"
   - Check email for reset link
   - Create new password

3. **Check account status:**
   - Verify email address
   - Check spam folder for verification email
   - Resend verification if needed

4. **Clear browser cookies:**
   - Clear site data
   - Restart browser
   - Try again

### Email Verification Not Received

**Issue:** Verification email doesn't arrive

**Solutions:**
1. **Check spam/junk folder:**
   - Look for email from NGMAT
   - Mark as "Not Spam"
   - Add to contacts

2. **Wait a few minutes:**
   - Email may be delayed
   - Check again after 5-10 minutes

3. **Resend verification:**
   - Click "Resend verification email"
   - Check inbox again

4. **Check email address:**
   - Ensure correct email entered
   - Update if wrong address used

### Two-Factor Authentication Issues

**Issue:** Cannot complete 2FA

**Solutions:**
1. **Time synchronization:**
   - Ensure device time is correct
   - Enable automatic time sync
   - Authenticator apps are time-sensitive

2. **Use backup codes:**
   - Enter backup code instead
   - Codes provided during 2FA setup

3. **Reset 2FA:**
   - Contact support
   - Verify identity
   - Disable and re-enable 2FA

## Mission Management

### Cannot Create Mission

**Issue:** Mission creation fails

**Solutions:**
1. **Check required fields:**
   - Mission name must be unique
   - Start epoch must be valid date
   - All required fields filled

2. **Check permissions:**
   - Ensure you have create permission
   - Contact administrator if needed

3. **Browser issues:**
   - Refresh page
   - Clear cache
   - Try different browser

### Mission Not Saving

**Issue:** Changes not being saved

**Solutions:**
1. **Check connection:**
   - Verify internet connection
   - Look for connection indicator
   - Changes auto-save when reconnected

2. **Check permissions:**
   - Ensure you have edit permission
   - Owner or Editor role required

3. **Reload mission:**
   - Close and reopen mission
   - Check if changes persisted

4. **Check for errors:**
   - Look for error messages
   - Validate all fields
   - Fix validation errors

### Cannot Delete Mission

**Issue:** Mission deletion fails

**Solutions:**
1. **Close mission:**
   - Ensure mission not open in another tab
   - Close all instances

2. **Check permissions:**
   - Only mission owner can delete
   - Request deletion from owner

3. **Try soft delete:**
   - Move to "Deleted" status first
   - Then permanently delete if needed

## Propagation Issues

### Propagation Fails

**Issue:** Propagation aborts with error

**Common Causes and Solutions:**

**1. Invalid Initial State:**
```
Error: "Orbit energy is positive (escape trajectory)"

Solution:
- Check velocity is not too high
- Verify state vector correctness
- Use Keplerian elements instead
```

**2. Perigee Below Surface:**
```
Error: "Altitude below minimum (collision with Earth)"

Solution:
- Check orbital elements
- Increase periapsis altitude
- Verify coordinate system
```

**3. Integration Failure:**
```
Error: "Step size reduced below minimum"

Solution:
- Increase tolerance
- Use simpler force models
- Check for unrealistic parameters
```

**4. Force Model Issues:**
```
Error: "Gravity model file not found"

Solution:
- Verify gravity model installed
- Use default model
- Reinstall application
```

### Propagation Too Slow

**Issue:** Takes too long to complete

**Solutions:**
1. **Optimize settings:**
   - Use RK45 instead of RK89
   - Increase step size (e.g., 60 → 120 seconds)
   - Reduce gravity degree (70x70 → 20x20)
   - Simplify drag model

2. **Reduce output:**
   - Output every 60s instead of every step
   - Limit data storage

3. **Shorter time span:**
   - Break into smaller segments
   - Propagate in batches

4. **Check system performance:**
   - Close other applications
   - Ensure adequate RAM
   - Check CPU usage

### Results Seem Inaccurate

**Issue:** Propagation results don't match expectations

**Solutions:**
1. **Check force models:**
   - Ensure appropriate models enabled
   - Verify gravity degree/order
   - Include drag for LEO
   - Include SRP for high orbits

2. **Check integrator settings:**
   - Use tighter tolerance (1e-12)
   - Smaller step size
   - Higher-order integrator (RK89)

3. **Verify initial state:**
   - Double-check orbital elements
   - Confirm coordinate system
   - Validate epoch

4. **Compare with known data:**
   - Use TLE for verification
   - Compare with published ephemeris
   - Check with another tool

## Visualization Problems

### 3D View Not Loading

**Issue:** Orbit visualization blank or not rendering

**Solutions:**
1. **Check WebGL support:**
   - Visit [get.webgl.org](https://get.webgl.org/)
   - Update graphics drivers
   - Try different browser

2. **Clear cache:**
   - Clear browser cache
   - Reload page

3. **Check data:**
   - Ensure propagation completed
   - Verify results exist
   - Check for empty dataset

4. **Reduce complexity:**
   - Fewer spacecraft visible
   - Shorter time span
   - Lower resolution

### Ground Track Not Showing

**Issue:** Ground track map is blank

**Solutions:**
1. **Check map library:**
   - Ensure internet connection
   - Map tiles may be loading
   - Wait a moment

2. **Check coordinates:**
   - Verify lat/lon within valid range
   - Check coordinate conversion

3. **Zoom out:**
   - Ground track may be off-screen
   - Reset view to default

### Charts Not Displaying

**Issue:** Time-series plots empty

**Solutions:**
1. **Check data:**
   - Ensure results available
   - Select valid parameter
   - Check time range

2. **Refresh:**
   - Close and reopen chart
   - Select parameter again

3. **Browser issues:**
   - Try different browser
   - Update browser
   - Disable extensions

## Performance Issues

### Application Slow or Laggy

**Issue:** UI unresponsive or slow

**Solutions:**
1. **Close unused tabs:**
   - Desktop: Close extra mission tabs
   - Web: Limit open browser tabs

2. **Check system resources:**
   - Close other applications
   - Check RAM usage
   - Restart application

3. **Reduce visualization:**
   - Hide complex 3D views
   - Limit orbit paths shown
   - Reduce animation speed

4. **Clear old data:**
   - Delete completed missions
   - Clear cache
   - Export and archive old data

### High Memory Usage

**Issue:** Application consuming too much RAM

**Solutions:**
1. **Close missions:**
   - Close missions not in use
   - Limit concurrent operations

2. **Reduce data:**
   - Limit propagation output
   - Export and clear results
   - Delete unnecessary data

3. **Restart application:**
   - Save work
   - Close and reopen
   - Memory released

## Data Import/Export

### Cannot Import TLE

**Issue:** TLE import fails

**Solutions:**
1. **Check format:**
   - Use 2 or 3-line format
   - No extra lines or spaces
   - Copy entire TLE

**Valid formats:**
```
Two-line:
1 25544U 98067A   26018.50000000  .00002182  00000-0  41420-4 0  9990
2 25544  51.6416 247.4627 0006703 130.5360 325.0288 15.72125391563710

Three-line:
ISS (ZARYA)
1 25544U 98067A   26018.50000000  .00002182  00000-0  41420-4 0  9990
2 25544  51.6416 247.4627 0006703 130.5360 325.0288 15.72125391563710
```

2. **Check source:**
   - Use reliable TLE source
   - Verify TLE is current
   - Try different TLE

3. **Manual entry:**
   - Extract elements manually
   - Enter as Keplerian elements

### Export Fails

**Issue:** Cannot export mission or data

**Solutions:**
1. **Check permissions:**
   - Ensure write permission
   - Check destination folder
   - Try different location

2. **Check file size:**
   - Large datasets may timeout
   - Export in smaller chunks
   - Limit data range

3. **Try different format:**
   - JSON vs CSV
   - Simpler format may work

4. **Browser issues:**
   - Allow downloads
   - Check download folder
   - Try different browser

## Frequently Asked Questions

### General Questions

**Q: Is NGMAT free to use?**
A: NGMAT is open-source software. Check the repository license for details.

**Q: Can I use NGMAT offline?**
A: The desktop application supports offline mode. The web application requires internet connection.

**Q: What missions can NGMAT simulate?**
A: NGMAT supports LEO, MEO, GEO, HEO, lunar, and interplanetary missions.

**Q: Is NGMAT accurate enough for real missions?**
A: NGMAT uses industry-standard models and algorithms. However, always verify critical results.

### Technical Questions

**Q: What coordinate systems does NGMAT support?**
A: ECI J2000, ECEF, Moon-Centered Inertial, and custom coordinate systems.

**Q: Can NGMAT import GMAT scripts?**
A: Yes, NGMAT has limited GMAT script import capability (best effort).

**Q: What gravity models are available?**
A: JGM-3, EGM-96, GGM-03 up to 70x70 degree and order.

**Q: Does NGMAT support constellation analysis?**
A: Yes, multiple spacecraft can be propagated in parallel.

**Q: Can I write custom force models?**
A: The desktop application supports plugins for custom force models (advanced users).

### Data and Results

**Q: Where is my data stored?**
A: Web: Cloud database. Desktop: Local database in user directory.

**Q: How long are results retained?**
A: Depends on your configuration. Completed missions are retained indefinitely unless deleted.

**Q: Can I export data to other tools?**
A: Yes, export to CSV, JSON, and GMAT script formats.

**Q: What's the maximum propagation duration?**
A: No hard limit, but performance degrades for multi-year high-fidelity propagations.

### Collaboration

**Q: Can multiple users work on the same mission?**
A: Yes, missions can be shared with edit permissions. Use comments to coordinate.

**Q: Are changes tracked?**
A: Yes, all mission changes are logged with user and timestamp.

**Q: Can I revert changes?**
A: Clone the mission before major changes. Full version control coming soon.

## Getting Additional Help

### Documentation

- [User Guide](README.md) - Comprehensive documentation
- [Getting Started](getting-started.md) - Quick start guide
- [API Documentation](../README.md) - Developer resources

### Community Support

- **GitHub Issues** - Report bugs and request features
- **GitHub Discussions** - Ask questions and share experiences
- **Wiki** - Community-contributed guides and tips

### Reporting Bugs

When reporting issues, include:

1. **Description** - Clear description of problem
2. **Steps to reproduce** - How to recreate issue
3. **Expected behavior** - What should happen
4. **Actual behavior** - What actually happens
5. **Screenshots** - If applicable
6. **System info** - OS, browser, NGMAT version
7. **Error messages** - Complete error text

**Example:**
```
Title: Propagation fails for high eccentricity orbits

Description:
When propagating orbit with e > 0.9, propagation fails after a few steps.

Steps to reproduce:
1. Create mission with e = 0.95
2. Set propagator to RK45
3. Run propagation for 1 day

Expected: Propagation completes successfully
Actual: Fails with "Step size below minimum" error

System: Windows 11, NGMAT Desktop v0.1.0
Error: [Full error message here]
```

### Feature Requests

Submit feature requests via GitHub Issues with:
- Clear description of desired feature
- Use cases and benefits
- Examples from other tools (if applicable)

---

**Still need help?** Visit the [Community Forum](https://github.com/QuinntyneBrown/NGMAT/discussions) or create an [Issue](https://github.com/QuinntyneBrown/NGMAT/issues).
