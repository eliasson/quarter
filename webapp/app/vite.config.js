import path from "path";

export default {
    // The root is where the HTML file that is the entry point is located.
    root: path.resolve(__dirname, "www"),
    // The web client is always accessed using the /ui/ base path.
    base: "/ui/",

    server: {
        fs: {
            // Allow the dev server to serve the CSS and fonts from the shared project.
            allow: [path.resolve(__dirname, "..")],
        },
    },

    build: {
        // The dist directory is where the paper backend will serve it from.
        outDir: path.resolve(__dirname, "..", "..", "dist", "ui"),
        emptyOutDir: true,
        rollupOptions: {
            input: {
                main: path.resolve(__dirname, "www", "index.html"),
                login: path.resolve(__dirname, "www", "login.html"),
                welcome: path.resolve(__dirname, "www", "welcome.html"),
            },
        },
    },
};
