const fs = require("fs");
const path = require("path");

const run = async ({ github, context, core, fetch }) => {
  console.log("Downloading Thunderstore package...");

  const manifest = JSON.parse(fs.readFileSync("manifest.json", "utf8"));
  // const dependencies = manifest.dependencies.map((dep) => {
  //   const [, team, package, version] = dep.match(
  //     /^([a-zA-Z0-9_]*)-([a-zA-Z0-9_]*)-([0-9.]*)$/
  //   );

  //   return { team, package, version };
  // });
  const { dependencies } = manifest;

  console.log(":: Downloading dependencies");

  // for await (const { team, package, version } of dependencies) {
  //   console.log(`-> Fetching: ${team}_${package} @ ${version}`);

  //   const res = await fetch(
  //     `https://gtfo.thunderstore.io/api/experimental/package/${team}/${package}/${version}/`
  //   );
  //   const body = await res.json();
  //   const { download_url } = body;

  //   console.log("     Found download url: ", download_url);

  //   await sleep(1000);

  //   const res2 = await fetch(download_url);
  //   const zipUrl = res2.headers.get("location");

  //   console.log("     Zip:", zipUrl);

  //   await sleep(1000);
  // }

  // io.mkdirP("./deps");

  for await (const dependency of dependencies) {
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
