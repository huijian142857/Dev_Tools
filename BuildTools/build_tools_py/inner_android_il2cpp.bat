@echo off
:clearproject
@python buildclient.py clear
if %errorlevel% NEQ 0 (
    echo error:clearproject fail %errorlevel%
    pause
)

:svnup
@python buildclient.py svnup
if %errorlevel% NEQ 0 (
    echo error:svnup fail %errorlevel%
    pause
)

:devconfig
@python buildclient.py --buildconf=debug devconfig
if %errorlevel% NEQ 0 (
    echo error:devconfig fail %errorlevel%
    pause
)

:pack ab file
@python buildclient.py ptab_android
if %errorlevel% NEQ 0 (
    echo error:ptab_android fail %errorlevel%
    pause
)

:copyabtostreamassert
@python buildclient.py copyabtostreamassert
if %errorlevel% NEQ 0 (
    echo error:copyabtostreamassert fail %errorlevel%
    pause
)

:build_debug
::python buildclient.py --buildconf=release build_android
@python buildclient.py --buildconf=debug build_android
if %errorlevel% NEQ 0 (
    echo error:build_debug fail %errorlevel%
    pause
)

:rename apk
@python renameapk.py
if %errorlevel% NEQ 0 (
    echo error:rename apk fail %errorlevel%
    pause
)

@pause