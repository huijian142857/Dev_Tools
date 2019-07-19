#!/bin/bash
#clear project
python /Users/yangqijun/Desktop/stick/trunk/Tools/BuildTools/buildclient.py clear
if [ $? -ne 0 ]; then
	exit 1;
else
	echo "--------clear project success!--------";
fi

#svn up
python /Users/yangqijun/Desktop/stick/trunk/Tools/BuildTools/buildclient.py svnup
if [ $? -ne 0 ]; then
	exit 1;
else
	echo "--------svn up success!--------";
fi

#devconfig
python /Users/yangqijun/Desktop/stick/trunk/Tools/BuildTools/buildclient.py --buildconf=debug devconfig
if [ $? -ne 0 ]; then
	exit 1;
else
	echo "--------modify devconfig success!--------";
fi

#pack ab file
python /Users/yangqijun/Desktop/stick/trunk/Tools/BuildTools/buildclient.py ptab_android
if [ $? -ne 0 ]; then
	exit 1;
else
	echo "--------pack ab file success!--------";
fi

#copyabtostreamassert
python /Users/yangqijun/Desktop/stick/trunk/Tools/BuildTools/buildclient.py copyabtostreamassert
if [ $? -ne 0 ]; then
	exit 1;
else
	echo "--------copyabtostreamassert success!--------";
fi

#build_debug
#python /Users/yangqijun/Desktop/stick/trunk/Tools/BuildTools/buildclient.py --buildconf=release build_android
python /Users/yangqijun/Desktop/stick/trunk/Tools/BuildTools/buildclient.py --buildconf=debug build_android
if [ $? -ne 0 ]; then
	exit 1;
else
	echo "--------build_debug success!--------";
fi

#rename apk
python /Users/yangqijun/Desktop/stick/trunk/Tools/BuildTools/renameapk.py
if [ $? -ne 0 ]; then
	exit 1;
else
	echo "--------rename apk success!--------";
fi
