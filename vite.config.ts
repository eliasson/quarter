import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
    root: "./src/Quarter.Web",
    base: "ui",
    build: {
        outDir: path.resolve(__dirname, "dist")
    },
    plugins: [
        vue(),
        vueDevTools(),
    ],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src/Quarter.Web', import.meta.url))
        },
    },
})
