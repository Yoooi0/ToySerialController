import os
import sys
import glob
import json
from zipfile import ZipFile

if len(sys.argv) == 2:
    version = int(sys.argv[1])
else:
    version = int(input('var version: '))

cslistName = 'ADD_ME.cslist'
varName = 'Yoooi.ToySerialController.{}.var'.format(version)
zipPath = 'Custom\\Scripts\\Yoooi\\ToySerialController\\'

print('Reading meta.json')
with open('meta.json') as f:
    meta = json.load(f)

print('Creating "{}"'.format(cslistName))
with open('ADD_ME.cslist', 'w+', encoding='utf-8') as cslist:
    for file in sorted(glob.glob('**/*.cs', recursive=True)):
        if not file.startswith('src') and not file.startswith('lib'):
            continue

        print('Adding "{}"'.format(file))
        cslist.write('{}\n'.format(file))
        meta['contentList'].append(os.path.join(zipPath, file))

print('Creating "{}"'.format(varName))
with open(cslistName, 'r', encoding = 'utf-8') as cslist:
    with ZipFile(varName, 'w') as var:
        var.writestr('meta.json', json.dumps(meta, indent=3))
        var.write('LICENSE.md')
        var.write(cslistName, os.path.join(zipPath, cslistName))
        for file in [x.strip() for x in cslist]:
            var.write(file, os.path.join(zipPath, file))

        for file in var.namelist():
            print('Adding "{}"'.format(file))
