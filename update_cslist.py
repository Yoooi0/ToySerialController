import os
from pathlib import Path

with open('ADD_ME.cslist', 'w', encoding = 'utf-8') as cslist:
    for file in sorted(list(Path('.').rglob('*.cs'))):
    	if str(file).startswith('src') or str(file).startswith('lib'):
	    	cslist.write(str(file) + "\n")
    		print(str(file))
