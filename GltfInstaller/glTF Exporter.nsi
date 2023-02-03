Unicode true
ManifestSupportedOS all
!include LogicLib.nsh
!include x64.nsh
!include FileFunc.nsh
!macro REG_KEY_VALUE_EXISTS ROOT_KEY SUB_KEY NAME
    ClearErrors
    push $0

    ${If} "${NAME}" == ""
        ${If} ${RunningX64}
            SetRegView 64
            EnumRegKey $0 "${ROOT_KEY}" "${SUB_KEY}" 0
            SetRegView 32
        ${Else}
            SetErrors
        ${EndIf}

        ${If} ${Errors}
            EnumRegKey $0 "${ROOT_KEY}" "${SUB_KEY}" 0
        ${EndIf}
    ${Else}
        ${If} ${RunningX64}
            SetRegView 64
            ReadRegStr $0 "${ROOT_KEY}" "${SUB_KEY}" "${NAME}"
            SetRegView 32
        ${Else}
            SetErrors
        ${EndIf}

        ${If} ${Errors}
            ReadRegStr $0 "${ROOT_KEY}" "${SUB_KEY}" "${NAME}"
        ${EndIf}
    ${EndIf}

    pop $0
!macroend

OutFile "C:\Repos\e-verse\revit-glTF-exporter\GltfInstaller\glTF Exporter.exe"
RequestExecutionLevel none
SilentInstall silent
VIProductVersion "1.0.0.0"
VIAddVersionKey "ProductName" "Test Application"
VIAddVersionKey "CompanyName" "Test company"
VIAddVersionKey "LegalCopyright" "Copyright Test company"
VIAddVersionKey "FileDescription" "Test Application"
VIAddVersionKey "FileVersion" "1.0.0"
VIAddVersionKey "ProductVersion" "1.0.0.0"
VIAddVersionKey "InternalName" "setup.exe"
VIAddVersionKey "OriginalFilename" "setup.exe"
Function .onInit
InitPluginsDir
${GetParameters} $R0
primary:
StrCpy $R1 $R0
File "/oname=$PLUGINSDIR\MyProduct.msi" "C:\Repos\e-verse\revit-glTF-exporter\GltfInstaller\MyProduct.msi"
ExpandEnvStrings $R1 '$R1'
ExecWait '"$%WINDIR%\System32\msiexec.exe" /I "$PLUGINSDIR\MyProduct.msi" $R1' $0
SetErrorlevel $0
goto end
end:
FunctionEnd
Section
SectionEnd
