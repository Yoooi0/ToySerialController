#!/usr/bin/env python3
import os
import sys
import glob
from zipfile import ZipFile

if len(sys.argv) == 2:
    version = int(sys.argv[1])
else:
    version = int(input('var version: '))

cslistName = 'ToySerialController.cslist'
varName = 'Yoooi.ToySerialController.{}.var'.format(version)
zipPath = 'Custom/Scripts/Yoooi/ToySerialController/'

print('Creating "{}"'.format(cslistName))
with open(cslistName, 'w+', encoding='utf-8') as cslist:
    for file in sorted(glob.glob('**/*.cs', recursive=True)):
        if not file.startswith('src') and not file.startswith('lib'):
            continue

        cslist.write('{}\n'.format(file))
        print('Adding "{}"'.format(file))

print('Creating "{}"'.format(varName))
with open(cslistName, 'r', encoding = 'utf-8') as cslist:
    with ZipFile(varName, 'w') as var:
        var.write('meta.json')
        var.write('LICENSE.md')
        var.write(cslistName, os.path.join(zipPath, cslistName))
        for file in [x.strip() for x in cslist]:
            var.write(file, os.path.join(zipPath, file))

        for file in var.namelist():
            print('Adding "{}"'.format(file))
