# Symbol Swinger

Outputs a list of any special unicode symbols used in the html document
This can then be fed to a tool like [pyftsubset](https://fonttools.readthedocs.io/en/latest/subset/index.html) to trim a font to only include used glyphs

use like: `symbolswinger icons.html`
Note: If you can Purge the css before running this it will be much much faster (not that it's slow )

## Features

Currently supports unicode charactors in css contents blocks and in hexidecimal html entities eg:
supports both local file and http/https url css links

```html
<span> &#xf00;</span>
```

## Name

The name is a riff on [glyphhanger](https://github.com/zachleat/glyphhanger) which is a much more complex tool that this aims to emulate the basic features.
glyphhanger doesn't work well on linux for me and has a pretty crazy looking method of extracting glyphs which involves starting a webserver and injecting javascript into the html document.
