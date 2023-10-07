# Autogen Rundown 🎲

Automatic Rundown generation, using procedural seed based generation.

## Usage

Go to the Autogen Rundown Thunderstore mod page (https://gtfo.thunderstore.io/package/the_tavern/AutogenRundown/) and install either via your mod manager or manually by downloading the zip archive.

## Features progress

* Levels
    * [x] A Tier
    * [x] B Tier
    * [ ] C Tier
    * [ ] D Tier
    * [ ] E Tier
    * Additional objectives
        * [ ] Secondary
        * [ ] Overload
    * [ ] Dimensions
* Objectives
    * [x] Clear Path — *Navigate through the zones to a separate extraction elevator*
    * [x] Gather Small Items — *IDs, GLPs etc.*
    * [ ] HSU Find Sample
    * [ ] Reactor Startup
    * [ ] Reactor Shutdown
    * [ ] Input Special Terminal Command
    * [ ] Retrieve Big Items — *Fog Turbine etc.*
    * [ ] Power Cell Distribution — *Distributing cells to individual generators*
    * [ ] Terminal Uplink
    * [ ] Central Generator Cluster — *Fetching cells for a central generator cluster*
    * [ ] HSU Activate Small
    * [ ] Survival
    * [ ] Gather Terminal
    * [ ] Corrupted Terminal Uplink
    * [ ] Timed Terminal Sequence
* Enemies
    * [x] Basic hybernation
    * [ ] Event based activation
    * Types of enemies present
        * [x] Strikers / Shooters
        * [x] Giants
        * [ ] Scouts
        * [ ] Chargers
        * [ ] Shadows
        * [ ] Hybrids
        * [ ] Mothers
        * [ ] Tanks
        * [ ] Pouncers (Snatchers)
* Alarms
    * [x] Basic alarms
    * [x] Blood doors
    * [x] Error alarms
    * [ ] S-Class alarms
    * [ ] Surge alarms
    * [ ] High class alarms (> Class V)
* Challenges
    * [ ] Fog
    * [ ] Infectious fog
    * [ ] Darkness / Lights change 
* Other
    * [ ] More and better lights
    * [ ] More custom geomorphs

## Build flow

Query the API for the package
```console
$ https GET https://gtfo.thunderstore.io/api/experimental/package/BepInEx/BepInExPack_GTFO/3.2.1/

HTTP/1.1 200 OK
CF-Cache-Status: DYNAMIC
CF-RAY: 812301df6c621568-SJC
Connection: keep-alive
Content-Encoding: gzip
Content-Type: application/json
Date: Sat, 07 Oct 2023 03:20:16 GMT
NEL: {"success_fraction":0,"report_to":"cf-nel","max_age":604800}
Report-To: {"endpoints":[{"url":"https:\/\/a.nel.cloudflare.com\/report\/v3?s=KdSrQUxI4c1GR9QqPSVpQWA6s%2Fa6lIVIVTlxVhNp3xxyRusi6CT7LIktt%2F%2BBNFco1gQEI1UgIIbQ53OBpZ39bK%2FT7%2FqVvmpa9fIVLKY7RnWMx1zaKI3KpxS0NWv9UxTZnFbGmd6%2F"}],"group":"cf-nel","max_age":604800}
Server: cloudflare
Transfer-Encoding: chunked
allow: GET, HEAD, OPTIONS
referrer-policy: same-origin
strict-transport-security: max-age=15724800; includeSubDomains
vary: Cookie, Origin
x-content-type-options: nosniff
x-frame-options: DENY

{
    "date_created": "2023-08-07T23:35:49.213399Z",
    "dependencies": [],
    "description": "BepInEx pack for GTFO. Preconfigured and includes Unity Base DLLs.",
    "download_url": "https://thunderstore.io/package/download/BepInEx/BepInExPack_GTFO/3.2.1/",
    "downloads": 8250,
    "full_name": "BepInEx-BepInExPack_GTFO-3.2.1",
    "icon": "https://gcdn.thunderstore.io/live/repository/icons/BepInEx-BepInExPack_GTFO-3.2.1.png",
    "is_active": true,
    "name": "BepInExPack_GTFO",
    "namespace": "BepInEx",
    "version_number": "3.2.1",
    "website_url": "https://github.com/BepInEx/BepInEx"
}
```

Use the download URL returned to query for the download file:

```console
$ https GET https://thunderstore.io/package/download/BepInEx/BepInExPack_GTFO/3.2.1/

HTTP/1.1 302 Found
CF-Cache-Status: DYNAMIC
CF-RAY: 812307be6d07cea4-SJC
Connection: keep-alive
Content-Type: text/html; charset=utf-8
Date: Sat, 07 Oct 2023 03:24:17 GMT
NEL: {"success_fraction":0,"report_to":"cf-nel","max_age":604800}
Report-To: {"endpoints":[{"url":"https:\/\/a.nel.cloudflare.com\/report\/v3?s=L9mBoe5cHR%2Bk3puWUy%2BK1EbHe98VU%2BJf3Rp0E4j8KY1e%2BT4mrY4FQzkR3YBmx87zO9fgphd0FpH2XP3dbQuqkzIVsUHR8gmyLI97pEO3bq5E1L4dvVgCEXk1wy5S4x7iGw%3D%3D"}],"group":"cf-nel","max_age":604800}
Server: cloudflare
Transfer-Encoding: chunked
location: https://gcdn.thunderstore.io/live/repository/packages/BepInEx-BepInExPack_GTFO-3.2.1.zip
referrer-policy: same-origin
strict-transport-security: max-age=15724800; includeSubDomains
vary: Cookie, Origin
x-content-type-options: nosniff
x-frame-options: DENY


```

Finally use the `location` return header to download the package
```console
$ curl -O https://gcdn.thunderstore.io/live/repository/packages/BepInEx-BepInExPack_GTFO-3.2.1.zip
  % Total    % Received % Xferd  Average Speed   Time    Time     Time  Current
                                 Dload  Upload   Total   Spent    Left  Speed
100 31.6M  100 31.6M    0     0  54.1M      0 --:--:-- --:--:-- --:--:-- 54.7M
```