const fs = require("fs");
const path = require("path");

const run = async ({ github, context, core, io, fetch }) => {
  console.log("Downloading Thunderstore package...");

  const manifest = JSON.parse(fs.readFileSync("manifest.json", "utf8"));
  const { dependencies } = manifest;

  console.log(":: Downloading dependencies");

  io.mkdirP("./deps");

  for await (const dependency of dependencies) {
    console.log(`-> Fetching: ${dependency}`);

    const response = await fetch(
      `https://gcdn.thunderstore.io/live/repository/packages/${dependency}.zip`
    );

    // Create a write stream for the output file
    const dest = fs.createWriteStream(
      path.resolve(".", "deps", `${dependency}.zip`)
    );

    await new Promise((resolve, reject) => {
      response.body.pipe(dest);
      response.body.on("error", reject);
      dest.on("finish", resolve);
    });
  }
};

module.exports = run;
