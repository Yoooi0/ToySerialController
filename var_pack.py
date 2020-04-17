import os
from zipfile import ZipFile

path = 'Custom\\Scripts\\Yoooi\\ToySerialController\\'
with open('ADD_ME.cslist', 'r', encoding = 'utf-8') as cslist:
    with ZipFile('Yoooi.ToySerialController.1.var', 'w') as output:
        output.write('meta.json')
        output.write('LICENSE')
        os.chdir('../../../../')
        output.write(os.path.join(path, 'ADD_ME.cslist'))
        for line in cslist:
            output.write(os.path.join(path, line.strip()))
