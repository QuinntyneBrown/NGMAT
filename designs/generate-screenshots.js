/**
 * NGMAT Screenshot Generator
 *
 * Generates PNG screenshots from HTML mockups using Puppeteer.
 *
 * Prerequisites:
 *   npm install puppeteer
 *
 * Usage:
 *   node generate-screenshots.js
 *
 * Options:
 *   --file <name>    Generate screenshot for a specific file only
 *   --width <px>     Set viewport width (default: 1920)
 *   --height <px>    Set viewport height (default: 1080)
 */

const puppeteer = require('puppeteer');
const path = require('path');
const fs = require('fs');

// Configuration
const CONFIG = {
    mockupsDir: path.join(__dirname, 'mockups'),
    screenshotsDir: path.join(__dirname, 'screenshots'),
    viewport: {
        width: 1920,
        height: 1080,
        deviceScaleFactor: 1
    },
    // Files to generate screenshots for (in order)
    files: [
        '01-login.html',
        '02-dashboard.html',
        '03-mission-editor.html',
        '04-spacecraft-builder.html',
        '05-orbit-visualization.html',
        '06-maneuver-planner.html',
        '07-propagation-control.html',
        '08-charts.html',
        '09-ground-track.html',
        '10-reporting.html',
        '11-script-editor.html',
        '12-settings.html',
        '13-profile.html',
        '14-notifications.html'
    ]
};

// Parse command line arguments
function parseArgs() {
    const args = process.argv.slice(2);
    const options = {
        file: null,
        width: CONFIG.viewport.width,
        height: CONFIG.viewport.height
    };

    for (let i = 0; i < args.length; i++) {
        switch (args[i]) {
            case '--file':
                options.file = args[++i];
                break;
            case '--width':
                options.width = parseInt(args[++i], 10);
                break;
            case '--height':
                options.height = parseInt(args[++i], 10);
                break;
            case '--help':
                console.log(`
NGMAT Screenshot Generator

Usage: node generate-screenshots.js [options]

Options:
  --file <name>    Generate screenshot for a specific file only
                   Example: --file 01-login.html
  --width <px>     Set viewport width (default: 1920)
  --height <px>    Set viewport height (default: 1080)
  --help           Show this help message
                `);
                process.exit(0);
        }
    }

    return options;
}

// Ensure screenshots directory exists
function ensureDirectoryExists(dir) {
    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
        console.log(`Created directory: ${dir}`);
    }
}

// Generate screenshot for a single HTML file
async function generateScreenshot(page, htmlFile, options) {
    const htmlPath = path.join(CONFIG.mockupsDir, htmlFile);
    const screenshotName = htmlFile.replace('.html', '.png');
    const screenshotPath = path.join(CONFIG.screenshotsDir, screenshotName);

    // Check if HTML file exists
    if (!fs.existsSync(htmlPath)) {
        console.error(`  ✗ File not found: ${htmlFile}`);
        return false;
    }

    try {
        // Navigate to the HTML file
        const fileUrl = `file://${htmlPath.replace(/\\/g, '/')}`;
        await page.goto(fileUrl, {
            waitUntil: 'networkidle0',
            timeout: 30000
        });

        // Wait for any animations to complete
        await page.evaluate(() => new Promise(resolve => setTimeout(resolve, 500)));

        // Take screenshot
        await page.screenshot({
            path: screenshotPath,
            fullPage: false,
            type: 'png'
        });

        console.log(`  ✓ ${htmlFile} → ${screenshotName}`);
        return true;
    } catch (error) {
        console.error(`  ✗ Error generating ${htmlFile}: ${error.message}`);
        return false;
    }
}

// Main function
async function main() {
    const options = parseArgs();

    console.log('\n╔══════════════════════════════════════════════════════════════╗');
    console.log('║           NGMAT Screenshot Generator                         ║');
    console.log('╚══════════════════════════════════════════════════════════════╝\n');

    // Ensure screenshots directory exists
    ensureDirectoryExists(CONFIG.screenshotsDir);

    // Determine which files to process
    let filesToProcess = CONFIG.files;
    if (options.file) {
        if (!CONFIG.files.includes(options.file)) {
            console.error(`Error: File "${options.file}" not found in mockups list.`);
            console.log('\nAvailable files:');
            CONFIG.files.forEach(f => console.log(`  - ${f}`));
            process.exit(1);
        }
        filesToProcess = [options.file];
    }

    console.log(`Viewport: ${options.width}x${options.height}`);
    console.log(`Files to process: ${filesToProcess.length}\n`);
    console.log('Generating screenshots...\n');

    // Launch browser
    let browser;
    try {
        browser = await puppeteer.launch({
            headless: 'new',
            args: [
                '--no-sandbox',
                '--disable-setuid-sandbox',
                '--disable-dev-shm-usage',
                '--disable-gpu'
            ]
        });

        const page = await browser.newPage();

        // Set viewport
        await page.setViewport({
            width: options.width,
            height: options.height,
            deviceScaleFactor: CONFIG.viewport.deviceScaleFactor
        });

        // Process each file
        let successCount = 0;
        let failCount = 0;

        for (const file of filesToProcess) {
            const success = await generateScreenshot(page, file, options);
            if (success) {
                successCount++;
            } else {
                failCount++;
            }
        }

        console.log('\n────────────────────────────────────────────────────────────────');
        console.log(`\nComplete! ${successCount} screenshots generated, ${failCount} failed.`);
        console.log(`Screenshots saved to: ${CONFIG.screenshotsDir}\n`);

    } catch (error) {
        console.error(`\nFatal error: ${error.message}`);
        console.error('\nMake sure Puppeteer is installed:');
        console.error('  npm install puppeteer\n');
        process.exit(1);
    } finally {
        if (browser) {
            await browser.close();
        }
    }
}

// Run
main().catch(console.error);
