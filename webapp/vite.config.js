import path from "path";

export default {
    // The root is where the HTML file that is the entry point is located.
    root: "www",
    // The web client is always accessed using the /ui/ base path.
    base: "ui",

    build: {
        // The dist directory is where the paper backend will serve it from.
        outDir: path.resolve(__dirname, "..", "dist"),
        rollupOptions: {
            input: {
                main: path.resolve(__dirname, "www", "index.html"),
                login: path.resolve(__dirname, "www", "login.html"),
            },
        },
    },
};
