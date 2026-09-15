import path from "node:path";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(import.meta.dirname, "./src"),
    },
  },
  server: {
    // El backend (Program.cs) usa este mismo puerto fijo solo cuando corre en
    // Development (dotnet run); en producción sigue usando el puerto efímero.
    proxy: {
      "/api": "http://127.0.0.1:5244",
    },
  },
});
