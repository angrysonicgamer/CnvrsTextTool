# Cnvrs-text tool
Simple console tool to work with text stored in cnvrs-text files (Sonic Frontiers, Shadow Generations, etc.)

## Usage
Drag and drop a cnvrs-text file to the executable to extract text to a json file.
The only important data in the created json is text strings. Everything else is added mainly for context.
Edit text in that json and then drag and drop it to the executable to modify the corresponding cnvrs-text file data (it must also be in the same folder).
The modified file will be saved in the "New files" subfolder.

## CMD usage
`CnvrsTextTool filename`
##
The tool appends new text to the end of the source (cnvrs-text) file and rewrites text pointers (along with text length parameters), making its size slightly bigger.
If a provided cnvrs-text file already contains new text segment (marked as NEWTEXT) it will be removed and replaced with newer text.
It doesn't modify any other data so the speaker info is preserved.

This solution is basically a proof of concept.
But it allows text modding without losing speaker names in Shadow Generations or losing static event animations in Sonic Frontiers.
