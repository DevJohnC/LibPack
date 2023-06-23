# LibPack

Native library packaging tool for macOS and Linux.

## What is library packing?

`libpack` copies the specified shared library (.dylib, .so) into an output directory with it's dependencies and rewrites
shared library references to work from `@loader_path` (ie, it rewrites shared libraries to work with all dependencies
located in a single directory).

## Installation

Coming soon after github actions are built.

### Dependencies

#### MacOS

* xcode (`lipo`, `otool`, `install_name_tool`)

#### Linux

* `ldd`
* `patchelf`
* `pkg-config`

## Usage

### Simple Usage

After you have built your library package it to a directory of your choosing with:

`libpack /path/to/library.dylib /path/to/pack/dir`

### More Detailed

A more detailed example might look like this:

```
# cd mylibsrc
# ./configure
# make
# libpack ./objs/.libs/mylib.so ./packed
```

Notice that we are running `make` without a `make install`. Installing your library in your system is not a requirement.