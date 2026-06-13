const http = require("http");
const fs = require("fs");
const path = require("path");

const port = Number(process.env.PORT || 5173);
const rootDirectory = __dirname;
const assetsDirectory = path.join(__dirname, "..", "assets");

const mimeTypes = {
  ".html": "text/html; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".json": "application/json; charset=utf-8",
  ".map": "application/json; charset=utf-8",
  ".png": "image/png",
};

function resolveFilePath(requestUrl) {
  const parsedUrl = new URL(requestUrl, `http://localhost:${port}`);
  const pathname = decodeURIComponent(parsedUrl.pathname);

  if (pathname.startsWith("/assets/")) {
    const assetPath = path.normalize(path.join(assetsDirectory, pathname.replace("/assets/", "")));

    return assetPath.startsWith(assetsDirectory) ? assetPath : null;
  }

  const requestedPath = pathname === "/" ? "/index.html" : pathname;
  const filePath = path.normalize(path.join(rootDirectory, requestedPath));

  if (!filePath.startsWith(rootDirectory)) {
    return null;
  }

  return filePath;
}

const server = http.createServer((request, response) => {
  const filePath = resolveFilePath(request.url);

  if (!filePath) {
    response.writeHead(403, { "Content-Type": "text/plain; charset=utf-8" });
    response.end("Forbidden");
    return;
  }

  fs.readFile(filePath, (error, content) => {
    if (error) {
      response.writeHead(404, { "Content-Type": "text/plain; charset=utf-8" });
      response.end("Not found");
      return;
    }

    const extension = path.extname(filePath);
    response.writeHead(200, {
      "Content-Type": mimeTypes[extension] || "application/octet-stream",
      "Cache-Control": "no-store",
    });
    response.end(content);
  });
});

server.listen(port, () => {
  console.log(`SysScore dashboard is running at http://localhost:${port}`);
});
