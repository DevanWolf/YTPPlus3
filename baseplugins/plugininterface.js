// Implementation of YTP+ CLI for YTP+++

const fs = require('fs'), path = require('path');

// Get command line parameters
const args = process.argv.slice(2);
const params = {};
for (let i = 0; i < args.length; i++) {
    const arg = args[i];
    if (arg.startsWith('--')) {
        const key = arg.slice(2);
        const value = args[i + 1];
        params[key] = value;
        i++;
    }
}

// Param plugin is which plugin to run
const plugin = params.plugin;
if (!plugin) {
    console.error('No plugin specified');
    process.exit(1);
}

// Find the plugin in ./js
const pluginPath = `./js/${plugin}`;
if (!fs.existsSync(pluginPath)) {
    console.error(`Plugin ${plugin} not found`);
    process.exit(1);
}

// Load the plugin
const pluginModule = require(pluginPath);

// Construct shared folder
const sharedFolder = `${process.cwd()}/shared`;
if (!fs.existsSync(sharedFolder)) {
    fs.mkdirSync(sharedFolder);
}

// Construct temp folder in shared folder
const tempFolder = `${sharedFolder}/temp`;
if (!fs.existsSync(tempFolder)) {
    fs.mkdirSync(tempFolder);
}

const toolbox = {
    input: "",
    output: "",
    clips: parseInt(params.clips),
    minstream: parseFloat(params.minstream),
    maxstream: parseFloat(params.maxstream),
    width: parseInt(params.width),
    height: parseInt(params.height),
    fps: parseInt(params.fps),
    usetransitions: parseInt(params.usetransitions) === 1,
    transitions: ""
};

const video = path.join(process.cwd(), "..", "temp", params.video);

// Run the plugin
pluginModule.plugin(video, toolbox, process.cwd(), true);
