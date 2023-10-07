const fs = require("fs");
const path = require("path");

const run = async ({ github, context, core, io, fetch }) => {
  console.log("Downloading Thunderstore package...");

  const manifest = JSON.parse(fs.readFileSync("manifest.json", "utf8"));
  const { dependencies } = manifest;

  console.log(":: Downloading dependencies");

  io.mkdirP("./deps");

  for await (const dependency of dependencies) {
    const [, team, package, version] = dependency.match(
      /^([a-zA-Z0-9_]*)-([a-zA-Z0-9_]*)-([0-9.]*)$/
    );

    console.log(`   -> Fetching: ${team}-${package} @ ${version}`);

    const response = await fetch(
      `https://gcdn.thunderstore.io/live/repository/packages/${dependency}.zip`
    );

    // Create a write stream for the output file
    const dest = fs.createWriteStream(
      path.resolve(".", "deps", `${team}-${package}.zip`)
    );

    await new Promise((resolve, reject) => {
      response.body.pipe(dest);
      response.body.on("error", reject);
      dest.on("finish", resolve);
    });
  }
};

module.exports = run;
