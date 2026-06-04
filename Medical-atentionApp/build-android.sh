#!/bin/bash
set -e

CONFIG=${1:-Debug}
IMAGE="xamarin-medical-build"
PROJECT="Medical-atention/Medical-atention.Android/Medical-atention.Android.csproj"
APK_DIR="Medical-atention/Medical-atention.Android/bin/$CONFIG"

if ! docker image inspect "$IMAGE" > /dev/null 2>&1; then
    echo ">>> Imagen no encontrada, construyendo..."
    docker build -f Dockerfile.android -t "$IMAGE" .
fi

echo ">>> Compilando y firmando (configuración: $CONFIG)..."
docker run --rm \
    -v "$(pwd):/workspace" \
    -w /workspace \
    "$IMAGE" \
    bash -c "msbuild /t:Restore /v:quiet /p:Configuration=$CONFIG Medical-atention/Medical-atention/Medical-atention.csproj \
          && nuget restore $PROJECT -Verbosity quiet \
          && msbuild /t:SignAndroidPackage /p:Configuration=$CONFIG $PROJECT"

APK=$(find "$APK_DIR" -name "*-Signed.apk" | head -1)

if [ -z "$APK" ]; then
    echo "ERROR: No se encontró APK firmado en $APK_DIR"
    exit 1
fi

echo ">>> APK generado: $APK"

if adb devices | grep -q "device$"; then
    echo ">>> Instalando en dispositivo..."
    adb shell settings put global verifier_verify_adb_installs 0
    adb shell settings put global package_verifier_enable 0
    adb install -r "$APK"
    adb shell settings put global verifier_verify_adb_installs 1
    adb shell settings put global package_verifier_enable 1
    echo ">>> Instalación completada."
else
    echo ">>> No hay dispositivo conectado. APK listo para instalar manualmente: $APK"
fi
