Due to the fact that font building can take a long time, we copy built fonts from bin\<platform>\<font>.xnb to here.
In the platform projects, we reference the built fonts here.
This way we can:
1. Add in sprite-font definitions to the pipeline tool when needed.
2. Build the sprite-font.
3. Copy the resulting xnb file to here.
4. Remove the sprite-font definition from the pipeline tool.

Notes:
- Current arial-core.xnb is defined for font size of 24.