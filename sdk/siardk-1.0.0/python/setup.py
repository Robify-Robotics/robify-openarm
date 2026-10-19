
from setuptools import setup, find_packages
import platform

setup(
    name='siardk',
    version='3.7.31',
    packages=['siardk'],
    package_dir={'': 'src'},
    license='Apache 2.0',
    description='SIARDK 3. python package',
    long_description=open('README.txt').read(),
    url='https://github.com/VKROBOT-GIT/siarSDK',
    author='L Jung',
    author_email='rwjung@vkrobot.com',
    install_requires=['pyserial']
)
