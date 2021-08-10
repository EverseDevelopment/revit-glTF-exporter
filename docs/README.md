# revit-glTF-exporter
Export glTF files from revit.

<img src="images/revit-gltf-exporter-logo.png" height="300">

[![CircleCI](https://circleci.com/gh/voyansi/revit-glTF-exporter/tree/master.svg?style=svg)](https://circleci.com/gh/voyansi/revit-glTF-exporter/?branch=master)

## Details

- Builds for Revit versions 2017 - 2020
- Includes instance and type parameters of exported elements as glTF "extras" (e.g. in Three.js, this info will appear in an object's `userData` property)
- Includes a `batchId` buffer array identifying the Revit ElementId of each mesh vertex, for use in WebGL shader operations