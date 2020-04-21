import os
from pathlib import Path

with open('ADD_ME.cslist', 'w', encoding = 'utf-8') as cslist:
    for file in sorted(list(Path('.').rglob('*.cs'))):
    	cslist.write(str(file) + "\n")
    	print(file.name)
