// Implementation of YTP+ CLI for YTP+++

const { execSync } = require('child_process')

module.exports = {
    defaults: {
        input: "videos.txt",
        output: "output.mp4",
        clips: 20,
        minstream: 0.2,
        maxstream: 0.4,
        width: 640,
        height: 480,
        fps: 30,
        usetransitions: false,
        transitions: "transitions.txt"
    },
    ffmpeg: {
        runSync(command) {
            // Run ffmpeg natively
            execSync("ffmpeg " + command);
        }
    },
    mediainfo: null, // No documented plugin uses this
    getVideoProbe: (video) => {
        /*
        {
            duration : parseFloat(Duration),
            width    : parseFloat(Width),
            height   : parseFloat(Height),
            fps      : parseFloat(FrameRate),
            bitrate  : parseFloat(OverallBitRate),
            size     : parseFloat(FileSize),
        }
        */
        const ffprobeCommand = 'ffprobe -v error -print_format json -show_entries format=duration,width,height,avg_frame_rate,bit_rate,size ';
        const stdout = execSync(`${ffprobeCommand}"${video}"`);
        const ffprobeOutput = JSON.parse(stdout);
        const { format: { duration, width, height, avg_frame_rate, bit_rate, size } } = ffprobeOutput;
        const fps = eval(avg_frame_rate);
        const result = {
            duration: parseFloat(duration),
            width: parseFloat(width),
            height: parseFloat(height),
            fps: parseFloat(fps),
            bitrate: parseFloat(bit_rate),
            size: parseFloat(size),
        };
        return result;
    },
    getAudioProbe: (video) => {
        // Using ffprobe, gather a JSON object with this structure:
        /*
        {
            duration : parseFloat(Duration),
            bitrate  : parseFloat(OverallBitRate),
            size     : parseFloat(FileSize),
        }
        */
        const ffprobeCommand = 'ffprobe -v error -print_format json -show_entries format=duration,bit_rate,size ';
        const stdout = execSync(`${ffprobeCommand}"${video}"`);
        const ffprobeOutput = JSON.parse(stdout);
        const { format: { duration, bit_rate, size } } = ffprobeOutput;
        const result = {
            duration: parseFloat(duration),
            bitrate: parseFloat(bit_rate),
            size: parseFloat(size),
        };
        return result;
    },
    /* Trims down videos to a specific time and length */
    snipVideo: (video, startTime, endTime, output, resolution, fps, debug) => {
        var args = " -i \"" + `${video.replace(/\\/g,"\\\\").replace(/\//g,(process.platform === "win32" ? "\\\\" : "/"))}` + "\" -ss " + startTime + " -to " + endTime + " -pix_fmt yuv420p -vf scale=" + resolution[0]+"x" + resolution[1] + ",setsar=1:1,fps=fps=" + fps + " -ar 44100 -ac 2 -map_metadata -1 -map_chapters -1 -y \"" + output + ".mp4\"";
        return ffmpeg.runSync(args + (debug == false ? " -hide_banner -loglevel quiet" : ""));
    },
    /* Copies videos to what is normally the temporary directory */
    copyVideo: (video, output, resolution, fps, debug) => {
        var args =" -i \"" + `${video.replace(/\\/g,"\\\\").replace(/\//g,(process.platform === "win32" ? "\\\\" : "/"))}` + "\" -pix_fmt yuv420p -vf scale="+resolution[0]+"x"+resolution[1]+",setsar=1:1,fps=fps="+fps + " -ar 44100 -ac 2 -map_metadata -1 -map_chapters -1 -y \"" + output + ".mp4\"";
        return ffmpeg.runSync(args + (debug == false ? " -hide_banner -loglevel quiet" : ""));
    },
    /* Dummied out because it's not for plugin use */
    concatenateVideo: (count, out, debug) => {},
    /* Get a random integer between a minimum and a maximum number */
    randomInt: (min, max) => {
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }
}
