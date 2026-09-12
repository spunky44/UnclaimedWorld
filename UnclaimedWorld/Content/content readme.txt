
Monogame cannot currently build:
.fbx 
Curve xml files

Until Monogame has support for all content, build it in the XNA project.
Then copy the xnb files to Content/bin
The files will be copied automatically to the output dirs during build.

Model fbx'es contain references to textures too. They have suffixes like _01 etc. because they were built multiple times by XNA. 
These must also be copied to the Content/bin folder. 
Otherwise content load will fail with an exception. Look at the Inner exception to get more info.